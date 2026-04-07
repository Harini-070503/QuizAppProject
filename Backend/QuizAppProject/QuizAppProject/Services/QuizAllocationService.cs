using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Services
{
    public class QuizAllocationService : IQuizAllocationService
    {
        private readonly AppDbContext _db;

        public QuizAllocationService(AppDbContext db)
        {
            _db = db;
        }

        // ── Allocate ──────────────────────────────────────────────────────────
        public async Task<QuizAllocationDto> Allocate(QuizAllocationCreateDto dto)
        {
            try
            {
                var quiz = await _db.Quizzes.AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuizId == dto.QuizId)
                    ?? throw new KeyNotFoundException("Quiz not found.");

                var user = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == dto.UserId)
                    ?? throw new KeyNotFoundException("User not found.");

                if (!string.Equals(user.Role, "Taker", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        "Only normal Takers need quiz allocations. PremiumTakers have access to all quizzes.");

                var exists = await _db.QuizAllocations
                    .AnyAsync(a => a.QuizId == dto.QuizId && a.UserId == dto.UserId);
                if (exists)
                    throw new InvalidOperationException("This quiz is already allocated to the user.");

                var allocation = new QuizAllocation
                {
                    AllocationId = Guid.NewGuid(),
                    QuizId       = dto.QuizId,
                    UserId       = dto.UserId,
                    AllocatedAt  = DateTime.UtcNow
                };

                _db.QuizAllocations.Add(allocation);
                await _db.SaveChangesAsync();

                return new QuizAllocationDto
                {
                    AllocationId = allocation.AllocationId,
                    QuizId       = quiz.QuizId,
                    QuizName     = quiz.QuizName,
                    UserId       = user.UserId,
                    Username     = user.Username,
                    AllocatedAt  = allocation.AllocatedAt
                };
            }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while allocating the quiz.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while allocating quiz.", ex);
            }
        }

        // ── Deallocate ────────────────────────────────────────────────────────
        public async Task<bool> Deallocate(Guid allocationId)
        {
            try
            {
                var allocation = await _db.QuizAllocations.FindAsync(allocationId);
                if (allocation == null) return false;

                _db.QuizAllocations.Remove(allocation);
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while removing the allocation.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while deallocating quiz.", ex);
            }
        }

        // ── GetByUser ─────────────────────────────────────────────────────────
        public async Task<List<QuizAllocationDto>> GetByUser(Guid userId)
        {
            try
            {
                return await _db.QuizAllocations
                    .Include(a => a.Quiz)
                    .Include(a => a.User)
                    .Where(a => a.UserId == userId)
                    .AsNoTracking()
                    .Select(a => new QuizAllocationDto
                    {
                        AllocationId = a.AllocationId,
                        QuizId       = a.QuizId,
                        QuizName     = a.Quiz.QuizName,
                        UserId       = a.UserId,
                        Username     = a.User.Username,
                        AllocatedAt  = a.AllocatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching allocations for user.", ex);
            }
        }

        // ── GetByQuiz ─────────────────────────────────────────────────────────
        public async Task<List<QuizAllocationDto>> GetByQuiz(Guid quizId)
        {
            try
            {
                return await _db.QuizAllocations
                    .Include(a => a.Quiz)
                    .Include(a => a.User)
                    .Where(a => a.QuizId == quizId)
                    .AsNoTracking()
                    .Select(a => new QuizAllocationDto
                    {
                        AllocationId = a.AllocationId,
                        QuizId       = a.QuizId,
                        QuizName     = a.Quiz.QuizName,
                        UserId       = a.UserId,
                        Username     = a.User.Username,
                        AllocatedAt  = a.AllocatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching allocations for quiz.", ex);
            }
        }
    }
}
