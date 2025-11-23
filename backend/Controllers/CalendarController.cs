using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Services;
using BarqTMS.API.DTOs;
using System.Security.Claims;

namespace BarqTMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpPost("view")]
        public async Task<ActionResult<CalendarViewDto>> GetEvents([FromBody] CalendarFilterDto filter)
        {
            var events = await _calendarService.GetEventsAsync(filter);
            return Ok(new CalendarViewDto { Events = events });
        }

        [HttpGet("events/{id}")]
        public async Task<ActionResult<CalendarEventDto>> GetEventById(int id)
        {
            var evt = await _calendarService.GetEventByIdAsync(id);
            if (evt == null) return NotFound();
            return Ok(evt);
        }

        [HttpPost("events")]
        public async Task<ActionResult<CalendarEventDto>> CreateEvent(CreateCalendarEventDto eventDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                return Unauthorized("Invalid User ID");
            }

            var evt = await _calendarService.CreateEventAsync(userId, eventDto);
            return CreatedAtAction(nameof(GetEventById), new { id = evt.EventId }, evt);
        }

        [HttpPut("events/{id}")]
        public async Task<ActionResult<CalendarEventDto>> UpdateEvent(int id, UpdateCalendarEventDto eventDto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                return Unauthorized("Invalid User ID");
            }

            var evt = await _calendarService.UpdateEventAsync(id, userId, eventDto);
            if (evt == null) return NotFound();
            return Ok(evt);
        }

        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId) || userId == 0)
            {
                return Unauthorized("Invalid User ID");
            }

            var result = await _calendarService.DeleteEventAsync(id, userId);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("stats")]
        public async Task<ActionResult<CalendarStatsDto>> GetStats()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var stats = await _calendarService.GetCalendarStatsAsync(userId);
            return Ok(stats);
        }
    }
}
