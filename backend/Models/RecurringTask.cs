using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("RECURRING_TASK")]
    public class RecurringTask
    {
        [Key]
        [Column("recurring_id")]
        public int RecurringId { get; set; }

        [Column("template_task_id")]
        public int TemplateTaskId { get; set; }

        [Column("frequency_type")]
        [StringLength(20)]
        public string FrequencyType { get; set; } = string.Empty; // Daily, Weekly, Monthly, Yearly

        [Column("frequency_interval")]
        public int FrequencyInterval { get; set; } = 1; // Every X days/weeks/months

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("last_generated")]
        public DateTime? LastGenerated { get; set; }

        [Column("next_due_date")]
        public DateTime NextDueDate { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TemplateTaskId")]
        public virtual WorkTask TemplateTask { get; set; } = null!;
    }
}