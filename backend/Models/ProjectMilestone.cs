using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("PROJECT_MILESTONE")]
    public class ProjectMilestone
    {
        [Key]
        [Column("milestone_id")]
        public int MilestoneId { get; set; }

        [Column("project_id")]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [Column("due_date")]
        public DateTime DueDate { get; set; }

        [Column("completion_date")]
        public DateTime? CompletionDate { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;
    }
}