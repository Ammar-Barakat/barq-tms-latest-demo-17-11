using Microsoft.AspNetCore.SignalR;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.Hubs;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface IRealTimeService
    {
        Task SendNotificationToUserAsync(int userId, string message, object? data = null);
        Task SendNotificationToProjectAsync(int projectId, string message, object? data = null);
        Task SendNotificationToDepartmentAsync(int departmentId, string message, object? data = null);
        Task SendTaskUpdateAsync(int taskId, string action, object taskData);
        Task SendProjectUpdateAsync(int projectId, string action, object projectData);
        Task BroadcastSystemAnnouncementAsync(string message, object? data = null);
        Task SendToUsersAsync(List<int> userIds, string eventName, object data);
        Task SendToUserAsync(int userId, string eventName, object data);
    }

    public class RealTimeService : IRealTimeService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<RealTimeService> _logger;

        public RealTimeService(IHubContext<NotificationHub> hubContext, BarqTMSDbContext context, ILogger<RealTimeService> logger)
        {
            _hubContext = hubContext;
            _context = context;
            _logger = logger;
        }

        public async Task SendNotificationToUserAsync(int userId, string message, object? data = null)
        {
            try
            {
                await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    Type = "notification"
                });

                _logger.LogInformation("Sent notification to user {UserId}: {Message}", userId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToProjectAsync(int projectId, string message, object? data = null)
        {
            try
            {
                await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    Type = "project_notification"
                });

                _logger.LogInformation("Sent notification to project {ProjectId}: {Message}", projectId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to project {ProjectId}", projectId);
            }
        }

        public async Task SendNotificationToDepartmentAsync(int departmentId, string message, object? data = null)
        {
            try
            {
                await _hubContext.Clients.Group($"Department_{departmentId}").SendAsync("ReceiveNotification", new
                {
                    Message = message,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    Type = "department_notification"
                });

                _logger.LogInformation("Sent notification to department {DepartmentId}: {Message}", departmentId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to department {DepartmentId}", departmentId);
            }
        }

        public async Task SendTaskUpdateAsync(int taskId, string action, object taskData)
        {
            try
            {
                // Get task details to determine project and department
                var task = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.Department)
                    .FirstOrDefaultAsync(t => t.TaskId == taskId);

                if (task != null)
                {
                    var updateData = new
                    {
                        TaskId = taskId,
                        Action = action,
                        TaskData = taskData,
                        Timestamp = DateTime.UtcNow,
                        Type = "task_update"
                    };

                    // Send to project group
                    await _hubContext.Clients.Group($"Project_{task.ProjectId}").SendAsync("ReceiveTaskUpdate", updateData);

                    // Send to department group
                    await _hubContext.Clients.Group($"Department_{task.DeptId}").SendAsync("ReceiveTaskUpdate", updateData);

                    // Send to assigned user if any
                    if (task.AssignedTo.HasValue)
                    {
                        await _hubContext.Clients.Group($"User_{task.AssignedTo}").SendAsync("ReceiveTaskUpdate", updateData);
                    }

                    _logger.LogInformation("Sent task update for task {TaskId}: {Action}", taskId, action);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send task update for task {TaskId}", taskId);
            }
        }

        public async Task SendProjectUpdateAsync(int projectId, string action, object projectData)
        {
            try
            {
                var updateData = new
                {
                    ProjectId = projectId,
                    Action = action,
                    ProjectData = projectData,
                    Timestamp = DateTime.UtcNow,
                    Type = "project_update"
                };

                await _hubContext.Clients.Group($"Project_{projectId}").SendAsync("ReceiveProjectUpdate", updateData);

                _logger.LogInformation("Sent project update for project {ProjectId}: {Action}", projectId, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send project update for project {ProjectId}", projectId);
            }
        }

        public async Task BroadcastSystemAnnouncementAsync(string message, object? data = null)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveSystemAnnouncement", new
                {
                    Message = message,
                    Data = data,
                    Timestamp = DateTime.UtcNow,
                    Type = "system_announcement"
                });

                _logger.LogInformation("Sent system announcement: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send system announcement");
            }
        }

        public async Task SendToUsersAsync(List<int> userIds, string eventName, object data)
        {
            try
            {
                foreach (var userId in userIds)
                {
                    await _hubContext.Clients.Group($"User_{userId}").SendAsync(eventName, data);
                }
                _logger.LogInformation("Sent {EventName} to {UserCount} users", eventName, userIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {EventName} to users", eventName);
            }
        }

        public async Task SendToUserAsync(int userId, string eventName, object data)
        {
            try
            {
                await _hubContext.Clients.Group($"User_{userId}").SendAsync(eventName, data);
                _logger.LogInformation("Sent {EventName} to user {UserId}", eventName, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {EventName} to user {UserId}", eventName, userId);
            }
        }
    }
}