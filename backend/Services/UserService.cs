using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using BarqTMS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BarqTMS.API.Services
{
    public class UserService : IUserService
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(BarqTMSDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Supervisor)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .Include(u => u.Supervisor)
                .FirstOrDefaultAsync(u => u.UserId == id);

            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (!string.IsNullOrEmpty(createUserDto.Email) && await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                throw new InvalidOperationException("Email already exists.");
            }

            var user = new User
            {
                Username = createUserDto.Username,
                FullName = createUserDto.Name,
                Email = createUserDto.Email ?? string.Empty,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                Role = createUserDto.Role,
                DepartmentId = createUserDto.DepartmentIds.Any() ? createUserDto.DepartmentIds.First() : null,
                SupervisorId = createUserDto.TeamLeaderId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Reload to get included properties
            return (await GetUserByIdAsync(user.UserId))!;
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            user.FullName = updateUserDto.Name;
            if (!string.IsNullOrEmpty(updateUserDto.Username)) user.Username = updateUserDto.Username;
            if (!string.IsNullOrEmpty(updateUserDto.Email)) user.Email = updateUserDto.Email;
            if (updateUserDto.Role.HasValue) user.Role = updateUserDto.Role.Value;
            
            // Update Department if provided
            if (updateUserDto.DepartmentIds.Any())
            {
                user.DepartmentId = updateUserDto.DepartmentIds.First();
            }

            if (updateUserDto.TeamLeaderId.HasValue)
            {
                user.SupervisorId = updateUserDto.TeamLeaderId.Value;
            }

            await _context.SaveChangesAsync();

            return (await GetUserByIdAsync(user.UserId))!;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Name = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                RoleId = (int)user.Role,
                RoleName = user.Role.ToString(),
                TeamLeaderId = user.SupervisorId,
                TeamLeaderName = user.Supervisor?.FullName,
                Departments = user.Department != null 
                    ? new List<DepartmentDto> 
                    { 
                        new DepartmentDto 
                        { 
                            DeptId = user.Department.DeptId, 
                            DeptName = user.Department.Name 
                        } 
                    } 
                    : new List<DepartmentDto>()
            };
        }
    }
}
