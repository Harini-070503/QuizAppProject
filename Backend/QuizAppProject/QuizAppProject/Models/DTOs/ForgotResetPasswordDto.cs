using System;
using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{
    public class ForgotPasswordRequestDto
    {
        /// <summary>Username or Email</summary>
        [Required, MaxLength(254)]
        public string UsernameOrEmail { get; set; } = default!;
    }

    public class ForgotPasswordResponseDto
    {
        /// <summary>Short-lived token for resetting password (normally emailed).</summary>
        public string ResetToken { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = default!;

        /// <summary>Token returned by /auth/forgot-password</summary>
        [Required]
        public string ResetToken { get; set; } = default!;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = default!;
    }
}
