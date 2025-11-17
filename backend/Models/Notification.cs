using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("NOTIFICATION")]
    public class Notification
    {
        [Key]
        [Column("notif_id")]
        public int NotifId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("task_id")]
        public int? TaskId { get; set; }

        [Column("project_id")]
        public int? ProjectId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        [ForeignKey("TaskId")]
        public virtual WorkTask? Task { get; set; }
        
        [ForeignKey("ProjectId")]
        public virtual Project? Project { get; set; }
    }
}