using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using QuizAppProject.Interfaces;
using QuizAppProject.Models;
using QuizAppProject.DTOs;          // If your Auth/Login/Register DTOs are here
using QuizAppProject.Models.DTOs;   // If your User DTOs are here
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuizAppProject.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<Guid, User> _userRepo;
        private readonly ITokenService _tokenService;

        public UserService(IRepository<Guid, User> userRepo, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
        }

        public async Task<UserDto?> GetByUsername(string username)
        {
            try
            {
                var u = await _userRepo.Query()
                    .Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (u == null) return null;

                return new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    Name = u.UserDetails?.Name
                };
            }
            catch (Exception ex)
            {
                // Keeping nullable contract; bubble up with added context
                throw new Exception("Unexpected error while fetching user by username.", ex);
            }
        }

        public async Task<UserDto?> GetById(Guid id)
        {
            try
            {
                var u = await _userRepo.Query()
                    .Include(u => u.UserDetails)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (u == null) return null;

                return new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    Name = u.UserDetails?.Name
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while fetching user by id.", ex);
            }
        }

        public async Task<UserDto> UpdateUser(Guid id, UpdateUserDto request)
        {
            try
            {
                var u = await _userRepo.Query()
                    .Include(x => x.UserDetails)
                    .FirstOrDefaultAsync(x => x.UserId == id)
                    ?? throw new KeyNotFoundException("User not found.");

                // Allow updating specific fields on nested UserDetails
                if (!string.IsNullOrWhiteSpace(request.Name))
                    (u.UserDetails ??= new Models.UserDetails()).Name = request.Name;

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    (u.UserDetails ??= new Models.UserDetails()).PhoneNumber = request.PhoneNumber;

                if (!string.IsNullOrWhiteSpace(request.AddressLine1))
                    (u.UserDetails ??= new Models.UserDetails()).AddressLine1 = request.AddressLine1;

                if (!string.IsNullOrWhiteSpace(request.AddressLine2))
                    (u.UserDetails ??= new Models.UserDetails()).AddressLine2 = request.AddressLine2;

                if (!string.IsNullOrWhiteSpace(request.City))
                    (u.UserDetails ??= new Models.UserDetails()).City = request.City;

                if (!string.IsNullOrWhiteSpace(request.State))
                    (u.UserDetails ??= new Models.UserDetails()).State = request.State;

                if (!string.IsNullOrWhiteSpace(request.Pincode))
                    (u.UserDetails ??= new Models.UserDetails()).Pincode = request.Pincode;

                // Persist via repository
                var updated = await _userRepo.Update(id, u);
                if (updated is null)
                    throw new InvalidOperationException("Failed to update user.");

                return new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    Name = u.UserDetails?.Name
                };
            }
            catch (KeyNotFoundException) { throw; } // preserve original 404 semantics
            catch (InvalidOperationException) { throw; } // business/persistence failures
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while updating the user.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error while updating the user.", ex);
            }
        }

        public async Task<AuthResponseDto> Register(RegisterRequestDto request)
        {
            try
            {
                var exists = await _userRepo.Query()
                    .AnyAsync(x => x.Username == request.Username || x.Email == request.Email);

                if (exists)
                    throw new Exception("User already exists.");

                var user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Role = "User"
                };

                var added = await _userRepo.Add(user);
                if (added is null)
                    throw new InvalidOperationException("Failed to create user.");

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                };

                return new AuthResponseDto
                {
                    Token = "TOKEN_TO_BE_GENERATED",
                    //User = userDto
                };
            }
            catch (InvalidOperationException) { throw; } // failed create
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("A database error occurred while registering the user.", ex);
            }
            catch (Exception) { throw; } // keep your original "User already exists." behavior & others
        }

        public async Task<AuthResponseDto> Login(LoginRequestDto request)
        {
            try
            {
                var user = await _userRepo.Query()
                    .Include(x => x.UserDetails)
                    .FirstOrDefaultAsync(x => x.Username == request.Username);

                if (user == null)
                    throw new Exception("Invalid username or password.");

                var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!valid)
                    throw new Exception("Invalid username or password.");

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Name = user.UserDetails?.Name
                };

                return new AuthResponseDto
                {
                    Token = _tokenService.GenerateToken(
                        user.UserId,
                        user.Username,
                        user.Email,
                        user.Role),
                    //User = userDto
                };
            }
            catch (Exception) { throw; } // preserve your existing login error messages
        }
    }
}
