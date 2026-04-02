namespace QuizAppProject.Models
{
    public class User
    {
            public Guid UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
            public string Salt { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;  // Creator / Taker
            public DateTime CreatedAt { get; set; }

            public UserDetails UserDetails { get; set; }   
            public ICollection<Quiz> Quizzes { get; set; }
    }

}
