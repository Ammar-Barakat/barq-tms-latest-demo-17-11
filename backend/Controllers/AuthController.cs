using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;
using BarqTMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly AuthService _authService;
        private readonly ISecurityService _securityService;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(BarqTMSDbContext context, AuthService authService, 
            ISecurityService securityService, IEmailService emailService, IAuditService auditService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _authService = authService;
            _securityService = securityService;
            _emailService = emailService;
            _auditService = auditService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserDepartments)
                        .ThenInclude(ud => ud.Department)
                    .FirstOrDefaultAsync(u => u.Username == loginDto.UserName && u.IsActive);

                if (user == null || !_authService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return BadRequest("Invalid username or password.");
                }

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var roleNames = new List<string> { user.Role.ToString() };
                var token = _authService.GenerateJwtToken(user, roleNames);
                var refreshToken = _authService.GenerateRefreshToken();

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    ClientId = user.ClientId,
                    Departments = user.UserDepartments.Select(ud => new DepartmentDto
                    {
                        DeptId = ud.Department.DeptId,
                        DeptName = ud.Department.DeptName
                    }).ToList()
                };

                var response = new LoginResponseDto
                {
                    User = userDto,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresIn = 1440 * 60 // 24 hours in seconds
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {UserName}", loginDto.UserName);
                return StatusCode(500, "An error occurred during login.");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Username == registerDto.UserName))
                {
                    return BadRequest("A user with this username already exists.");
                }

                // Validate departments exist (if provided)
                var departments = new List<Department>();
                if (registerDto.DepartmentIds.Any())
                {
                    departments = await _context.Departments
                        .Where(d => registerDto.DepartmentIds.Contains(d.DeptId))
                        .ToListAsync();

                    if (departments.Count != registerDto.DepartmentIds.Count)
                    {
                        return BadRequest("One or more department IDs are invalid.");
                    }
                }

                // Create new user
                var user = new User
                {
                    Name = registerDto.Name,
                    Username = registerDto.UserName,
                    Email = registerDto.Email,
                    Role = registerDto.Role,
                    PasswordHash = _authService.HashPassword(registerDto.Password),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Add user departments
                foreach (var department in departments)
                {
                    _context.UserDepartments.Add(new UserDepartment
                    {
                        UserId = user.UserId,
                        DeptId = department.DeptId
                    });
                }

                await _context.SaveChangesAsync();

                // Generate tokens
                var roleNames = new List<string> { user.Role.ToString() };
                var token = _authService.GenerateJwtToken(user, roleNames);
                var refreshToken = _authService.GenerateRefreshToken();

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    Departments = departments.Select(d => new DepartmentDto
                    {
                        DeptId = d.DeptId,
                        DeptName = d.DeptName
                    }).ToList()
                };

                var response = new LoginResponseDto
                {
                    User = userDto,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresIn = 1440 * 60 // 24 hours in seconds
                };

                return CreatedAtAction(nameof(Login), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {UserName}", registerDto.UserName);
                return StatusCode(500, "An error occurred during registration.");
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                // In a real application, you would store refresh tokens in database
                // For now, we'll just generate new tokens
                var principal = _authService.GetPrincipalFromExpiredToken(refreshTokenDto.RefreshToken);
                if (principal == null)
                {
                    return BadRequest("Invalid refresh token.");
                }

                var userIdClaim = principal.FindFirst("user_id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Invalid token claims.");
                }

                var user = await _context.Users
                    .Include(u => u.UserDepartments)
                        .ThenInclude(ud => ud.Department)
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);

                if (user == null)
                {
                    return BadRequest("User not found or inactive.");
                }

                var roleNames = new List<string> { user.Role.ToString() };
                var newToken = _authService.GenerateJwtToken(user, roleNames);
                var newRefreshToken = _authService.GenerateRefreshToken();

                var userDto = new UserDto
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    Departments = user.UserDepartments.Select(ud => new DepartmentDto
                    {
                        DeptId = ud.Department.DeptId,
                        DeptName = ud.Department.DeptName
                    }).ToList()
                };

                var response = new LoginResponseDto
                {
                    User = userDto,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 1440 * 60 // 24 hours in seconds
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, "An error occurred during token refresh.");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            try
            {
                // In a real application, you would invalidate the refresh token here
                // For now, we'll just return success
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, "An error occurred during logout.");
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Invalid user token.");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return BadRequest("User not found or inactive.");
                }

                if (!_authService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest("Current password is incorrect.");
                }

                user.PasswordHash = _authService.HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred during password change.");
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Invalid user token.");
                }

                var user = await _context.Users
                    .Include(u => u.UserDepartments)
                        .ThenInclude(ud => ud.Department)
                    .Where(u => u.UserId == userId)
                    .Select(u => new UserDto
                    {
                        UserId = u.UserId,
                        Name = u.Name,
                        Email = u.Email,
                        Role = u.Role,
                        Departments = u.UserDepartments.Select(ud => new DepartmentDto
                        {
                            DeptId = ud.Department.DeptId,
                            DeptName = ud.Department.DeptName
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info for user {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "An error occurred while retrieving user information.");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == forgotPasswordDto.UserName);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    // Don't reveal that the user doesn't exist or has no email
                    return Ok(new { message = "If the user exists and has an email, a password reset link has been sent." });
                }

                var token = await _securityService.GeneratePasswordResetTokenAsync(user.UserId);
                await _emailService.SendPasswordResetEmailAsync(user.Email, user.Name, token);

                // Log the action
                await _auditService.LogAsync("User", user.UserId, "Password Reset Requested", $"Password reset requested for username {user.Username}", user.UserId);

                return Ok(new { message = "If the user exists and has an email, a password reset link has been sent." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for username {UserName}", forgotPasswordDto.UserName);
                return StatusCode(500, "An error occurred while processing the password reset request.");
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                if (!await _securityService.ValidatePasswordResetTokenAsync(resetPasswordDto.Token))
                {
                    return BadRequest("Invalid or expired reset token.");
                }

                var user = await _securityService.GetUserByPasswordResetTokenAsync(resetPasswordDto.Token);
                if (user == null)
                {
                    return BadRequest("Invalid reset token.");
                }

                user.PasswordHash = _authService.HashPassword(resetPasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _securityService.MarkPasswordResetTokenAsUsedAsync(resetPasswordDto.Token);
                await _context.SaveChangesAsync();

                // Log the action
                await _auditService.LogAsync("User", user.UserId, "Password Reset Completed", $"Password reset completed for {user.Email}", user.UserId);

                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, "An error occurred during password reset.");
            }
        }

        [HttpPost("validate-reset-token")]
        public async Task<ActionResult> ValidateResetToken(ValidateTokenDto validateTokenDto)
        {
            try
            {
                var isValid = await _securityService.ValidatePasswordResetTokenAsync(validateTokenDto.Token);
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reset token");
                return StatusCode(500, "An error occurred while validating the token.");
            }
        }
    }
}