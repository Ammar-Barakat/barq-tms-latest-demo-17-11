using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("STATUS")]
    public class Status
    {
        [Key]
        [Column("status_id")]
        public int StatusId { get; set; }
        
        [Required]
        [StringLength(50)]
        [Column("status_name")]
        public string StatusName { get; set; } = string.Empty; // To Do, In Progress, Review, Done
        
        // Navigation properties
        public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
    }
}