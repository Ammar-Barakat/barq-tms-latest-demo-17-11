using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("ATTACHMENT")]
    public class Attachment
    {
        [Key]
        [Column("file_id")]
        public int FileId { get; set; }

        [Column("task_id")]
        public int TaskId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("file_url")]
        public string FileUrl { get; set; } = string.Empty;

        [Column("uploaded_by")]
        public int UploadedBy { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TaskId")]
        public virtual WorkTask Task { get; set; } = null!;
        
        [ForeignKey("UploadedBy")]
        public virtual User UploadedByUser { get; set; } = null!;
    }
}