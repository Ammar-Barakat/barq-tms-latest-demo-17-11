using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BarqTMS.API.Services
{
    public interface IAuditService
    {
        Task LogAsync(string entityType, int? entityId, string action, string changes, int userId, string? oldValues = null, string? newValues = null);
        Task<List<AuditLog>> GetAuditLogsAsync(string? entityType = null, int? entityId = null, int? userId = null, int page = 1, int pageSize = 10);
        Task<AuditStatsDto> GetAuditStatsAsync();
    }

    public class AuditService : IAuditService
    {
        private readonly BarqTMSDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(BarqTMSDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string entityType, int? entityId, string action, string changes, int userId, 
            string? oldValues = null, string? newValues = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    Changes = changes,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    OldValues = oldValues,
                    NewValues = newValues
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the main operation
                Console.WriteLine($"Failed to create audit log: {ex.Message}");
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string? entityType = null, int? entityId = null, int? userId = null, int page = 1, int pageSize = 10)
        {
            var query = _context.AuditLogs
                .Include(al => al.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(al => al.EntityType == entityType);
            }

            if (entityId.HasValue)
            {
                query = query.Where(al => al.EntityId == entityId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(al => al.UserId == userId.Value);
            }

            return await query
                .OrderByDescending(al => al.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<AuditStatsDto> GetAuditStatsAsync()
        {
            var now = DateTime.UtcNow;
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            var allLogs = await _context.AuditLogs.ToListAsync();

            return new AuditStatsDto
            {
                TotalLogs = allLogs.Count,
                TodayLogs = allLogs.Count(l => l.Timestamp.Date == today),
                ThisWeekLogs = allLogs.Count(l => l.Timestamp.Date >= weekStart),
                LogsByAction = allLogs.GroupBy(l => l.Action)
                    .ToDictionary(g => g.Key, g => g.Count()),
                LogsByEntityType = allLogs.GroupBy(l => l.EntityType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                LogsByUser = allLogs.Where(l => l.User != null)
                    .GroupBy(l => l.User!.Name)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        private string? GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.Connection?.RemoteIpAddress != null)
                {
                    return httpContext.Connection.RemoteIpAddress.ToString();
                }
            }
            catch
            {
                // Ignore exceptions
            }
            return null;
        }

        private string? GetUserAgent()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Request?.Headers["User-Agent"].FirstOrDefault();
            }
            catch
            {
                // Ignore exceptions
            }
            return null;
        }
    }

    public class AuditStatsDto
    {
        public int TotalLogs { get; set; }
        public int TodayLogs { get; set; }
        public int ThisWeekLogs { get; set; }
        public Dictionary<string, int> LogsByAction { get; set; } = new();
        public Dictionary<string, int> LogsByEntityType { get; set; } = new();
        public Dictionary<string, int> LogsByUser { get; set; } = new();
    }
}