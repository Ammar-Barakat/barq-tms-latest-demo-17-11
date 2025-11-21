using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    public enum UserRole
    {
        Manager = 1,
        AssistantManager = 2,
        AccountManager = 3,
        TeamLeader = 4,
        Employee = 5,
        Client = 6
    }

    [Table("USER")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;
        
        [EmailAddress]
        [StringLength(255)]
        [Column("email")]
        public string? Email { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [Column("role")]
        public UserRole Role { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [Column("last_login")]
        public DateTime? LastLogin { get; set; }
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [StringLength(100)]
        [Column("position")]
        public string? Position { get; set; }
        
        [Column("team_leader_id")]
        public int? TeamLeaderId { get; set; }
        
        [Column("client_id")]
        public int? ClientId { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(TeamLeaderId))]
        public virtual User? TeamLeader { get; set; }
        
        [ForeignKey(nameof(ClientId))]
        public virtual Client? Client { get; set; }
        
        public virtual ICollection<User> ManagedEmployees { get; set; } = new List<User>(); // Employees under this Team Leader
        public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
        public virtual ICollection<WorkTask> AssignedTasks { get; set; } = new List<WorkTask>();
        public virtual ICollection<WorkTask> CreatedTasks { get; set; } = new List<WorkTask>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
        public virtual ICollection<Attachment> UploadedAttachments { get; set; } = new List<Attachment>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
        public virtual UserSettings? Settings { get; set; }
        public virtual ICollection<Client> ManagedClients { get; set; } = new List<Client>(); // Clients for Account Manager
    }
}