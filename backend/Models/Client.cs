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
        
        // Navigation properties
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}