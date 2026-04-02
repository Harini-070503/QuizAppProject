namespace QuizAppProject.Models
{
    public class UserDetails
    {
            public Guid UserDetailsId { get; set; }
            public Guid UserId { get; set; }

            public string PhoneNumber { get; set; } = string.Empty;
            public string AddressLine1 { get; set; } = string.Empty;
            public string AddressLine2 { get; set; } = string.Empty;
            public string State { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Pincode { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;

            public DateTime CreatedAt { get; set; }

            public User User { get; set; }

    }
}
