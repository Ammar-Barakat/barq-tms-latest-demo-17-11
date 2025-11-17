using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("TASK_DEPENDENCY")]
    public class TaskDependency
    {
        [Key]
        [Column("dependency_id")]
        public int DependencyId { get; set; }

        [Column("task_id")]
        public int TaskId { get; set; }

        [Column("prerequisite_task_id")]
        public int PrerequisiteTaskId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
    [ForeignKey("TaskId")]
    public virtual WorkTask Task { get; set; } = null!;

    [ForeignKey("PrerequisiteTaskId")]
    public virtual WorkTask PrerequisiteTask { get; set; } = null!;
    }
}