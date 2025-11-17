using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Services;
using BarqTMS.API.DTOs;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<SearchResultsDto>> Search(
            [FromQuery] string q,
            [FromQuery] string[]? types = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest("Search query must be at least 2 characters long.");
            }

            try
            {
                var results = await _searchService.SearchAsync(q, types, page, pageSize);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search with query: {Query}", q);
                return StatusCode(500, "An error occurred while searching.");
            }
        }

        [HttpGet("tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> SearchTasks(
            [FromQuery] string q,
            [FromQuery] int? statusId = null,
            [FromQuery] int? priorityId = null,
            [FromQuery] int? assignedTo = null,
            [FromQuery] int? departmentId = null,
            [FromQuery] int? projectId = null)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest("Search query must be at least 2 characters long.");
            }

            try
            {
                var results = await _searchService.SearchTasksAsync(q, statusId, priorityId, assignedTo, departmentId, projectId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tasks with query: {Query}", q);
                return StatusCode(500, "An error occurred while searching tasks.");
            }
        }

        [HttpGet("projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> SearchProjects(
            [FromQuery] string q,
            [FromQuery] int? clientId = null)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest("Search query must be at least 2 characters long.");
            }

            try
            {
                var results = await _searchService.SearchProjectsAsync(q, clientId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching projects with query: {Query}", q);
                return StatusCode(500, "An error occurred while searching projects.");
            }
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> SearchUsers(
            [FromQuery] string q,
            [FromQuery] int? departmentId = null,
            [FromQuery] string? role = null)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return BadRequest("Search query must be at least 2 characters long.");
            }

            try
            {
                var results = await _searchService.SearchUsersAsync(q, departmentId, role);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", q);
                return StatusCode(500, "An error occurred while searching users.");
            }
        }
    }
}