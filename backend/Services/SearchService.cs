using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface ISearchService
    {
        Task<SearchResultsDto> SearchAsync(string query, string[]? entityTypes = null, int page = 1, int pageSize = 20);
        Task<IEnumerable<TaskDto>> SearchTasksAsync(string query, int? statusId = null, int? priorityId = null, 
            int? assignedTo = null, int? departmentId = null, int? projectId = null);
        Task<IEnumerable<ProjectDto>> SearchProjectsAsync(string query, int? clientId = null);
        Task<IEnumerable<UserDto>> SearchUsersAsync(string query, int? departmentId = null, string? role = null);
    }

    public class SearchService : ISearchService
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<SearchService> _logger;

        public SearchService(BarqTMSDbContext context, ILogger<SearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SearchResultsDto> SearchAsync(string query, string[]? entityTypes = null, int page = 1, int pageSize = 20)
        {
            var results = new SearchResultsDto { Query = query };
            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (entityTypes == null || entityTypes.Contains("tasks"))
            {
                results.Tasks = await SearchTasksInternalAsync(searchTerms);
            }

            if (entityTypes == null || entityTypes.Contains("projects"))
            {
                results.Projects = await SearchProjectsInternalAsync(searchTerms);
            }

            if (entityTypes == null || entityTypes.Contains("users"))
            {
                results.Users = await SearchUsersInternalAsync(searchTerms);
            }

            if (entityTypes == null || entityTypes.Contains("departments"))
            {
                results.Departments = await SearchDepartmentsInternalAsync(searchTerms);
            }

            return results;
        }

        public async Task<IEnumerable<TaskDto>> SearchTasksAsync(string query, int? statusId = null, int? priorityId = null, 
            int? assignedTo = null, int? departmentId = null, int? projectId = null)
        {
            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var tasksQuery = _context.Tasks
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .AsQueryable();

            // Apply text search
            if (searchTerms.Any())
            {
                tasksQuery = tasksQuery.Where(t =>
                    searchTerms.Any(term =>
                        t.Title.ToLower().Contains(term) ||
                        (t.Description != null && t.Description.ToLower().Contains(term)) ||
                        (t.Tags != null && t.Tags.ToLower().Contains(term))
                    )
                );
            }

            // Apply filters
            if (statusId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.StatusId == statusId);

            if (priorityId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.PriorityId == priorityId);

            if (assignedTo.HasValue)
                tasksQuery = tasksQuery.Where(t => t.AssignedTo == assignedTo);

            if (departmentId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.DeptId == departmentId);

            if (projectId.HasValue)
                tasksQuery = tasksQuery.Where(t => t.ProjectId == projectId);

            return await tasksQuery
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
                .Take(50)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectDto>> SearchProjectsAsync(string query, int? clientId = null)
        {
            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var projectsQuery = _context.Projects
                .Include(p => p.Client)
                .AsQueryable();

            if (searchTerms.Any())
            {
                projectsQuery = projectsQuery.Where(p =>
                    searchTerms.Any(term =>
                        p.ProjectName.ToLower().Contains(term) ||
                        (p.Description != null && p.Description.ToLower().Contains(term)) ||
                        p.Client.Name.ToLower().Contains(term)
                    )
                );
            }

            if (clientId.HasValue)
                projectsQuery = projectsQuery.Where(p => p.ClientId == clientId);

            return await projectsQuery
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    ClientId = p.ClientId,
                    ClientName = p.Client != null ? p.Client.Name : string.Empty,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    TaskCount = p.Tasks.Count()
                })
                .Take(50)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string query, int? departmentId = null, string? role = null)
        {
            var searchTerms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var usersQuery = _context.Users
                .Include(u => u.UserDepartments).ThenInclude(ud => ud.Department)
                .AsQueryable();

            if (searchTerms.Any())
            {
                usersQuery = usersQuery.Where(u =>
                    searchTerms.Any(term =>
                        u.Name.ToLower().Contains(term) ||
                        (u.Email != null && u.Email.ToLower().Contains(term))
                    )
                );
            }

            if (departmentId.HasValue)
                usersQuery = usersQuery.Where(u => u.UserDepartments.Any(ud => ud.DeptId == departmentId));

            if (!string.IsNullOrEmpty(role))
                usersQuery = usersQuery.Where(u => u.Role.ToString().ToLower() == role.ToLower());

            return await usersQuery
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email ?? string.Empty,
                    Role = u.Role,
                    Departments = u.UserDepartments.Select(ud => new DepartmentDto
                    {
                        DeptId = ud.Department.DeptId,
                        DeptName = ud.Department.DeptName
                    }).ToList()
                })
                .Take(50)
                .ToListAsync();
        }

        private async Task<IEnumerable<TaskDto>> SearchTasksInternalAsync(string[] searchTerms)
        {
            var query = _context.Tasks
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.AssignedUser)
                .Include(t => t.Department)
                .Include(t => t.Project)
                .Where(t => searchTerms.Any(term =>
                    t.Title.ToLower().Contains(term) ||
                    (t.Description != null && t.Description.ToLower().Contains(term))
                ));

            return await query.Take(10)
                .Select(t => new TaskDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    PriorityLevel = t.Priority.Level,
                    StatusName = t.Status.StatusName,
                    ProjectName = t.Project.ProjectName,
                    AssignedToName = t.AssignedUser != null ? t.AssignedUser.Name : null
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<ProjectDto>> SearchProjectsInternalAsync(string[] searchTerms)
        {
            var query = _context.Projects
                .Include(p => p.Client)
                .Where(p => searchTerms.Any(term =>
                    p.ProjectName.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term))
                ));

            return await query.Take(10)
                .Select(p => new ProjectDto
                {
                    ProjectId = p.ProjectId,
                    ProjectName = p.ProjectName,
                    Description = p.Description,
                    ClientName = p.Client != null ? p.Client.Name : string.Empty
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<UserDto>> SearchUsersInternalAsync(string[] searchTerms)
        {
            var query = _context.Users
                .Where(u => searchTerms.Any(term =>
                    u.Name.ToLower().Contains(term) ||
                    (u.Email != null && u.Email.ToLower().Contains(term))
                ));

            return await query.Take(10)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email ?? string.Empty,
                    Role = u.Role
                })
                .ToListAsync();
        }

        private async Task<IEnumerable<DepartmentDto>> SearchDepartmentsInternalAsync(string[] searchTerms)
        {
            var query = _context.Departments
                .Where(d => searchTerms.Any(term => d.DeptName.ToLower().Contains(term)));

            return await query.Take(10)
                .Select(d => new DepartmentDto
                {
                    DeptId = d.DeptId,
                    DeptName = d.DeptName,
                    UserCount = d.UserDepartments.Count(),
                    TaskCount = d.Tasks.Count()
                })
                .ToListAsync();
        }
    }

    public class SearchResultsDto
    {
        public string Query { get; set; } = string.Empty;
        public IEnumerable<TaskDto> Tasks { get; set; } = new List<TaskDto>();
        public IEnumerable<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
        public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
        public IEnumerable<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
    }
}