using System.ComponentModel.DataAnnotations;

namespace QuizAppProject.Models.DTOs
{
    public class GroupCreateDto
    {
        [Required, MaxLength(100)] public string GroupName { get; set; } = string.Empty;
        [MaxLength(500)] public string? Description { get; set; }
    }

    public class GroupDto
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid EvaluatorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public List<GroupMemberDto> Members { get; set; } = new();
    }

    public class GroupMemberDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }

    public class AddMemberDto
    {
        [Required] public string UsernameOrEmail { get; set; } = string.Empty;
    }

    public class AssignGroupDto
    {
        public Guid? GroupId { get; set; }
    }
}
