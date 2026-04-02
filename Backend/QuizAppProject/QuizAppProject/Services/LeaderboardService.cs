using Microsoft.EntityFrameworkCore;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizAppProject.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IRepository<Guid, AttemptAnswer> _attemptRepo;

        public LeaderboardService(IRepository<Guid, AttemptAnswer> attemptRepo)
        {
            _attemptRepo = attemptRepo;
        }

        public async Task<List<LeaderboardEntryDto>> GetTopScores(Guid? categoryId = null, int count = 20)
        {
            try
            {
                var query = _attemptRepo.Query()
                    .Include(a => a.User).ThenInclude(u => u.UserDetails)
                    .Include(a => a.Quiz).ThenInclude(qz => qz.Category)
                    .AsNoTracking();

                if (categoryId.HasValue)
                {
                    query = query.Where(a => a.Quiz.CategoryId == categoryId.Value);
                }

                var items = await query
                    .OrderByDescending(a => a.Percentage)
                    .ThenBy(a => a.CreatedAt)
                    .Take(count)
                    .ToListAsync();

                return items.Select(a => new LeaderboardEntryDto
                {
                    UserId = a.UserId,
                    Username = a.User?.Username,
                    Name = a.User?.UserDetails?.Name,
                    QuizId = a.QuizId,
                    QuizName = a.Quiz?.QuizName,
                    Percentage = a.Percentage,
                    CreatedAt = a.CreatedAt
                }).ToList();
            }
            catch (DbUpdateException ex)
            {
                // Unlikely on reads, but included for completeness if the provider throws update-related exceptions
                throw new InvalidOperationException("A database error occurred while retrieving the leaderboard.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while retrieving the leaderboard.", ex);
            }
        }
    }
}