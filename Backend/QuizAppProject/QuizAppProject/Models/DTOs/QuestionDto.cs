using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{

    public class OptionCreateDto
    {
        [Required] public string OptionA { get; set; }
        [Required] public string OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        /// <summary>Single or comma-separated correct options e.g. "A" or "A,C"</summary>
        [Required]
        public string CorrectOption { get; set; }
    }

    public class OptionDto : OptionCreateDto
    {
        public Guid OptionId { get; set; }
    }

    public class QuestionCreateDto
    {
        [Required]
        public string QuestionText { get; set; }

        [Range(1, 100)]
        public int Marks { get; set; } = 1;

        [Required]
        public OptionCreateDto Options { get; set; }
    }

    public class QuestionDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int Marks { get; set; }
        public OptionDto Options { get; set; }
    }

}
