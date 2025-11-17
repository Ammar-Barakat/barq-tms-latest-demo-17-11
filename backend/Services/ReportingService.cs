using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Threading.Tasks;

namespace BarqTMS.API.Services
{
    public interface IReportingService
    {
        Task<ProjectReportDto> GetProjectReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null);
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
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(BarqTMSDbContext context, ILogger<ReportingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProjectReportDto> GetProjectReportAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Status)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.Priority)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);

            if (project == null)
                throw new ArgumentException($"Project with ID {projectId} not found");

            var tasks = project.Tasks.AsQueryable();
            if (startDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt >= startDate);
            if (endDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt <= endDate);

            var taskList = tasks.ToList();

            return new ProjectReportDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                ClientName = project.Client?.Name ?? string.Empty,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                TotalTasks = taskList.Count,
                CompletedTasks = taskList.Count(t => t.Status.StatusName == "Done"),
                InProgressTasks = taskList.Count(t => t.Status.StatusName == "In Progress"),
                PendingTasks = taskList.Count(t => t.Status.StatusName == "To Do"),
                OverdueTasks = taskList.Count(t => t.DueDate < DateTime.Now && t.Status.StatusName != "Done"),
                HighPriorityTasks = taskList.Count(t => t.Priority.Level == "High" || t.Priority.Level == "Critical"),
                CompletionPercentage = taskList.Count > 0 ? (double)taskList.Count(t => t.Status.StatusName == "Done") / taskList.Count * 100 : 0,
                TotalEstimatedHours = taskList.Sum(t => t.EstimatedHours ?? 0),
                TotalActualHours = taskList.Sum(t => t.ActualHours ?? 0)
            };
        }

        public async Task<UserPerformanceReportDto> GetUserPerformanceReportAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var user = await _context.Users
                .Include(u => u.AssignedTasks)
                    .ThenInclude(t => t.Status)
                .Include(u => u.AssignedTasks)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found");

            var tasks = user.AssignedTasks.AsQueryable();
            if (startDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt >= startDate);
            if (endDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt <= endDate);

            var taskList = tasks.ToList();

            var timeLogs = await _context.TimeLogs
                .Where(tl => tl.UserId == userId)
                .Where(tl => !startDate.HasValue || tl.StartTime >= startDate)
                .Where(tl => !endDate.HasValue || tl.StartTime <= endDate)
                .ToListAsync();

            return new UserPerformanceReportDto
            {
                UserId = user.UserId,
                UserName = user.Name,
                UserEmail = user.Email ?? string.Empty,
                TotalTasksAssigned = taskList.Count,
                CompletedTasks = taskList.Count(t => t.Status.StatusName == "Done"),
                InProgressTasks = taskList.Count(t => t.Status.StatusName == "In Progress"),
                OverdueTasks = taskList.Count(t => t.DueDate < DateTime.Now && t.Status.StatusName != "Done"),
                CompletionRate = taskList.Count > 0 ? (double)taskList.Count(t => t.Status.StatusName == "Done") / taskList.Count * 100 : 0,
                TotalHoursLogged = timeLogs.Sum(tl => tl.DurationMinutes ?? 0) / 60.0,
                ProjectsWorkedOn = taskList.Select(t => t.Project.ProjectName).Distinct().Count(),
                AverageTaskCompletionDays = CalculateAverageCompletionDays(taskList.Where(t => t.Status.StatusName == "Done"))
            };
        }

        public async Task<DepartmentReportDto> GetDepartmentReportAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var department = await _context.Departments
                .Include(d => d.Tasks)
                    .ThenInclude(t => t.Status)
                .Include(d => d.UserDepartments)
                    .ThenInclude(ud => ud.User)
                .FirstOrDefaultAsync(d => d.DeptId == departmentId);

            if (department == null)
                throw new ArgumentException($"Department with ID {departmentId} not found");

            var tasks = department.Tasks.AsQueryable();
            if (startDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt >= startDate);
            if (endDate.HasValue)
                tasks = tasks.Where(t => t.CreatedAt <= endDate);

            var taskList = tasks.ToList();

            return new DepartmentReportDto
            {
                DepartmentId = department.DeptId,
                DepartmentName = department.DeptName,
                TotalEmployees = department.UserDepartments.Count,
                TotalTasks = taskList.Count,
                CompletedTasks = taskList.Count(t => t.Status.StatusName == "Done"),
                InProgressTasks = taskList.Count(t => t.Status.StatusName == "In Progress"),
                OverdueTasks = taskList.Count(t => t.DueDate < DateTime.Now && t.Status.StatusName != "Done"),
                ProductivityScore = CalculateDepartmentProductivityScore(taskList),
                TopPerformers = await GetTopPerformersInDepartment(departmentId, 3)
            };
        }

        public async Task<SystemOverviewReportDto> GetSystemOverviewReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var tasksQuery = _context.Tasks.AsQueryable();
            var usersQuery = _context.Users.Where(u => u.IsActive);
            var projectsQuery = _context.Projects.AsQueryable();

            if (startDate.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.CreatedAt >= startDate);
                projectsQuery = projectsQuery.Where(p => p.StartDate >= startDate);
            }
            if (endDate.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.CreatedAt <= endDate);
                projectsQuery = projectsQuery.Where(p => p.StartDate <= endDate);
            }

            var tasks = await tasksQuery.Include(t => t.Status).ToListAsync();
            var users = await usersQuery.ToListAsync();
            var projects = await projectsQuery.ToListAsync();

            return new SystemOverviewReportDto
            {
                TotalUsers = users.Count,
                ActiveUsers = users.Count(u => u.LastLogin > DateTime.UtcNow.AddDays(-30)),
                TotalProjects = projects.Count,
                ActiveProjects = projects.Count(p => p.EndDate == null || p.EndDate > DateTime.Now),
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status.StatusName == "Done"),
                InProgressTasks = tasks.Count(t => t.Status.StatusName == "In Progress"),
                OverdueTasks = tasks.Count(t => t.DueDate < DateTime.Now && t.Status.StatusName != "Done"),
                OverallProductivity = tasks.Count > 0 ? (double)tasks.Count(t => t.Status.StatusName == "Done") / tasks.Count * 100 : 0,
                TotalDepartments = await _context.Departments.CountAsync()
            };
        }

        public async Task<byte[]> ExportProjectReportToCsvAsync(int projectId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var report = await GetProjectReportAsync(projectId, startDate, endDate);
            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.AssignedUser)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Task ID,Title,Status,Priority,Assigned To,Due Date,Created Date,Estimated Hours,Actual Hours");

            foreach (var task in tasks)
            {
                csv.AppendLine($"{task.TaskId},{EscapeCsv(task.Title)},{EscapeCsv(task.Status.StatusName)},{EscapeCsv(task.Priority.Level)},{EscapeCsv(task.AssignedUser?.Name ?? "Unassigned")},{task.DueDate:yyyy-MM-dd},{task.CreatedAt:yyyy-MM-dd},{task.EstimatedHours},{task.ActualHours}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<byte[]> ExportUserPerformanceReportToCsvAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var report = await GetUserPerformanceReportAsync(userId, startDate, endDate);
            var tasks = await _context.Tasks
                .Where(t => t.AssignedTo == userId)
                .Include(t => t.Status)
                .Include(t => t.Project)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Task ID,Title,Project,Status,Due Date,Completed Date,Estimated Hours,Actual Hours");

            foreach (var task in tasks)
            {
                csv.AppendLine($"{task.TaskId},{EscapeCsv(task.Title)},{EscapeCsv(task.Project.ProjectName)},{EscapeCsv(task.Status.StatusName)},{task.DueDate:yyyy-MM-dd},{(task.Status.StatusName == "Done" ? task.UpdatedAt.ToString("yyyy-MM-dd") : "")},{task.EstimatedHours},{task.ActualHours}");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }

        public async Task<IEnumerable<TaskProductivityDto>> GetTaskProductivityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var tasksQuery = _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.AssignedUser)
                .Include(t => t.Project)
                .AsQueryable();

            if (startDate.HasValue)
                tasksQuery = tasksQuery.Where(t => t.CreatedAt >= startDate);
            if (endDate.HasValue)
                tasksQuery = tasksQuery.Where(t => t.CreatedAt <= endDate);

            var tasks = await tasksQuery.ToListAsync();

            return tasks.Select(t => new TaskProductivityDto
            {
                TaskId = t.TaskId,
                Title = t.Title,
                ProjectName = t.Project.ProjectName,
                AssignedUserName = t.AssignedUser?.Name ?? "Unassigned",
                StatusName = t.Status.StatusName,
                PriorityLevel = t.Priority.Level,
                EstimatedHours = t.EstimatedHours ?? 0,
                ActualHours = t.ActualHours ?? 0,
                EfficiencyRatio = (t.EstimatedHours > 0 && t.ActualHours > 0) ? (double)t.EstimatedHours / (double)t.ActualHours : 0,
                DaysToComplete = t.Status.StatusName == "Done" ? (t.UpdatedAt - t.CreatedAt).Days : 0,
                IsOverdue = t.DueDate < DateTime.Now && t.Status.StatusName != "Done"
            });
        }

        public async Task<IEnumerable<TimeTrackingReportDto>> GetTimeTrackingReportAsync(int? userId = null, int? projectId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var timeLogsQuery = _context.TimeLogs
                .Include(tl => tl.User)
                .Include(tl => tl.Task)
                    .ThenInclude(t => t.Project)
                .AsQueryable();

            if (userId.HasValue)
                timeLogsQuery = timeLogsQuery.Where(tl => tl.UserId == userId);
            if (projectId.HasValue)
                timeLogsQuery = timeLogsQuery.Where(tl => tl.Task.ProjectId == projectId);
            if (startDate.HasValue)
                timeLogsQuery = timeLogsQuery.Where(tl => tl.StartTime >= startDate);
            if (endDate.HasValue)
                timeLogsQuery = timeLogsQuery.Where(tl => tl.StartTime <= endDate);

            var timeLogs = await timeLogsQuery.ToListAsync();

            return timeLogs.Select(tl => new TimeTrackingReportDto
            {
                TimeLogId = tl.TimeLogId,
                UserName = tl.User.Name,
                TaskTitle = tl.Task.Title,
                ProjectName = tl.Task.Project.ProjectName,
                StartTime = tl.StartTime,
                EndTime = tl.EndTime,
                DurationHours = (tl.DurationMinutes ?? 0) / 60.0,
                Description = tl.Description,
                IsBillable = tl.IsBillable
            });
        }

        private double CalculateAverageCompletionDays(IEnumerable<WorkTask> completedTasks)
        {
            var completionDays = completedTasks.Select(t => (t.UpdatedAt - t.CreatedAt).TotalDays).ToList();
            return completionDays.Any() ? completionDays.Average() : 0;
        }

        private double CalculateDepartmentProductivityScore(List<WorkTask> tasks)
        {
            if (!tasks.Any()) return 0;

            var completedTasks = tasks.Count(t => t.Status.StatusName == "Done");
            var totalTasks = tasks.Count;
            var onTimeCompletions = tasks.Count(t => t.Status.StatusName == "Done" && (t.DueDate == null || t.UpdatedAt <= t.DueDate));

            var completionRate = (double)completedTasks / totalTasks;
            var onTimeRate = completedTasks > 0 ? (double)onTimeCompletions / completedTasks : 0;

            return (completionRate * 0.7 + onTimeRate * 0.3) * 100;
        }

        private async System.Threading.Tasks.Task<List<string>> GetTopPerformersInDepartment(int departmentId, int count)
        {
            var userPerformances = await _context.UserDepartments
                .Where(ud => ud.DeptId == departmentId)
                .Select(ud => new
                {
                    UserName = ud.User.Name,
                    CompletedTasks = ud.User.AssignedTasks.Count(t => t.Status.StatusName == "Done"),
                    TotalTasks = ud.User.AssignedTasks.Count()
                })
                .Where(up => up.TotalTasks > 0)
                .OrderByDescending(up => (double)up.CompletedTasks / up.TotalTasks)
                .Take(count)
                .ToListAsync();

            return userPerformances.Select(up => up.UserName).ToList();
        }

        private string EscapeCsv(string value)
        {
            if (value == null) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}