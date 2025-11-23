using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using BarqTMS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BarqTMS.API.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskListDto>> GetAllTasksAsync(int userId, UserRole role);
        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int createdByUserId);
        Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int userId);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> UpdateTaskStatusAsync(int id, UpdateTaskStatusDto statusDto, int userId);
        Task<IEnumerable<TaskCommentDto>> GetTaskCommentsAsync(int taskId);
        Task<TaskCommentDto> AddTaskCommentAsync(int taskId, CreateTaskCommentDto commentDto, int userId);
    }

    public class TaskService : ITaskService
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(BarqTMSDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TaskListDto>> GetAllTasksAsync(int userId, UserRole role)
        {
            var query = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Assignees).ThenInclude(ta => ta.User)
                .Include(t => t.Delegator)
                .Include(t => t.OriginalAssigner)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .AsQueryable();

            // Filter based on role
            if (role != UserRole.Manager && role != UserRole.AssistantManager)
            {
                // Employees/TeamLeaders see tasks assigned to them or created by them
                query = query.Where(t => 
                    t.Assignees.Any(ta => ta.UserId == userId) || 
                    t.DelegatedBy == userId || 
                    t.OriginalAssignerId == userId);
            }

            var tasks = await query.ToListAsync();

            return tasks.Select(t => new TaskListDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                PriorityId = (int)t.Priority,
                PriorityLevel = t.Priority.ToString(),
                StatusId = (int)t.Status,
                StatusName = t.Status.ToString(),
                DueDate = t.DueDate,
                CreatedBy = t.OriginalAssignerId ?? 0,
                CreatedByName = t.OriginalAssigner?.FullName ?? "Unknown",
                AssignedTo = t.Assignees.FirstOrDefault()?.UserId,
                AssignedToName = t.Assignees.FirstOrDefault()?.User.FullName,
                OriginalAssignerId = t.OriginalAssignerId,
                OriginalAssignerName = t.OriginalAssigner?.FullName,
                DelegatedBy = t.DelegatedBy,
                DelegatedByName = t.Delegator?.FullName,
                ProjectId = t.ProjectId,
                ProjectName = t.Project?.Name,
                CommentCount = t.Comments.Count,
                AttachmentCount = t.Attachments.Count,
                DriveFolderLink = t.DriveFolderLink,
                MaterialDriveFolderLink = t.MaterialDriveFolderLink,
                SpecificTime = t.SpecificTime,
                EstimatedHours = t.EstimatedHours,
                Tags = t.Tags
            });
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Department)
                .Include(t => t.Assignees).ThenInclude(ta => ta.User)
                .Include(t => t.Delegator)
                .Include(t => t.OriginalAssigner)
                .Include(t => t.Comments)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null) return null;

            return MapToDto(task);
        }

        public async Task<TaskDto> CreateTaskAsync(CreateTaskDto createTaskDto, int createdByUserId)
        {
            // Validate due date is not in the past
            if (createTaskDto.DueDate.HasValue && createTaskDto.DueDate.Value.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Due date cannot be in the past. Please select a current or future date.");
            }

            var task = new WorkTask
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                Priority = (TaskPriority)createTaskDto.PriorityId,
                Status = (Models.Enums.TaskStatus)createTaskDto.StatusId,
                DueDate = createTaskDto.DueDate,
                DepartmentId = createTaskDto.DeptId,
                ProjectId = createTaskDto.ProjectId ?? 0, // Assuming 0 or nullable handling in DB, but model says int
                DriveFolderLink = createTaskDto.DriveFolderLink,
                MaterialDriveFolderLink = createTaskDto.MaterialDriveFolderLink,
                SpecificTime = createTaskDto.SpecificTime,
                EstimatedHours = createTaskDto.EstimatedHours,
                Tags = createTaskDto.Tags,
                OriginalAssignerId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            // Handle ProjectId if it's required by DB but optional in DTO
            // If ProjectId is 0, it might fail foreign key constraint if not nullable in DB
            // Checking model: public int ProjectId { get; set; } -> Required
            // We need a default project or handle this. For now assuming valid ProjectId provided or 0 is handled.
            // Actually, let's check if ProjectId is valid if provided.
            
            if (createTaskDto.ProjectId.HasValue && createTaskDto.ProjectId.Value > 0)
            {
                task.ProjectId = createTaskDto.ProjectId.Value;
            }
            else
            {
                // If no project, maybe assign to a default "General" project or handle as nullable if DB allows
                // Model says: public int ProjectId { get; set; }
                // Let's check if we have a default project or if we should create one.
                // For now, let's assume the user provides a valid project ID or we pick the first one.
                var firstProject = await _context.Projects.FirstOrDefaultAsync();
                if (firstProject != null)
                {
                    task.ProjectId = firstProject.ProjectId;
                }
            }

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            if (createTaskDto.AssignedTo.HasValue)
            {
                _context.TaskAssignees.Add(new TaskAssignee
                {
                    TaskId = task.TaskId,
                    UserId = createTaskDto.AssignedTo.Value
                });
                await _context.SaveChangesAsync();
            }

            return (await GetTaskByIdAsync(task.TaskId))!;
        }

        public async Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto, int userId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return null;

            // Validate due date is not in the past
            if (updateTaskDto.DueDate.HasValue && updateTaskDto.DueDate.Value.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Due date cannot be in the past. Please select a current or future date.");
            }

            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.Priority = (TaskPriority)updateTaskDto.PriorityId;
            task.Status = (Models.Enums.TaskStatus)updateTaskDto.StatusId;
            task.DueDate = updateTaskDto.DueDate;
            task.DepartmentId = updateTaskDto.DeptId;
            task.DriveFolderLink = updateTaskDto.DriveFolderLink;
            task.MaterialDriveFolderLink = updateTaskDto.MaterialDriveFolderLink;
            task.SpecificTime = updateTaskDto.SpecificTime;
            task.EstimatedHours = updateTaskDto.EstimatedHours;
            task.Tags = updateTaskDto.Tags;

            if (updateTaskDto.ProjectId.HasValue)
            {
                task.ProjectId = updateTaskDto.ProjectId.Value;
            }

            // Update Assignee
            if (updateTaskDto.AssignedTo.HasValue)
            {
                var currentAssignees = await _context.TaskAssignees.Where(ta => ta.TaskId == id).ToListAsync();
                _context.TaskAssignees.RemoveRange(currentAssignees);
                
                _context.TaskAssignees.Add(new TaskAssignee
                {
                    TaskId = task.TaskId,
                    UserId = updateTaskDto.AssignedTo.Value
                });
            }

            await _context.SaveChangesAsync();
            return (await GetTaskByIdAsync(task.TaskId))!;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTaskStatusAsync(int id, UpdateTaskStatusDto statusDto, int userId)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            task.Status = (Models.Enums.TaskStatus)statusDto.StatusId;
            
            // Add comment if notes provided
            if (!string.IsNullOrEmpty(statusDto.Notes))
            {
                _context.TaskComments.Add(new TaskComment
                {
                    TaskId = id,
                    UserId = userId,
                    Content = $"Status changed to {task.Status}: {statusDto.Notes}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TaskCommentDto>> GetTaskCommentsAsync(int taskId)
        {
            var comments = await _context.TaskComments
                .Include(c => c.Author)
                .Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => new TaskCommentDto
            {
                CommentId = c.CommentId,
                TaskId = c.TaskId,
                UserId = c.UserId,
                UserName = c.Author.FullName,
                Comment = c.Content,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<TaskCommentDto> AddTaskCommentAsync(int taskId, CreateTaskCommentDto commentDto, int userId)
        {
            var comment = new TaskComment
            {
                TaskId = taskId,
                UserId = userId,
                Content = commentDto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload to get Author info
            await _context.Entry(comment).Reference(c => c.Author).LoadAsync();

            return new TaskCommentDto
            {
                CommentId = comment.CommentId,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = comment.Author.FullName,
                Comment = comment.Content,
                CreatedAt = comment.CreatedAt
            };
        }

        private TaskDto MapToDto(WorkTask task)
        {
            return new TaskDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                PriorityId = (int)task.Priority,
                PriorityLevel = task.Priority.ToString(),
                StatusId = (int)task.Status,
                StatusName = task.Status.ToString(),
                DueDate = task.DueDate,
                CreatedBy = task.OriginalAssignerId ?? 0,
                CreatedByName = task.OriginalAssigner?.FullName ?? "Unknown",
                AssignedTo = task.Assignees.FirstOrDefault()?.UserId,
                AssignedToName = task.Assignees.FirstOrDefault()?.User.FullName,
                OriginalAssignerId = task.OriginalAssignerId,
                OriginalAssignerName = task.OriginalAssigner?.FullName,
                DelegatedBy = task.DelegatedBy,
                DelegatedByName = task.Delegator?.FullName,
                DeptId = task.DepartmentId,
                DeptName = task.Department?.Name ?? "Unknown",
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name,
                CommentCount = task.Comments.Count,
                AttachmentCount = task.Attachments.Count,
                DriveFolderLink = task.DriveFolderLink,
                MaterialDriveFolderLink = task.MaterialDriveFolderLink,
                SpecificTime = task.SpecificTime,
                EstimatedHours = task.EstimatedHours,
                Tags = task.Tags
            };
        }
    }
}
