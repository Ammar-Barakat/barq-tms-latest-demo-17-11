using BarqTMS.API.Data;
using BarqTMS.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BarqTMS.API.Services
{
    public interface ISecurityService
    {
        Task<string> GeneratePasswordResetTokenAsync(int userId);
        Task<bool> ValidatePasswordResetTokenAsync(string token);
        Task<User?> GetUserByPasswordResetTokenAsync(string token);
        Task MarkPasswordResetTokenAsUsedAsync(string token);
        Task<bool> IsAccountLockedAsync(string email);
        Task RecordLoginAttemptAsync(string email, string ipAddress, string? userAgent, bool wasSuccessful, string? failureReason = null);
        Task<int> GetFailedLoginAttemptsAsync(string email, TimeSpan timeWindow);
        string GenerateTwoFactorCode();
        bool ValidateTwoFactorCode(string providedCode, string expectedCode, DateTime generatedAt, TimeSpan validityPeriod);
    }

    public class SecurityService : ISecurityService
    {
        private readonly BarqTMSDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecurityService> _logger;

        public SecurityService(BarqTMSDbContext context, IConfiguration configuration, ILogger<SecurityService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(int userId)
        {
            var token = GenerateSecureToken();
            var expirationHours = _configuration.GetValue("Security:PasswordResetTokenExpirationHours", 24);

            var resetToken = new PasswordResetToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
                IsUsed = false
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset token generated for user {UserId}", userId);
            return token;
        }

        public async Task<bool> ValidatePasswordResetTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            return resetToken != null;
        }

        public async Task<User?> GetUserByPasswordResetTokenAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            return resetToken?.User;
        }

        public async Task MarkPasswordResetTokenAsUsedAsync(string token)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken != null)
            {
                resetToken.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var maxAttempts = _configuration.GetValue("Security:MaxFailedLoginAttempts", 5);
            var lockoutMinutes = _configuration.GetValue("Security:AccountLockoutMinutes", 30);

            var timeWindow = TimeSpan.FromMinutes(lockoutMinutes);
            var failedAttempts = await GetFailedLoginAttemptsAsync(email, timeWindow);

            return failedAttempts >= maxAttempts;
        }

        public async Task RecordLoginAttemptAsync(string email, string ipAddress, string? userAgent, bool wasSuccessful, string? failureReason = null)
        {
            var attempt = new LoginAttempt
            {
                Email = email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                WasSuccessful = wasSuccessful,
                FailureReason = failureReason,
                AttemptedAt = DateTime.UtcNow
            };

            _context.LoginAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Login attempt recorded for {Email}: {Success}", email, wasSuccessful ? "Success" : "Failed");
        }

        public async Task<int> GetFailedLoginAttemptsAsync(string email, TimeSpan timeWindow)
        {
            var cutoffTime = DateTime.UtcNow - timeWindow;

            return await _context.LoginAttempts
                .Where(la => la.Email == email && !la.WasSuccessful && la.AttemptedAt > cutoffTime)
                .CountAsync();
        }

        public string GenerateTwoFactorCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public bool ValidateTwoFactorCode(string providedCode, string expectedCode, DateTime generatedAt, TimeSpan validityPeriod)
        {
            if (DateTime.UtcNow - generatedAt > validityPeriod)
            {
                return false; // Code expired
            }

            return providedCode == expectedCode;
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}