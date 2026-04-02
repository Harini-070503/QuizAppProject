using System.Security.Claims;

namespace QuizAppProject.Interfaces
{
    public interface ITokenService
    {
        public string GenerateToken(Guid userId, string userName, string email, string role);


        // ✅ New
        string GeneratePasswordResetToken(Guid userId, string userName, TimeSpan? lifetime = null);

        /// <summary>Validate a password-reset token. Throws if invalid/expired.</summary>
        ClaimsPrincipal ValidatePasswordResetToken(string token, bool validateLifetime = true);

    }
}
