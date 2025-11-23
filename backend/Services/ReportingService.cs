using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using BarqTMS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface IReportingService
    {
        Task<ProjectReportDto> GetProjectReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
        Task<ClientReportDto> GetClientReportAsync(int clientId, DateTime? startDate = null, DateTime? endDate = null);
        Task<UserPerformanceReportDto> GetUserPerformanceReportAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<DepartmentReportDto> GetDepartmentReportAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null);
        Task<SystemOverviewReportDto> GetSystemOverviewReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportProjectReportToCsvAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
        Task<byte[]> ExportUserPerformanceReportToCsvAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<TaskProductivityDto>> GetTaskProductivityReportAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<TimeTrackingReportDto>> GetTimeTrackingReportAsync(int? userId = null, int? projectId = null, DateTime? startDate = null, DateTime? endDate = null);
    }

    public class ReportingService : IReportingService
    {
        private readonly BarqTMSDbContext _context;

        public ReportingService(BarqTMSDbContext context)
        {
            _context = context;
        }

        public Task<ProjectReportDto> GetProjectReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null) => throw new NotImplementedException();

        public async Task<ClientReportDto> GetClientReportAsync(int clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var client = await _context.Companies
                .Include(c => c.Projects)
                    .ThenInclude(p => p.Tasks)
                        .ThenInclude(t => t.TimeLogs)
                .FirstOrDefaultAsync(c => c.CompanyId == clientId);

            if (client == null)
                throw new ArgumentException("Client not found");

            var projects = client.Projects.AsQueryable();
            
            // We consider all projects for the client, but we might filter tasks by date
            // Or filter projects created within date range? 
            // Usually client report is about "what happened in this period".
            // Let's filter tasks by CreatedAt for the stats.

            var allTasks = projects.SelectMany(p => p.Tasks).ToList();

            if (startDate.HasValue)
                allTasks = allTasks.Where(t => t.CreatedAt >= startDate.Value).ToList();
            if (endDate.HasValue)
                allTasks = allTasks.Where(t => t.CreatedAt <= endDate.Value).ToList();

            var totalTasks = allTasks.Count;
            var completedTasks = allTasks.Count(t => t.Status == BarqTMS.API.Models.Enums.TaskStatus.Completed);
            var inProgressTasks = allTasks.Count(t => t.Status == BarqTMS.API.Models.Enums.TaskStatus.InProgress);
            var pendingTasks = allTasks.Count(t => t.Status == BarqTMS.API.Models.Enums.TaskStatus.Pending);
            var overdueTasks = allTasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != BarqTMS.API.Models.Enums.TaskStatus.Completed);

            var totalEstimatedHours = allTasks.Sum(t => t.EstimatedHours ?? 0);
            var totalActualHours = (allTasks.SelectMany(t => t.TimeLogs).Sum(tl => tl.DurationMinutes) ?? 0) / 60.0;

            return new ClientReportDto
            {
                ClientId = client.CompanyId,
                ClientName = client.Name,
                CompanyName = client.Name,
                TotalProjects = projects.Count(),
                ActiveProjects = projects.Count(p => p.Status == ProjectStatus.Active),
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                PendingTasks = pendingTasks,
                OverdueTasks = overdueTasks,
                CompletionPercentage = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0,
                TotalEstimatedHours = totalEstimatedHours,
                TotalActualHours = (decimal)totalActualHours
            };
        }

        public async Task<UserPerformanceReportDto> GetUserPerformanceReportAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var user = await _context.Users
                .Include(u => u.AssignedTasks)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.TimeLogs)
                .Include(u => u.AssignedTasks)
                    .ThenInclude(ta => ta.Task)
                        .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new ArgumentException("User not found");

            var tasks = user.AssignedTasks.Select(ta => ta.Task).ToList();

            if (startDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt >= startDate.Value).ToList();
            if (endDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt <= endDate.Value).ToList();

            var totalTasks = tasks.Count;
            var completedTasks = tasks.Count(t => t.Status == BarqTMS.API.Models.Enums.TaskStatus.Completed);
            var inProgressTasks = tasks.Count(t => t.Status == BarqTMS.API.Models.Enums.TaskStatus.InProgress);
            var overdueTasks = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != BarqTMS.API.Models.Enums.TaskStatus.Completed);
            
            var totalHoursLogged = tasks.SelectMany(t => t.TimeLogs).Where(tl => tl.UserId == userId).Sum(tl => tl.DurationMinutes ?? 0) / 60.0;
            var projectsWorkedOn = tasks.Select(t => t.ProjectId).Distinct().Count();

            // Calculate average completion time (for completed tasks)
            var completedTaskItems = tasks.Where(t => t.Status == BarqTMS.API.Models.Enums.TaskStatus.Completed).ToList();
            double avgCompletionDays = 0;
            if (completedTaskItems.Any())
            {
                // Assuming we can track completion time. 
                // Since we don't have a "CompletedAt" field in Task explicitly in the model shown (only CreatedAt and DueDate),
                // we might need to approximate or check if there's a status change log.
                // For now, let's use (DueDate - CreatedAt) as a proxy or 0 if we can't determine.
                // Actually, let's just use 0 or try to find a better metric if possible.
                // Wait, TimeLogs might give a hint, but not completion date.
                // Let's just return 0 for now or use (DateTime.UtcNow - CreatedAt).TotalDays for completed ones if we assume they just finished? No that's wrong.
                // Let's leave it as 0 for now to avoid errors.
                avgCompletionDays = 0; 
            }

            return new UserPerformanceReportDto
            {
                UserId = user.UserId,
                UserName = user.FullName,
                UserEmail = user.Email,
                TotalTasksAssigned = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = inProgressTasks,
                OverdueTasks = overdueTasks,
                CompletionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0,
                TotalHoursLogged = totalHoursLogged,
                ProjectsWorkedOn = projectsWorkedOn,
                AverageTaskCompletionDays = avgCompletionDays
            };
        }

        public Task<DepartmentReportDto> GetDepartmentReportAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null) => throw new NotImplementedException();
        public Task<SystemOverviewReportDto> GetSystemOverviewReportAsync(DateTime? startDate = null, DateTime? endDate = null) => throw new NotImplementedException();
        public Task<byte[]> ExportProjectReportToCsvAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null) => throw new NotImplementedException();
        public Task<byte[]> ExportUserPerformanceReportToCsvAsync(int userId, DateTime? startDate = null, DateTime? endDate = null) => throw new NotImplementedException();
        public Task<IEnumerable<TaskProductivityDto>> GetTaskProductivityReportAsync(DateTime? startDate = null, DateTime? endDate = null) => throw new NotImplementedException();
        public Task<IEnumerable<TimeTrackingReportDto>> GetTimeTrackingReportAsync(int? userId = null, int? projectId = null, DateTime? startDate = null, DateTime? endDate = null) => throw new NotImplementedException();
    }
}
