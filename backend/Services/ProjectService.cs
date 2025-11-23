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

            if (createDto.TeamLeaderId.HasValue)
            {
                var teamLeader = new ProjectTeamLeader
                {
                    ProjectId = project.ProjectId,
                    UserId = createDto.TeamLeaderId.Value
                };
                _context.ProjectTeamLeaders.Add(teamLeader);
                await _context.SaveChangesAsync();
            }

            return (await GetProjectByIdAsync(project.ProjectId))!;
        }

        public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectDto updateDto)
        {
            var project = await _context.Projects
                .Include(p => p.TeamLeaders)
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
            
            project.DueDate = updateDto.EndDate;

            // Update Team Leader
            if (updateDto.TeamLeaderId.HasValue)
            {
                _context.ProjectTeamLeaders.RemoveRange(project.TeamLeaders);
                _context.ProjectTeamLeaders.Add(new ProjectTeamLeader
                {
                    ProjectId = project.ProjectId,
                    UserId = updateDto.TeamLeaderId.Value
                });
            }
            else if (updateDto.TeamLeaderId == null)
            {
                 _context.ProjectTeamLeaders.RemoveRange(project.TeamLeaders);
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
            var teamLeader = p.TeamLeaders.FirstOrDefault()?.TeamLeader;
            return new ProjectDto
            {
                ProjectId = p.ProjectId,
                ProjectName = p.Name,
                Description = p.Description,
                ClientId = p.CompanyId,
                ClientName = p.Company?.Name,
                TeamLeaderId = teamLeader?.UserId,
                TeamLeaderName = teamLeader?.FullName,
                StartDate = p.StartDate,
                EndDate = p.DueDate,
                TaskCount = p.Tasks.Count
            };
        }
    }
}
