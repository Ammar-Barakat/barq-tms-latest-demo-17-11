using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.Helpers;
using System;
using System.Collections.Generic;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(BarqTMSDbContext context, ILogger<StatisticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/statistics/dashboard
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardStats>> GetDashboardStatistics()
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var currentUser = await _context.Users.FindAsync(currentUserId);

                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                var stats = new DashboardStats();

                // Get tasks based on role
                var tasksQuery = _context.Tasks.AsQueryable();

                switch (currentUser.Role)
                {
                    case UserRole.Employee:
                        tasksQuery = tasksQuery.Where(t => t.AssignedTo == currentUserId);
                        break;

                    case UserRole.Client:
                        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                        if (client != null)
                        {
                            var clientProjectIds = await _context.Projects
                                .Where(p => p.ClientId == client.ClientId)
                                .Select(p => p.ProjectId)
                                .ToListAsync();
                            tasksQuery = tasksQuery.Where(t => t.ProjectId.HasValue && clientProjectIds.Contains(t.ProjectId.Value));
                        }
                        break;

                    case UserRole.TeamLeader:
                        var userDepartments = await _context.UserDepartments
                            .Where(ud => ud.UserId == currentUserId)
                            .Select(ud => ud.DeptId)
                            .ToListAsync();
                        if (userDepartments.Any())
                        {
                            tasksQuery = tasksQuery.Where(t => userDepartments.Contains(t.DeptId));
                        }
                        break;
                }

                var statusLookup = await GetStatusLookupAsync();
                statusLookup.TryGetValue("To Do", out var toDoStatusId);
                statusLookup.TryGetValue("In Progress", out var inProgressStatusId);
                statusLookup.TryGetValue("Done", out var doneStatusId);

                // Task statistics
                stats.TotalTasks = await tasksQuery.CountAsync();
                stats.PendingTasks = toDoStatusId > 0
                    ? await tasksQuery.Where(t => t.StatusId == toDoStatusId).CountAsync()
                    : 0;
                stats.InProgressTasks = inProgressStatusId > 0
                    ? await tasksQuery.Where(t => t.StatusId == inProgressStatusId).CountAsync()
                    : 0;
                stats.CompletedTasks = doneStatusId > 0
                    ? await tasksQuery.Where(t => t.StatusId == doneStatusId).CountAsync()
                    : 0;
                stats.OverdueTasks = await tasksQuery
                    .Where(t => t.DueDate < DateTime.UtcNow && (doneStatusId == 0 || t.StatusId != doneStatusId))
                    .CountAsync();

                // Project statistics (based on role)
                var projectsQuery = _context.Projects.AsQueryable();

                if (currentUser.Role == UserRole.Client)
                {
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                    if (client != null)
                    {
                        projectsQuery = projectsQuery.Where(p => p.ClientId == client.ClientId);
                    }
                }

                stats.TotalProjects = await projectsQuery.CountAsync();
                stats.ActiveProjects = await projectsQuery
                    .Where(p => p.EndDate >= DateTime.UtcNow)
                    .CountAsync();
                stats.CompletedProjects = await projectsQuery
                    .Where(p => p.EndDate < DateTime.UtcNow)
                    .CountAsync();

                // User statistics
                if (currentUser.Role == UserRole.Manager || currentUser.Role == UserRole.AssistantManager || currentUser.Role == UserRole.TeamLeader)
                {
                    stats.TotalUsers = await _context.Users.Where(u => u.IsActive).CountAsync();
                    stats.TotalDepartments = await _context.Departments.CountAsync();
                }

                // Recent activity
                stats.RecentTasks = await tasksQuery
                    .OrderByDescending(t => t.TaskId)
                    .Take(5)
                    .Select(t => new TaskSummary
                    {
                        TaskId = t.TaskId,
                        Title = t.Title,
                        StatusName = t.Status.StatusName,
                        DueDate = t.DueDate
                    })
                    .ToListAsync();

                // Tasks by priority
                stats.HighPriorityTasks = await tasksQuery.Where(t => t.PriorityId == 1).CountAsync();
                stats.MediumPriorityTasks = await tasksQuery.Where(t => t.PriorityId == 2).CountAsync();
                stats.LowPriorityTasks = await tasksQuery.Where(t => t.PriorityId == 3).CountAsync();

                // Weekly progress (tasks completed this week)
                var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
                stats.TasksCompletedThisWeek = doneStatusId > 0
                    ? await tasksQuery
                        .Where(t => t.StatusId == doneStatusId && t.DueDate >= startOfWeek)
                        .CountAsync()
                    : 0;

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                return StatusCode(500, "An error occurred while retrieving statistics");
            }
        }

        // GET: api/statistics/tasks-by-status
        [HttpGet("tasks-by-status")]
        public async Task<ActionResult<object>> GetTasksByStatus()
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var currentUser = await _context.Users.FindAsync(currentUserId);

                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                var tasksQuery = _context.Tasks.AsQueryable();

                // Filter based on role
                switch (currentUser.Role)
                {
                    case UserRole.Employee:
                        tasksQuery = tasksQuery.Where(t => t.AssignedTo == currentUserId);
                        break;

                    case UserRole.Client:
                        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                        if (client != null)
                        {
                            var clientProjectIds = await _context.Projects
                                .Where(p => p.ClientId == client.ClientId)
                                .Select(p => p.ProjectId)
                                .ToListAsync();
                            tasksQuery = tasksQuery.Where(t => t.ProjectId.HasValue && clientProjectIds.Contains(t.ProjectId.Value));
                        }
                        break;

                    case UserRole.TeamLeader:
                        var userDepartments = await _context.UserDepartments
                            .Where(ud => ud.UserId == currentUserId)
                            .Select(ud => ud.DeptId)
                            .ToListAsync();
                        if (userDepartments.Any())
                        {
                            tasksQuery = tasksQuery.Where(t => userDepartments.Contains(t.DeptId));
                        }
                        break;
                }

                var tasksByStatus = await tasksQuery
                    .Include(t => t.Status)
                    .GroupBy(t => t.Status.StatusName)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(tasksByStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks by status");
                return StatusCode(500, "An error occurred while retrieving task statistics");
            }
        }

        // GET: api/statistics/tasks-by-priority
        [HttpGet("tasks-by-priority")]
        public async Task<ActionResult<object>> GetTasksByPriority()
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var currentUser = await _context.Users.FindAsync(currentUserId);

                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                var tasksQuery = _context.Tasks.AsQueryable();

                // Filter based on role
                switch (currentUser.Role)
                {
                    case UserRole.Employee:
                        tasksQuery = tasksQuery.Where(t => t.AssignedTo == currentUserId);
                        break;

                    case UserRole.Client:
                        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                        if (client != null)
                        {
                            var clientProjectIds = await _context.Projects
                                .Where(p => p.ClientId == client.ClientId)
                                .Select(p => p.ProjectId)
                                .ToListAsync();
                            tasksQuery = tasksQuery.Where(t => t.ProjectId.HasValue && clientProjectIds.Contains(t.ProjectId.Value));
                        }
                        break;

                    case UserRole.TeamLeader:
                        var userDepartments = await _context.UserDepartments
                            .Where(ud => ud.UserId == currentUserId)
                            .Select(ud => ud.DeptId)
                            .ToListAsync();
                        if (userDepartments.Any())
                        {
                            tasksQuery = tasksQuery.Where(t => userDepartments.Contains(t.DeptId));
                        }
                        break;
                }

                var tasksByPriority = await tasksQuery
                    .Include(t => t.Priority)
                    .GroupBy(t => t.Priority.Level)
                    .Select(g => new
                    {
                        Priority = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(tasksByPriority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks by priority");
                return StatusCode(500, "An error occurred while retrieving task statistics");
            }
        }

        // GET: api/statistics/project-progress
        [HttpGet("project-progress")]
        public async Task<ActionResult<object>> GetProjectProgress()
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
                var currentUser = await _context.Users.FindAsync(currentUserId);

                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                var statusLookup = await GetStatusLookupAsync();
                statusLookup.TryGetValue("Done", out var doneStatusId);

                var projectsQuery = _context.Projects.AsQueryable();

                if (currentUser.Role == UserRole.Client)
                {
                    var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                    if (client != null)
                    {
                        projectsQuery = projectsQuery.Where(p => p.ClientId == client.ClientId);
                    }
                }

                var projectProgress = await projectsQuery
                    .Select(p => new
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        TotalTasks = p.Tasks.Count(),
                        CompletedTasks = doneStatusId > 0 ? p.Tasks.Count(t => t.StatusId == doneStatusId) : 0,
                        Progress = p.Tasks.Count() > 0
                            ? (int)((double)(doneStatusId > 0 ? p.Tasks.Count(t => t.StatusId == doneStatusId) : 0) / p.Tasks.Count() * 100)
                            : 0
                    })
                    .OrderByDescending(p => p.Progress)
                    .Take(10)
                    .ToListAsync();

                return Ok(projectProgress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project progress");
                return StatusCode(500, "An error occurred while retrieving project progress");
            }
        }

        private async Task<Dictionary<string, int>> GetStatusLookupAsync()
        {
            var statuses = await _context.Statuses
                .Select(s => new { s.StatusId, s.StatusName })
                .ToListAsync();

            return statuses.ToDictionary(
                s => s.StatusName,
                s => s.StatusId,
                StringComparer.OrdinalIgnoreCase);
        }
    }

    // DTOs
    public class DashboardStats
    {
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CompletedProjects { get; set; }
        public int TotalUsers { get; set; }
        public int TotalDepartments { get; set; }
        public int HighPriorityTasks { get; set; }
        public int MediumPriorityTasks { get; set; }
        public int LowPriorityTasks { get; set; }
        public int TasksCompletedThisWeek { get; set; }
        public List<TaskSummary> RecentTasks { get; set; } = new();
    }

    public class TaskSummary
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
    }
}
