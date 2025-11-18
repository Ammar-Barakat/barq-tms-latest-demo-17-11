using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BarqTMS.API.DTOs;
using BarqTMS.API.Services;

namespace BarqTMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientService clientService, ILogger<ClientsController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients()
        {
            var clients = await _clientService.GetClientsAsync();
            return Ok(clients);
        }

        // GET: api/clients/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetClient(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
                return NotFound($"Client with ID {id} not found.");
            return Ok(client);
        }

        // POST: api/clients
        // Only Manager can create clients
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientDto dto)
        {
            try
            {
                var result = await _clientService.CreateClientAsync(dto);
                return CreatedAtAction(nameof(GetClient), new { id = result.ClientId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/clients/{id}
        // Only Manager can update clients
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientDto dto)
        {
            try
            {
                var success = await _clientService.UpdateClientAsync(id, dto);
                if (!success)
                    return NotFound($"Client with ID {id} not found.");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/clients/{id}
        // Only Manager can delete clients
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var (success, error) = await _clientService.DeleteClientAsync(id);
            if (!success)
            {
                if (error == "notfound")
                    return NotFound($"Client with ID {id} not found.");
                return BadRequest(error);
            }
            return NoContent();
        }

        // GET: api/clients/{id}/projects
        [HttpGet("{id}/projects")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetClientProjects(int id)
        {
            try
            {
                var projects = await _clientService.GetClientProjectsAsync(id);
                return Ok(projects);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Client with ID {id} not found.");
            }
        }
    }
}
