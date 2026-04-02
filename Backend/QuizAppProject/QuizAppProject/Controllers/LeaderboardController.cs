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
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _svc;

        public LeaderboardController(ILeaderboardService svc)
        {
            _svc = svc;
        }

        // GET: api/leaderboard?categoryId={categoryId}&take={take}
        [HttpGet]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> Top([FromQuery] Guid? categoryId = null, [FromQuery] int take = 20)
        {
            try
            {
                var result = await _svc.GetTopScores(categoryId, take);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}

