using System.ComponentModel.DataAnnotations;

namespace BarqTMS.API.DTOs
{
    public class CreateClientDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Company { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Required]
        public int AccountManagerId { get; set; }
    }

    public class UpdateClientDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Company { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Required]
        public int AccountManagerId { get; set; }
    }
}
