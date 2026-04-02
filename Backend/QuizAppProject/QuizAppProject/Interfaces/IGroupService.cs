using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IGroupService
    {
        Task<List<GroupDto>> GetMyGroups(Guid evaluatorId);
        Task<GroupDto?> GetById(Guid groupId);
        Task<GroupDto> Create(Guid evaluatorId, GroupCreateDto dto);
        Task<bool> Delete(Guid groupId, Guid evaluatorId);
        Task<GroupMemberDto> AddMember(Guid groupId, Guid evaluatorId, string usernameOrEmail);
        Task<bool> RemoveMember(Guid groupId, Guid evaluatorId, Guid userId);
        Task AssignQuiz(Guid groupId, Guid quizId, Guid evaluatorId);
        Task UnassignQuiz(Guid quizId);
        Task<bool> CheckAccess(Guid quizId, Guid userId);
    }
}
