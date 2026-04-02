using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuizAppProject.Context;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Repositories;
using QuizAppProject.Services;

namespace Testing.Services
{
    public class AuthServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly AuthService _svc;
        private readonly TokenService _tokenSvc;

        private static readonly Guid UserId1 = Guid.Parse("a0000001-0000-0000-0000-000000000001");
        private static readonly Guid UserId2 = Guid.Parse("a0000001-0000-0000-0000-000000000002");

        private static IConfiguration BuildConfig() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"]                  = "SuperSecretTestKey1234567890ABCDEF",
                    ["Jwt:Issuer"]               = "TestIssuer",
                    ["Jwt:Audience"]             = "TestAudience",
                    ["Jwt:DurationInMinutes"]    = "60",
                    ["Jwt:PasswordResetMinutes"] = "15"
                })
                .Build();

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            var config = BuildConfig();
            _tokenSvc = new TokenService(config);

            SeedData();

            var userRepo = new Repository<Guid, User>(_ctx);
            _svc = new AuthService(userRepo, config, _tokenSvc);
        }

        private void SeedData()
        {
            // Pre-register alice using AuthService logic (PBKDF2 hash)
            // We seed raw so we can test login with known password
            var salt = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAA=="); // 16 zero bytes base64
            // Use the same HashPassword logic as AuthService (PBKDF2 SHA256 100k iterations)
            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                "Password1!", salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            _ctx.Users.Add(new User
            {
                UserId       = UserId1,
                Username     = "alice",
                Email        = "alice@test.com",
                PasswordHash = hash,
                Salt         = Convert.ToBase64String(salt),
                Role         = "Taker",
                CreatedAt    = DateTime.UtcNow
            });
            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // ── Register ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_NewUser_ReturnsTokenResponse()
        {
            var result = await _svc.Register(new RegisterRequestDto
            {
                Username = "bob",
                Email    = "bob@test.com",
                Password = "Password2!",
                Role     = "Creator"
            });

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task Register_TokenContainsCorrectRole()
        {
            var result = await _svc.Register(new RegisterRequestDto
            {
                Username = "charlie",
                Email    = "charlie@test.com",
                Password = "Password3!",
                Role     = "Creator"
            });

            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                          .ReadJwtToken(result.Token);
            var role = jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value;
            Assert.Equal("Creator", role);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ThrowsInvalidOperation()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.Register(new RegisterRequestDto
                {
                    Username = "alice",
                    Email    = "new@test.com",
                    Password = "Password2!",
                    Role     = "Taker"
                }));
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsInvalidOperation()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _svc.Register(new RegisterRequestDto
                {
                    Username = "newuser",
                    Email    = "alice@test.com",
                    Password = "Password2!",
                    Role     = "Taker"
                }));
        }

        [Fact]
        public async Task Register_UserPersistedInDatabase()
        {
            await _svc.Register(new RegisterRequestDto
            {
                Username = "dave",
                Email    = "dave@test.com",
                Password = "Password4!",
                Role     = "Taker"
            });

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Username == "dave");
            Assert.NotNull(user);
            Assert.Equal("dave@test.com", user!.Email);
        }

        [Fact]
        public async Task Register_PasswordIsHashed_NotStoredPlaintext()
        {
            await _svc.Register(new RegisterRequestDto
            {
                Username = "eve",
                Email    = "eve@test.com",
                Password = "MySecret!",
                Role     = "Taker"
            });

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Username == "eve");
            Assert.NotNull(user);
            Assert.NotEqual("MySecret!", user!.PasswordHash);
            Assert.False(string.IsNullOrEmpty(user.Salt));
        }

        // ── Login ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var result = await _svc.Login(new LoginRequestDto
            {
                Username = "alice",
                Password = "Password1!"
            });

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task Login_TokenContainsUsername()
        {
            var result = await _svc.Login(new LoginRequestDto
            {
                Username = "alice",
                Password = "Password1!"
            });

            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
                          .ReadJwtToken(result.Token);
            var name = jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Name).Value;
            Assert.Equal("alice", name);
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.Login(new LoginRequestDto { Username = "alice", Password = "WrongPass!" }));
        }

        [Fact]
        public async Task Login_UnknownUser_ThrowsUnauthorized()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.Login(new LoginRequestDto { Username = "ghost", Password = "Password1!" }));
        }

        // ── ForgotPassword ────────────────────────────────────────────────────

        [Fact]
        public async Task ForgotPassword_ExistingUser_ReturnsResetToken()
        {
            var result = await _svc.ForgotPassword(new ForgotPasswordRequestDto
            {
                UsernameOrEmail = "alice"
            });

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.ResetToken));
            Assert.True(result.ExpiresAtUtc > DateTime.UtcNow);
        }

        [Fact]
        public async Task ForgotPassword_ByEmail_ReturnsResetToken()
        {
            var result = await _svc.ForgotPassword(new ForgotPasswordRequestDto
            {
                UsernameOrEmail = "alice@test.com"
            });

            Assert.False(string.IsNullOrEmpty(result.ResetToken));
        }

        [Fact]
        public async Task ForgotPassword_UnknownUser_ThrowsKeyNotFound()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.ForgotPassword(new ForgotPasswordRequestDto
                {
                    UsernameOrEmail = "nobody@test.com"
                }));
        }

        // ── ResetPassword ─────────────────────────────────────────────────────

        [Fact]
        public async Task ResetPassword_ValidToken_ChangesPassword()
        {
            var forgot = await _svc.ForgotPassword(new ForgotPasswordRequestDto
            {
                UsernameOrEmail = "alice"
            });

            var result = await _svc.ResetPassword(new ResetPasswordRequestDto
            {
                Username    = "alice",
                ResetToken  = forgot.ResetToken,
                NewPassword = "NewPassword1!"
            });

            Assert.True(result);
        }

        [Fact]
        public async Task ResetPassword_CanLoginWithNewPassword()
        {
            var forgot = await _svc.ForgotPassword(new ForgotPasswordRequestDto
            {
                UsernameOrEmail = "alice"
            });

            await _svc.ResetPassword(new ResetPasswordRequestDto
            {
                Username    = "alice",
                ResetToken  = forgot.ResetToken,
                NewPassword = "NewPassword1!"
            });

            // Should now login with new password
            var loginResult = await _svc.Login(new LoginRequestDto
            {
                Username = "alice",
                Password = "NewPassword1!"
            });

            Assert.False(string.IsNullOrEmpty(loginResult.Token));
        }

        [Fact]
        public async Task ResetPassword_OldPasswordNoLongerWorks()
        {
            var forgot = await _svc.ForgotPassword(new ForgotPasswordRequestDto
            {
                UsernameOrEmail = "alice"
            });

            await _svc.ResetPassword(new ResetPasswordRequestDto
            {
                Username    = "alice",
                ResetToken  = forgot.ResetToken,
                NewPassword = "NewPassword1!"
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.Login(new LoginRequestDto { Username = "alice", Password = "Password1!" }));
        }

        [Fact]
        public async Task ResetPassword_WrongUsername_ThrowsKeyNotFound()
        {
            var forgot = await _svc.ForgotPassword(new ForgotPasswordRequestDto
            {
                UsernameOrEmail = "alice"
            });

            // "bob" doesn't exist in DB → KeyNotFoundException
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.ResetPassword(new ResetPasswordRequestDto
                {
                    Username    = "bob",
                    ResetToken  = forgot.ResetToken,
                    NewPassword = "NewPassword1!"
                }));
        }

        [Fact]
        public async Task ResetPassword_ExpiredToken_ThrowsUnauthorized()
        {
            // Generate a token that is already expired
            var expiredToken = _tokenSvc.GeneratePasswordResetToken(
                UserId1, "alice", TimeSpan.FromMinutes(-1));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _svc.ResetPassword(new ResetPasswordRequestDto
                {
                    Username    = "alice",
                    ResetToken  = expiredToken,
                    NewPassword = "NewPassword1!"
                }));
        }
    }
}
