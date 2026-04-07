using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IQuizAllocationService
    {
        Task<QuizAllocationDto> Allocate(QuizAllocationCreateDto dto);
        Task<bool> Deallocate(Guid allocationId);
        Task<List<QuizAllocationDto>> GetByUser(Guid userId);
        Task<List<QuizAllocationDto>> GetByQuiz(Guid quizId);
    }
}
