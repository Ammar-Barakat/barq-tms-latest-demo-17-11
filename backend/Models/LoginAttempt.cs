using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("LOGIN_ATTEMPT")]
    public class LoginAttempt
    {
        [Key]
        [Column("attempt_id")]
        public int AttemptId { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Column("ip_address")]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [Column("user_agent")]
        [StringLength(500)]
        public string? UserAgent { get; set; }

        [Column("was_successful")]
        public bool WasSuccessful { get; set; }

        [Column("attempted_at")]
        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        [Column("failure_reason")]
        [StringLength(200)]
        public string? FailureReason { get; set; }
    }
}