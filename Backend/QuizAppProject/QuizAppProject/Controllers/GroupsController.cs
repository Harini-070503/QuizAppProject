using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizAppProject.Interfaces;
using QuizAppProject.Models.DTOs;
using System.Security.Claims;

namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _svc;
        public GroupsController(IGroupService svc) => _svc = svc;

        private Guid CurrentUserId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET: api/groups
        [HttpGet]
        public async Task<ActionResult<List<GroupDto>>> GetMyGroups()
        {
            var list = await _svc.GetMyGroups(CurrentUserId());
            return Ok(list);
        }

        // GET: api/groups/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GroupDto>> Get(Guid id)
        {
            var g = await _svc.GetById(id);
            return g == null ? NotFound() : Ok(g);
        }

        // POST: api/groups
        [HttpPost]
        public async Task<ActionResult<GroupDto>> Create([FromBody] GroupCreateDto dto)
        {
            var g = await _svc.Create(CurrentUserId(), dto);
            return CreatedAtAction(nameof(Get), new { id = g.GroupId }, g);
        }

        // DELETE: api/groups/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var ok = await _svc.Delete(id, CurrentUserId());
                return ok ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // POST: api/groups/{id}/members
        [HttpPost("{id:guid}/members")]
        public async Task<ActionResult<GroupMemberDto>> AddMember(Guid id, [FromBody] AddMemberDto dto)
        {
            try
            {
                var member = await _svc.AddMember(id, CurrentUserId(), dto.UsernameOrEmail);
                return Ok(member);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // DELETE: api/groups/{id}/members/{userId}
        [HttpDelete("{id:guid}/members/{userId:guid}")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
        {
            try
            {
                var ok = await _svc.RemoveMember(id, CurrentUserId(), userId);
                return ok ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // PUT: api/groups/{groupId}/assign-quiz/{quizId}
        [HttpPut("{groupId:guid}/assign-quiz/{quizId:guid}")]
        public async Task<IActionResult> AssignQuiz(Guid groupId, Guid quizId)
        {
            try
            {
                await _svc.AssignQuiz(groupId, quizId, CurrentUserId());
                return NoContent();
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // DELETE: api/groups/unassign-quiz/{quizId}
        [HttpDelete("unassign-quiz/{quizId:guid}")]
        public async Task<IActionResult> UnassignQuiz(Guid quizId)
        {
            try { await _svc.UnassignQuiz(quizId); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // GET: api/groups/check-access/{quizId}
        [HttpGet("check-access/{quizId:guid}")]
        public async Task<ActionResult<bool>> CheckAccess(Guid quizId)
        {
            var ok = await _svc.CheckAccess(quizId, CurrentUserId());
            return Ok(ok);
        }
    }
}
