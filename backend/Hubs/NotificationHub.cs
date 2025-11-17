using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Helpers;

namespace BarqTMS.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User != null)
            {
                var userId = UserContextHelper.GetCurrentUserId(Context.User);
                if (userId.HasValue)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                    _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User != null)
            {
                var userId = UserContextHelper.GetCurrentUserId(Context.User);
                if (userId.HasValue)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                    _logger.LogInformation("User {UserId} disconnected with connection {ConnectionId}", userId, Context.ConnectionId);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinProjectGroup(int projectId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        public async Task LeaveProjectGroup(int projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
        }

        public async Task JoinDepartmentGroup(int departmentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Department_{departmentId}");
        }

        public async Task LeaveDepartmentGroup(int departmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Department_{departmentId}");
        }
    }
}