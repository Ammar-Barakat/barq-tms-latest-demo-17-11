using System.ComponentModel.DataAnnotations;
using BarqTMS.API.Models;

namespace BarqTMS.API.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public List<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        public List<int> DepartmentIds { get; set; } = new List<int>();
    }

    public class UpdateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Username { get; set; }
        
        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }
        
        public UserRole? Role { get; set; }
        public List<int> DepartmentIds { get; set; } = new List<int>();
    }
}