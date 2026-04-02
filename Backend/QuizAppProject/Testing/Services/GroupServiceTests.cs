using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class GroupServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly GroupService _svc;

        private static readonly Guid EvalId1   = Guid.Parse("a1000000-0000-0000-0000-000000000001");
        private static readonly Guid EvalId2   = Guid.Parse("a1000000-0000-0000-0000-000000000002");
        private static readonly Guid TakerId1  = Guid.Parse("b1000000-0000-0000-0000-000000000001");
        private static readonly Guid TakerId2  = Guid.Parse("b1000000-0000-0000-0000-000000000002");
        private static readonly Guid GroupId1  = Guid.Parse("c1000000-0000-0000-0000-000000000001");
        private static readonly Guid GroupId2  = Guid.Parse("c1000000-0000-0000-0000-000000000002");
        private static readonly Guid QuizId1   = Guid.Parse("d1000000-0000-0000-0000-000000000001");
        private static readonly Guid QuizId2   = Guid.Parse("d1000000-0000-0000-0000-000000000002");
        private static readonly Guid CatId     = Guid.Parse("e1000000-0000-0000-0000-000000000001");

        public GroupServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();
            _svc = new GroupService(_ctx);
        }

        private void SeedData()
        {
            _ctx.Users.AddRange(
                new User { UserId = EvalId1,  Username = "eval1",   Email = "eval1@t.com",  PasswordHash = "x", Salt = "s", Role = "Evaluator", CreatedAt = DateTime.UtcNow },
                new User { UserId = EvalId2,  Username = "eval2",   Email = "eval2@t.com",  PasswordHash = "x", Salt = "s", Role = "Evaluator", CreatedAt = DateTime.UtcNow },
                new User { UserId = TakerId1, Username = "taker1",  Email = "taker1@t.com", PasswordHash = "x", Salt = "s", Role = "Taker",     CreatedAt = DateTime.UtcNow },
                new User { UserId = TakerId2, Username = "taker2",  Email = "taker2@t.com", PasswordHash = "x", Salt = "s", Role = "Taker",     CreatedAt = DateTime.UtcNow }
            );

            _ctx.Categories.Add(new Category { CategoryId = CatId, CategoryName = "Test", CreatedAt = DateTime.UtcNow });

            _ctx.Quizzes.AddRange(
                new Quiz { QuizId = QuizId1, UserId = EvalId1, CategoryId = CatId, QuizName = "Quiz A", Description = "d", PassMark = 5, TotalQuestion = 1, DifficultyLevel = "Easy", CreatedAt = DateTime.UtcNow },
                new Quiz { QuizId = QuizId2, UserId = EvalId1, CategoryId = CatId, QuizName = "Quiz B", Description = "d", PassMark = 5, TotalQuestion = 1, DifficultyLevel = "Easy", CreatedAt = DateTime.UtcNow, GroupId = GroupId1 }
            );

            _ctx.QuizGroups.AddRange(
                new QuizGroup { GroupId = GroupId1, GroupName = "Group Alpha", Description = "Alpha desc", EvaluatorId = EvalId1, CreatedAt = DateTime.UtcNow },
                new QuizGroup { GroupId = GroupId2, GroupName = "Group Beta",  Description = "Beta desc",  EvaluatorId = EvalId2, CreatedAt = DateTime.UtcNow }
            );

            _ctx.QuizGroupMembers.Add(new QuizGroupMember
            {
                Id = Guid.NewGuid(), GroupId = GroupId1, UserId = TakerId1, AddedAt = DateTime.UtcNow
            });

            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── GetMyGroups ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetMyGroups_ReturnsOnlyEvaluatorsGroups()
        {
            var result = await _svc.GetMyGroups(EvalId1);

            Assert.Single(result);
            Assert.Equal("Group Alpha", result[0].GroupName);
        }

        [Fact]
        public async Task GetMyGroups_IncludesMemberCount()
        {
            var result = await _svc.GetMyGroups(EvalId1);

            Assert.Equal(1, result[0].MemberCount);
        }

        [Fact]
        public async Task GetMyGroups_OtherEvaluator_ReturnsTheirGroups()
        {
            var result = await _svc.GetMyGroups(EvalId2);

            Assert.Single(result);
            Assert.Equal("Group Beta", result[0].GroupName);
        }

        [Fact]
        public async Task GetMyGroups_UnknownEvaluator_ReturnsEmpty()
        {
            var result = await _svc.GetMyGroups(Guid.NewGuid());
            Assert.Empty(result);
        }

        // ── GetById ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_ExistingGroup_ReturnsDto()
        {
            var result = await _svc.GetById(GroupId1);

            Assert.NotNull(result);
            Assert.Equal("Group Alpha", result!.GroupName);
            Assert.Equal(EvalId1,       result.EvaluatorId);
        }

        [Fact]
        public async Task GetById_IncludesMembers()
        {
            var result = await _svc.GetById(GroupId1);

            Assert.Single(result!.Members);
            Assert.Equal("taker1", result.Members[0].Username);
        }

        [Fact]
        public async Task GetById_UnknownGroup_ReturnsNull()
        {
            var result = await _svc.GetById(Guid.NewGuid());
            Assert.Null(result);
        }

        // ── Create ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ValidDto_CreatesGroup()
        {
            var dto = new GroupCreateDto { GroupName = "New Group", Description = "New desc" };

            var result = await _svc.Create(EvalId1, dto);

            Assert.NotNull(result);
            Assert.Equal("New Group",  result.GroupName);
            Assert.Equal("New desc",   result.Description);
            Assert.Equal(EvalId1,      result.EvaluatorId);
            Assert.Equal(0,            result.MemberCount);
        }

        [Fact]
        public async Task Create_AppearsInGetMyGroups()
        {
            await _svc.Create(EvalId1, new GroupCreateDto { GroupName = "Extra Group" });

            var groups = await _svc.GetMyGroups(EvalId1);
            Assert.Equal(2, groups.Count);
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ValidOwner_DeletesGroup()
        {
            var result = await _svc.Delete(GroupId1, EvalId1);

            Assert.True(result);
            Assert.Null(await _svc.GetById(GroupId1));
        }

        [Fact]
        public async Task Delete_WrongOwner_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.Delete(GroupId1, EvalId2));
        }

        [Fact]
        public async Task Delete_UnknownGroup_ReturnsFalse()
        {
            var result = await _svc.Delete(Guid.NewGuid(), EvalId1);
            Assert.False(result);
        }

        // ── AddMember ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddMember_ByUsername_AddsMember()
        {
            var result = await _svc.AddMember(GroupId1, EvalId1, "taker2");

            Assert.NotNull(result);
            Assert.Equal(TakerId2, result.UserId);
            Assert.Equal("taker2", result.Username);
        }

        [Fact]
        public async Task AddMember_ByEmail_AddsMember()
        {
            var result = await _svc.AddMember(GroupId1, EvalId1, "taker2@t.com");

            Assert.Equal(TakerId2, result.UserId);
        }

        [Fact]
        public async Task AddMember_DuplicateMember_ThrowsInvalidOperation()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.AddMember(GroupId1, EvalId1, "taker1")); // already a member
        }

        [Fact]
        public async Task AddMember_UnknownUser_ThrowsKeyNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.AddMember(GroupId1, EvalId1, "ghost@t.com"));
        }

        [Fact]
        public async Task AddMember_WrongOwner_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.AddMember(GroupId1, EvalId2, "taker2"));
        }

        [Fact]
        public async Task AddMember_UnknownGroup_ThrowsKeyNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.AddMember(Guid.NewGuid(), EvalId1, "taker2"));
        }

        // ── RemoveMember ──────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveMember_ExistingMember_ReturnsTrue()
        {
            var result = await _svc.RemoveMember(GroupId1, EvalId1, TakerId1);

            Assert.True(result);
            var group = await _svc.GetById(GroupId1);
            Assert.Empty(group!.Members);
        }

        [Fact]
        public async Task RemoveMember_NonMember_ReturnsFalse()
        {
            var result = await _svc.RemoveMember(GroupId1, EvalId1, TakerId2);
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveMember_WrongOwner_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.RemoveMember(GroupId1, EvalId2, TakerId1));
        }

        // ── AssignQuiz ────────────────────────────────────────────────────────

        [Fact]
        public async Task AssignQuiz_ValidOwner_SetsGroupId()
        {
            await _svc.AssignQuiz(GroupId1, QuizId1, EvalId1);

            var quiz = await _ctx.Quizzes.FindAsync(QuizId1);
            Assert.Equal(GroupId1, quiz!.GroupId);
        }

        [Fact]
        public async Task AssignQuiz_SendsNotificationToMembers()
        {
            await _svc.AssignQuiz(GroupId1, QuizId1, EvalId1);

            var notif = await _ctx.Notifications
                .FirstOrDefaultAsync(n => n.UserId == TakerId1 && n.Type == "quiz_assigned");
            Assert.NotNull(notif);
            Assert.Contains("Quiz A", notif!.Message);
        }

        [Fact]
        public async Task AssignQuiz_WrongOwner_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.AssignQuiz(GroupId1, QuizId1, EvalId2));
        }

        [Fact]
        public async Task AssignQuiz_UnknownQuiz_ThrowsKeyNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.AssignQuiz(GroupId1, Guid.NewGuid(), EvalId1));
        }

        // ── UnassignQuiz ──────────────────────────────────────────────────────

        [Fact]
        public async Task UnassignQuiz_ClearsGroupId()
        {
            await _svc.UnassignQuiz(QuizId2);

            var quiz = await _ctx.Quizzes.FindAsync(QuizId2);
            Assert.Null(quiz!.GroupId);
        }

        [Fact]
        public async Task UnassignQuiz_UnknownQuiz_ThrowsKeyNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.UnassignQuiz(Guid.NewGuid()));
        }

        // ── CheckAccess ───────────────────────────────────────────────────────

        [Fact]
        public async Task CheckAccess_OpenQuiz_ReturnsTrue()
        {
            // QuizId1 has no group
            var result = await _svc.CheckAccess(QuizId1, TakerId2);
            Assert.True(result);
        }

        [Fact]
        public async Task CheckAccess_GroupMember_ReturnsTrue()
        {
            // QuizId2 is in GroupId1; TakerId1 is a member
            var result = await _svc.CheckAccess(QuizId2, TakerId1);
            Assert.True(result);
        }

        [Fact]
        public async Task CheckAccess_NonMember_ReturnsFalse()
        {
            // QuizId2 is in GroupId1; TakerId2 is NOT a member
            var result = await _svc.CheckAccess(QuizId2, TakerId2);
            Assert.False(result);
        }

        [Fact]
        public async Task CheckAccess_UnknownQuiz_ReturnsFalse()
        {
            var result = await _svc.CheckAccess(Guid.NewGuid(), TakerId1);
            Assert.False(result);
        }
    }
}

