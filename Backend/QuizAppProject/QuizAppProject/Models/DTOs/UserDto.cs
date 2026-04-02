using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{
    public class UserDto
    {

        public Guid UserId { get; set; }
        public string Username { get; set; }=string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Creator | Taker
        public string? Name { get; set; }

        // Optional profile fields
        public string? PhoneNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }

    }

    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(200)]
        public string? AddressLine1 { get; set; }

        [MaxLength(200)]

        public string? AddressLine2 { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? Pincode { get; set; }
    }
}
