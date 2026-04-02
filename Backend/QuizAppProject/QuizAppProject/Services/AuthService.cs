using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuizAppProject.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<Guid, User> _userRepo;
        private readonly IConfiguration _config;
        private readonly ITokenService _tokenService;

        public AuthService(IRepository<Guid, User> userRepo, IConfiguration config, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _config = config;
            _tokenService = tokenService;
        }

        // -----------------------------
        // Register
        // -----------------------------
        public async Task<AuthResponseDto> Register(RegisterRequestDto request)
        {
            try
            {
                var exists = await _userRepo.Query()
                    .AnyAsync(u => u.Username == request.Username || u.Email == request.Email);

                if (exists)
                    throw new InvalidOperationException("Username or Email already exists.");

                var salt = GenerateSalt(16);
                var hash = HashPassword(request.Password, salt); // base64 string

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = request.Username,
                    Email = request.Email,
                    Role = request.Role,                // "Creator" or "Taker"
                    PasswordHash = hash,                // base64 string
                    Salt = Convert.ToBase64String(salt),// base64 string
                    CreatedAt = DateTime.UtcNow
                };

                var added = await _userRepo.Add(user);
                if (added is null)
                    throw new InvalidOperationException("Failed to create user.");

                return new AuthResponseDto
                {
                    Token = GenerateJwt(user)
                    // If you later include UserDto in the response, you can add it here.
                };
            }
            catch (InvalidOperationException) { throw; } // business rule or failed creation
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while registering the user.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error during user registration.", ex);
            }
        }

        // -----------------------------
        // Login
        // -----------------------------
        public async Task<AuthResponseDto> Login(LoginRequestDto request)
        {
            try
            {
                var user = await _userRepo.Query()
                    .Include(u => u.UserDetails)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                    throw new UnauthorizedAccessException("Invalid credentials.");

                // Recompute hash with stored salt and compare
                var saltBytes = Convert.FromBase64String(user.Salt);
                var computedHashBase64 = HashPassword(request.Password, saltBytes);

                var valid = CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(user.PasswordHash),
                    Convert.FromBase64String(computedHashBase64)
                );

                if (!valid)
                    throw new UnauthorizedAccessException("Invalid credentials.");

                return new AuthResponseDto
                {
                    Token = GenerateJwt(user)
                };
            }
            catch (UnauthorizedAccessException) { throw; } // preserve 401 semantics
            catch (DbUpdateException ex)
            {
                // Rare on reads; included for completeness if provider throws
                throw new InvalidOperationException("A database error occurred while logging in.", ex);
            }
            catch (FormatException ex)
            {
                // Covers invalid base64 in stored hash/salt
                throw new InvalidOperationException("Stored credentials are in an invalid format.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error during login.", ex);
            }
        }

        // -----------------------------
        // Forgot Password (generate short-lived token)
        // -----------------------------
        public async Task<ForgotPasswordResponseDto> ForgotPassword(ForgotPasswordRequestDto request)
        {
            try
            {
                var usernameOrEmail = request.UsernameOrEmail.Trim();

                var user = await _userRepo.Query()
                    .FirstOrDefaultAsync(u =>
                        u.Username == usernameOrEmail || u.Email == usernameOrEmail);

                if (user == null)
                    throw new KeyNotFoundException("User not found.");

                // Generate short-lived reset token (default 15 minutes, configurable)
                var token = _tokenService.GeneratePasswordResetToken(user.UserId, user.Username);

                var configured = _config["Jwt:PasswordResetMinutes"];
                var minutes = double.TryParse(configured, out var m) ? m : 15;

                return new ForgotPasswordResponseDto
                {
                    ResetToken = token,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(minutes)
                };
            }
            catch (KeyNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error during forgot-password.", ex);
            }
        }

        // -----------------------------
        // Reset Password (validate token, re-hash, update)
        // -----------------------------
        public async Task<bool> ResetPassword(ResetPasswordRequestDto request)
        {
            try
            {
                // Validate token and ensure purpose
                var principal = _tokenService.ValidatePasswordResetToken(request.ResetToken, validateLifetime: true);

                // Extract identity from token
                var tokenUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenUserName = principal.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(tokenUserId) || string.IsNullOrWhiteSpace(tokenUserName))
                    throw new UnauthorizedAccessException("Invalid reset token.");

                // Fetch the target user by username
                var user = await _userRepo.Query()
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                    throw new KeyNotFoundException("User not found.");

                // Token must belong to this user
                if (!string.Equals(user.UserId.ToString(), tokenUserId, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(user.Username, tokenUserName, StringComparison.Ordinal))
                    throw new UnauthorizedAccessException("Reset token does not match the user.");

                // Re-hash with a new salt
                var newSalt = GenerateSalt(16);
                var newHash = HashPassword(request.NewPassword, newSalt);

                user.Salt = Convert.ToBase64String(newSalt);
                user.PasswordHash = newHash;

                var updated = await _userRepo.Update(user.UserId, user);
                if (updated is null)
                    throw new InvalidOperationException("Failed to update password.");

                return true;
            }
            catch (UnauthorizedAccessException) { throw; }
            catch (KeyNotFoundException) { throw; }
            catch (InvalidOperationException) { throw; }
            catch (SecurityTokenExpiredException)
            {
                throw new UnauthorizedAccessException("Reset token has expired.");
            }
            catch (SecurityTokenException ex)
            {
                throw new UnauthorizedAccessException($"Invalid reset token. {ex.Message}");
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("Reset token format is invalid.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error during password reset.", ex);
            }
        }

        // ----- Helpers (unchanged) -----
        private static byte[] GenerateSalt(int size)
        {
            var salt = new byte[size];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var bytes = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        private string GenerateJwt(User user)
        {
            try
            {
                var jwtSection = _config.GetSection("Jwt");
                var keyValue = jwtSection["Key"];
                if (string.IsNullOrWhiteSpace(keyValue))
                    throw new InvalidOperationException("JWT key is not configured.");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.Role, user.Role),
                    new(JwtRegisteredClaimNames.Email, user.Email)
                };

                var token = new JwtSecurityToken(
                    issuer: jwtSection["Issuer"],
                    audience: jwtSection["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(8),
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to generate JWT.", ex);
            }
        }
    }
}
