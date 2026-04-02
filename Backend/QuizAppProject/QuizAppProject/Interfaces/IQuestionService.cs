using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IQuestionService
    {
        Task<QuestionDto> Add(Guid quizId, QuestionCreateDto request);
        Task<QuestionDto?> Get(Guid questionId);
        Task<List<QuestionDto>> GetByQuiz(Guid quizId);
        Task<QuestionDto> Update(Guid questionId, QuestionCreateDto request);
        Task<bool> Delete(Guid questionId);
    }
}
