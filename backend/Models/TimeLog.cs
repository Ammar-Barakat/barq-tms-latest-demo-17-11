using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("TIME_LOG")]
    public class TimeLog
    {
        [Key]
        [Column("time_log_id")]
        public int TimeLogId { get; set; }

        [Column("task_id")]
        public int TaskId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("duration_minutes")]
        public int? DurationMinutes { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("is_billable")]
        public bool IsBillable { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TaskId")]
        public virtual WorkTask Task { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}