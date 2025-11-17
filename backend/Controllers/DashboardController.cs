using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarqTMS.API.Services;
using BarqTMS.API.Data;
using BarqTMS.API.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;

        public DashboardController(BarqTMSDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var currentUserId = UserContextHelper.GetCurrentUserId(User);
                
                // إحصائيات المهام
                var totalTasks = await _context.Tasks.CountAsync();
                var completedTasks = await _context.Tasks.CountAsync(t => t.StatusId == 4); // Assuming status 4 is completed
                var inProgressTasks = await _context.Tasks.CountAsync(t => t.StatusId == 2); // Assuming status 2 is in progress
                var overdueTasks = await _context.Tasks.CountAsync(t => t.DueDate < DateTime.Now && t.StatusId != 4);
                
                // إحصائيات المشاريع
                var totalProjects = await _context.Projects.CountAsync();
                var activeProjects = await _context.Projects.CountAsync(p => p.EndDate == null || p.EndDate > DateTime.Now);
                
                // إحصائيات الفريق
                var totalTeamMembers = await _context.Users.CountAsync(u => u.IsActive);

                // الأنشطة الحديثة (من جدول AuditLogs)
                var taskHistoryData = await _context.AuditLogs
                    .Where(al => al.EntityType == "Task")
                    .Include(al => al.User)
                    .OrderByDescending(al => al.Timestamp)
                    .Take(10)
                    .ToListAsync();

                var recentActivities = taskHistoryData.Select(al => new
                    {
                        Id = al.AuditId,
                        Type = "task_update",
                        Message = $"تم تحديث المهمة: {al.EntityType}",
                        Timestamp = GetArabicTimeAgo(al.Timestamp),
                        User = al.User.Name,
                        Icon = "edit"
                    })
                    .ToList();

                var stats = new
                {
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    InProgressTasks = inProgressTasks,
                    OverdueTasks = overdueTasks,
                    TotalProjects = totalProjects,
                    ActiveProjects = activeProjects,
                    TotalTeamMembers = totalTeamMembers,
                    RecentActivities = recentActivities
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في استرداد إحصائيات لوحة التحكم", error = ex.Message });
            }
        }

        [HttpGet("activities")]
        public async Task<IActionResult> GetRecentActivities()
        {
            try
            {
                var taskHistoryData = await _context.AuditLogs
                    .Where(al => al.EntityType == "Task")
                    .Include(al => al.User)
                    .OrderByDescending(al => al.Timestamp)
                    .Take(20)
                    .ToListAsync();

                var activities = taskHistoryData.Select(al => new
                    {
                        Id = al.AuditId,
                        Type = "task_update",
                        Message = $"تم تحديث المهمة: {al.EntityType}",
                        Timestamp = GetArabicTimeAgo(al.Timestamp),
                        User = al.User.Name,
                        Icon = "edit"
                    })
                    .ToList();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في استرداد الأنشطة الحديثة", error = ex.Message });
            }
        }

        [HttpGet("recent-projects")]
        public async Task<IActionResult> GetRecentProjects()
        {
            try
            {
                var recentProjectsData = await _context.Projects
                    .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedUser)
                    .OrderByDescending(p => p.ProjectId)
                    .Take(5)
                    .ToListAsync();

                var recentProjects = recentProjectsData.Select(p => new
                    {
                        Id = p.ProjectId,
                        Name = p.ProjectName,
                        Progress = p.Tasks.Any() ? (int)((double)p.Tasks.Count(t => t.StatusId == 4) / p.Tasks.Count() * 100) : 0,
                        DueDate = p.EndDate?.ToString("yyyy-MM-dd") ?? "",
                        Team = p.Tasks.Where(t => t.AssignedUser != null).Select(t => t.AssignedUser!.Name).Distinct().Take(3).ToArray()
                    })
                    .ToList();

                return Ok(recentProjects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في استرداد المشاريع الحديثة", error = ex.Message });
            }
        }

        [HttpGet("tasks-by-status")]
        public async Task<IActionResult> GetTasksByStatus()
        {
            try
            {
                var statusData = await _context.Tasks
                    .Include(t => t.Status)
                    .GroupBy(t => t.Status)
                    .Select(g => new
                    {
                        Status = g.Key.StatusName,
                        Count = g.Count(),
                        Color = GetStatusColor(g.Key.StatusId),
                        BorderColor = GetStatusBorderColor(g.Key.StatusId)
                    })
                    .ToListAsync();

                return Ok(statusData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في استرداد إحصائيات المهام", error = ex.Message });
            }
        }

        [HttpGet("user-stats/{userId}")]
        public async Task<IActionResult> GetUserStats(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                var totalTasks = await _context.Tasks.CountAsync(t => t.AssignedTo == userId);
                var completedTasks = await _context.Tasks.CountAsync(t => t.AssignedTo == userId && t.StatusId == 4);
                var overdueTasks = await _context.Tasks.CountAsync(t => t.AssignedTo == userId && t.DueDate < DateTime.Now && t.StatusId != 4);
                var projectsInvolved = await _context.Tasks
                    .Where(t => t.AssignedTo == userId)
                    .Select(t => t.ProjectId)
                    .Distinct()
                    .CountAsync();

                var stats = new
                {
                    TotalTasks = totalTasks,
                    CompletedTasks = completedTasks,
                    OverdueTasks = overdueTasks,
                    ProjectsInvolved = projectsInvolved,
                    CompletionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في استرداد إحصائيات المستخدم", error = ex.Message });
            }
        }

        [HttpGet("team-stats")]
        public async Task<IActionResult> GetTeamStats()
        {
            try
            {
                var totalTeams = await _context.Departments.CountAsync();
                var totalMembers = await _context.Users.CountAsync(u => u.IsActive);
                var activeProjects = await _context.Projects.CountAsync(p => p.EndDate == null || p.EndDate > DateTime.Now);
                
                var completedTasks = await _context.Tasks.CountAsync(t => t.StatusId == 4);
                var totalTasks = await _context.Tasks.CountAsync();
                var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

                var topPerformer = await _context.Departments
                    .Include(d => d.UserDepartments)
                    .ThenInclude(ud => ud.User)
                    .ThenInclude(u => u.AssignedTasks)
                    .OrderByDescending(d => d.UserDepartments
                        .SelectMany(ud => ud.User.AssignedTasks)
                        .Count(t => t.StatusId == 4))
                    .Select(d => d.DeptName)
                    .FirstOrDefaultAsync();

                var averageTeamSize = totalTeams > 0 ? (double)totalMembers / totalTeams : 0;

                var stats = new
                {
                    TotalTeams = totalTeams,
                    TotalMembers = totalMembers,
                    ActiveProjects = activeProjects,
                    CompletionRate = Math.Round(completionRate, 1),
                    TopPerformer = topPerformer ?? "غير محدد",
                    AverageTeamSize = Math.Round(averageTeamSize, 1)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في استرداد إحصائيات الفريق", error = ex.Message });
            }
        }

        private string GetArabicTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Now";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} ساعة";
            else if (timeSpan.TotalDays < 30)
                return $"{(int)timeSpan.TotalDays} يوم";
            else
                return dateTime.ToString("yyyy-MM-dd");
        }

        private string GetStatusColor(int statusId)
        {
            return statusId switch
            {
                1 => "bg-gray-100 text-gray-600",
                2 => "bg-blue-100 text-blue-600",
                3 => "bg-yellow-100 text-yellow-600",
                4 => "bg-green-100 text-green-600",
                _ => "bg-gray-100 text-gray-600"
            };
        }

        private string GetStatusBorderColor(int statusId)
        {
            return statusId switch
            {
                1 => "border-gray-200",
                2 => "border-blue-200",
                3 => "border-yellow-200",
                4 => "border-green-200",
                _ => "border-gray-200"
            };
        }
    }
}