//using QuizAppProject.DTOs.Attempts;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IAttemptService
    {
        Task<AttemptResultDto> SubmitAttempt(Guid userId, AttemptSubmitDto request);
        Task<List<AttemptResultDto>> GetAttemptsByUser(Guid userId);
        Task<AttemptResultDto?> GetAttempt(Guid attemptId);
        Task<List<SubmissionDetailDto>> GetSubmissionsByQuiz(Guid quizId);
        Task<SubmissionDetailDto?> GetSubmissionDetail(Guid attemptId);
        Task<bool> UpdateScore(Guid attemptId, UpdateScoreDto dto);
    }
}
