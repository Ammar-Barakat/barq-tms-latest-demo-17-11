using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;
using BarqTMS.API.Helpers;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(BarqTMSDbContext context, ILogger<ProjectsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            // Get current user from JWT token
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var currentUser = await _context.Users.FindAsync(currentUserId);
            
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            // Filter projects based on user role
            var query = _context.Projects
                .Include(p => p.Client)
                .Include(p => p.TeamLeader)
                .AsQueryable();
            
            if (currentUser.Role == UserRole.Client)
            {
                // Find client record for this user
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                if (client != null)
                {
                    query = query.Where(p => p.ClientId == client.ClientId);
                }
                else
                {
                    // If no client record found, return empty list
                    return Ok(new List<ProjectDto>());
                }
            }

            var projects = await query
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    ClientId = p.ClientId,
                    ClientName = p.Client != null ? p.Client.Name : null,
                    TeamLeaderId = p.TeamLeaderId,
                    TeamLeaderName = p.TeamLeader != null ? p.TeamLeader.Name : null,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TaskCount = p.Tasks.Count()
                })
                .ToListAsync();

            return Ok(projects);
        }

        // GET: api/projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            // Get current user from JWT token
            var currentUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);
            var currentUser = await _context.Users.FindAsync(currentUserId);
            
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.TeamLeader)
                .Where(p => p.ProjectId == id)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    ClientId = p.ClientId,
                    ClientName = p.Client != null ? p.Client.Name : null,
                    TeamLeaderId = p.TeamLeaderId,
                    TeamLeaderName = p.TeamLeader != null ? p.TeamLeader.Name : null,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TaskCount = p.Tasks.Count()
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            // If user is Client (Role 6), check if this is their project
            if (currentUser.Role == UserRole.Client)
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == currentUser.Email);
                if (client == null || project.ClientId != client.ClientId)
                {
                    return Forbid(); // 403 Forbidden
                }
            }

            return Ok(project);
        }

        // POST: api/projects
        // Only Manager and Assistant Manager can create projects
        [HttpPost]
        [Authorize(Roles = "Manager,AssistantManager")]
        public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectDto createProjectDto)
        {
            // Validate client exists if provided
            if (createProjectDto.ClientId.HasValue)
            {
                var client = await _context.Clients.FindAsync(createProjectDto.ClientId.Value);
                if (client == null)
                {
                    return BadRequest($"Client with ID {createProjectDto.ClientId} not found.");
                }
            }

            // Validate team leader exists and has correct role if provided
            if (createProjectDto.TeamLeaderId.HasValue)
            {
                var teamLeader = await _context.Users.FindAsync(createProjectDto.TeamLeaderId.Value);
                if (teamLeader == null)
                {
                    return BadRequest($"Team Leader with ID {createProjectDto.TeamLeaderId} not found.");
                }
                if (teamLeader.Role != UserRole.TeamLeader)
                {
                    return BadRequest($"User with ID {createProjectDto.TeamLeaderId} is not a Team Leader.");
                }
            }

            // FIX 1: Validate project dates - DueDate must be after StartDate
            if (createProjectDto.StartDate.HasValue && createProjectDto.EndDate.HasValue)
            {
                if (createProjectDto.EndDate.Value <= createProjectDto.StartDate.Value)
                {
                    return BadRequest("Project Due Date must be after the Start Date.");
                }
            }

            var project = new Project
            {
                ProjectName = createProjectDto.ProjectName,
                Description = createProjectDto.Description,
                ClientId = createProjectDto.ClientId,
                TeamLeaderId = createProjectDto.TeamLeaderId,
                StartDate = createProjectDto.StartDate,
                EndDate = createProjectDto.EndDate
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Create audit log entry
            await CreateProjectAuditLog(project.ProjectId, "Created", $"Project '{project.ProjectName}' was created");

            // Get client name if ClientId is provided
            string? clientName = null;
            if (project.ClientId.HasValue)
            {
                var client = await _context.Clients.FindAsync(project.ClientId.Value);
                clientName = client?.Name;
            }

            // Get team leader name if TeamLeaderId is provided
            string? teamLeaderName = null;
            if (project.TeamLeaderId.HasValue)
            {
                var teamLeader = await _context.Users.FindAsync(project.TeamLeaderId.Value);
                teamLeaderName = teamLeader?.Name;
            }

            var projectDto = new ProjectDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                ClientId = project.ClientId,
                ClientName = clientName,
                TeamLeaderId = project.TeamLeaderId,
                TeamLeaderName = teamLeaderName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                TaskCount = 0
            };

            return CreatedAtAction(nameof(GetProject), new { id = project.ProjectId }, projectDto);
        }

        // PUT: api/projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDto updateProjectDto)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            // Validate client exists if provided
            if (updateProjectDto.ClientId.HasValue)
            {
                var client = await _context.Clients.FindAsync(updateProjectDto.ClientId.Value);
                if (client == null)
                {
                    return BadRequest($"Client with ID {updateProjectDto.ClientId} not found.");
                }
            }

            // Validate team leader exists and has correct role if provided
            if (updateProjectDto.TeamLeaderId.HasValue)
            {
                var teamLeader = await _context.Users.FindAsync(updateProjectDto.TeamLeaderId.Value);
                if (teamLeader == null)
                {
                    return BadRequest($"Team Leader with ID {updateProjectDto.TeamLeaderId} not found.");
                }
                if (teamLeader.Role != UserRole.TeamLeader)
                {
                    return BadRequest($"User with ID {updateProjectDto.TeamLeaderId} is not a Team Leader.");
                }
            }

            // FIX 1: Validate project dates - DueDate must be after StartDate
            if (updateProjectDto.StartDate.HasValue && updateProjectDto.EndDate.HasValue)
            {
                if (updateProjectDto.EndDate.Value <= updateProjectDto.StartDate.Value)
                {
                    return BadRequest("Project Due Date must be after the Start Date.");
                }
            }

            // Store old values for audit log
            var oldName = project.ProjectName;
            var oldClientId = project.ClientId;
            var oldTeamLeaderId = project.TeamLeaderId;

            project.ProjectName = updateProjectDto.ProjectName;
            project.Description = updateProjectDto.Description;
            project.ClientId = updateProjectDto.ClientId;
            project.TeamLeaderId = updateProjectDto.TeamLeaderId;
            project.StartDate = updateProjectDto.StartDate;
            project.EndDate = updateProjectDto.EndDate;

            try
            {
                await _context.SaveChangesAsync();

                // Create audit log entry
                var changes = new List<string>();
                if (oldName != project.ProjectName)
                    changes.Add($"Name changed from '{oldName}' to '{project.ProjectName}'");
                if (oldClientId != project.ClientId)
                    changes.Add($"Client changed");

                if (changes.Any())
                {
                    await CreateProjectAuditLog(project.ProjectId, "Updated", string.Join(", ", changes));
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/projects/5
        // Only Manager can delete projects
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            // Create audit log entry before deletion
            await CreateProjectAuditLog(project.ProjectId, "Deleted", $"Project '{project.ProjectName}' was deleted");

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/projects/5/tasks
        [HttpGet("{id}/tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetProjectTasks(int id)
        {
            if (!await ProjectExists(id))
            {
                return NotFound($"Project with ID {id} not found.");
            }

            var tasks = await _context.Tasks
                .Where(t => t.ProjectId == id)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .Include(t => t.Department)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    PriorityId = t.PriorityId,
                    PriorityLevel = t.Priority.Level,
                    StatusId = t.StatusId,
                    StatusName = t.Status.StatusName,
                    DueDate = t.DueDate,
                    CreatedBy = t.CreatedBy,
                    CreatedByName = t.Creator.Name,
                    AssignedTo = t.AssignedTo,
                    AssignedToName = t.AssignedUser != null ? t.AssignedUser.Name : null,
                    DeptId = t.DeptId,
                    DeptName = t.Department.DeptName,
                    ProjectId = t.ProjectId,
                    ProjectName = t.Project.ProjectName,
                    CommentCount = t.TaskComments.Count(),
                    AttachmentCount = t.Attachments.Count()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/projects/5/auditlogs
        [HttpGet("{id}/auditlogs")]
        public async Task<ActionResult<IEnumerable<ProjectHistoryDto>>> GetProjectAuditLogs(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound($"Project with ID {id} not found.");
            }

            var auditLogs = await _context.AuditLogs
                .Where(al => al.EntityType == "Project" && al.EntityId == id)
                .Include(al => al.User)
                .OrderByDescending(al => al.Timestamp)
                .Select(al => new ProjectHistoryDto
                {
                    HistoryId = al.AuditId,
                    ProjectId = id,
                    ProjectName = project.ProjectName,
                    UserId = al.UserId,
                    UserName = al.User.Name,
                    Action = al.Action,
                    ActionDate = al.Timestamp
                })
                .ToListAsync();

            return Ok(auditLogs);
        }

        private async Task<bool> ProjectExists(int id)
        {
            return await _context.Projects.AnyAsync(e => e.ProjectId == id);
        }

        private async System.Threading.Tasks.Task CreateProjectAuditLog(int projectId, string action, string details)
        {
            // Get current user from JWT token
            var systemUserId = UserContextHelper.GetCurrentUserIdOrThrow(User);

            var auditLog = new AuditLog
            {
                EntityType = "Project",
                EntityId = projectId,
                UserId = systemUserId,
                Action = $"{action}: {details}",
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}