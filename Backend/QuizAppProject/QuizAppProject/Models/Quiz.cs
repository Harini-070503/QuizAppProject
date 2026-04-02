namespace QuizAppProject.Models
{
    public class Quiz
    {
        public Guid QuizId { get; set; }
        public Guid UserId { get; set; }
        public Guid? CategoryId { get; set; }

        public string QuizName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PassMark { get; set; }
        public int TotalQuestion { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public int? TimeLimit { get; set; }
        public DateTime? Deadline { get; set; }
        public Guid? GroupId { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
        public Category Category { get; set; }
        public QuizGroup? Group { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
