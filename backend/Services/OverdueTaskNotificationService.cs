using Microsoft.EntityFrameworkCore;
using BarqTMS.API.Data;
using BarqTMS.API.Models;

namespace BarqTMS.API.Services
{
    public class OverdueTaskNotificationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueTaskNotificationService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public OverdueTaskNotificationService(
            IServiceProvider serviceProvider,
            ILogger<OverdueTaskNotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Overdue Task Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyOverdueTasks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking overdue tasks");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckAndNotifyOverdueTasks()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BarqTMSDbContext>();
            var realTimeService = scope.ServiceProvider.GetRequiredService<IRealTimeService>();

            var now = DateTime.UtcNow;

            // Get tasks that are overdue and not completed/cancelled
            var overdueTasks = await context.Tasks
                .Include(t => t.AssignedUser)
                    .ThenInclude(u => u!.TeamLeader)
                .Include(t => t.Status)
                .Include(t => t.Department)
                .Where(t => t.DueDate.HasValue &&
                           t.DueDate.Value < now &&
                           t.StatusId != 4 && // Not Done
                           t.StatusId != 5)   // Not Cancelled
                .ToListAsync();

            if (!overdueTasks.Any())
            {
                _logger.LogInformation("No overdue tasks found");
                return;
            }

            _logger.LogInformation("Found {Count} overdue tasks", overdueTasks.Count);

            foreach (var task in overdueTasks)
            {
                // Check if we already sent a notification today for this task
                var todayStart = DateTime.UtcNow.Date;
                var existingNotification = await context.Notifications
                    .Where(n => n.TaskId == task.TaskId &&
                               n.Message.Contains("overdue") &&
                               n.CreatedAt >= todayStart)
                    .AnyAsync();

                if (existingNotification)
                {
                    continue; // Already notified today
                }

                var daysOverdue = (now - task.DueDate!.Value).Days;
                var message = $"Task '{task.Title}' is overdue by {daysOverdue} day(s). Due date was {task.DueDate.Value:MMM dd, yyyy}.";

                var notificationIds = new List<int>();

                // 1. Notify the assigned user
                if (task.AssignedTo.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = task.AssignedTo.Value,
                        Message = message,
                        TaskId = task.TaskId,
                        ProjectId = task.ProjectId,
                        CreatedAt = now
                    };
                    context.Notifications.Add(notification);
                    notificationIds.Add(task.AssignedTo.Value);
                }

                // 2. Notify the team leader
                if (task.AssignedUser?.TeamLeaderId.HasValue == true)
                {
                    var teamLeaderNotification = new Notification
                    {
                        UserId = task.AssignedUser.TeamLeaderId.Value,
                        Message = $"Team member's task is overdue: {message}",
                        TaskId = task.TaskId,
                        ProjectId = task.ProjectId,
                        CreatedAt = now
                    };
                    context.Notifications.Add(teamLeaderNotification);
                    notificationIds.Add(task.AssignedUser.TeamLeaderId.Value);
                }

                // 3. Notify managers and assistant managers
                var managersAndAssistants = await context.Users
                    .Where(u => u.IsActive &&
                               (u.Role == UserRole.Manager || u.Role == UserRole.AssistantManager))
                    .Select(u => u.UserId)
                    .ToListAsync();

                foreach (var managerId in managersAndAssistants)
                {
                    var managerNotification = new Notification
                    {
                        UserId = managerId,
                        Message = $"Overdue task requires attention: {message}",
                        TaskId = task.TaskId,
                        ProjectId = task.ProjectId,
                        CreatedAt = now
                    };
                    context.Notifications.Add(managerNotification);
                    notificationIds.Add(managerId);
                }

                await context.SaveChangesAsync();

                // Send real-time notifications
                foreach (var userId in notificationIds.Distinct())
                {
                    await realTimeService.SendNotificationToUserAsync(
                        userId,
                        message,
                        new
                        {
                            TaskId = task.TaskId,
                            TaskTitle = task.Title,
                            DueDate = task.DueDate,
                            DaysOverdue = daysOverdue,
                            Type = "overdue_task"
                        });
                }

                _logger.LogInformation(
                    "Sent overdue notifications for task {TaskId} to {Count} users",
                    task.TaskId,
                    notificationIds.Distinct().Count());
            }
        }
    }
}
