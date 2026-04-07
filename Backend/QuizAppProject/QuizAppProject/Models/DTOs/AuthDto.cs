using System.ComponentModel.DataAnnotations;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Models.DTOs
{
    public class AuthDto
    {
        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        /// <summary>Allowed values: "Creator" | "Taker" | "PremiumTaker" | "Evaluator"</summary>
        [Required, MaxLength(20)]
        public string Role { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }
    }

    public class RegisterRequestDto
    {
        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, MaxLength(254)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        /// <summary>Allowed values: "Creator" | "Taker" | "PremiumTaker" | "Evaluator"</summary>
        [Required, MaxLength(20)]
        public string Role { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }
    }

    public class LoginRequestDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
    }
}