//using QuizAppProject.DTOs.Auth;
using QuizAppProject.Models.DTOs;
using System.Threading.Tasks;

namespace QuizAppProject.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterRequestDto request);
        Task<AuthResponseDto> Login(LoginRequestDto request);



        // ✅ New
        Task<ForgotPasswordResponseDto> ForgotPassword(ForgotPasswordRequestDto request);
        Task<bool> ResetPassword(ResetPasswordRequestDto request);

    }
}
