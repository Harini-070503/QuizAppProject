using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IOptionService
    {
        Task<OptionDto> Add(Guid questionId, OptionCreateDto request);
        Task<OptionDto?> Get(Guid optionId);
        Task<OptionDto> Update(Guid optionId, OptionCreateDto request);
        Task<bool> Delete(Guid optionId);
    }
}
