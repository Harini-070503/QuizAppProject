using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizAppProject.Context;
using QuizAppProject.Models;
using System.Security.Claims;

namespace QuizAppProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public NotificationsController(AppDbContext db) => _db = db;

        private Guid CurrentUserId() =>
            Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // GET: api/notifications  — get my notifications
        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetMine()
        {
            var uid = CurrentUserId();
            var list = await _db.Notifications
                .Where(n => n.UserId == uid)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .AsNoTracking()
                .ToListAsync();

            return Ok(list.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                LinkUrl = n.LinkUrl,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList());
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> UnreadCount()
        {
            var uid = CurrentUserId();
            var count = await _db.Notifications.CountAsync(n => n.UserId == uid && !n.IsRead);
            return Ok(count);
        }

        // PUT: api/notifications/{id}/read
        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.NotificationId == id && x.UserId == CurrentUserId());
            if (n == null) return NotFound();
            n.IsRead = true;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var uid = CurrentUserId();
            await _db.Notifications
                .Where(n => n.UserId == uid && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
            return NoContent();
        }
    }

    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
