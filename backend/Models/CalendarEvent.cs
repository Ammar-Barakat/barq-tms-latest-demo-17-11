using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("CALENDAR_EVENT")]
    public class CalendarEvent
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        public bool IsAllDay { get; set; }
        
        [StringLength(7)] // For hex color codes like #FF5733
        public string Color { get; set; } = "#007bff";
        
        public CalendarEventType EventType { get; set; }
        
        // Foreign Keys
        public int? TaskId { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public int? DepartmentId { get; set; }
        
        // Navigation Properties
        [ForeignKey("TaskId")]
        public virtual WorkTask? Task { get; set; }
        
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
        
        // Recurrence properties
        public bool IsRecurring { get; set; }
        public RecurrencePattern? RecurrencePattern { get; set; }
        public int? RecurrenceInterval { get; set; } // Every X days/weeks/months
        public DateTime? RecurrenceEndDate { get; set; }
        public string? RecurrenceDays { get; set; } // JSON array for days of week
        
        // Metadata
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; } = null!;
        
        // Calendar event attendees
        public virtual ICollection<CalendarEventAttendee> Attendees { get; set; } = new List<CalendarEventAttendee>();
        
        // Reminders
        public virtual ICollection<CalendarReminder> Reminders { get; set; } = new List<CalendarReminder>();
    }
    
    public enum CalendarEventType
    {
        Task = 1,
        Meeting = 2,
        Deadline = 3,
        Milestone = 4,
        Holiday = 5,
        Personal = 6,
        ProjectStart = 7,
        ProjectEnd = 8,
        Custom = 9
    }
    
    public enum RecurrencePattern
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4,
        Weekdays = 5, // Monday to Friday
        Custom = 6
    }
}