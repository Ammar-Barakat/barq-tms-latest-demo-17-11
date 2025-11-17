using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
        [Table("TASK")]
        public class WorkTask
    {
    [Key]
    [Column("task_id")]
    public int TaskId { get; set; }

    [Required]
    [StringLength(200)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("priority_id")]
    public int PriorityId { get; set; }

    [Column("status_id")]
    public int StatusId { get; set; }

    [Column("due_date")]
    public DateTime? DueDate { get; set; }

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("assigned_to")]
    public int? AssignedTo { get; set; }

    [Column("dept_id")]
    public int DeptId { get; set; }

    [Column("project_id")]
        public int ProjectId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Column("estimated_hours")]
        public decimal? EstimatedHours { get; set; }

        [Column("actual_hours")]
        public decimal? ActualHours { get; set; }

        [Column("tags")]
        [StringLength(500)]
        public string? Tags { get; set; } // Comma-separated tags

        [Column("is_recurring")]
        public bool IsRecurring { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PriorityId")]
        public virtual Priority Priority { get; set; } = null!;
        
        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; } = null!;
        
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; } = null!;
        
        [ForeignKey("AssignedTo")]
        public virtual User? AssignedUser { get; set; }
        
        [ForeignKey("DeptId")]
        public virtual Department Department { get; set; } = null!;
        
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
        
        [ForeignKey("CategoryId")]
        public virtual TaskCategory? Category { get; set; }
        
        public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
        public virtual ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
        public virtual ICollection<TaskDependency> PrerequisiteFor { get; set; } = new List<TaskDependency>();
    }
}