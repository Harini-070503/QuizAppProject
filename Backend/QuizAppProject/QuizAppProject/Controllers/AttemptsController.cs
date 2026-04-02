    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using QuizAppProject.Interfaces;
    using QuizAppProject.Models.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace QuizAppProject.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AttemptsController : ControllerBase
        {
            private readonly IAttemptService _svc;

            public AttemptsController(IAttemptService svc)
            {
                _svc = svc;
            }

            // POST: api/attempts
            [HttpPost]
            public async Task<ActionResult<AttemptResultDto>> Submit([FromBody] AttemptSubmitDto dto)
            {
              try
              {
                // User ID must now come from the DTO or client request
                if (dto.UserId == Guid.Empty)
                    return BadRequest("UserId is required.");

                var res = await _svc.SubmitAttempt(dto.UserId, dto);
                return Ok(res);
              }
              catch
              {
                throw;
              }
            }

            // GET: api/attempts/mine?userId={userId}
            [HttpGet("mine")]
            public async Task<ActionResult<List<AttemptResultDto>>> Mine([FromQuery] Guid userId)
            {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest("UserId is required.");

                return Ok(await _svc.GetAttemptsByUser(userId));
            }
            catch
            {
                throw;
            }
            }

            // GET: api/attempts/{attemptId}
            [HttpGet("{attemptId:guid}")]
            public async Task<ActionResult<AttemptResultDto>> Get(Guid attemptId)
            {
            try
            {
                var a = await _svc.GetAttempt(attemptId);
                if (a == null) return NotFound();

                return Ok(a);
            }
            catch
            {
                throw;
            }
            }

            // GET: api/attempts/quiz/{quizId}/submissions  (Evaluator)
            [HttpGet("quiz/{quizId:guid}/submissions")]
            public async Task<ActionResult<List<SubmissionDetailDto>>> GetByQuiz(Guid quizId)
            {
                var list = await _svc.GetSubmissionsByQuiz(quizId);
                return Ok(list);
            }

            // GET: api/attempts/{attemptId}/detail  (Evaluator)
            [HttpGet("{attemptId:guid}/detail")]
            public async Task<ActionResult<SubmissionDetailDto>> GetDetail(Guid attemptId)
            {
                var detail = await _svc.GetSubmissionDetail(attemptId);
                if (detail == null) return NotFound();
                return Ok(detail);
            }

            // PUT: api/attempts/{attemptId}/score  (Evaluator)
            [HttpPut("{attemptId:guid}/score")]
            public async Task<IActionResult> UpdateScore(Guid attemptId, [FromBody] UpdateScoreDto dto)
            {
                var ok = await _svc.UpdateScore(attemptId, dto);
                return ok ? NoContent() : NotFound();
            }
        }
    }

