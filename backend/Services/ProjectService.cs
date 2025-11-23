using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using BarqTMS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<ProjectDto?> GetProjectByIdAsync(int id);
        Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto);
        Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto updateDto);
        Task<bool> DeleteProjectAsync(int id);
    }

    public class ProjectService : IProjectService
    {
        private readonly BarqTMSDbContext _context;

        public ProjectService(BarqTMSDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.TeamLeaders)
                    .ThenInclude(ptl => ptl.TeamLeader)
                .Include(p => p.Departments)
                    .ThenInclude(pd => pd.Department)
                .Include(p => p.Tasks)
                .ToListAsync();

            return projects.Select(MapToDto);
        }

        public async Task<ProjectDto?> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.TeamLeaders)
                    .ThenInclude(ptl => ptl.TeamLeader)
                .Include(p => p.Departments)
                    .ThenInclude(pd => pd.Department)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            return project == null ? null : MapToDto(project);
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto createDto)
        {
            var project = new Project
            {
                Name = createDto.ProjectName,
                Description = createDto.Description,
                CompanyId = createDto.ClientId ?? 0,
                StartDate = createDto.StartDate ?? DateTime.UtcNow,
                DueDate = createDto.EndDate,
                Status = ProjectStatus.Planned
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Handle Team Leaders
            var teamLeaderIds = createDto.TeamLeaderIds;
            if (createDto.TeamLeaderId.HasValue && !teamLeaderIds.Contains(createDto.TeamLeaderId.Value))
            {
                teamLeaderIds.Add(createDto.TeamLeaderId.Value);
            }

            if (teamLeaderIds.Any())
            {
                foreach (var tlId in teamLeaderIds)
                {
                    _context.ProjectTeamLeaders.Add(new ProjectTeamLeader
                    {
                        ProjectId = project.ProjectId,
                        UserId = tlId
                    });
                }
            }

            // Handle Departments
            if (createDto.DepartmentIds.Any())
            {
                foreach (var deptId in createDto.DepartmentIds)
                {
                    _context.ProjectDepartments.Add(new ProjectDepartment
                    {
                        ProjectId = project.ProjectId,
                        DeptId = deptId
                    });
                }
            }
            
            await _context.SaveChangesAsync();

            return (await GetProjectByIdAsync(project.ProjectId))!;
        }

        public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto updateDto)
        {
            var project = await _context.Projects
                .Include(p => p.TeamLeaders)
                .Include(p => p.Departments)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null) return null;

            project.Name = updateDto.ProjectName;
            project.Description = updateDto.Description;
            
            if (updateDto.ClientId.HasValue)
            {
                project.CompanyId = updateDto.ClientId.Value;
            }
            
            if (updateDto.StartDate.HasValue)
            {
                project.StartDate = updateDto.StartDate.Value;
            }
            
            if (updateDto.Status.HasValue)
            {
                project.Status = updateDto.Status.Value;
            }
            
            project.DueDate = updateDto.EndDate;

            // Update Team Leaders
            var teamLeaderIds = updateDto.TeamLeaderIds;
            if (updateDto.TeamLeaderId.HasValue && !teamLeaderIds.Contains(updateDto.TeamLeaderId.Value))
            {
                teamLeaderIds.Add(updateDto.TeamLeaderId.Value);
            }

            if (teamLeaderIds.Any() || updateDto.TeamLeaderId == null) // If explicit null or new list provided
            {
                _context.ProjectTeamLeaders.RemoveRange(project.TeamLeaders);
                foreach (var tlId in teamLeaderIds)
                {
                    _context.ProjectTeamLeaders.Add(new ProjectTeamLeader
                    {
                        ProjectId = project.ProjectId,
                        UserId = tlId
                    });
                }
            }

            // Update Departments
            if (updateDto.DepartmentIds != null)
            {
                _context.ProjectDepartments.RemoveRange(project.Departments);
                foreach (var deptId in updateDto.DepartmentIds)
                {
                    _context.ProjectDepartments.Add(new ProjectDepartment
                    {
                        ProjectId = project.ProjectId,
                        DeptId = deptId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return (await GetProjectByIdAsync(project.ProjectId))!;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ProjectDto MapToDto(Project p)
        {
            var teamLeaders = p.TeamLeaders.Select(ptl => ptl.TeamLeader).ToList();
            var departments = p.Departments.Select(pd => new DepartmentDto 
            { 
                DeptId = pd.Department.DeptId, 
                DeptName = pd.Department.Name 
            }).ToList();

            return new ProjectDto
            {
                ProjectId = p.ProjectId,
                ProjectName = p.Name,
                Description = p.Description,
                ClientId = p.CompanyId,
                ClientName = p.Company?.Name,
                TeamLeaderId = teamLeaders.FirstOrDefault()?.UserId,
                TeamLeaderName = teamLeaders.FirstOrDefault()?.FullName,
                TeamLeaderIds = teamLeaders.Select(u => u.UserId).ToList(),
                TeamLeaderNames = teamLeaders.Select(u => u.FullName).ToList(),
                Departments = departments,
                DepartmentIds = departments.Select(d => d.DeptId).ToList(),
                StartDate = p.StartDate,
                EndDate = p.DueDate,
                TaskCount = p.Tasks.Count,
                Status = p.Status,
                StatusId = (int)p.Status,
                StatusName = p.Status.ToString()
            };
        }
    }
}
