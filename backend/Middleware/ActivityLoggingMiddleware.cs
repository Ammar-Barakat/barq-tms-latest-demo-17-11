using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.Helpers;
using System.Text.Json;

namespace BarqTMS.API.Middleware
{
    public class ActivityLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActivityLoggingMiddleware> _logger;

        public ActivityLoggingMiddleware(RequestDelegate next, ILogger<ActivityLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, BarqTMSDbContext dbContext)
        {
            // Only log authenticated requests
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // Get request details
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;
            var startTime = DateTime.UtcNow;

            // Create a buffer for the response
            var originalBodyStream = context.Response.Body;
            
            try
            {
                await _next(context);
            }
            finally
            {
                // Log activity for specific endpoints
                if (ShouldLogActivity(method, path))
                {
                    try
                    {
                        var userId = UserContextHelper.GetCurrentUserId(context.User);
                        if (userId.HasValue)
                        {
                            var action = DetermineAction(method, path, context.Response.StatusCode);
                            var entityType = ExtractEntityType(path);
                            var entityId = ExtractEntityId(path);

                            var auditLog = new AuditLog
                            {
                                EntityType = entityType,
                                EntityId = entityId ?? 0,
                                UserId = userId.Value,
                                Action = action,
                                Timestamp = DateTime.UtcNow
                            };

                            dbContext.AuditLogs.Add(auditLog);
                            await dbContext.SaveChangesAsync();

                            _logger.LogInformation(
                                "Activity logged: User {UserId} performed {Action} on {EntityType} {EntityId}",
                                userId.Value, action, entityType, entityId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error logging activity");
                        // Don't throw - logging failure shouldn't break the request
                    }
                }
            }
        }

        private bool ShouldLogActivity(string method, string path)
        {
            // Only log POST, PUT, DELETE operations
            if (method != "POST" && method != "PUT" && method != "DELETE")
                return false;

            // Skip authentication endpoints
            if (path.Contains("/auth/", StringComparison.OrdinalIgnoreCase))
                return false;

            // Skip statistics and search
            if (path.Contains("/statistics/", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/search/", StringComparison.OrdinalIgnoreCase))
                return false;

            // Log API endpoints
            return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
        }

        private string DetermineAction(string method, string path, int statusCode)
        {
            // If request failed, indicate that
            if (statusCode >= 400)
            {
                return $"{method} Failed ({statusCode})";
            }

            var action = method switch
            {
                "POST" => "Created",
                "PUT" => "Updated",
                "DELETE" => "Deleted",
                _ => method
            };

            // Add more context based on path
            if (path.Contains("/comments", StringComparison.OrdinalIgnoreCase))
                return $"{action} Comment";
            if (path.Contains("/attachments", StringComparison.OrdinalIgnoreCase))
                return $"{action} Attachment";
            if (path.Contains("/markread", StringComparison.OrdinalIgnoreCase))
                return "Marked as Read";

            return action;
        }

        private string ExtractEntityType(string path)
        {
            // Extract entity type from path like /api/tasks/5 -> Task
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            if (segments.Length >= 2 && segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
            {
                var entityType = segments[1];
                
                // Singularize common entities
                if (entityType.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                {
                    entityType = entityType[..^1]; // Remove trailing 's'
                }

                return char.ToUpper(entityType[0]) + entityType[1..];
            }

            return "Unknown";
        }

        private int? ExtractEntityId(string path)
        {
            // Extract ID from path like /api/tasks/5 -> 5
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Look for numeric segments
            foreach (var segment in segments.Reverse())
            {
                if (int.TryParse(segment, out var id))
                {
                    return id;
                }
            }

            return null;
        }
    }

    // Extension method to register the middleware
    public static class ActivityLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseActivityLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActivityLoggingMiddleware>();
        }
    }
}
