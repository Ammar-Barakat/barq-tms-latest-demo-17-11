using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("TASK_CATEGORY")]
    public class TaskCategory
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("color")]
        [StringLength(7)]
        public string? Color { get; set; } // Hex color code

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    }
}