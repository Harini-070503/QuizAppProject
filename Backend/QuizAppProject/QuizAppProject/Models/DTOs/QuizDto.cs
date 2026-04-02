using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{

    public class QuizCreateDto
    {
        public Guid userId { get; set; }
        [Required, MaxLength(150)]
        public string QuizName { get; set; }=string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Range(0, int.MaxValue)]
        public int PassMark { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalQuestion { get; set; }

        [MaxLength(30)]
        public string? DifficultyLevel { get; set; } // Easy/Medium/Hard

        public int? TimeLimit { get; set; } // minutes, optional
        public DateTime? Deadline { get; set; }

        public List<QuestionCreateDto>? Questions { get; set; }
    }

    public class QuizUpdateDto
    {
        [Required, MaxLength(150)]
        public string QuizName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Range(0, int.MaxValue)]
        public int PassMark { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalQuestion { get; set; }

        [MaxLength(30)]
        public string? DifficultyLevel { get; set; }

        public int? TimeLimit { get; set; }
        public DateTime? Deadline { get; set; }

        public List<QuestionCreateDto>? Questions { get; set; }
    }
    public class QuizDto
    {
        public Guid QuizId { get; set; }
        public string QuizName { get; set; }
        public string? Description { get; set; }
        public string? DifficultyLevel { get; set; }
        public int? TimeLimit { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsExpired => Deadline.HasValue && Deadline.Value < DateTime.UtcNow;
        public int PassMark { get; set; }
        public int TotalQuestion { get; set; }

        public CategoryDto Category { get; set; }

        public Guid CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string? CreatorRole { get; set; }
        public Guid? GroupId { get; set; }

        public List<QuestionDto> Questions { get; set; }
    }
}
