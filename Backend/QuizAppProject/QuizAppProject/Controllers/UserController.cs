using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _users;
        private readonly AppDbContext _db;

        public UserController(IUserService users, AppDbContext db)
        {
            _users = users;
            _db = db;
        }

        // GET: api/user/takers  — list Taker and PremiumTaker users (for evaluator group management)
        [Authorize]
        [HttpGet("takers")]
        public async Task<ActionResult<List<UserDto>>> GetTakers([FromQuery] string? search = null)
        {
            var query = _db.Users
                .Where(u => u.Role == "Taker" || u.Role == "PremiumTaker")
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search));

            var users = await query.OrderBy(u => u.Username).Take(50).ToListAsync();

            return Ok(users.Select(u => new UserDto
            {
                UserId   = u.UserId,
                Username = u.Username,
                Email    = u.Email,
                Role     = u.Role
            }).ToList());
        }

        // GET: api/user/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetById(Guid id)
        {
            try
            {
                var user = await _users.GetById(id);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch
            {
                throw;
            }
        }

        // GET: api/user/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult<UserDto>> GetByUsername(string username)
        {
            try
            {
                var user = await _users.GetByUsername(username);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch
            {
                throw;
            }
        }

        // PUT: api/user/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserDto>> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var updated = await _users.UpdateUser(id, dto);
                return Ok(updated);
            }
            catch
            {
                throw;
            }
        }

        // POST: api/user/{id}/upgrade-to-premium
        [Authorize]
        [HttpPost("{id:guid}/upgrade-to-premium")]
        public async Task<ActionResult<AuthResponseDto>> UpgradeToPremium(Guid id)
        {
            try
            {
                var result = await _users.UpgradeToPremium(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)      { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception)                    { return StatusCode(500, new { message = "Unexpected error during upgrade." }); }
        }
    }
}


