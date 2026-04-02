namespace QuizAppProject.Models
{
    public class AttemptAnswerDetail
    {
        public Guid Id { get; set; }
        public Guid AttemptAnswerId { get; set; }
        public Guid QuestionId { get; set; }
        public string? ChosenOption { get; set; }
        public bool IsCorrect { get; set; }
        public int MarksAwarded { get; set; }
        public int MaxMarks { get; set; }

        public AttemptAnswer AttemptAnswer { get; set; } = null!;
        public Question Question { get; set; } = null!;
    }
}
