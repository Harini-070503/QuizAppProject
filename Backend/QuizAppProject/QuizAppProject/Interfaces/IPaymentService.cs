using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IPaymentService
    {
        /// <summary>Initiate a per-retry payment for a specific quiz.</summary>
        Task<PaymentResponseDto> Initiate(PaymentInitiateDto dto);

        /// <summary>Initiate a monthly subscription — unlocks unlimited retries for 30 days.</summary>
        Task<PaymentResponseDto> InitiateMonthly(MonthlySubscriptionInitiateDto dto);

        /// <summary>Confirm payment after gateway callback.</summary>
        Task<PaymentResponseDto> Confirm(PaymentConfirmDto dto);

        /// <summary>Get all payments for a user.</summary>
        Task<List<PaymentResponseDto>> GetByUser(Guid userId);

        /// <summary>Check if user has an active monthly subscription.</summary>
        Task<bool> HasActiveMonthlySubscription(Guid userId);
    }
}
