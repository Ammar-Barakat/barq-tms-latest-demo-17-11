using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("CALENDAR_EVENT_ATTENDEE")]
    public class CalendarEventAttendee
    {
        public int Id { get; set; }
        
        [Required]
        public int CalendarEventId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public AttendeeStatus Status { get; set; } = AttendeeStatus.Pending;
        
        public bool IsOrganizer { get; set; }
        
        public DateTime? ResponseDate { get; set; }
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation Properties
        [ForeignKey("CalendarEventId")]
        public virtual CalendarEvent CalendarEvent { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
    
    public enum AttendeeStatus
    {
        Pending = 1,
        Accepted = 2,
        Declined = 3,
        Tentative = 4,
        NoResponse = 5
    }
}