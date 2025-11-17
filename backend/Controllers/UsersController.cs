using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(BarqTMSDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserDepartments)
                    .ThenInclude(ud => ud.Department)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    Departments = u.UserDepartments.Select(ud => new DepartmentDto
                    {
                        DeptId = ud.Department.DeptId,
                        DeptName = ud.Department.DeptName
                    }).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserDepartments)
                    .ThenInclude(ud => ud.Department)
                .Where(u => u.UserId == id)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    Departments = u.UserDepartments.Select(ud => new DepartmentDto
                    {
                        DeptId = ud.Department.DeptId,
                        DeptName = ud.Department.DeptName
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }

        // POST: api/users
        // Only Manager can create new users
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == createUserDto.Username))
            {
                return BadRequest("A user with this username already exists.");
            }

            // Check if email already exists (only if email is provided)
            if (!string.IsNullOrEmpty(createUserDto.Email) && 
                await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                return BadRequest("A user with this email already exists.");
            }

            // Validate departments exist
            var departments = await _context.Departments
                .Where(d => createUserDto.DepartmentIds.Contains(d.DeptId))
                .ToListAsync();
            
            if (departments.Count != createUserDto.DepartmentIds.Count)
            {
                return BadRequest("One or more department IDs are invalid.");
            }

            var user = new User
            {
                Name = createUserDto.Name,
                Username = createUserDto.Username,
                Email = createUserDto.Email,
                Role = createUserDto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("tempPassword123") // Default password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Add user departments
            foreach (var deptId in createUserDto.DepartmentIds)
            {
                _context.UserDepartments.Add(new UserDepartment
                {
                    UserId = user.UserId,
                    DeptId = deptId
                });
            }

            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Departments = departments.Select(d => new DepartmentDto
                {
                    DeptId = d.DeptId,
                    DeptName = d.DeptName
                }).ToList()
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
        }

        // PUT: api/users/5
        // Manager can update anyone, Assistant Manager can update limited roles
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,AssistantManager")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users
                .Include(u => u.UserDepartments)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            // Check if username already exists for another user (if username is being updated)
            if (!string.IsNullOrEmpty(updateUserDto.Username) && 
                await _context.Users.AnyAsync(u => u.Username == updateUserDto.Username && u.UserId != id))
            {
                return BadRequest("A user with this username already exists.");
            }

            // Check if email already exists for another user (only if email is provided)
            if (!string.IsNullOrEmpty(updateUserDto.Email) && 
                await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.UserId != id))
            {
                return BadRequest("A user with this email already exists.");
            }

            // Update basic properties
            user.Name = updateUserDto.Name;
            if (!string.IsNullOrEmpty(updateUserDto.Username))
                user.Username = updateUserDto.Username;
            user.Email = updateUserDto.Email;

            // Update role if provided
            if (updateUserDto.Role.HasValue)
            {
                user.Role = updateUserDto.Role.Value;
            }

            // Update departments
            if (updateUserDto.DepartmentIds.Any())
            {
                // Validate departments exist
                var departments = await _context.Departments
                    .Where(d => updateUserDto.DepartmentIds.Contains(d.DeptId))
                    .ToListAsync();
                
                if (departments.Count != updateUserDto.DepartmentIds.Count)
                {
                    return BadRequest("One or more department IDs are invalid.");
                }

                // Remove existing departments
                _context.UserDepartments.RemoveRange(user.UserDepartments);

                // Add new departments
                foreach (var deptId in updateUserDto.DepartmentIds)
                {
                    _context.UserDepartments.Add(new UserDepartment
                    {
                        UserId = user.UserId,
                        DeptId = deptId
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/users/5
        // Only Manager can delete users
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/users/5/departments
        [HttpGet("{id}/departments")]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetUserDepartments(int id)
        {
            if (!await UserExists(id))
            {
                return NotFound($"User with ID {id} not found.");
            }

            var departments = await _context.UserDepartments
                .Where(ud => ud.UserId == id)
                .Include(ud => ud.Department)
                .Select(ud => new DepartmentDto
                {
                    DeptId = ud.Department.DeptId,
                    DeptName = ud.Department.DeptName,
                    UserCount = ud.Department.UserDepartments.Count(),
                    TaskCount = ud.Department.Tasks.Count()
                })
                .ToListAsync();

            return Ok(departments);
        }

        // GET: api/users/5/tasks
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetUserTasks(int id)
        {
            if (!await UserExists(id))
            {
                return NotFound($"User with ID {id} not found.");
            }

            var tasks = await _context.Tasks
                .Where(t => t.AssignedTo == id)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    PriorityId = t.PriorityId,
                    PriorityLevel = t.Priority.Level,
                    StatusId = t.StatusId,
                    StatusName = t.Status.StatusName,
                    DueDate = t.DueDate,
                    CreatedBy = t.CreatedBy,
                    CreatedByName = t.Creator.Name,
                    AssignedTo = t.AssignedTo,
                    AssignedToName = t.AssignedUser != null ? t.AssignedUser.Name : null,
                    DeptId = t.DeptId,
                    DeptName = t.Department.DeptName,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.ProjectName,
                    CommentCount = t.TaskComments.Count(),
                    AttachmentCount = t.Attachments.Count()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }
    }
}