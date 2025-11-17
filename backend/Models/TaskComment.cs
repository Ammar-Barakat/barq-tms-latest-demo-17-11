using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("TASK_COMMENT")]
    public class TaskComment
    {
        [Key]
        [Column("comment_id")]
        public int CommentId { get; set; }
        
        [Column("task_id")]
        public int TaskId { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(1000)]
        [Column("comment")]
        public string Comment { get; set; } = string.Empty;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("TaskId")]
        public virtual WorkTask Task { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}