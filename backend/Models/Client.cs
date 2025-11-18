using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarqTMS.API.Models
{
    [Table("CLIENT")]
    public class Client
    {
        [Key]
        [Column("client_id")]
        public int ClientId { get; set; }
        
        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(50)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(255)]
        [Column("company")]
        public string? Company { get; set; }
        
        [StringLength(500)]
        [Column("address")]
        public string? Address { get; set; }
        
        [Required]
        [Column("account_manager_id")]
        public int AccountManagerId { get; set; }
        
        [ForeignKey(nameof(AccountManagerId))]
        public virtual User AccountManager { get; set; } = null!;
        
        // Navigation properties
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}