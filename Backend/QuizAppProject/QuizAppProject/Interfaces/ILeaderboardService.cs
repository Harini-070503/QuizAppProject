using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntryDto>> GetTopScores(Guid? categoryId = null, int count = 20);
    }
}
