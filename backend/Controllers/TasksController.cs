using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;
using BarqTMS.API.Helpers;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(BarqTMSDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            // Get current user from JWT token
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var currentUser = await _context.Users.FindAsync(currentUserId);
            
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            var query = _context.Tasks
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .AsQueryable();

            // Filter based on user role
            switch (currentUser.Role)
            {
                case UserRole.Employee: // Employee - only their assigned tasks
                    query = query.Where(t => t.AssignedTo == currentUserId);
                    break;
                    
                case UserRole.Client: // Client - tasks from their projects only
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                    if (client != null)
                    {
                        var clientProjectIds = await _context.Projects
                            .Where(p => p.ClientId == client.ClientId)
                            .Select(p => p.ProjectId)
                            .ToListAsync();
                        query = query.Where(t => clientProjectIds.Contains(t.ProjectId));
                    }
                    else
                    {
                        // No client record, return empty
                        return Ok(new List<TaskDto>());
                    }
                    break;
                    
                case UserRole.TeamLeader: // Team Leader - tasks from their department
                    var userDepartments = await _context.UserDepartments
                        .Where(ud => ud.UserId == currentUserId)
                        .Select(ud => ud.DeptId)
                        .ToListAsync();
                    if (userDepartments.Any())
                    {
                        query = query.Where(t => userDepartments.Contains(t.DeptId));
                    }
                    break;
                    
                // Manager (1), Assistant Manager (2), Accountant (3) - see all tasks
                default:
                    // No filtering
                    break;
            }

            var tasks = await query
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

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            // Get current user from JWT token
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var currentUser = await _context.Users.FindAsync(currentUserId);
            
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            var task = await _context.Tasks
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Where(t => t.TaskId == id)
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
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            // Check access based on user role
            var hasAccess = currentUser.Role switch
            {
                UserRole.Employee => task.AssignedTo == currentUserId, // Employee - only their tasks
                UserRole.Client => await IsClientProject(currentUser.Email ?? string.Empty, task.ProjectId), // Client - their projects
                UserRole.TeamLeader => await IsUserInDepartment(currentUserId, task.DeptId), // Team Leader - their department
                _ => true // Manager, Assistant, Accountant - all tasks
            };

            if (!hasAccess)
            {
                return Forbid(); // 403 Forbidden
            }

            return Ok(task);
        }

        private async Task<bool> IsClientProject(string userEmail, int projectId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == userEmail);
            if (client == null) return false;
            
            var project = await _context.Projects.FindAsync(projectId);
            return project != null && project.ClientId == client.ClientId;
        }

        private async Task<bool> IsUserInDepartment(int userId, int deptId)
        {
            return await _context.UserDepartments.AnyAsync(ud => ud.UserId == userId && ud.DeptId == deptId);
        }

        // POST: api/tasks
        // Only Manager, Assistant Manager, and Team Leader can create tasks
        [HttpPost]
        [Authorize(Roles = "Manager,AssistantManager,TeamLeader")]
        public async Task<ActionResult<TaskDto>> CreateTask(CreateTaskDto createTaskDto)
        {
            // Validate dependencies
            if (!await _context.Priorities.AnyAsync(p => p.PriorityId == createTaskDto.PriorityId))
            {
                return BadRequest($"Priority with ID {createTaskDto.PriorityId} not found.");
            }

            if (!await _context.Statuses.AnyAsync(s => s.StatusId == createTaskDto.StatusId))
            {
                return BadRequest($"Status with ID {createTaskDto.StatusId} not found.");
            }

            if (!await _context.Departments.AnyAsync(d => d.DeptId == createTaskDto.DeptId))
            {
                return BadRequest($"Department with ID {createTaskDto.DeptId} not found.");
            }

            if (!await _context.Projects.AnyAsync(p => p.ProjectId == createTaskDto.ProjectId))
            {
                return BadRequest($"Project with ID {createTaskDto.ProjectId} not found.");
            }

            if (createTaskDto.AssignedTo.HasValue && !await _context.Users.AnyAsync(u => u.UserId == createTaskDto.AssignedTo))
            {
                return BadRequest($"Assigned user with ID {createTaskDto.AssignedTo} not found.");
            }

            // Get current user from JWT token
            var createdBy = UserContextHelper.GetCurrentUserIdOrThrow(User);

            var task = new WorkTask
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                PriorityId = createTaskDto.PriorityId,
                StatusId = createTaskDto.StatusId,
                DueDate = createTaskDto.DueDate,
                CreatedBy = createdBy,
                AssignedTo = createTaskDto.AssignedTo,
                DeptId = createTaskDto.DeptId,
                ProjectId = createTaskDto.ProjectId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Create task history entry
            await CreateTaskHistory(task.TaskId, createdBy, $"Task '{task.Title}' created");

            // Create notification for assigned user
            if (task.AssignedTo.HasValue)
            {
                await CreateNotification(task.AssignedTo.Value, $"You have been assigned a new task: {task.Title}", task.TaskId, task.ProjectId);
            }

            // Return the created task
            var taskDto = await GetTaskDto(task.TaskId);
            return CreatedAtAction(nameof(GetTask), new { id = task.TaskId }, taskDto);
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            // Validate dependencies
            if (!await _context.Priorities.AnyAsync(p => p.PriorityId == updateTaskDto.PriorityId))
            {
                return BadRequest($"Priority with ID {updateTaskDto.PriorityId} not found.");
            }

            if (!await _context.Statuses.AnyAsync(s => s.StatusId == updateTaskDto.StatusId))
            {
                return BadRequest($"Status with ID {updateTaskDto.StatusId} not found.");
            }

            if (!await _context.Departments.AnyAsync(d => d.DeptId == updateTaskDto.DeptId))
            {
                return BadRequest($"Department with ID {updateTaskDto.DeptId} not found.");
            }

            if (!await _context.Projects.AnyAsync(p => p.ProjectId == updateTaskDto.ProjectId))
            {
                return BadRequest($"Project with ID {updateTaskDto.ProjectId} not found.");
            }

            if (updateTaskDto.AssignedTo.HasValue && !await _context.Users.AnyAsync(u => u.UserId == updateTaskDto.AssignedTo))
            {
                return BadRequest($"Assigned user with ID {updateTaskDto.AssignedTo} not found.");
            }

            // Store old values for history and notifications
            var oldAssignedTo = task.AssignedTo;
            var oldStatusId = task.StatusId;
            var oldTitle = task.Title;

            // Update task properties
            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.PriorityId = updateTaskDto.PriorityId;
            task.StatusId = updateTaskDto.StatusId;
            task.DueDate = updateTaskDto.DueDate;
            task.AssignedTo = updateTaskDto.AssignedTo;
            task.DeptId = updateTaskDto.DeptId;
            task.ProjectId = updateTaskDto.ProjectId;

            try
            {
                await _context.SaveChangesAsync();

                // Create history entries for changes
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var changes = new List<string>();

                if (oldTitle != task.Title)
                    changes.Add($"Title changed from '{oldTitle}' to '{task.Title}'");
                if (oldAssignedTo != task.AssignedTo)
                    changes.Add("Assignment changed");
                if (oldStatusId != task.StatusId)
                    changes.Add("Status changed");

                if (changes.Any())
                {
                    await CreateTaskHistory(task.TaskId, currentUserId, string.Join(", ", changes));
                }

                // Create notifications for assignment changes
                if (oldAssignedTo != task.AssignedTo)
                {
                    if (task.AssignedTo.HasValue)
                    {
                        await CreateNotification(task.AssignedTo.Value, $"You have been assigned to task: {task.Title}", task.TaskId, task.ProjectId);
                    }
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TaskExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/tasks/5
        // Only Manager can delete tasks
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            // Create history entry before deletion
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            await CreateTaskHistory(task.TaskId, currentUserId, $"Task '{task.Title}' deleted");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/tasks/5/comments
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<TaskCommentDto>> AddTaskComment(int id, CreateTaskCommentDto createCommentDto)
        {
            if (!await TaskExists(id))
            {
                return NotFound($"Task with ID {id} not found.");
            }

            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);

            var comment = new TaskComment
            {
                TaskId = id,
                UserId = currentUserId,
                Comment = createCommentDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            // Create history entry
            await CreateTaskHistory(id, currentUserId, "Comment added");

            var commentDto = await _context.TaskComments
                .Where(tc => tc.CommentId == comment.CommentId)
                .Include(tc => tc.User)
                .Select(tc => new TaskCommentDto
                {
                    CommentId = tc.CommentId,
                    TaskId = tc.TaskId,
                    UserId = tc.UserId,
                    UserName = tc.User.Name,
                    Comment = tc.Comment,
                    CreatedAt = tc.CreatedAt
                })
                .FirstOrDefaultAsync();

            return Ok(commentDto);
        }

        // GET: api/tasks/5/comments
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<TaskCommentDto>>> GetTaskComments(int id)
        {
            if (!await TaskExists(id))
            {
                return NotFound($"Task with ID {id} not found.");
            }

            var comments = await _context.TaskComments
                .Where(tc => tc.TaskId == id)
                .Include(tc => tc.User)
                .OrderBy(tc => tc.CreatedAt)
                .Select(tc => new TaskCommentDto
                {
                    CommentId = tc.CommentId,
                    TaskId = tc.TaskId,
                    UserId = tc.UserId,
                    UserName = tc.User.Name,
                    Comment = tc.Comment,
                    CreatedAt = tc.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/tasks/5/attachments
        [HttpPost("{id}/attachments")]
        public async Task<ActionResult<AttachmentDto>> AddTaskAttachment(int id, [FromBody] AttachmentDto attachmentDto)
        {
            if (!await TaskExists(id))
            {
                return NotFound($"Task with ID {id} not found.");
            }

            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);

            var attachment = new Attachment
            {
                TaskId = id,
                FileName = attachmentDto.FileName,
                FileUrl = attachmentDto.FileUrl,
                UploadedBy = currentUserId,
                UploadedAt = DateTime.UtcNow
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            // Create history entry
            await CreateTaskHistory(id, currentUserId, $"Attachment '{attachment.FileName}' uploaded");

            var resultDto = await _context.Attachments
                .Where(a => a.FileId == attachment.FileId)
                .Include(a => a.UploadedByUser)
                .Select(a => new AttachmentDto
                {
                    FileId = a.FileId,
                    TaskId = a.TaskId,
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    UploadedBy = a.UploadedBy,
                    UploadedByName = a.UploadedByUser.Name,
                    UploadedAt = a.UploadedAt
                })
                .FirstOrDefaultAsync();

            return Ok(resultDto);
        }

        // GET: api/tasks/5/attachments
        [HttpGet("{id}/attachments")]
        public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetTaskAttachments(int id)
        {
            if (!await TaskExists(id))
            {
                return NotFound($"Task with ID {id} not found.");
            }

            var attachments = await _context.Attachments
                .Where(a => a.TaskId == id)
                .Include(a => a.UploadedByUser)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new AttachmentDto
                {
                    FileId = a.FileId,
                    TaskId = a.TaskId,
                    FileName = a.FileName,
                    FileUrl = a.FileUrl,
                    UploadedBy = a.UploadedBy,
                    UploadedByName = a.UploadedByUser.Name,
                    UploadedAt = a.UploadedAt
                })
                .ToListAsync();

            return Ok(attachments);
        }

        // GET: api/tasks/5/history
        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetTaskHistory(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }

            var history = await _context.AuditLogs
                .Where(al => al.EntityType == "Task" && al.EntityId == id)
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Select(al => new TaskHistoryDto
                {
                    HistoryId = al.AuditId,
                    TaskId = id,
                    TaskTitle = task.Title,
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            return Ok(history);
        }

        private async Task<bool> TaskExists(int id)
        {
            return await _context.Tasks.AnyAsync(e => e.TaskId == id);
        }

        private async Task CreateTaskHistory(int taskId, int userId, string action)
        {
            var history = new AuditLog
            {
                EntityType = "Task",
                EntityId = taskId,
                UserId = userId,
                Action = action,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(history);
            await _context.SaveChangesAsync();
        }

        private async Task CreateNotification(int userId, string message, int? taskId = null, int? projectId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                TaskId = taskId,
                ProjectId = projectId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private async Task<TaskDto?> GetTaskDto(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Where(t => t.TaskId == taskId)
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
                .FirstOrDefaultAsync();
        }
    }
}