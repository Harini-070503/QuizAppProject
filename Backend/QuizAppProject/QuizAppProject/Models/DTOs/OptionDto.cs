using System;
using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.DTOs.Options
{
    public class OptionCreateDto
    {
        [Required] public string OptionA { get; set; }
        [Required] public string OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        [Required]
        public string CorrectOption { get; set; }
    }

    public class OptionDto : OptionCreateDto
    {
        public Guid OptionId { get; set; }
    }
}