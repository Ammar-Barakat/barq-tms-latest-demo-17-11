using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using BarqTMS.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskListDto>> GetTasksAsync(int currentUserId);
        Task<TaskDto?> GetTaskByIdAsync(int id, int currentUserId);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, int createdBy);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto, int currentUserId);
        Task<(bool Success, string? Error)> DeleteTaskAsync(int id, int currentUserId);
        Task<TaskCommentDto> AddTaskCommentAsync(int id, int userId, CreateTaskCommentDto dto);
        Task<IEnumerable<TaskCommentDto>> GetTaskCommentsAsync(int id);
        Task<AttachmentDto> AddTaskAttachmentAsync(int id, int userId, AttachmentDto dto);
        Task<IEnumerable<AttachmentDto>> GetTaskAttachmentsAsync(int id);
        Task<IEnumerable<TaskHistoryDto>> GetTaskHistoryAsync(int id);
        Task<bool> RequestTaskCompletionAsync(int id, int currentUserId);
        Task<bool> ReviewTaskCompletionAsync(int id, ReviewTaskCompletionDto reviewDto, int currentUserId);
    }

    public class TaskService : ITaskService
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<TaskService> _logger;
        private readonly IRealTimeService _realTimeService;

        public TaskService(BarqTMSDbContext context, ILogger<TaskService> logger, IRealTimeService realTimeService)
        {
            _context = context;
            _logger = logger;
            _realTimeService = realTimeService;
        }

        public async Task<IEnumerable<TaskListDto>> GetTasksAsync(int currentUserId)
        {
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null) throw new ArgumentException("User not found");

            var query = _context.Tasks
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.AssignedUser)
                .Include(t => t.Project)
                .AsQueryable();

            switch (currentUser.Role)
            {
                case UserRole.Employee:
                    query = query.Where(t => t.AssignedTo == currentUserId);
                    break;
                case UserRole.Client:
                {
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                    if (client == null) return Enumerable.Empty<TaskListDto>();
                    var clientProjectIds = await _context.Projects
                        .Where(p => p.ClientId == client.ClientId)
                        .Select(p => p.ProjectId)
                        .ToListAsync();
                    query = query.Where(t => clientProjectIds.Contains(t.ProjectId));
                    break;
                }
                case UserRole.TeamLeader:
                {
                    var userDepartments = await _context.UserDepartments
                        .Where(ud => ud.UserId == currentUserId)
                        .Select(ud => ud.DeptId)
                        .ToListAsync();
                    if (userDepartments.Any()) query = query.Where(t => userDepartments.Contains(t.DeptId));
                    break;
                }
                default:
                    break;
            }

            return await query
                .Select(t => new TaskListDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    PriorityId = t.PriorityId,
                    PriorityLevel = t.Priority.Level,
                    StatusId = t.StatusId,
                    StatusName = t.Status.StatusName,
                    DueDate = t.DueDate,
                    AssignedTo = t.AssignedTo,
                    AssignedToName = t.AssignedUser != null ? t.AssignedUser.Name : null,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.ProjectName,
                    CommentCount = t.TaskComments.Count(),
                    AttachmentCount = t.Attachments.Count(),
                    DriveFolderLink = t.DriveFolderLink,
                    MaterialDriveFolderLink = t.MaterialDriveFolderLink
                })
                .ToListAsync();
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id, int currentUserId)
        {
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
                    AttachmentCount = t.Attachments.Count(),
                    DriveFolderLink = t.DriveFolderLink,
                    MaterialDriveFolderLink = t.MaterialDriveFolderLink
                })
                .FirstOrDefaultAsync();

            if (task == null) return null;

            // Authorization checks are left to caller; service can provide the task
            return task;
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, int createdBy)
        {
            // Validate dependencies
            if (!await _context.Priorities.AnyAsync(p => p.PriorityId == dto.PriorityId))
                throw new ArgumentException($"Priority with ID {dto.PriorityId} not found.");
            if (!await _context.Statuses.AnyAsync(s => s.StatusId == dto.StatusId))
                throw new ArgumentException($"Status with ID {dto.StatusId} not found.");
            if (!await _context.Departments.AnyAsync(d => d.DeptId == dto.DeptId))
                throw new ArgumentException($"Department with ID {dto.DeptId} not found.");
            if (!await _context.Projects.AnyAsync(p => p.ProjectId == dto.ProjectId))
                throw new ArgumentException($"Project with ID {dto.ProjectId} not found.");
            if (dto.AssignedTo.HasValue && !await _context.Users.AnyAsync(u => u.UserId == dto.AssignedTo))
                throw new ArgumentException($"Assigned user with ID {dto.AssignedTo} not found.");

            // FIX 2: Validate task due date is within project timeline
            if (dto.DueDate.HasValue)
            {
                var project = await _context.Projects.FindAsync(dto.ProjectId);
                if (project != null)
                {
                    if (project.StartDate.HasValue && dto.DueDate.Value < project.StartDate.Value)
                    {
                        throw new ArgumentException($"Task due date ({dto.DueDate.Value:MM/dd/yyyy}) cannot be before the project start date ({project.StartDate.Value:MM/dd/yyyy}).");
                    }
                    if (project.EndDate.HasValue && dto.DueDate.Value > project.EndDate.Value)
                    {
                        throw new ArgumentException($"Task due date ({dto.DueDate.Value:MM/dd/yyyy}) cannot be after the project due date ({project.EndDate.Value:MM/dd/yyyy}).");
                    }
                }
            }

            // FIX 4: Manager/Assistant Manager assignment restrictions
            if (dto.AssignedTo.HasValue)
            {
                var creatorUser = await _context.Users.FindAsync(createdBy);
                if (creatorUser != null && (creatorUser.Role == UserRole.Manager || creatorUser.Role == UserRole.AssistantManager))
                {
                    var assignedUser = await _context.Users.FindAsync(dto.AssignedTo.Value);
                    if (assignedUser != null && (assignedUser.Role == UserRole.Manager || assignedUser.Role == UserRole.AssistantManager))
                    {
                        throw new ArgumentException("Managers and Assistant Managers can only assign tasks to Team Leaders or Employees, not to other Managers or Assistant Managers.");
                    }
                }
            }

            var task = new WorkTask
            {
                Title = dto.Title,
                Description = dto.Description,
                PriorityId = dto.PriorityId,
                StatusId = dto.StatusId,
                DueDate = dto.DueDate,
                CreatedBy = createdBy,
                AssignedTo = dto.AssignedTo,
                DeptId = dto.DeptId,
                ProjectId = dto.ProjectId,
                DriveFolderLink = dto.DriveFolderLink,
                MaterialDriveFolderLink = dto.MaterialDriveFolderLink
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            await CreateTaskHistory(task.TaskId, createdBy, $"Task '{task.Title}' created");

            if (task.AssignedTo.HasValue)
            {
                // Create and send notification via helper
                await CreateAndSendNotificationAsync(task.AssignedTo.Value, $"You have been assigned a new task: {task.Title}", task.TaskId, task.ProjectId);
            }

            var taskDto = await GetTaskByIdAsync(task.TaskId, createdBy);
            return taskDto!;
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto, int currentUserId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            if (!await _context.Priorities.AnyAsync(p => p.PriorityId == dto.PriorityId))
                throw new ArgumentException($"Priority with ID {dto.PriorityId} not found.");
            if (!await _context.Statuses.AnyAsync(s => s.StatusId == dto.StatusId))
                throw new ArgumentException($"Status with ID {dto.StatusId} not found.");
            if (!await _context.Departments.AnyAsync(d => d.DeptId == dto.DeptId))
                throw new ArgumentException($"Department with ID {dto.DeptId} not found.");
            if (!await _context.Projects.AnyAsync(p => p.ProjectId == dto.ProjectId))
                throw new ArgumentException($"Project with ID {dto.ProjectId} not found.");
            if (dto.AssignedTo.HasValue && !await _context.Users.AnyAsync(u => u.UserId == dto.AssignedTo))
                throw new ArgumentException($"Assigned user with ID {dto.AssignedTo} not found.");

            // FIX 2: Validate task due date is within project timeline
            if (dto.DueDate.HasValue)
            {
                var project = await _context.Projects.FindAsync(dto.ProjectId);
                if (project != null)
                {
                    if (project.StartDate.HasValue && dto.DueDate.Value < project.StartDate.Value)
                    {
                        throw new ArgumentException($"Task due date ({dto.DueDate.Value:MM/dd/yyyy}) cannot be before the project start date ({project.StartDate.Value:MM/dd/yyyy}).");
                    }
                    if (project.EndDate.HasValue && dto.DueDate.Value > project.EndDate.Value)
                    {
                        throw new ArgumentException($"Task due date ({dto.DueDate.Value:MM/dd/yyyy}) cannot be after the project due date ({project.EndDate.Value:MM/dd/yyyy}).");
                    }
                }
            }

            // FIX 4: Manager/Assistant Manager assignment restrictions
            if (dto.AssignedTo.HasValue)
            {
                var currentUser = await _context.Users.FindAsync(currentUserId);
                if (currentUser != null && (currentUser.Role == UserRole.Manager || currentUser.Role == UserRole.AssistantManager))
                {
                    var assignedUser = await _context.Users.FindAsync(dto.AssignedTo.Value);
                    if (assignedUser != null && (assignedUser.Role == UserRole.Manager || assignedUser.Role == UserRole.AssistantManager))
                    {
                        throw new ArgumentException("Managers and Assistant Managers can only assign tasks to Team Leaders or Employees, not to other Managers or Assistant Managers.");
                    }
                }
            }

            var oldAssignedTo = task.AssignedTo;
            var oldStatusId = task.StatusId;
            var oldTitle = task.Title;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.PriorityId = dto.PriorityId;
            task.StatusId = dto.StatusId;
            task.DueDate = dto.DueDate;
            task.AssignedTo = dto.AssignedTo;
            task.DeptId = dto.DeptId;
            task.ProjectId = dto.ProjectId;
            task.DriveFolderLink = dto.DriveFolderLink;
            task.MaterialDriveFolderLink = dto.MaterialDriveFolderLink;

            await _context.SaveChangesAsync();

            var changes = new List<string>();
            if (oldTitle != task.Title) changes.Add($"Title changed from '{oldTitle}' to '{task.Title}'");
            if (oldAssignedTo != task.AssignedTo) changes.Add("Assignment changed");
            if (oldStatusId != task.StatusId) changes.Add("Status changed");

            if (changes.Any())
            {
                await CreateTaskHistory(task.TaskId, currentUserId, string.Join(", ", changes));
            }

            if (oldAssignedTo != task.AssignedTo && task.AssignedTo.HasValue)
            {
                await CreateAndSendNotificationAsync(task.AssignedTo.Value, $"You have been assigned to task: {task.Title}", task.TaskId, task.ProjectId);
            }

            return true;
        }

        public async Task<(bool Success, string? Error)> DeleteTaskAsync(int id, int currentUserId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return (false, "notfound");

            await CreateTaskHistory(task.TaskId, currentUserId, $"Task '{task.Title}' deleted");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<TaskCommentDto> AddTaskCommentAsync(int id, int userId, CreateTaskCommentDto dto)
        {
            if (!await TaskExists(id)) throw new KeyNotFoundException("Task not found");

            var comment = new TaskComment
            {
                TaskId = id,
                UserId = userId,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            await CreateTaskHistory(id, userId, "Comment added");

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

            return commentDto!;
        }

        public async Task<IEnumerable<TaskCommentDto>> GetTaskCommentsAsync(int id)
        {
            if (!await TaskExists(id)) throw new KeyNotFoundException("Task not found");

            return await _context.TaskComments
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
        }

        public async Task<AttachmentDto> AddTaskAttachmentAsync(int id, int userId, AttachmentDto dto)
        {
            if (!await TaskExists(id)) throw new KeyNotFoundException("Task not found");

            var attachment = new Attachment
            {
                TaskId = id,
                FileName = dto.FileName,
                FileUrl = dto.FileUrl,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            await CreateTaskHistory(id, userId, $"Attachment '{attachment.FileName}' uploaded");

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

            return resultDto!;
        }

        public async Task<IEnumerable<AttachmentDto>> GetTaskAttachmentsAsync(int id)
        {
            if (!await TaskExists(id)) throw new KeyNotFoundException("Task not found");

            return await _context.Attachments
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
        }

        public async Task<IEnumerable<TaskHistoryDto>> GetTaskHistoryAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) throw new KeyNotFoundException("Task not found");

            return await _context.AuditLogs
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
        }

        public async Task<bool> RequestTaskCompletionAsync(int id, int currentUserId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;
            // Set status to "In Review"
            var inReviewStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.StatusName == "In Review");
            if (inReviewStatus == null) throw new ArgumentException("'In Review' status not found.");
            task.StatusId = inReviewStatus.StatusId;
            await _context.SaveChangesAsync();
            // Notify creator
            await CreateAndSendNotificationAsync(task.CreatedBy, $"Task '{task.Title}' marked as ready for review.", task.TaskId, task.ProjectId);
            await CreateTaskHistory(task.TaskId, currentUserId, "Completion requested");
            return true;
        }

        public async Task<bool> ReviewTaskCompletionAsync(int id, ReviewTaskCompletionDto reviewDto, int currentUserId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;
            var employeeId = task.AssignedTo;
            if (reviewDto.Approve)
            {
                // Set status to "Done"
                var doneStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.StatusName == "Done");
                if (doneStatus == null) throw new ArgumentException("'Done' status not found.");
                task.StatusId = doneStatus.StatusId;
                task.AssignedTo = null; // Remove from employee's list
                await _context.SaveChangesAsync();
                if (employeeId.HasValue)
                {
                    await CreateAndSendNotificationAsync(employeeId.Value, $"Task '{task.Title}' has been approved and marked as done.", task.TaskId, task.ProjectId);
                }
                await CreateTaskHistory(task.TaskId, currentUserId, "Task approved and completed");
            }
            else
            {
                // Set status to "In Progress"
                var inProgressStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.StatusName == "In Progress");
                if (inProgressStatus == null) throw new ArgumentException("'In Progress' status not found.");
                task.StatusId = inProgressStatus.StatusId;
                // Update due date if provided
                if (reviewDto.NewDueDate.HasValue)
                {
                    task.DueDate = reviewDto.NewDueDate.Value;
                }
                await _context.SaveChangesAsync();
                // Add notes as comment and notify employee
                if (!string.IsNullOrWhiteSpace(reviewDto.Notes) && employeeId.HasValue)
                {
                    var comment = new TaskComment
                    {
                        TaskId = id,
                        UserId = currentUserId,
                        Comment = reviewDto.Notes,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.TaskComments.Add(comment);
                    await _context.SaveChangesAsync();
                    await CreateAndSendNotificationAsync(employeeId.Value, $"Task '{task.Title}' review not approved. Notes: {reviewDto.Notes}", task.TaskId, task.ProjectId);
                }
                await CreateTaskHistory(task.TaskId, currentUserId, "Task review not approved");
            }
            return true;
        }

        private async Task<bool> TaskExists(int id) => await _context.Tasks.AnyAsync(e => e.TaskId == id);

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

        private async Task CreateAndSendNotificationAsync(int userId, string message, int? taskId = null, int? projectId = null)
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

            // Build DTO to send
            var payload = new
            {
                NotifId = notification.NotifId,
                UserId = notification.UserId,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead,
                TaskId = notification.TaskId,
                TaskTitle = null as string,
                ProjectId = notification.ProjectId,
                ProjectName = null as string
            };

            // Try to include task/project titles if available
            if (notification.TaskId.HasValue)
            {
                var task = await _context.Tasks.FindAsync(notification.TaskId.Value);
                if (task != null) payload = payload with { TaskTitle = task.Title };
            }
            if (notification.ProjectId.HasValue)
            {
                var project = await _context.Projects.FindAsync(notification.ProjectId.Value);
                if (project != null) payload = payload with { ProjectName = project.ProjectName };
            }

            // Send via real-time service
            try
            {
                await _realTimeService.SendToUserAsync(userId, "ReceiveNotification", payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time notification to user {UserId}", userId);
            }
        }

        private async Task CreateNotification(int userId, string message, int? taskId = null, int? projectId = null)
        {
            // Backwards-compatible helper used elsewhere; delegate to CreateAndSendNotificationAsync
            await CreateAndSendNotificationAsync(userId, message, taskId, projectId);
        }

        private async Task CreateNotification(int userId, string message)
        {
            await CreateAndSendNotificationAsync(userId, message, null, null);
        }
    }
}
