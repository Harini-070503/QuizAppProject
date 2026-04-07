using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{
    public class QuizAllocationCreateDto
    {
        [Required] public Guid QuizId { get; set; }
        [Required] public Guid UserId { get; set; }
    }

    public class QuizAllocationDto
    {
        public Guid AllocationId { get; set; }
        public Guid QuizId { get; set; }
        public string QuizName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime AllocatedAt { get; set; }
    }
}
