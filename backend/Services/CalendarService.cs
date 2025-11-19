using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;

namespace BarqTMS.API.Services
{
    public interface ICalendarService
    {
        Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventDto dto, int currentUserId);
        Task<CalendarEventDto> UpdateEventAsync(int id, UpdateCalendarEventDto dto, int currentUserId);
        Task<bool> DeleteEventAsync(int id, int currentUserId);
        Task<CalendarEventDto?> GetEventByIdAsync(int id, int currentUserId);
        Task<CalendarViewDto> GetCalendarViewAsync(CalendarFilterDto filter, int currentUserId);
        Task<List<CalendarEventDto>> GetUserEventsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<CalendarEventDto>> GetUpcomingEventsAsync(int userId, int days = 7);
        Task<CalendarStatsDto> GetCalendarStatsAsync(int userId);
        Task<bool> UpdateAttendeeStatusAsync(int eventId, int attendeeId, UpdateAttendeeStatusDto dto, int currentUserId);
        Task<List<CalendarEventDto>> GenerateRecurringEventsAsync(int recurringEventId, DateTime startDate, DateTime endDate);
        Task SendEventRemindersAsync();
        Task<List<CalendarEventDto>> SearchEventsAsync(string query, int userId);
        Task SyncTaskDeadlinesAsync();
        Task SyncProjectMilestonesAsync();
    }

    public class CalendarService : ICalendarService
    {
        private readonly BarqTMSDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;
        private readonly IRealTimeService _realTimeService;

        public CalendarService(
            BarqTMSDbContext context,
            IAuditService auditService,
            IEmailService emailService,
            IRealTimeService realTimeService)
        {
            _context = context;
            _auditService = auditService;
            _emailService = emailService;
            _realTimeService = realTimeService;
        }

        public async Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventDto dto, int currentUserId)
        {
            // FIX 5: Validate calendar event dates - EndDate must be after StartDate
            if (dto.EndDate <= dto.StartDate)
            {
                throw new ArgumentException("Event end date must be after the start date.");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("Event title is required.");
            }

            var calendarEvent = new CalendarEvent
            {
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsAllDay = dto.IsAllDay,
                Color = dto.Color,
                EventType = dto.EventType,
                TaskId = dto.TaskId,
                ProjectId = dto.ProjectId,
                UserId = dto.UserId,
                DepartmentId = dto.DepartmentId,
                IsRecurring = dto.IsRecurring,
                RecurrencePattern = dto.RecurrencePattern,
                RecurrenceInterval = dto.RecurrenceInterval,
                RecurrenceEndDate = dto.RecurrenceEndDate,
                RecurrenceDays = dto.RecurrenceDays?.Any() == true ? JsonSerializer.Serialize(dto.RecurrenceDays) : null,
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            // Add attendees
            if (dto.AttendeeUserIds?.Any() == true)
            {
                var attendees = dto.AttendeeUserIds.Select(userId => new CalendarEventAttendee
                {
                    CalendarEventId = calendarEvent.Id,
                    UserId = userId,
                    Status = userId == currentUserId ? AttendeeStatus.Accepted : AttendeeStatus.Pending,
                    IsOrganizer = userId == currentUserId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.CalendarEventAttendees.AddRange(attendees);
            }

            // Add reminders
            if (dto.Reminders?.Any() == true)
            {
                var reminders = dto.Reminders.Select(r => new CalendarReminder
                {
                    CalendarEventId = calendarEvent.Id,
                    UserId = r.UserId,
                    MinutesBefore = r.MinutesBefore,
                    Type = r.Type,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.CalendarReminders.AddRange(reminders);
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CalendarEvent", calendarEvent.Id, "CREATE", 
                $"Created calendar event: {calendarEvent.Title}", currentUserId);

            // Send notifications to attendees
            if (dto.AttendeeUserIds?.Any() == true)
            {
                var attendeeIds = dto.AttendeeUserIds.Where(id => id != currentUserId).ToList();
                if (attendeeIds.Any())
                {
                    await _realTimeService.SendToUsersAsync(attendeeIds, "calendar_event_invitation", new
                    {
                        EventId = calendarEvent.Id,
                        Title = calendarEvent.Title,
                        StartDate = calendarEvent.StartDate,
                        EndDate = calendarEvent.EndDate
                    });
                }
            }

            return await GetEventByIdAsync(calendarEvent.Id, currentUserId) ?? throw new Exception("Failed to retrieve created event");
        }

        public async Task<CalendarEventDto> UpdateEventAsync(int id, UpdateCalendarEventDto dto, int currentUserId)
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .Include(e => e.Reminders)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
                throw new ArgumentException("Calendar event not found");

            // Check permissions
            var isOrganizer = calendarEvent.CreatedByUserId == currentUserId;
            var isAttendee = calendarEvent.Attendees.Any(a => a.UserId == currentUserId);
            if (!isOrganizer && !isAttendee)
                throw new UnauthorizedAccessException("You don't have permission to update this event");

            var oldValues = JsonSerializer.Serialize(new
            {
                calendarEvent.Title,
                calendarEvent.Description,
                calendarEvent.StartDate,
                calendarEvent.EndDate,
                calendarEvent.IsAllDay,
                calendarEvent.Color,
                calendarEvent.EventType
            });

            // FIX 5: Validate calendar event dates if both are being updated
            var newStartDate = dto.StartDate ?? calendarEvent.StartDate;
            var newEndDate = dto.EndDate ?? calendarEvent.EndDate;
            if (newEndDate <= newStartDate)
            {
                throw new ArgumentException("Event end date must be after the start date.");
            }

            // Update properties
            if (dto.Title != null) calendarEvent.Title = dto.Title;
            if (dto.Description != null) calendarEvent.Description = dto.Description;
            if (dto.StartDate.HasValue) calendarEvent.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) calendarEvent.EndDate = dto.EndDate.Value;
            if (dto.IsAllDay.HasValue) calendarEvent.IsAllDay = dto.IsAllDay.Value;
            if (dto.Color != null) calendarEvent.Color = dto.Color;
            if (dto.EventType.HasValue) calendarEvent.EventType = dto.EventType.Value;
            if (dto.TaskId.HasValue) calendarEvent.TaskId = dto.TaskId;
            if (dto.ProjectId.HasValue) calendarEvent.ProjectId = dto.ProjectId;
            if (dto.UserId.HasValue) calendarEvent.UserId = dto.UserId;
            if (dto.DepartmentId.HasValue) calendarEvent.DepartmentId = dto.DepartmentId;
            if (dto.IsRecurring.HasValue) calendarEvent.IsRecurring = dto.IsRecurring.Value;
            if (dto.RecurrencePattern.HasValue) calendarEvent.RecurrencePattern = dto.RecurrencePattern;
            if (dto.RecurrenceInterval.HasValue) calendarEvent.RecurrenceInterval = dto.RecurrenceInterval;
            if (dto.RecurrenceEndDate.HasValue) calendarEvent.RecurrenceEndDate = dto.RecurrenceEndDate;
            if (dto.RecurrenceDays != null)
                calendarEvent.RecurrenceDays = dto.RecurrenceDays.Any() ? JsonSerializer.Serialize(dto.RecurrenceDays) : null;

            calendarEvent.UpdatedAt = DateTime.UtcNow;

            // Update attendees if provided
            if (dto.AttendeeUserIds != null && isOrganizer)
            {
                // Remove old attendees
                _context.CalendarEventAttendees.RemoveRange(calendarEvent.Attendees);

                // Add new attendees
                var newAttendees = dto.AttendeeUserIds.Select(userId => new CalendarEventAttendee
                {
                    CalendarEventId = calendarEvent.Id,
                    UserId = userId,
                    Status = userId == currentUserId ? AttendeeStatus.Accepted : AttendeeStatus.Pending,
                    IsOrganizer = userId == currentUserId,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.CalendarEventAttendees.AddRange(newAttendees);
            }

            // Update reminders if provided
            if (dto.Reminders != null)
            {
                _context.CalendarReminders.RemoveRange(calendarEvent.Reminders);

                var newReminders = dto.Reminders.Select(r => new CalendarReminder
                {
                    CalendarEventId = calendarEvent.Id,
                    UserId = r.UserId,
                    MinutesBefore = r.MinutesBefore,
                    Type = r.Type,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                _context.CalendarReminders.AddRange(newReminders);
            }

            await _context.SaveChangesAsync();

            var newValues = JsonSerializer.Serialize(new
            {
                calendarEvent.Title,
                calendarEvent.Description,
                calendarEvent.StartDate,
                calendarEvent.EndDate,
                calendarEvent.IsAllDay,
                calendarEvent.Color,
                calendarEvent.EventType
            });

            await _auditService.LogAsync("CalendarEvent", calendarEvent.Id, "UPDATE", 
                $"Updated calendar event: {calendarEvent.Title}", currentUserId, oldValues, newValues);

            return await GetEventByIdAsync(calendarEvent.Id, currentUserId) ?? throw new Exception("Failed to retrieve updated event");
        }

        public async Task<bool> DeleteEventAsync(int id, int currentUserId)
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Attendees)
                .Include(e => e.Reminders)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
                return false;

            // Check permissions
            if (calendarEvent.CreatedByUserId != currentUserId)
                throw new UnauthorizedAccessException("You don't have permission to delete this event");

            _context.CalendarReminders.RemoveRange(calendarEvent.Reminders);
            _context.CalendarEventAttendees.RemoveRange(calendarEvent.Attendees);
            _context.CalendarEvents.Remove(calendarEvent);

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CalendarEvent", calendarEvent.Id, "DELETE", 
                $"Deleted calendar event: {calendarEvent.Title}", currentUserId);

            return true;
        }

        public async Task<CalendarEventDto?> GetEventByIdAsync(int id, int currentUserId)
        {
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Task)
                .Include(e => e.Project)
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.CreatedByUser)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Include(e => e.Reminders)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (calendarEvent == null)
                return null;

            // Check permissions
            var isOrganizer = calendarEvent.CreatedByUserId == currentUserId;
            var isAttendee = calendarEvent.Attendees.Any(a => a.UserId == currentUserId);
            if (!isOrganizer && !isAttendee)
                return null;

            return MapToDto(calendarEvent);
        }

        public async Task<CalendarViewDto> GetCalendarViewAsync(CalendarFilterDto filter, int currentUserId)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Task)
                .Include(e => e.Project)
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.CreatedByUser)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Include(e => e.Reminders)
                    .ThenInclude(r => r.User)
                .Where(e => e.CreatedByUserId == currentUserId || 
                           e.Attendees.Any(a => a.UserId == currentUserId));

            // Apply filters
            if (filter.StartDate.HasValue)
                query = query.Where(e => e.EndDate >= filter.StartDate.Value);
            
            if (filter.EndDate.HasValue)
                query = query.Where(e => e.StartDate <= filter.EndDate.Value);

            if (filter.EventTypes?.Any() == true)
                query = query.Where(e => filter.EventTypes.Contains(e.EventType));

            if (filter.ProjectIds?.Any() == true)
                query = query.Where(e => e.ProjectId.HasValue && filter.ProjectIds.Contains(e.ProjectId.Value));

            if (filter.UserIds?.Any() == true)
                query = query.Where(e => e.UserId.HasValue && filter.UserIds.Contains(e.UserId.Value));

            if (filter.DepartmentIds?.Any() == true)
                query = query.Where(e => e.DepartmentId.HasValue && filter.DepartmentIds.Contains(e.DepartmentId.Value));

            var events = await query
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            var eventDtos = events.Select(MapToDto).ToList();

            // Generate recurring events if requested
            if (filter.IncludeRecurring && filter.StartDate.HasValue && filter.EndDate.HasValue)
            {
                var recurringEvents = events.Where(e => e.IsRecurring).ToList();
                foreach (var recurringEvent in recurringEvents)
                {
                    var generatedEvents = await GenerateRecurringEventsForEvent(recurringEvent, filter.StartDate.Value, filter.EndDate.Value);
                    eventDtos.AddRange(generatedEvents);
                }
            }

            return new CalendarViewDto
            {
                StartDate = filter.StartDate ?? DateTime.Today.AddDays(-30),
                EndDate = filter.EndDate ?? DateTime.Today.AddDays(30),
                Events = eventDtos.OrderBy(e => e.StartDate).ToList(),
                ViewType = filter.ViewType
            };
        }

        public async Task<List<CalendarEventDto>> GetUserEventsAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.CalendarEvents
                .Include(e => e.Task)
                .Include(e => e.Project)
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.CreatedByUser)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Include(e => e.Reminders)
                    .ThenInclude(r => r.User)
                .Where(e => e.CreatedByUserId == userId || 
                           e.Attendees.Any(a => a.UserId == userId));

            if (startDate.HasValue)
                query = query.Where(e => e.EndDate >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(e => e.StartDate <= endDate.Value);

            var events = await query
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDto).ToList();
        }

        public async Task<List<CalendarEventDto>> GetUpcomingEventsAsync(int userId, int days = 7)
        {
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(days);

            return await GetUserEventsAsync(userId, startDate, endDate);
        }

        public async Task<CalendarStatsDto> GetCalendarStatsAsync(int userId)
        {
            var userEvents = await GetUserEventsAsync(userId);
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            return new CalendarStatsDto
            {
                TotalEvents = userEvents.Count,
                UpcomingEvents = userEvents.Count(e => e.StartDate > now),
                OverdueEvents = userEvents.Count(e => e.EndDate < now && e.EventType == CalendarEventType.Deadline),
                TodayEvents = userEvents.Count(e => e.StartDate.Date == today),
                ThisWeekEvents = userEvents.Count(e => e.StartDate.Date >= weekStart && e.StartDate.Date < weekStart.AddDays(7)),
                ThisMonthEvents = userEvents.Count(e => e.StartDate.Date >= monthStart && e.StartDate.Date < monthStart.AddMonths(1)),
                EventsByType = userEvents.GroupBy(e => e.EventType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EventsByProject = userEvents.Where(e => !string.IsNullOrEmpty(e.ProjectName))
                    .GroupBy(e => e.ProjectName!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EventsByUser = userEvents.Where(e => !string.IsNullOrEmpty(e.UserName))
                    .GroupBy(e => e.UserName!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task<bool> UpdateAttendeeStatusAsync(int eventId, int attendeeId, UpdateAttendeeStatusDto dto, int currentUserId)
        {
            var attendee = await _context.CalendarEventAttendees
                .FirstOrDefaultAsync(a => a.CalendarEventId == eventId && a.Id == attendeeId && a.UserId == currentUserId);

            if (attendee == null)
                return false;

            attendee.Status = dto.Status;
            attendee.Notes = dto.Notes;
            attendee.ResponseDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("CalendarEventAttendee", attendee.Id, "UPDATE", 
                $"Updated attendee status to {dto.Status} for event {eventId}", currentUserId);

            return true;
        }

        public async Task<List<CalendarEventDto>> GenerateRecurringEventsAsync(int recurringEventId, DateTime startDate, DateTime endDate)
        {
            var recurringEvent = await _context.CalendarEvents
                .Include(e => e.Task)
                .Include(e => e.Project)
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.CreatedByUser)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Include(e => e.Reminders)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == recurringEventId && e.IsRecurring);

            if (recurringEvent == null)
                return new List<CalendarEventDto>();

            return await GenerateRecurringEventsForEvent(recurringEvent, startDate, endDate);
        }

        private Task<List<CalendarEventDto>> GenerateRecurringEventsForEvent(CalendarEvent recurringEvent, DateTime startDate, DateTime endDate)
        {
            var generatedEvents = new List<CalendarEventDto>();
            
            if (!recurringEvent.IsRecurring || !recurringEvent.RecurrencePattern.HasValue)
                return Task.FromResult(generatedEvents);

            var eventDuration = recurringEvent.EndDate - recurringEvent.StartDate;
            var currentDate = recurringEvent.StartDate;
            var interval = recurringEvent.RecurrenceInterval ?? 1;
            var recurrenceEndDate = recurringEvent.RecurrenceEndDate ?? endDate;

            while (currentDate <= recurrenceEndDate && currentDate <= endDate)
            {
                if (currentDate >= startDate)
                {
                    var generatedEvent = MapToDto(recurringEvent);
                    generatedEvent.Id = -Math.Abs(recurringEvent.Id * 1000 + (int)(currentDate - recurringEvent.StartDate).TotalDays);
                    generatedEvent.StartDate = currentDate;
                    generatedEvent.EndDate = currentDate + eventDuration;
                    generatedEvents.Add(generatedEvent);
                }

                // Calculate next occurrence
                currentDate = recurringEvent.RecurrencePattern switch
                {
                    RecurrencePattern.Daily => currentDate.AddDays(interval),
                    RecurrencePattern.Weekly => currentDate.AddDays(7 * interval),
                    RecurrencePattern.Monthly => currentDate.AddMonths(interval),
                    RecurrencePattern.Yearly => currentDate.AddYears(interval),
                    RecurrencePattern.Weekdays => GetNextWeekday(currentDate),
                    _ => currentDate.AddDays(1)
                };
            }

            return Task.FromResult(generatedEvents);
        }

        private DateTime GetNextWeekday(DateTime currentDate)
        {
            do
            {
                currentDate = currentDate.AddDays(1);
            }
            while (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday);
            
            return currentDate;
        }

        public async Task SendEventRemindersAsync()
        {
            var now = DateTime.UtcNow;
            var reminders = await _context.CalendarReminders
                .Include(r => r.CalendarEvent)
                .Include(r => r.User)
                .Where(r => r.IsActive && r.SentAt == null)
                .Where(r => r.CalendarEvent.StartDate <= now.AddMinutes(r.MinutesBefore) && 
                           r.CalendarEvent.StartDate > now)
                .ToListAsync();

            foreach (var reminder in reminders)
            {
                try
                {
                    if (reminder.Type == ReminderType.Email || reminder.Type == ReminderType.Both)
                    {
                        if (!string.IsNullOrEmpty(reminder.User.Email))
                        {
                            await _emailService.SendEmailAsync(
                                reminder.User.Email,
                                $"Reminder: {reminder.CalendarEvent.Title}",
                                $"Your event '{reminder.CalendarEvent.Title}' is starting at {reminder.CalendarEvent.StartDate:MM/dd/yyyy HH:mm}.");
                        }
                    }

                    if (reminder.Type == ReminderType.Popup || reminder.Type == ReminderType.Both)
                    {
                        await _realTimeService.SendToUserAsync(reminder.UserId, "calendar_reminder", new
                        {
                            EventId = reminder.CalendarEventId,
                            Title = reminder.CalendarEvent.Title,
                            StartDate = reminder.CalendarEvent.StartDate,
                            MinutesBefore = reminder.MinutesBefore
                        });
                    }

                    reminder.SentAt = now;
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other reminders
                    Console.WriteLine($"Failed to send reminder {reminder.Id}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<CalendarEventDto>> SearchEventsAsync(string query, int userId)
        {
            var events = await _context.CalendarEvents
                .Include(e => e.Task)
                .Include(e => e.Project)
                .Include(e => e.User)
                .Include(e => e.Department)
                .Include(e => e.CreatedByUser)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Include(e => e.Reminders)
                    .ThenInclude(r => r.User)
                .Where(e => e.CreatedByUserId == userId || 
                           e.Attendees.Any(a => a.UserId == userId))
                .Where(e => e.Title.Contains(query) || 
                           (e.Description != null && e.Description.Contains(query)))
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return events.Select(MapToDto).ToList();
        }

        public async Task SyncTaskDeadlinesAsync()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.DueDate.HasValue)
                .ToListAsync();

            foreach (var task in tasks)
            {
                var existingEvent = await _context.CalendarEvents
                    .FirstOrDefaultAsync(e => e.TaskId == task.TaskId && e.EventType == CalendarEventType.Deadline);

                if (existingEvent == null)
                {
                    var calendarEvent = new CalendarEvent
                    {
                        Title = $"Task Deadline: {task.Title}",
                        Description = $"Deadline for task: {task.Title}",
                        StartDate = task.DueDate!.Value.AddHours(-1), // 1 hour before deadline
                        EndDate = task.DueDate!.Value,
                        IsAllDay = false,
                        Color = "#dc3545", // Red for deadlines
                        EventType = CalendarEventType.Deadline,
                        TaskId = task.TaskId,
                        ProjectId = task.ProjectId,
                        UserId = task.AssignedTo,
                        CreatedByUserId = task.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.CalendarEvents.Add(calendarEvent);
                }
                else if (existingEvent.EndDate != task.DueDate)
                {
                    existingEvent.StartDate = task.DueDate!.Value.AddHours(-1);
                    existingEvent.EndDate = task.DueDate!.Value;
                    existingEvent.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task SyncProjectMilestonesAsync()
        {
            var milestones = await _context.ProjectMilestones
                .Include(m => m.Project)
                .ToListAsync();

            foreach (var milestone in milestones)
            {
                var existingEvent = await _context.CalendarEvents
                    .FirstOrDefaultAsync(e => e.ProjectId == milestone.ProjectId && 
                                            e.EventType == CalendarEventType.Milestone &&
                                            e.Title.Contains(milestone.Name));

                if (existingEvent == null)
                {
                    var calendarEvent = new CalendarEvent
                    {
                        Title = $"Milestone: {milestone.Name}",
                        Description = milestone.Description,
                        StartDate = milestone.DueDate,
                        EndDate = milestone.DueDate.AddHours(1),
                        IsAllDay = false,
                        Color = "#28a745", // Green for milestones
                        EventType = CalendarEventType.Milestone,
                        ProjectId = milestone.ProjectId,
                        CreatedByUserId = 1, // System user
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.CalendarEvents.Add(calendarEvent);
                }
                else if (existingEvent.StartDate != milestone.DueDate)
                {
                    existingEvent.StartDate = milestone.DueDate;
                    existingEvent.EndDate = milestone.DueDate.AddHours(1);
                    existingEvent.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        private CalendarEventDto MapToDto(CalendarEvent calendarEvent)
        {
            return new CalendarEventDto
            {
                Id = calendarEvent.Id,
                Title = calendarEvent.Title,
                Description = calendarEvent.Description,
                StartDate = calendarEvent.StartDate,
                EndDate = calendarEvent.EndDate,
                IsAllDay = calendarEvent.IsAllDay,
                Color = calendarEvent.Color,
                EventType = calendarEvent.EventType,
                TaskId = calendarEvent.TaskId,
                ProjectId = calendarEvent.ProjectId,
                UserId = calendarEvent.UserId,
                DepartmentId = calendarEvent.DepartmentId,
                IsRecurring = calendarEvent.IsRecurring,
                RecurrencePattern = calendarEvent.RecurrencePattern,
                RecurrenceInterval = calendarEvent.RecurrenceInterval,
                RecurrenceEndDate = calendarEvent.RecurrenceEndDate,
                RecurrenceDays = !string.IsNullOrEmpty(calendarEvent.RecurrenceDays) 
                    ? JsonSerializer.Deserialize<List<string>>(calendarEvent.RecurrenceDays) 
                    : null,
                CreatedAt = calendarEvent.CreatedAt,
                UpdatedAt = calendarEvent.UpdatedAt,
                TaskTitle = calendarEvent.Task?.Title,
                ProjectName = calendarEvent.Project?.ProjectName,
                UserName = calendarEvent.User?.Name,
                DepartmentName = calendarEvent.Department?.DeptName,
                CreatedByUserName = calendarEvent.CreatedByUser?.Name ?? string.Empty,
                Attendees = calendarEvent.Attendees?.Select(a => new CalendarEventAttendeeDto
                {
                    Id = a.Id,
                    CalendarEventId = a.CalendarEventId,
                    UserId = a.UserId,
                    UserName = a.User?.Name ?? string.Empty,
                    UserEmail = a.User?.Email ?? string.Empty,
                    Status = a.Status,
                    IsOrganizer = a.IsOrganizer,
                    ResponseDate = a.ResponseDate,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                }).ToList() ?? new List<CalendarEventAttendeeDto>(),
                Reminders = calendarEvent.Reminders?.Select(r => new CalendarReminderDto
                {
                    Id = r.Id,
                    CalendarEventId = r.CalendarEventId,
                    UserId = r.UserId,
                    UserName = r.User?.Name ?? string.Empty,
                    MinutesBefore = r.MinutesBefore,
                    Type = r.Type,
                    IsActive = r.IsActive,
                    SentAt = r.SentAt,
                    CreatedAt = r.CreatedAt
                }).ToList() ?? new List<CalendarReminderDto>()
            };
        }
    }
}