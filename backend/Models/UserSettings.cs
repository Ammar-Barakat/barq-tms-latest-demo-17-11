using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("USER_SETTINGS")]
    public class UserSettings
    {
        [Key]
        [Column("setting_id")]
        public int SettingId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("theme")]
        [StringLength(20)]
        public string Theme { get; set; } = "light";

        [Column("language")]
        [StringLength(10)]
        public string Language { get; set; } = "en";

        [Column("timezone")]
        [StringLength(50)]
        public string Timezone { get; set; } = "UTC";

        [Column("email_notifications")]
        public bool EmailNotifications { get; set; } = true;

        [Column("push_notifications")]
        public bool PushNotifications { get; set; } = true;

        [Column("task_reminders")]
        public bool TaskReminders { get; set; } = true;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}