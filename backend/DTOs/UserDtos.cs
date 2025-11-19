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
        public string? Position { get; set; }
        public UserRole Role { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int? TeamLeaderId { get; set; }
        public string? TeamLeaderName { get; set; }
        public List<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
        public List<int> ManagedEmployeeIds { get; set; } = new List<int>();
        public List<int> ManagedClientIds { get; set; } = new List<int>();
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
        
        [StringLength(100)]
        public string? Position { get; set; }
        
        [Required]
        public UserRole Role { get; set; }
        
        public int? TeamLeaderId { get; set; }
        
        public List<int> DepartmentIds { get; set; } = new List<int>();
        
        // For Team Leader - assign employees
        public List<int> ManagedEmployeeIds { get; set; } = new List<int>();
        
        // For Account Manager - assign clients
        public List<int> ManagedClientIds { get; set; } = new List<int>();
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
        
        [StringLength(100)]
        public string? Position { get; set; }
        
        public UserRole? Role { get; set; }
        
        public int? TeamLeaderId { get; set; }
        
        public List<int> DepartmentIds { get; set; } = new List<int>();
        
        // For Team Leader - assign employees
        public List<int>? ManagedEmployeeIds { get; set; }
        
        // For Account Manager - assign clients
        public List<int>? ManagedClientIds { get; set; }
    }
}