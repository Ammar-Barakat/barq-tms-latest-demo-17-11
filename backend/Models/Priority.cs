using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("PRIORITY")]
    public class Priority
    {
        [Key]
        [Column("priority_id")]
        public int PriorityId { get; set; }
        
        [Required]
        [StringLength(50)]
        [Column("level")]
        public string Level { get; set; } = string.Empty; // Low, Medium, High, Critical
        
        // Navigation properties
        public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    }
}