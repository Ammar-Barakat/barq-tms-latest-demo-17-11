using BarqTMS.API.DTOs;
using BarqTMS.API.Helpers;
using BarqTMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BarqTMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpPost("events")]
        public async Task<ActionResult<CalendarEventDto>> CreateEvent([FromBody] CreateCalendarEventDto dto)
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var result = await _calendarService.CreateEventAsync(dto, currentUserId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("events/{id}")]
        public async Task<ActionResult<CalendarEventDto>> UpdateEvent(int id, [FromBody] UpdateCalendarEventDto dto)
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var result = await _calendarService.UpdateEventAsync(id, dto, currentUserId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("events/{id}")]
        public async Task<ActionResult> DeleteEvent(int id)
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var result = await _calendarService.DeleteEventAsync(id, currentUserId);
                
                if (!result)
                    return NotFound("Calendar event not found");

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("events/{id}")]
        public async Task<ActionResult<CalendarEventDto>> GetEvent(int id)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var result = await _calendarService.GetEventByIdAsync(id, currentUserId);
            
            if (result == null)
                return NotFound("Calendar event not found");

            return Ok(result);
        }

        [HttpPost("view")]
        public async Task<ActionResult<CalendarViewDto>> GetCalendarView([FromBody] CalendarFilterDto filter)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var result = await _calendarService.GetCalendarViewAsync(filter, currentUserId);
            return Ok(result);
        }

        [HttpGet("events")]
        public async Task<ActionResult<List<CalendarEventDto>>> GetUserEvents(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var result = await _calendarService.GetUserEventsAsync(currentUserId, startDate, endDate);
            return Ok(result);
        }

        [HttpGet("events/upcoming")]
        public async Task<ActionResult<List<CalendarEventDto>>> GetUpcomingEvents([FromQuery] int days = 7)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var result = await _calendarService.GetUpcomingEventsAsync(currentUserId, days);
            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<CalendarStatsDto>> GetCalendarStats()
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var result = await _calendarService.GetCalendarStatsAsync(currentUserId);
            return Ok(result);
        }

        [HttpPut("events/{eventId}/attendees/{attendeeId}/status")]
        public async Task<ActionResult> UpdateAttendeeStatus(
            int eventId, 
            int attendeeId, 
            [FromBody] UpdateAttendeeStatusDto dto)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var result = await _calendarService.UpdateAttendeeStatusAsync(eventId, attendeeId, dto, currentUserId);
            
            if (!result)
                return NotFound("Attendee not found or you don't have permission to update this status");

            return NoContent();
        }

        [HttpGet("events/recurring/{id}/generate")]
        public async Task<ActionResult<List<CalendarEventDto>>> GenerateRecurringEvents(
            int id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var result = await _calendarService.GenerateRecurringEventsAsync(id, startDate, endDate);
            return Ok(result);
        }

        [HttpGet("events/search")]
        public async Task<ActionResult<List<CalendarEventDto>>> SearchEvents([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required");

            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var result = await _calendarService.SearchEventsAsync(query, currentUserId);
            return Ok(result);
        }

        [HttpPost("sync/tasks")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> SyncTaskDeadlines()
        {
            await _calendarService.SyncTaskDeadlinesAsync();
            return Ok(new { Message = "Task deadlines synced successfully" });
        }

        [HttpPost("sync/milestones")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> SyncProjectMilestones()
        {
            await _calendarService.SyncProjectMilestonesAsync();
            return Ok(new { Message = "Project milestones synced successfully" });
        }

        [HttpPost("reminders/send")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendEventReminders()
        {
            await _calendarService.SendEventRemindersAsync();
            return Ok(new { Message = "Event reminders sent successfully" });
        }

        // Additional calendar endpoints
        [HttpGet("events/today")]
        public async Task<ActionResult<List<CalendarEventDto>>> GetTodayEvents()
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var result = await _calendarService.GetUserEventsAsync(currentUserId, today, tomorrow);
            return Ok(result);
        }

        [HttpGet("events/this-week")]
        public async Task<ActionResult<List<CalendarEventDto>>> GetThisWeekEvents()
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);
            var result = await _calendarService.GetUserEventsAsync(currentUserId, weekStart, weekEnd);
            return Ok(result);
        }

        [HttpGet("events/this-month")]
        public async Task<ActionResult<List<CalendarEventDto>>> GetThisMonthEvents()
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            var result = await _calendarService.GetUserEventsAsync(currentUserId, monthStart, monthEnd);
            return Ok(result);
        }

        [HttpGet("availability/{userId}")]
        public async Task<ActionResult<List<AvailabilitySlotDto>>> GetUserAvailability(
            int userId,
            [FromQuery] DateTime date,
            [FromQuery] int durationMinutes = 60)
        {
            var events = await _calendarService.GetUserEventsAsync(userId, date.Date, date.Date.AddDays(1));
            var availability = CalculateAvailability(events, date, durationMinutes);
            return Ok(availability);
        }

        private List<AvailabilitySlotDto> CalculateAvailability(List<CalendarEventDto> events, DateTime date, int durationMinutes)
        {
            var availability = new List<AvailabilitySlotDto>();
            var workingHours = new TimeSpan[] { new(9, 0, 0), new(17, 0, 0) }; // 9 AM to 5 PM
            
            var dayStart = date.Date.Add(workingHours[0]);
            var dayEnd = date.Date.Add(workingHours[1]);
            var slotDuration = TimeSpan.FromMinutes(durationMinutes);

            // Sort events by start time
            var dayEvents = events
                .Where(e => e.StartDate.Date == date.Date && !e.IsAllDay)
                .OrderBy(e => e.StartDate)
                .ToList();

            var currentTime = dayStart;
            
            foreach (var evt in dayEvents)
            {
                // Check if there's a free slot before this event
                if (evt.StartDate > currentTime && (evt.StartDate - currentTime) >= slotDuration)
                {
                    availability.Add(new AvailabilitySlotDto
                    {
                        StartTime = currentTime,
                        EndTime = evt.StartDate,
                        IsAvailable = true,
                        Duration = evt.StartDate - currentTime
                    });
                }
                
                // Move current time to after this event
                currentTime = evt.EndDate > currentTime ? evt.EndDate : currentTime;
            }

            // Check for availability after the last event
            if (currentTime < dayEnd && (dayEnd - currentTime) >= slotDuration)
            {
                availability.Add(new AvailabilitySlotDto
                {
                    StartTime = currentTime,
                    EndTime = dayEnd,
                    IsAvailable = true,
                    Duration = dayEnd - currentTime
                });
            }

            return availability;
        }
    }

    public class AvailabilitySlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public TimeSpan Duration { get; set; }
    }
}