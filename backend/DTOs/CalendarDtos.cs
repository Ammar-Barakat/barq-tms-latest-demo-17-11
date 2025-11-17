using BarqTMS.API.Models;

namespace BarqTMS.API.DTOs
{
    // Calendar Event DTOs
    public class CalendarEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public string Color { get; set; } = "#007bff";
        public CalendarEventType EventType { get; set; }
        public int? TaskId { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public bool IsRecurring { get; set; }
        public RecurrencePattern? RecurrencePattern { get; set; }
        public int? RecurrenceInterval { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public List<string>? RecurrenceDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Related data
        public string? TaskTitle { get; set; }
        public string? ProjectName { get; set; }
        public string? UserName { get; set; }
        public string? DepartmentName { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        
        public List<CalendarEventAttendeeDto> Attendees { get; set; } = new();
        public List<CalendarReminderDto> Reminders { get; set; } = new();
    }
    
    public class CreateCalendarEventDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsAllDay { get; set; }
        public string Color { get; set; } = "#007bff";
        public CalendarEventType EventType { get; set; }
        public int? TaskId { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public bool IsRecurring { get; set; }
        public RecurrencePattern? RecurrencePattern { get; set; }
        public int? RecurrenceInterval { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public List<string>? RecurrenceDays { get; set; }
        
        public List<int> AttendeeUserIds { get; set; } = new();
        public List<CreateCalendarReminderDto> Reminders { get; set; } = new();
    }
    
    public class UpdateCalendarEventDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsAllDay { get; set; }
        public string? Color { get; set; }
        public CalendarEventType? EventType { get; set; }
        public int? TaskId { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        public bool? IsRecurring { get; set; }
        public RecurrencePattern? RecurrencePattern { get; set; }
        public int? RecurrenceInterval { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public List<string>? RecurrenceDays { get; set; }
        
        public List<int>? AttendeeUserIds { get; set; }
        public List<CreateCalendarReminderDto>? Reminders { get; set; }
    }
    
    // Calendar Event Attendee DTOs
    public class CalendarEventAttendeeDto
    {
        public int Id { get; set; }
        public int CalendarEventId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public AttendeeStatus Status { get; set; }
        public bool IsOrganizer { get; set; }
        public DateTime? ResponseDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class UpdateAttendeeStatusDto
    {
        public AttendeeStatus Status { get; set; }
        public string? Notes { get; set; }
    }
    
    // Calendar Reminder DTOs
    public class CalendarReminderDto
    {
        public int Id { get; set; }
        public int CalendarEventId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int MinutesBefore { get; set; }
        public ReminderType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class CreateCalendarReminderDto
    {
        public int UserId { get; set; }
        public int MinutesBefore { get; set; }
        public ReminderType Type { get; set; } = ReminderType.Popup;
    }
    
    // Calendar View DTOs
    public class CalendarViewDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CalendarEventDto> Events { get; set; } = new();
        public CalendarViewType ViewType { get; set; }
    }
    
    public class CalendarFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<CalendarEventType>? EventTypes { get; set; }
        public List<int>? ProjectIds { get; set; }
        public List<int>? UserIds { get; set; }
        public List<int>? DepartmentIds { get; set; }
        public bool IncludeRecurring { get; set; } = true;
        public CalendarViewType ViewType { get; set; } = CalendarViewType.Month;
    }
    
    public enum CalendarViewType
    {
        Day = 1,
        Week = 2,
        Month = 3,
        Year = 4,
        Agenda = 5
    }
    
    // Calendar Statistics DTOs
    public class CalendarStatsDto
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int OverdueEvents { get; set; }
        public int TodayEvents { get; set; }
        public int ThisWeekEvents { get; set; }
        public int ThisMonthEvents { get; set; }
        public Dictionary<CalendarEventType, int> EventsByType { get; set; } = new();
        public Dictionary<string, int> EventsByProject { get; set; } = new();
        public Dictionary<string, int> EventsByUser { get; set; } = new();
    }
}