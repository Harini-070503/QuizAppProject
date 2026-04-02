namespace QuizAppProject.Models
{
    public class AttemptAnswer
    {
        public Guid AttemptAnswerId { get; set; }
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }

        public int TotalMark { get; set; }
        public double Percentage { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
        public Quiz Quiz { get; set; }
        public ICollection<AttemptAnswerDetail> Details { get; set; } = new List<AttemptAnswerDetail>();
    }
}
