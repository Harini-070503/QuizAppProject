using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Controllers
{
    /// <summary>
    /// Manages quiz allocations for normal Takers.
    /// Only Creators and Evaluators can allocate/deallocate quizzes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizAllocationsController : ControllerBase
    {
        private readonly IQuizAllocationService _svc;

        public QuizAllocationsController(IQuizAllocationService svc)
        {
            _svc = svc;
        }

        // POST: api/quizallocations
        [HttpPost]
        [Authorize(Roles = "Creator,Evaluator")]
        public async Task<ActionResult<QuizAllocationDto>> Allocate([FromBody] QuizAllocationCreateDto dto)
        {
            try
            {
                var result = await _svc.Allocate(dto);
                return CreatedAtAction(nameof(GetByUser), new { userId = dto.UserId }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        // DELETE: api/quizallocations/{allocationId}
        [HttpDelete("{allocationId:guid}")]
        [Authorize(Roles = "Creator,Evaluator")]
        public async Task<IActionResult> Deallocate(Guid allocationId)
        {
            var ok = await _svc.Deallocate(allocationId);
            return ok ? NoContent() : NotFound();
        }

        // GET: api/quizallocations/user/{userId}
        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<List<QuizAllocationDto>>> GetByUser(Guid userId)
        {
            return Ok(await _svc.GetByUser(userId));
        }

        // GET: api/quizallocations/quiz/{quizId}
        [HttpGet("quiz/{quizId:guid}")]
        [Authorize(Roles = "Creator,Evaluator")]
        public async Task<ActionResult<List<QuizAllocationDto>>> GetByQuiz(Guid quizId)
        {
            return Ok(await _svc.GetByQuiz(quizId));
        }
    }
}
