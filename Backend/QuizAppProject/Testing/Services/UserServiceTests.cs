using Moq;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.Models.DTOs;
using QuizAppProject.Services;
using QuizAppProject.Context;
using QuizAppProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Testing.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly AppDbContext _ctx;
        private readonly UserService _svc;
        private readonly Mock<ITokenService> _tokenMock;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _ctx = new AppDbContext(options);
            SeedData();

            var userRepo = new Repository<Guid, User>(_ctx);
            _tokenMock = new Mock<ITokenService>();
            _tokenMock.Setup(t => t.GenerateToken(
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns("mock-jwt-token");

            _svc = new UserService(userRepo, _tokenMock.Object);
        }

        private void SeedData()
        {
            _ctx.Users.AddRange(
                new User
                {
                    UserId       = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                    Username     = "alice",
                    Email        = "alice@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1!"),
                    Salt         = "salt1",
                    Role         = "Taker",
                    CreatedAt    = DateTime.UtcNow,
                    UserDetails  = new UserDetails { Name = "Alice Smith", City = "Chennai" }
                },
                new User
                {
                    UserId       = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                    Username     = "bob",
                    Email        = "bob@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password2!"),
                    Salt         = "salt2",
                    Role         = "Creator",
                    CreatedAt    = DateTime.UtcNow,
                    UserDetails  = new UserDetails { Name = "Bob Jones" }
                }
            );
            _ctx.SaveChanges();
        }

        public void Dispose() => _ctx.Dispose();

        // GetByUsername
        [Fact]
        public async Task GetByUsername_ExistingUser_ReturnsDto()
        {
            var result = await _svc.GetByUsername("alice");
            Assert.NotNull(result);
            Assert.Equal("alice", result!.Username);
            Assert.Equal("alice@example.com", result.Email);
            Assert.Equal("Taker", result.Role);
            Assert.Equal("Alice Smith", result.Name);
        }

        [Fact]
        public async Task GetByUsername_UnknownUser_ReturnsNull()
        {
            Assert.Null(await _svc.GetByUsername("nobody"));
        }

        // GetById
        [Fact]
        public async Task GetById_ExistingId_ReturnsDto()
        {
            var result = await _svc.GetById(Guid.Parse("11111111-0000-0000-0000-000000000001"));
            Assert.NotNull(result);
            Assert.Equal("alice", result!.Username);
        }

        [Fact]
        public async Task GetById_UnknownId_ReturnsNull()
        {
            Assert.Null(await _svc.GetById(Guid.NewGuid()));
        }

        // UpdateUser
        [Fact]
        public async Task UpdateUser_ValidRequest_ReturnsUpdatedDto()
        {
            var id = Guid.Parse("11111111-0000-0000-0000-000000000001");
            var result = await _svc.UpdateUser(id, new UpdateUserDto { Name = "Alice Updated", City = "Mumbai" });
            Assert.NotNull(result);
            Assert.Equal("Alice Updated", result.Name);
        }

        [Fact]
        public async Task UpdateUser_UnknownUser_ThrowsKeyNotFoundException()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _svc.UpdateUser(Guid.NewGuid(), new UpdateUserDto { Name = "X" }));
        }

        // Register
        [Fact]
        public async Task Register_NewUser_ReturnsToken()
        {
            var result = await _svc.Register(new RegisterRequestDto
            {
                Username = "charlie",
                Email    = "charlie@example.com",
                Password = "Password3!",
                Role     = "Taker"
            });
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));
        }

        [Fact]
        public async Task Register_DuplicateUsername_ThrowsException()
        {
            await Assert.ThrowsAsync<Exception>(() =>
                _svc.Register(new RegisterRequestDto
                {
                    Username = "alice",
                    Email    = "new@example.com",
                    Password = "Password3!",
                    Role     = "Taker"
                }));
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsException()
        {
            await Assert.ThrowsAsync<Exception>(() =>
                _svc.Register(new RegisterRequestDto
                {
                    Username = "newuser",
                    Email    = "alice@example.com",
                    Password = "Password3!",
                    Role     = "Taker"
                }));
        }

        // Login
        [Fact]
        public async Task Login_ValidCredentials_ReturnsJwtToken()
        {
            var result = await _svc.Login(new LoginRequestDto
            {
                Username = "alice",
                Password = "Password1!"
            });
            Assert.NotNull(result);
            Assert.Equal("mock-jwt-token", result.Token);
            _tokenMock.Verify(t => t.GenerateToken(
                It.IsAny<Guid>(), "alice", "alice@example.com", "Taker"), Times.Once);
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsException()
        {
            await Assert.ThrowsAsync<Exception>(() =>
                _svc.Login(new LoginRequestDto { Username = "alice", Password = "WrongPass!" }));
        }

        [Fact]
        public async Task Login_UnknownUser_ThrowsException()
        {
            await Assert.ThrowsAsync<Exception>(() =>
                _svc.Login(new LoginRequestDto { Username = "ghost", Password = "Password1!" }));
        }
    }
}
