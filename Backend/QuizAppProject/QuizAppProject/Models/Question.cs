namespace QuizAppProject.Models
{
    public class Question
    {
        public Guid QuestionId { get; set; }
        public Guid QuizId { get; set; }

        public string QuestionText { get; set; } = string.Empty;
        public int Marks { get; set; } = 1;
        public DateTime CreatedAt { get; set; }

        public Quiz Quiz { get; set; }
       public Option Options { get; set; }
    }
}
