namespace QuizAppProject.Models
{
    /// <summary>
    /// Records a payment made by a PremiumTaker.
    /// SubscriptionType = "Monthly"  → unlocks unlimited retries for 30 days (QuizId is null).
    /// SubscriptionType = "PerRetry" → unlocks one extra attempt on a specific quiz.
    /// </summary>
    public class QuizPayment
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }

        /// <summary>Null for Monthly subscriptions; set for PerRetry payments.</summary>
        public Guid? QuizId { get; set; }

        public string SubscriptionType { get; set; } = "PerRetry"; // PerRetry | Monthly

        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending | Completed | Failed
        public string? TransactionRef { get; set; }

        /// <summary>For PerRetry: true once the attempt slot is consumed. Always false for Monthly.</summary>
        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        /// <summary>For Monthly: subscription is valid until this date (PaidAt + 30 days).</summary>
        public DateTime? ExpiresAt { get; set; }

        public User User { get; set; } = null!;
        public Quiz? Quiz { get; set; }
    }
}
