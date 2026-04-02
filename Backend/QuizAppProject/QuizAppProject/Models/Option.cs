namespace QuizAppProject.Models
{
    public class Option
    {
        public Guid OptionId { get; set; }
        public Guid QuestionId { get; set; }

        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }

        // Single or comma-separated e.g. "A" or "A,C"
        public string CorrectOption { get; set; }

        public DateTime CreatedAt { get; set; }

        public Question Question { get; set; }
    }
}
