using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("AUDIT_LOG")]
    public class AuditLog
    {
        [Key]
        [Column("audit_id")]
        public int AuditId { get; set; }

        [Column("entity_type")]
        [StringLength(50)]
        public string EntityType { get; set; } = string.Empty; // Task, Project, User, Department, etc.

        [Column("entity_id")]
        public int? EntityId { get; set; }

        [Column("action")]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, Assigned, etc.

        [Column("changes")]
        [StringLength(2000)]
        public string? Changes { get; set; } // JSON string of what changed

        [Column("old_values")]
        [StringLength(2000)]
        public string? OldValues { get; set; } // JSON string of old values

        [Column("new_values")]
        [StringLength(2000)]
        public string? NewValues { get; set; } // JSON string of new values

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Column("ip_address")]
        [StringLength(45)]
        public string? IpAddress { get; set; }

        [Column("user_agent")]
        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Column("table_name")]
        [StringLength(100)]
        public string? TableName { get; set; }

        [Column("description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}