using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{
    public class CategoryDto
    {

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CategoryCreateDto
    {
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        //public Guid CategoryId { get; set; }
    }
}
