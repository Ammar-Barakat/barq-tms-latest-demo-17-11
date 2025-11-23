using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BarqTMS.API.Models.Enums;

namespace BarqTMS.API.Models
{
    public class UserChangeRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int UserId { get; set; }

        public ChangeRequestType RequestType { get; set; }

        public string? NewData { get; set; } // JSON string

        public string? NewPasswordHash { get; set; }

        public ChangeRequestStatus Status { get; set; } = ChangeRequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User Requester { get; set; } = null!;

        [ForeignKey("ReviewedBy")]
        public virtual User? Reviewer { get; set; }
    }
}
