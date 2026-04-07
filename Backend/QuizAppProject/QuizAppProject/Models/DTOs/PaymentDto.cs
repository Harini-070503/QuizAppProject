using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{
    /// <summary>Initiate a per-retry payment for a specific quiz.</summary>
    public class PaymentInitiateDto
    {
        [Required] public Guid UserId { get; set; }
        [Required] public Guid QuizId { get; set; }
    }

    /// <summary>Initiate a monthly subscription payment (covers all quizzes for 30 days).</summary>
    public class MonthlySubscriptionInitiateDto
    {
        [Required] public Guid UserId { get; set; }
    }

    public class PaymentConfirmDto
    {
        [Required] public Guid PaymentId { get; set; }
        [Required] public string TransactionRef { get; set; } = string.Empty;
    }

    public class PaymentResponseDto
    {
        public Guid PaymentId { get; set; }
        public Guid? QuizId { get; set; }
        public string? QuizName { get; set; }
        public string SubscriptionType { get; set; } = string.Empty; // PerRetry | Monthly
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public string? TransactionRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } // true if Monthly and not expired
    }
}
