using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("CALENDAR_REMINDER")]
    public class CalendarReminder
    {
        public int Id { get; set; }
        
        [Required]
        public int CalendarEventId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int MinutesBefore { get; set; } // Minutes before event to remind
        
        public ReminderType Type { get; set; } = ReminderType.Popup;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? SentAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation Properties
        [ForeignKey("CalendarEventId")]
        public virtual CalendarEvent CalendarEvent { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
    
    public enum ReminderType
    {
        Popup = 1,
        Email = 2,
        Both = 3
    }
}