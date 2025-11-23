using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface ICalendarService
    {
        Task<List<CalendarEventDto>> GetEventsAsync(CalendarFilterDto filter);
        Task<CalendarEventDto?> GetEventByIdAsync(int id);
        Task<CalendarEventDto> CreateEventAsync(int userId, CreateCalendarEventDto eventDto);
        Task<CalendarEventDto?> UpdateEventAsync(int id, int userId, UpdateCalendarEventDto eventDto);
        Task<bool> DeleteEventAsync(int id, int userId);
        Task<CalendarStatsDto> GetCalendarStatsAsync(int userId);
    }

    public class CalendarService : ICalendarService
    {
        private readonly BarqTMSDbContext _context;

        public CalendarService(BarqTMSDbContext context)
        {
            _context = context;
        }

        public async Task<List<CalendarEventDto>> GetEventsAsync(CalendarFilterDto filter)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Attendees)
                .AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(e => e.StartTime >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(e => e.EndTime <= filter.EndDate.Value);

            var events = await query.ToListAsync();

            return events.Select(MapToDto).ToList();
        }

        public async Task<CalendarEventDto?> GetEventByIdAsync(int id)
        {
            var evt = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.EventId == id);

            return evt == null ? null : MapToDto(evt);
        }

        public async Task<CalendarEventDto> CreateEventAsync(int userId, CreateCalendarEventDto eventDto)
        {
            var evt = new CalendarEvent
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartTime = eventDto.StartTime,
                EndTime = eventDto.EndTime,
                EventType = eventDto.EventType,
                CreatedBy = userId,
                RelatedProjectId = eventDto.RelatedProjectId,
                RelatedTaskId = eventDto.RelatedTaskId,
                RelatedCompanyId = eventDto.RelatedCompanyId
            };

            _context.CalendarEvents.Add(evt);
            await _context.SaveChangesAsync();

            if (eventDto.AttendeeIds != null && eventDto.AttendeeIds.Any())
            {
                foreach (var attendeeId in eventDto.AttendeeIds)
                {
                    _context.EventAttendees.Add(new EventAttendee
                    {
                        EventId = evt.EventId,
                        UserId = attendeeId,
                    });
                }
                await _context.SaveChangesAsync();
            }

            return MapToDto(evt);
        }

        public async Task<CalendarEventDto?> UpdateEventAsync(int id, int userId, UpdateCalendarEventDto eventDto)
        {
            var evt = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (evt == null) return null;
            // Optional: Check if user is creator or has permission
            // if (evt.CreatedBy != userId) ...

            if (eventDto.Title != null) evt.Title = eventDto.Title;
            if (eventDto.Description != null) evt.Description = eventDto.Description;
            if (eventDto.StartTime.HasValue) evt.StartTime = eventDto.StartTime.Value;
            if (eventDto.EndTime.HasValue) evt.EndTime = eventDto.EndTime.Value;
            if (eventDto.EventType.HasValue) evt.EventType = eventDto.EventType.Value;
            if (eventDto.RelatedProjectId.HasValue) evt.RelatedProjectId = eventDto.RelatedProjectId.Value;
            if (eventDto.RelatedTaskId.HasValue) evt.RelatedTaskId = eventDto.RelatedTaskId.Value;
            if (eventDto.RelatedCompanyId.HasValue) evt.RelatedCompanyId = eventDto.RelatedCompanyId.Value;

            if (eventDto.AttendeeIds != null)
            {
                _context.EventAttendees.RemoveRange(evt.Attendees);
                foreach (var attendeeId in eventDto.AttendeeIds)
                {
                    _context.EventAttendees.Add(new EventAttendee
                    {
                        EventId = evt.EventId,
                        UserId = attendeeId,
                    });
                }
            }

            await _context.SaveChangesAsync();
            return MapToDto(evt);
        }

        public async Task<bool> DeleteEventAsync(int id, int userId)
        {
            var evt = await _context.CalendarEvents.FindAsync(id);
            if (evt == null) return false;
            // Optional: Check permission

            _context.CalendarEvents.Remove(evt);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CalendarStatsDto> GetCalendarStatsAsync(int userId)
        {
            var totalEvents = await _context.CalendarEvents.CountAsync(e => e.CreatedBy == userId || e.Attendees.Any(a => a.UserId == userId));
            return new CalendarStatsDto { TotalEvents = totalEvents };
        }

        private static CalendarEventDto MapToDto(CalendarEvent e)
        {
            return new CalendarEventDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Description = e.Description,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                EventType = e.EventType,
                CreatedBy = e.CreatedBy,
                RelatedProjectId = e.RelatedProjectId,
                RelatedTaskId = e.RelatedTaskId,
                RelatedCompanyId = e.RelatedCompanyId,
                AttendeeIds = e.Attendees.Select(a => a.UserId).ToList()
            };
        }
    }
}
