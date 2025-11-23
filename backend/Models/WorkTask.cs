using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BarqTMS.API.Models.Enums;

namespace BarqTMS.API.Models
{
    [Table("Tasks")]
    public class WorkTask
    {
        [Key]
        public int TaskId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int DepartmentId { get; set; }

        public DateTime? DueDate { get; set; }

        public TimeSpan? SpecificTime { get; set; }

        public BarqTMS.API.Models.Enums.TaskStatus Status { get; set; }

        public TaskPriority Priority { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? EstimatedHours { get; set; }

        [Required]
        [StringLength(500)]
        public string DriveFolderLink { get; set; } = string.Empty;

        [StringLength(500)]
        public string? MaterialDriveFolderLink { get; set; }

        [StringLength(500)]
        public string? Tags { get; set; }

        public int? DelegatedBy { get; set; }

        public int? OriginalAssignerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        [ForeignKey("DelegatedBy")]
        public virtual User? Delegator { get; set; }

        [ForeignKey("OriginalAssignerId")]
        public virtual User? OriginalAssigner { get; set; }

        public virtual ICollection<TaskAssignee> Assignees { get; set; } = new List<TaskAssignee>();
        public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
