namespace QuizAppProject.Models
{
    public class QuizGroup
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid EvaluatorId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Evaluator { get; set; } = null!;
        public ICollection<QuizGroupMember> Members { get; set; } = new List<QuizGroupMember>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }

    public class QuizGroupMember
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AddedAt { get; set; }

        public QuizGroup Group { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
