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
    public class DepartmentsController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(BarqTMSDbContext context, ILogger<DepartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new DepartmentDto
                {
                    DeptId = d.DeptId,
                    DeptName = d.DeptName,
                    UserCount = d.UserDepartments.Count(),
                    TaskCount = d.Tasks.Count()
                })
                .ToListAsync();

            return Ok(departments);
        }

        // GET: api/departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var department = await _context.Departments
                .Where(d => d.DeptId == id)
                .Select(d => new DepartmentDto
                {
                    DeptId = d.DeptId,
                    DeptName = d.DeptName,
                    UserCount = d.UserDepartments.Count(),
                    TaskCount = d.Tasks.Count()
                })
                .FirstOrDefaultAsync();

            if (department == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }

            return Ok(department);
        }

        // POST: api/departments
        // Only Manager can create departments
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentDto createDepartmentDto)
        {
            // Check if department name already exists
            if (await _context.Departments.AnyAsync(d => d.DeptName.ToLower() == createDepartmentDto.DeptName.ToLower()))
            {
                return BadRequest("A department with this name already exists.");
            }

            var department = new Department
            {
                DeptName = createDepartmentDto.DeptName
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            var departmentDto = new DepartmentDto
            {
                DeptId = department.DeptId,
                DeptName = department.DeptName,
                UserCount = 0,
                TaskCount = 0
            };

            return CreatedAtAction(nameof(GetDepartment), new { id = department.DeptId }, departmentDto);
        }

        // PUT: api/departments/5
        // Only Manager can update departments
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto updateDepartmentDto)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }

            // Check if department name already exists for another department
            if (await _context.Departments.AnyAsync(d => d.DeptName.ToLower() == updateDepartmentDto.DeptName.ToLower() && d.DeptId != id))
            {
                return BadRequest("A department with this name already exists.");
            }

            department.DeptName = updateDepartmentDto.DeptName;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DepartmentExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/departments/5
        // Only Manager can delete departments
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound($"Department with ID {id} not found.");
            }

            // Check if department has users
            var hasUsers = await _context.UserDepartments.AnyAsync(ud => ud.DeptId == id);
            if (hasUsers)
            {
                return BadRequest("Cannot delete department because it has assigned users.");
            }

            // Check if department has tasks
            var hasTasks = await _context.Tasks.AnyAsync(t => t.DeptId == id);
            if (hasTasks)
            {
                return BadRequest("Cannot delete department because it has tasks.");
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/departments/5/tasks
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetDepartmentTasks(int id)
        {
            if (!await DepartmentExists(id))
            {
                return NotFound($"Department with ID {id} not found.");
            }

            var tasks = await _context.Tasks
                .Where(t => t.DeptId == id)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
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

        // GET: api/departments/5/projects
        [HttpGet("{id}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetDepartmentProjects(int id)
        {
            if (!await DepartmentExists(id))
            {
                return NotFound($"Department with ID {id} not found.");
            }

            // Note: Projects are linked to clients, not departments directly
            // So we'll find projects that have tasks in this department
            var projects = await _context.Projects
                .Where(p => p.Tasks.Any(t => t.DeptId == id))
                .Include(p => p.Client)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    ClientId = p.ClientId,
                    ClientName = p.Client.Name,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TaskCount = p.Tasks.Count()
                })
                .Distinct()
                .ToListAsync();

            return Ok(projects);
        }

        // GET: api/departments/5/users
        [HttpGet("{id}/users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetDepartmentUsers(int id)
        {
            if (!await DepartmentExists(id))
            {
                return NotFound($"Department with ID {id} not found.");
            }

            var users = await _context.UserDepartments
                .Where(ud => ud.DeptId == id)
                .Include(ud => ud.User)
                .Select(ud => new UserDto
                {
                    UserId = ud.User.UserId,
                    Name = ud.User.Name,
                    Email = ud.User.Email,
                    Role = ud.User.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        private async Task<bool> DepartmentExists(int id)
        {
            return await _context.Departments.AnyAsync(e => e.DeptId == id);
        }
    }
}