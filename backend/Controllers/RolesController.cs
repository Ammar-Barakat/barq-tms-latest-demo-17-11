using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.Data;
using BarqTMS.API.Models;
using BarqTMS.API.DTOs;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(BarqTMSDbContext context, ILogger<RolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToListAsync();

            return Ok(roles);
        }

        // GET: api/roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(int id)
        {
            var role = await _context.Roles
                .Where(r => r.RoleId == id)
                .Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            return Ok(role);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto createRoleDto)
        {
            // Check if role name already exists
            if (await _context.Roles.AnyAsync(r => r.RoleName.ToLower() == createRoleDto.RoleName.ToLower()))
            {
                return BadRequest("A role with this name already exists.");
            }

            var role = new Role
            {
                RoleName = createRoleDto.RoleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var roleDto = new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };

            return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, roleDto);
        }

        // PUT: api/roles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, CreateRoleDto updateRoleDto)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            // Check if role name already exists for another role
            if (await _context.Roles.AnyAsync(r => r.RoleName.ToLower() == updateRoleDto.RoleName.ToLower() && r.RoleId != id))
            {
                return BadRequest("A role with this name already exists.");
            }

            role.RoleName = updateRoleDto.RoleName;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoleExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found.");
            }

            // Roles cannot be deleted as they are system-defined enums
            return BadRequest("Roles cannot be deleted as they are now system-defined enums.");
        }

        private async Task<bool> RoleExists(int id)
        {
            return await _context.Roles.AnyAsync(e => e.RoleId == id);
        }
    }
}