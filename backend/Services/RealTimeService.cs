using BarqTMS.API.Models;

namespace BarqTMS.API.Services
{
    public interface IRealTimeService
    {
        Task NotifyTaskAssigned(WorkTask task, string userId);
        Task NotifyTaskStatusChanged(WorkTask task, string oldStatus, string newStatus);
        Task NotifyTaskCommentAdded(TaskComment comment);
        Task NotifyTaskOverdue(WorkTask task);
    }

    public class RealTimeService : IRealTimeService
    {
        public Task NotifyTaskAssigned(WorkTask task, string userId) => throw new NotImplementedException();
        public Task NotifyTaskStatusChanged(WorkTask task, string oldStatus, string newStatus) => throw new NotImplementedException();
        public Task NotifyTaskCommentAdded(TaskComment comment) => throw new NotImplementedException();
        public Task NotifyTaskOverdue(WorkTask task) => throw new NotImplementedException();
    }
}
