using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
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
    public class NotificationsController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<NotificationsController> _logger;
        private readonly IRealTimeService _realTimeService;

        public NotificationsController(BarqTMSDbContext context, ILogger<NotificationsController> logger, IRealTimeService realTimeService)
        {
            _context = context;
            _logger = logger;
            _realTimeService = realTimeService;
        }

        // GET: api/notifications/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications(int userId)
        {
            if (!await UserExists(userId))
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Task)
                .Include(n => n.Project)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotifId = n.NotifId,
                    UserId = n.UserId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    TaskId = n.TaskId,
                    TaskTitle = n.Task != null ? n.Task.Title : null,
                    ProjectId = n.ProjectId,
                    ProjectName = n.Project != null ? n.Project.ProjectName : null
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/notifications/user/5/unread
        [HttpGet("user/{userId}/unread")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserUnreadNotifications(int userId)
        {
            if (!await UserExists(userId))
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .Include(n => n.Task)
                .Include(n => n.Project)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotifId = n.NotifId,
                    UserId = n.UserId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    TaskId = n.TaskId,
                    TaskTitle = n.Task != null ? n.Task.Title : null,
                    ProjectId = n.ProjectId,
                    ProjectName = n.Project != null ? n.Project.ProjectName : null
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/notifications/user/5/count/unread
        [HttpGet("user/{userId}/count/unread")]
        public async Task<ActionResult<int>> GetUserUnreadNotificationCount(int userId)
        {
            if (!await UserExists(userId))
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var count = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Ok(count);
        }

        // PUT: api/notifications/5/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            if (notification.IsRead)
            {
                return BadRequest("Notification is already marked as read.");
            }

            notification.IsRead = true;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await NotificationExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // PUT: api/notifications/user/5/read-all
        [HttpPut("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllUserNotificationsAsRead(int userId)
        {
            if (!await UserExists(userId))
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new { UpdatedCount = unreadNotifications.Count });
        }

        // DELETE: api/notifications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/notifications/user/5/read
        [HttpDelete("user/{userId}/read")]
        public async Task<IActionResult> DeleteReadNotifications(int userId)
        {
            if (!await UserExists(userId))
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var readNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(readNotifications);
            await _context.SaveChangesAsync();

            return Ok(new { DeletedCount = readNotifications.Count });
        }

        // GET: api/notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            var notification = await _context.Notifications
                .Include(n => n.Task)
                .Include(n => n.Project)
                .Where(n => n.NotifId == id)
                .Select(n => new NotificationDto
                {
                    NotifId = n.NotifId,
                    UserId = n.UserId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    TaskId = n.TaskId,
                    TaskTitle = n.Task != null ? n.Task.Title : null,
                    ProjectId = n.ProjectId,
                    ProjectName = n.Project != null ? n.Project.ProjectName : null
                })
                .FirstOrDefaultAsync();

            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            return Ok(notification);
        }

        // POST: api/notifications/test - Test endpoint for real-time notification delivery
        [HttpPost("test")]
        public async Task<ActionResult<NotificationDto>> SendTestNotification([FromBody] CreateNotificationDto notificationDto)
        {
            // Validate user exists
            if (!await UserExists(notificationDto.UserId))
            {
                return BadRequest($"User with ID {notificationDto.UserId} not found.");
            }

            var notification = new Notification
            {
                UserId = notificationDto.UserId,
                Message = notificationDto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                TaskId = notificationDto.TaskId,
                ProjectId = notificationDto.ProjectId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Build DTO for SignalR
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

            // Send via SignalR
            try
            {
                await _realTimeService.SendToUserAsync(notification.UserId, "ReceiveNotification", payload);
                _logger.LogInformation("Test notification sent to user {UserId} via SignalR", notification.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test notification via SignalR to user {UserId}", notification.UserId);
            }

            // Return the created notification
            var createdNotification = await _context.Notifications
                .Where(n => n.NotifId == notification.NotifId)
                .Include(n => n.Task)
                .Include(n => n.Project)
                .Select(n => new NotificationDto
                {
                    NotifId = n.NotifId,
                    UserId = n.UserId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    TaskId = n.TaskId,
                    TaskTitle = n.Task != null ? n.Task.Title : null,
                    ProjectId = n.ProjectId,
                    ProjectName = n.Project != null ? n.Project.ProjectName : null
                })
                .FirstOrDefaultAsync();

            return Ok(createdNotification);
        }

        // POST: api/notifications
        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification(CreateNotificationDto notificationDto)
        {
            // Validate user exists
            if (!await UserExists(notificationDto.UserId))
            {
                return BadRequest($"User with ID {notificationDto.UserId} not found.");
            }

            // Validate task exists if provided
            if (notificationDto.TaskId.HasValue && !await TaskExists(notificationDto.TaskId.Value))
            {
                return BadRequest($"Task with ID {notificationDto.TaskId} not found.");
            }

            // Validate project exists if provided
            if (notificationDto.ProjectId.HasValue && !await ProjectExists(notificationDto.ProjectId.Value))
            {
                return BadRequest($"Project with ID {notificationDto.ProjectId} not found.");
            }

            var notification = new Notification
            {
                UserId = notificationDto.UserId,
                Message = notificationDto.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                TaskId = notificationDto.TaskId,
                ProjectId = notificationDto.ProjectId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Return the created notification with full details
            var createdNotification = await _context.Notifications
                .Where(n => n.NotifId == notification.NotifId)
                .Include(n => n.Task)
                .Include(n => n.Project)
                .Select(n => new NotificationDto
                {
                    NotifId = n.NotifId,
                    UserId = n.UserId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    TaskId = n.TaskId,
                    TaskTitle = n.Task != null ? n.Task.Title : null,
                    ProjectId = n.ProjectId,
                    ProjectName = n.Project != null ? n.Project.ProjectName : null
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.NotifId }, createdNotification);
        }

        // PUT: api/notifications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(int id, UpdateNotificationDto notificationDto)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound($"Notification with ID {id} not found.");
            }

            notification.Message = notificationDto.Message;
            notification.IsRead = notificationDto.IsRead;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await NotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // GET: api/notifications/{id}/details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<NotificationDetailsDto>> GetNotificationDetails(int id)
        {
            // Get current user id from claims (or pass as param if needed)
            var userIdClaim = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var details = await _realTimeService.GetNotificationDetailsWithTaskNotesAsync(id, userIdClaim);
            if (details == null)
                return NotFound($"Notification with ID {id} not found.");
            return Ok(details);
        }

        private async Task<bool> NotificationExists(int id)
        {
            return await _context.Notifications.AnyAsync(e => e.NotifId == id);
        }

        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        private async Task<bool> TaskExists(int id)
        {
            return await _context.Tasks.AnyAsync(e => e.TaskId == id);
        }

        private async Task<bool> ProjectExists(int id)
        {
            return await _context.Projects.AnyAsync(e => e.ProjectId == id);
        }
    }
}