using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IQuizService
    {
        Task<QuizDto> Add(Guid creatorId, QuizCreateDto request);
        Task<QuizDto?> Get(Guid quizId);
        Task<List<QuizDto>> GetAll(Guid? categoryId = null, Guid? requestingUserId = null);
        Task<QuizDto> Update(Guid quizId, QuizUpdateDto request, Guid creatorId);
        Task<bool> Delete(Guid quizId, Guid creatorId);
    }
}
