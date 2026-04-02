using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _svc;
        private readonly IConfiguration _configuration;

        public QuizzesController(IQuizService svc,IConfiguration configuration)
        {
            _svc = svc;
            _configuration = configuration;


        }

        // GET: api/quizzes
        [HttpGet]
        public async Task<ActionResult<List<QuizDto>>> List([FromQuery] Guid? categoryId = null)
        {
            try
            {
                // Pass requesting user ID so evaluator quizzes are filtered by group membership
                Guid? requestingUserId = null;
                var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(claim, out var uid)) requestingUserId = uid;

                var result = await _svc.GetAll(categoryId, requestingUserId);
                return Ok(result);
            }
            catch(KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/quizzes/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuizDto>> Get(Guid id)
        {
            try
            {
                var res = await _svc.Get(id);
                if (res == null)
                    return NotFound();

                return Ok(res);
            }
            catch
            {
                throw;
            }
        }

        // POST: api/quizzes
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<QuizDto>> Create([FromBody] QuizCreateDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                var res = await _svc.Add(userId, dto);

                return CreatedAtAction(nameof(Get), new { id = res.QuizId }, res);
            }
            catch
            {
                throw;
            }
        }

        // PUT: api/quizzes/{id}
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<QuizDto>> Update(Guid id, [FromBody] QuizUpdateDto dto)
        {
            try{
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                var res = await _svc.Update(id, dto, userId);
                return Ok(res);
            }
            catch
            {
                throw;
            }
        }

        // DELETE: api/quizzes/{id}
        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return Unauthorized();

                var ok = await _svc.Delete(id, userId);

                if (!ok)
                    return NotFound();

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
        }


        private string GenerateToken(Guid userId, string userName, string email, string role)
        {
            var keyValue = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyValue))
                throw new InvalidOperationException("JWT Key is not configured.");


            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, userName),
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var durationValue = _configuration["Jwt:DurationInMinutes"];
            if (!double.TryParse(durationValue, out var expiryMinutes))
                expiryMinutes = 60;

            //var expiryMinutes =
            //    Convert.ToDouble(_configuration["Jwt:DurationInMinutes"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


