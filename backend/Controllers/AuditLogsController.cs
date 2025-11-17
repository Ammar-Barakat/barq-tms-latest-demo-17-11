using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Data;
using BarqTMS.API.DTOs;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(BarqTMSDbContext context, ILogger<AuditLogsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/auditlogs
        [HttpGet]
        public async Task<ActionResult> GetAllAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var skip = (page - 1) * pageSize;

            var projectHistories = await _context.AuditLogs
                .Where(al => al.EntityType == "Project")
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(al => new
                {
                    Type = "Project",
                    HistoryId = al.AuditId,
                    EntityId = al.EntityId,
                    EntityName = al.EntityType,
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            var taskHistories = await _context.AuditLogs
                .Where(al => al.EntityType == "Task")
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(al => new
                {
                    Type = "Task",
                    HistoryId = al.AuditId,
                    EntityId = al.EntityId,
                    EntityName = al.EntityType,
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            var combinedLogs = projectHistories.Concat(taskHistories)
                .OrderByDescending(log => log.ActionDate)
                .Take(pageSize)
                .ToList();

            var totalProjectLogs = await _context.AuditLogs.Where(al => al.EntityType == "Project").CountAsync();
            var totalTaskLogs = await _context.AuditLogs.Where(al => al.EntityType == "Task").CountAsync();
            var totalLogs = totalProjectLogs + totalTaskLogs;

            return Ok(new
            {
                Logs = combinedLogs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalLogs = totalLogs,
                    TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize)
                }
            });
        }

        // GET: api/auditlogs/project/5
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ProjectHistoryDto>>> GetProjectAuditLogs(int projectId)
        {
            if (!await ProjectExists(projectId))
            {
                return NotFound($"Project with ID {projectId} not found.");
            }

            var auditLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Project" && al.EntityId == projectId)
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Select(al => new ProjectHistoryDto
                {
                    HistoryId = al.AuditId,
                    ProjectId = al.EntityId ?? 0,
                    ProjectName = "Project", // We can't get project name from audit log directly
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            return Ok(auditLogs);
        }

        // GET: api/auditlogs/task/5
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetTaskAuditLogs(int taskId)
        {
            if (!await TaskExists(taskId))
            {
                return NotFound($"Task with ID {taskId} not found.");
            }

            var auditLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Task" && al.EntityId == taskId)
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Select(al => new TaskHistoryDto
                {
                    HistoryId = al.AuditId,
                    TaskId = al.EntityId ?? 0,
                    TaskTitle = "Task", // We can't get task title from audit log directly
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            return Ok(auditLogs);
        }

        // GET: api/auditlogs/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult> GetUserAuditLogs(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!await UserExists(userId))
            {
                return NotFound($"User with ID {userId} not found.");
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var skip = (page - 1) * pageSize;

            var projectLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Project" && al.UserId == userId)
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(al => new
                {
                    Type = "Project",
                    HistoryId = al.AuditId,
                    EntityId = al.EntityId,
                    EntityName = al.EntityType,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            var taskLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Task" && al.UserId == userId)
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(al => new
                {
                    Type = "Task",
                    HistoryId = al.AuditId,
                    EntityId = al.EntityId,
                    EntityName = al.EntityType,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            var combinedLogs = projectLogs.Concat(taskLogs)
                .OrderByDescending(log => log.ActionDate)
                .Take(pageSize)
                .ToList();

            var totalProjectLogs = await _context.AuditLogs.Where(al => al.EntityType == "Project" && al.UserId == userId).CountAsync();
            var totalTaskLogs = await _context.AuditLogs.Where(al => al.EntityType == "Task" && al.UserId == userId).CountAsync();
            var totalLogs = totalProjectLogs + totalTaskLogs;

            return Ok(new
            {
                Logs = combinedLogs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalLogs = totalLogs,
                    TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize)
                }
            });
        }

        // GET: api/auditlogs/department/5
        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult> GetDepartmentAuditLogs(int departmentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!await DepartmentExists(departmentId))
            {
                return NotFound($"Department with ID {departmentId} not found.");
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var skip = (page - 1) * pageSize;

            // Get task audit logs for tasks in this department - we need to join with Tasks
            var taskIds = await _context.Tasks.Where(t => t.DeptId == departmentId).Select(t => t.TaskId).ToListAsync();
            var taskLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Task" && taskIds.Contains(al.EntityId ?? 0))
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(al => new
                {
                    Type = "Task",
                    HistoryId = al.AuditId,
                    EntityId = al.EntityId,
                    EntityName = al.EntityType,
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            var totalLogs = await _context.AuditLogs.Where(al => al.EntityType == "Task" && taskIds.Contains(al.EntityId ?? 0)).CountAsync();

            return Ok(new
            {
                Logs = taskLogs,
                Pagination = new
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalLogs = totalLogs,
                    TotalPages = (int)Math.Ceiling((double)totalLogs / pageSize)
                }
            });
        }

        // GET: api/auditlogs/recent
        [HttpGet("recent")]
        public async Task<ActionResult> GetRecentAuditLogs([FromQuery] int count = 20)
        {
            if (count < 1 || count > 100) count = 20;

            var recentProjectLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Project")
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Take(count)
                .Select(al => new
                {
                    Type = "Project",
                    HistoryId = al.AuditId,
                    EntityId = al.EntityId,
                    EntityName = al.EntityType,
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            var recentTaskLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Task")
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Take(count)
                .Select(al => new
                {
                    Type = "Task",
                    HistoryId = al.AuditId,
                    EntityId = al.EntityId,
                    EntityName = al.EntityType,
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            var recentLogs = recentProjectLogs.Concat(recentTaskLogs)
                .OrderByDescending(log => log.ActionDate)
                .Take(count)
                .ToList();

            return Ok(recentLogs);
        }

        private async Task<bool> ProjectExists(int id)
        {
            return await _context.Projects.AnyAsync(e => e.ProjectId == id);
        }

        private async Task<bool> TaskExists(int id)
        {
            return await _context.Tasks.AnyAsync(e => e.TaskId == id);
        }

        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        private async Task<bool> DepartmentExists(int id)
        {
            return await _context.Departments.AnyAsync(e => e.DeptId == id);
        }
    }
}