using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("PROJECT")]
    public class Project
    {
        [Key]
        [Column("project_id")]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        [Column("project_name")]
        public string ProjectName { get; set; } = string.Empty;

        [StringLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("client_id")]
        public int ClientId { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        // Navigation properties
        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; } = null!;
        
        public virtual ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    }
}