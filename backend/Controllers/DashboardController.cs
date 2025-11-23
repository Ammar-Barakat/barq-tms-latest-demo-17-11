using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BarqTMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;

        public DashboardController(BarqTMSDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetStats()
        {
            var stats = new DashboardStatsDto
            {
                TotalTasks = await _context.Tasks.CountAsync(),
                TotalProjects = await _context.Projects.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalClients = await _context.Companies.CountAsync()
            };

            return Ok(stats);
        }
    }
}
