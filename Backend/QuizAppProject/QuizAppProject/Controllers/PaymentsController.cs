using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "PremiumTaker")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _svc;

        public PaymentsController(IPaymentService svc) => _svc = svc;

        // POST api/payments/initiate  — per-retry for a specific quiz
        [HttpPost("initiate")]
        public async Task<ActionResult<PaymentResponseDto>> Initiate([FromBody] PaymentInitiateDto dto)
        {
            try
            {
                return Ok(await _svc.Initiate(dto));
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException)  { return Forbid(); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // POST api/payments/initiate-monthly  — monthly subscription
        [HttpPost("initiate-monthly")]
        public async Task<ActionResult<PaymentResponseDto>> InitiateMonthly([FromBody] MonthlySubscriptionInitiateDto dto)
        {
            try
            {
                return Ok(await _svc.InitiateMonthly(dto));
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedAccessException)  { return Forbid(); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // POST api/payments/confirm
        [HttpPost("confirm")]
        public async Task<ActionResult<PaymentResponseDto>> Confirm([FromBody] PaymentConfirmDto dto)
        {
            try
            {
                return Ok(await _svc.Confirm(dto));
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // GET api/payments/mine?userId={userId}
        [HttpGet("mine")]
        public async Task<ActionResult<List<PaymentResponseDto>>> Mine([FromQuery] Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest("UserId is required.");
            return Ok(await _svc.GetByUser(userId));
        }

        // GET api/payments/subscription-status?userId={userId}
        [HttpGet("subscription-status")]
        public async Task<ActionResult<object>> SubscriptionStatus([FromQuery] Guid userId)
        {
            if (userId == Guid.Empty) return BadRequest("UserId is required.");
            var active = await _svc.HasActiveMonthlySubscription(userId);
            return Ok(new { isActive = active });
        }
    }
}
