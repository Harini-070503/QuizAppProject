using Microsoft.AspNetCore.Mvc;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;

namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _svc;

        public QuestionsController(IQuestionService svc)
        {
            _svc = svc;
        }

        // GET: api/questions/by-quiz/{quizId}
        [HttpGet("by-quiz/{quizId:guid}")]
        public async Task<ActionResult<List<QuestionDto>>> ByQuiz(Guid quizId)
        {
            try
            {
                var result = await _svc.GetByQuiz(quizId);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        // GET: api/questions/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<QuestionDto>> Get(Guid id)
        {
            try
            {
                var q = await _svc.Get(id);
                if (q == null)
                    return NotFound();

                return Ok(q);
            }
            catch
            {
                throw;
            }
        }

        // POST: api/questions/{quizId}
        [HttpPost("{quizId:guid}")]
        public async Task<ActionResult<QuestionDto>> Create(Guid quizId, [FromBody] QuestionCreateDto dto)
        {
            try
            {
                var res = await _svc.Add(quizId, dto);
                return CreatedAtAction(nameof(Get), new { id = res.QuestionId }, res);
            }
            catch
            {
                throw;
            }
        }

        // PUT: api/questions/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<QuestionDto>> Update(Guid id, [FromBody] QuestionCreateDto dto)
        {
            try
            {
                var updated = await _svc.Update(id, dto);
                return Ok(updated);
            }
            catch
            {
                throw;
            }
        }

        // DELETE: api/questions/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var ok = await _svc.Delete(id);

                if (!ok)
                    return NotFound();

                return NoContent();
            }
            catch
            {
                throw;
            }
        }
    }
}


