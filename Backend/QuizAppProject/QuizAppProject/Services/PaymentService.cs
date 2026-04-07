using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuizAppProject.Context;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        private decimal RetryAmount    => decimal.TryParse(_config["Payment:RetryAmount"],   out var v) ? v : 49.00m;
        private decimal MonthlyAmount  => decimal.TryParse(_config["Payment:MonthlyAmount"], out var v) ? v : 299.00m;

        public PaymentService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ── Initiate Per-Retry ────────────────────────────────────────────────
        public async Task<PaymentResponseDto> Initiate(PaymentInitiateDto dto)
        {
            try
            {
                var user = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == dto.UserId)
                    ?? throw new KeyNotFoundException("User not found.");

                if (!string.Equals(user.Role, "PremiumTaker", StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Only PremiumTakers can make retry payments.");

                var quiz = await _db.Quizzes.AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuizId == dto.QuizId)
                    ?? throw new KeyNotFoundException("Quiz not found.");

                // Must have at least one attempt already
                var hasAttempt = await _db.AttemptAnswers
                    .AnyAsync(a => a.UserId == dto.UserId && a.QuizId == dto.QuizId);
                if (!hasAttempt)
                    throw new InvalidOperationException("No previous attempt found. First attempt is free.");

                // If user already has an active monthly subscription, no need for per-retry
                if (await HasActiveMonthlySubscription(dto.UserId))
                    throw new InvalidOperationException("You already have an active monthly subscription. No per-retry payment needed.");

                // Return existing pending per-retry payment if one exists
                var existing = await _db.QuizPayments
                    .FirstOrDefaultAsync(p => p.UserId == dto.UserId
                                           && p.QuizId == dto.QuizId
                                           && p.SubscriptionType == "PerRetry"
                                           && p.Status == "Pending"
                                           && !p.IsUsed);
                if (existing != null)
                    return MapDto(existing, quiz.QuizName);

                var payment = new QuizPayment
                {
                    PaymentId        = Guid.NewGuid(),
                    UserId           = dto.UserId,
                    QuizId           = dto.QuizId,
                    SubscriptionType = "PerRetry",
                    Amount           = RetryAmount,
                    Status           = "Pending",
                    IsUsed           = false,
                    CreatedAt        = DateTime.UtcNow
                };

                _db.QuizPayments.Add(payment);
                await _db.SaveChangesAsync();
                return MapDto(payment, quiz.QuizName);
            }
            catch (KeyNotFoundException) { throw; }
            catch (UnauthorizedAccessException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while initiating the payment.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while initiating payment.", ex);
            }
        }

        // ── Initiate Monthly Subscription ─────────────────────────────────────
        public async Task<PaymentResponseDto> InitiateMonthly(MonthlySubscriptionInitiateDto dto)
        {
            try
            {
                var user = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == dto.UserId)
                    ?? throw new KeyNotFoundException("User not found.");

                if (!string.Equals(user.Role, "PremiumTaker", StringComparison.OrdinalIgnoreCase))
                    throw new UnauthorizedAccessException("Only PremiumTakers can subscribe monthly.");

                // Don't create duplicate pending monthly payments
                var existing = await _db.QuizPayments
                    .FirstOrDefaultAsync(p => p.UserId == dto.UserId
                                           && p.SubscriptionType == "Monthly"
                                           && p.Status == "Pending");
                if (existing != null)
                    return MapDto(existing, null);

                var payment = new QuizPayment
                {
                    PaymentId        = Guid.NewGuid(),
                    UserId           = dto.UserId,
                    QuizId           = null,
                    SubscriptionType = "Monthly",
                    Amount           = MonthlyAmount,
                    Status           = "Pending",
                    IsUsed           = false,
                    CreatedAt        = DateTime.UtcNow
                };

                _db.QuizPayments.Add(payment);
                await _db.SaveChangesAsync();
                return MapDto(payment, null);
            }
            catch (KeyNotFoundException) { throw; }
            catch (UnauthorizedAccessException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while initiating monthly subscription.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while initiating monthly subscription.", ex);
            }
        }

        // ── Confirm ───────────────────────────────────────────────────────────
        public async Task<PaymentResponseDto> Confirm(PaymentConfirmDto dto)
        {
            try
            {
                var payment = await _db.QuizPayments
                    .Include(p => p.Quiz)
                    .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId)
                    ?? throw new KeyNotFoundException("Payment not found.");

                if (payment.Status == "Completed")
                    throw new InvalidOperationException("Payment is already confirmed.");

                if (payment.Status == "Failed")
                    throw new InvalidOperationException("Cannot confirm a failed payment.");

                payment.Status         = "Completed";
                payment.TransactionRef = dto.TransactionRef;
                payment.PaidAt         = DateTime.UtcNow;

                // Monthly subscription: set expiry to 30 days from now
                if (payment.SubscriptionType == "Monthly")
                    payment.ExpiresAt = DateTime.UtcNow.AddDays(30);

                await _db.SaveChangesAsync();
                return MapDto(payment, payment.Quiz?.QuizName);
            }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while confirming the payment.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while confirming payment.", ex);
            }
        }

        // ── GetByUser ─────────────────────────────────────────────────────────
        public async Task<List<PaymentResponseDto>> GetByUser(Guid userId)
        {
            try
            {
                var payments = await _db.QuizPayments
                    .Include(p => p.Quiz)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                return payments.Select(p => MapDto(p, p.Quiz?.QuizName)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching payment history.", ex);
            }
        }

        // ── HasActiveMonthlySubscription ──────────────────────────────────────
        public async Task<bool> HasActiveMonthlySubscription(Guid userId)
        {
            try
            {
                return await _db.QuizPayments
                    .AnyAsync(p => p.UserId == userId
                                && p.SubscriptionType == "Monthly"
                                && p.Status == "Completed"
                                && p.ExpiresAt > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while checking subscription.", ex);
            }
        }

        // ── Helper ────────────────────────────────────────────────────────────
        private static PaymentResponseDto MapDto(QuizPayment p, string? quizName)
        {
            bool isActive = p.SubscriptionType == "Monthly"
                         && p.Status == "Completed"
                         && p.ExpiresAt.HasValue
                         && p.ExpiresAt.Value > DateTime.UtcNow;

            return new PaymentResponseDto
            {
                PaymentId        = p.PaymentId,
                QuizId           = p.QuizId,
                QuizName         = quizName,
                SubscriptionType = p.SubscriptionType,
                Amount           = p.Amount,
                Status           = p.Status,
                IsUsed           = p.IsUsed,
                TransactionRef   = p.TransactionRef,
                CreatedAt        = p.CreatedAt,
                PaidAt           = p.PaidAt,
                ExpiresAt        = p.ExpiresAt,
                IsActive         = isActive
            };
        }
    }
}
