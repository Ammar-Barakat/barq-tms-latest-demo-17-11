using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;
using BarqTMS.API.Helpers;
using BarqTMS.API.Services;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, BarqTMSDbContext context, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _context = context;
            _logger = logger;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskListDto>>> GetTasks()
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var tasks = await _taskService.GetTasksAsync(currentUserId);
            return Ok(tasks);
        }

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null) return Unauthorized("User not found.");

            var task = await _taskService.GetTaskByIdAsync(id, currentUserId);
            if (task == null) return NotFound($"Task with ID {id} not found.");

            var hasAccess = currentUser.Role switch
            {
                UserRole.Employee => task.AssignedTo == currentUserId,
                UserRole.Client => await IsClientProject(currentUser.Email ?? string.Empty, task.ProjectId),
                UserRole.TeamLeader => await IsUserInDepartment(currentUserId, task.DeptId),
                _ => true
            };

            if (!hasAccess) return Forbid();

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
            var createdBy = UserContextHelper.GetCurrentUserIdOrThrow(User);
            try
            {
                var task = await _taskService.CreateTaskAsync(createTaskDto, createdBy);
                return CreatedAtAction(nameof(GetTask), new { id = task.TaskId }, task);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,AssistantManager,TeamLeader")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            try
            {
                var success = await _taskService.UpdateTaskAsync(id, updateTaskDto, currentUserId);
                if (!success) return NotFound($"Task with ID {id} not found.");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/tasks/{id}/request-complete
        // Only assigned employee can request completion (mark as done)
        [HttpPut("{id}/request-complete")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> RequestTaskCompletion(int id)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var task = await _taskService.GetTaskByIdAsync(id, currentUserId);
            if (task == null) return NotFound($"Task with ID {id} not found.");
            if (task.AssignedTo != currentUserId) return Forbid();
            var success = await _taskService.RequestTaskCompletionAsync(id, currentUserId);
            if (!success) return NotFound($"Task with ID {id} not found.");
            return NoContent();
        }

        // PUT: api/tasks/{id}/review-completion
        // Only task creator (Manager, AssistantManager, TeamLeader) can approve/reject
        [HttpPut("{id}/review-completion")]
        [Authorize(Roles = "Manager,AssistantManager,TeamLeader")]
        public async Task<IActionResult> ReviewTaskCompletion(int id, ReviewTaskCompletionDto reviewDto)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var task = await _taskService.GetTaskByIdAsync(id, currentUserId);
            if (task == null) return NotFound($"Task with ID {id} not found.");
            if (task.CreatedBy != currentUserId) return Forbid();
            var success = await _taskService.ReviewTaskCompletionAsync(id, reviewDto, currentUserId);
            if (!success) return NotFound($"Task with ID {id} not found.");
            return NoContent();
        }

        // DELETE: api/tasks/5
        // Only Manager can delete tasks
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var (success, error) = await _taskService.DeleteTaskAsync(id, currentUserId);
            if (!success)
            {
                if (error == "notfound") return NotFound($"Task with ID {id} not found.");
                return BadRequest(error);
            }
            return NoContent();
        }

        // POST: api/tasks/5/comments
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<TaskCommentDto>> AddTaskComment(int id, CreateTaskCommentDto createCommentDto)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            try
            {
                var comment = await _taskService.AddTaskCommentAsync(id, currentUserId, createCommentDto);
                return Ok(comment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET: api/tasks/5/comments
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<TaskCommentDto>>> GetTaskComments(int id)
        {
            try
            {
                var comments = await _taskService.GetTaskCommentsAsync(id);
                return Ok(comments);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/tasks/5/attachments
        [HttpPost("{id}/attachments")]
        public async Task<ActionResult<AttachmentDto>> AddTaskAttachment(int id, [FromBody] AttachmentDto attachmentDto)
        {
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            try
            {
                var result = await _taskService.AddTaskAttachmentAsync(id, currentUserId, attachmentDto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET: api/tasks/5/attachments
        [HttpGet("{id}/attachments")]
        public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetTaskAttachments(int id)
        {
            try
            {
                var attachments = await _taskService.GetTaskAttachmentsAsync(id);
                return Ok(attachments);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET: api/tasks/5/history
        [HttpGet("{id}/history")]
        public async Task<ActionResult<IEnumerable<TaskHistoryDto>>> GetTaskHistory(int id)
        {
            try
            {
                var history = await _taskService.GetTaskHistoryAsync(id);
                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}