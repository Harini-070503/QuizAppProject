namespace QuizAppProject.Models.DTOs
{

    public class LeaderboardEntryDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string? Name { get; set; }

        public Guid QuizId { get; set; }
        public string QuizName { get; set; }

        public double Percentage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}