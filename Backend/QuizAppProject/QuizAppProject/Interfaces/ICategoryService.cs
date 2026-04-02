//using QuizAppProject.DTOs;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDto> Add(CategoryCreateDto request);
        Task<CategoryDto?> Get(Guid id);
        Task<List<CategoryDto>> GetAll();
        Task<CategoryDto> Update(Guid id, CategoryCreateDto request);
        Task<bool> Delete(Guid id);
    }
}
