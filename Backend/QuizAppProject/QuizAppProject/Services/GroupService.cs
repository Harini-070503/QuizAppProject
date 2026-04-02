using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Services
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _db;

        public GroupService(AppDbContext db) => _db = db;

        public async Task<List<GroupDto>> GetMyGroups(Guid evaluatorId)
        {
            var groups = await _db.QuizGroups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Where(g => g.EvaluatorId == evaluatorId)
                .AsNoTracking()
                .ToListAsync();

            return groups.Select(MapDto).ToList();
        }

        public async Task<GroupDto?> GetById(Guid groupId)
        {
            var g = await _db.QuizGroups
                .Include(g => g.Members).ThenInclude(m => m.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            return g == null ? null : MapDto(g);
        }

        public async Task<GroupDto> Create(Guid evaluatorId, GroupCreateDto dto)
        {
            var group = new QuizGroup
            {
                GroupId     = Guid.NewGuid(),
                GroupName   = dto.GroupName,
                Description = dto.Description,
                EvaluatorId = evaluatorId,
                CreatedAt   = DateTime.UtcNow
            };
            _db.QuizGroups.Add(group);
            await _db.SaveChangesAsync();
            return MapDto(group);
        }

        public async Task<bool> Delete(Guid groupId, Guid evaluatorId)
        {
            var g = await _db.QuizGroups.FindAsync(groupId);
            if (g == null) return false;
            if (g.EvaluatorId != evaluatorId)
                throw new UnauthorizedAccessException("Only the group owner can delete it.");

            _db.QuizGroups.Remove(g);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<GroupMemberDto> AddMember(Guid groupId, Guid evaluatorId, string usernameOrEmail)
        {
            var g = await _db.QuizGroups.FindAsync(groupId)
                ?? throw new KeyNotFoundException("Group not found.");
            if (g.EvaluatorId != evaluatorId)
                throw new UnauthorizedAccessException("Only the group owner can add members.");

            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail)
                ?? throw new KeyNotFoundException("User not found.");

            var exists = await _db.QuizGroupMembers
                .AnyAsync(m => m.GroupId == groupId && m.UserId == user.UserId);
            if (exists)
                throw new InvalidOperationException("User is already a member.");

            var member = new QuizGroupMember
            {
                Id      = Guid.NewGuid(),
                GroupId = groupId,
                UserId  = user.UserId,
                AddedAt = DateTime.UtcNow
            };
            _db.QuizGroupMembers.Add(member);
            await _db.SaveChangesAsync();

            return new GroupMemberDto
            {
                UserId   = user.UserId,
                Username = user.Username,
                Email    = user.Email,
                AddedAt  = member.AddedAt
            };
        }

        public async Task<bool> RemoveMember(Guid groupId, Guid evaluatorId, Guid userId)
        {
            var g = await _db.QuizGroups.FindAsync(groupId)
                ?? throw new KeyNotFoundException("Group not found.");
            if (g.EvaluatorId != evaluatorId)
                throw new UnauthorizedAccessException("Only the group owner can remove members.");

            var member = await _db.QuizGroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
            if (member == null) return false;

            _db.QuizGroupMembers.Remove(member);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task AssignQuiz(Guid groupId, Guid quizId, Guid evaluatorId)
        {
            var g = await _db.QuizGroups
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.GroupId == groupId)
                ?? throw new KeyNotFoundException("Group not found.");
            if (g.EvaluatorId != evaluatorId)
                throw new UnauthorizedAccessException("Only the group owner can assign quizzes.");

            var quiz = await _db.Quizzes.FindAsync(quizId)
                ?? throw new KeyNotFoundException("Quiz not found.");

            quiz.GroupId = groupId;
            await _db.SaveChangesAsync();

            // Notify all group members
            foreach (var member in g.Members)
            {
                _db.Notifications.Add(new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId         = member.UserId,
                    Type           = "quiz_assigned",
                    Title          = "New Test Assigned",
                    Message        = $"A new test \"{quiz.QuizName}\" has been assigned to you by your evaluator.",
                    LinkUrl        = $"/quiz/{quiz.QuizId}",
                    IsRead         = false,
                    CreatedAt      = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
        }

        public async Task UnassignQuiz(Guid quizId)
        {
            var quiz = await _db.Quizzes.FindAsync(quizId)
                ?? throw new KeyNotFoundException("Quiz not found.");
            quiz.GroupId = null;
            await _db.SaveChangesAsync();
        }

        public async Task<bool> CheckAccess(Guid quizId, Guid userId)
        {
            var quiz = await _db.Quizzes.AsNoTracking()
                .FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null) return false;
            if (quiz.GroupId == null) return true; // open quiz

            return await _db.QuizGroupMembers
                .AnyAsync(m => m.GroupId == quiz.GroupId && m.UserId == userId);
        }

        private static GroupDto MapDto(QuizGroup g) => new()
        {
            GroupId     = g.GroupId,
            GroupName   = g.GroupName,
            Description = g.Description,
            EvaluatorId = g.EvaluatorId,
            CreatedAt   = g.CreatedAt,
            MemberCount = g.Members?.Count ?? 0,
            Members     = g.Members?.Select(m => new GroupMemberDto
            {
                UserId   = m.UserId,
                Username = m.User?.Username ?? "",
                Email    = m.User?.Email ?? "",
                AddedAt  = m.AddedAt
            }).ToList() ?? new()
        };
    }
}
