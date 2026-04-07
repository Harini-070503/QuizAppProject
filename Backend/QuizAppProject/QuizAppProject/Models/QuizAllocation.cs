namespace QuizAppProject.Models
{
    /// <summary>
    /// Allocates a specific quiz to a normal Taker (non-premium).
    /// PremiumTakers don't need allocations — they can access all quizzes.
    /// </summary>
    public class QuizAllocation
    {
        public Guid AllocationId { get; set; }
        public Guid QuizId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AllocatedAt { get; set; }

        public Quiz Quiz { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
