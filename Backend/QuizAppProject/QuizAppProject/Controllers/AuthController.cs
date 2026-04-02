using Microsoft.AspNetCore.Mvc;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto dto)
        {
            try
            {
                var res = await _auth.Register(dto);
                return Ok(res);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "Unexpected error during registration." }); }
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var res = await _auth.Login(dto);
                return Ok(res);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "Unexpected error during login." }); }
        }

        // ✅ POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            try
            {
                var res = await _auth.ForgotPassword(dto);
                // In production you would NOT return the token; you'd email it.
                return Ok(res);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "Unexpected error during forgot-password." }); }
        }

        // ✅ POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            try
            {
                var ok = await _auth.ResetPassword(dto);
                if (!ok) return BadRequest(new { message = "Could not reset the password." });
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception) { return StatusCode(500, new { message = "Unexpected error during reset-password." }); }
        }
    }
}
