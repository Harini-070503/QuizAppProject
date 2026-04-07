using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Interfaces
{
    public interface IUserService
    {

        Task<AuthResponseDto> Register(RegisterRequestDto request);
        Task<AuthResponseDto> Login(LoginRequestDto request);
        Task<UserDto?> GetByUsername(string username);
        Task<UserDto?> GetById(Guid id);
        Task<UserDto> UpdateUser(Guid id, UpdateUserDto request);
        Task<AuthResponseDto> UpgradeToPremium(Guid userId);
    }
}
