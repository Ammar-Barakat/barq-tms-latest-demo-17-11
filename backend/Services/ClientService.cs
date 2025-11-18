using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetClientsAsync();
        Task<ClientDto?> GetClientByIdAsync(int id);
        Task<ClientDto> CreateClientAsync(CreateClientDto dto);
        Task<bool> UpdateClientAsync(int id, UpdateClientDto dto);
        Task<(bool Success, string? Error)> DeleteClientAsync(int id);
        Task<IEnumerable<ProjectDto>> GetClientProjectsAsync(int id);
    }

    public class ClientService : IClientService
    {
        private readonly BarqTMSDbContext _context;
        private readonly ILogger<ClientService> _logger;

        public ClientService(BarqTMSDbContext context, ILogger<ClientService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ClientDto>> GetClientsAsync()
        {
            return await _context.Clients
                .Select(c => new ClientDto
                {
                    ClientId = c.ClientId,
                    Name = c.Name,
                    Email = c.Email,
                    ProjectCount = c.Projects.Count()
                })
                .ToListAsync();
        }

        public async Task<ClientDto?> GetClientByIdAsync(int id)
        {
            return await _context.Clients
                .Include(c => c.AccountManager)
                .Where(c => c.ClientId == id)
                .Select(c => new ClientDto
                {
                    ClientId = c.ClientId,
                    Name = c.Name,
                    Email = c.Email,
                    ProjectCount = c.Projects.Count(),
                    PhoneNumber = c.PhoneNumber,
                    Company = c.Company,
                    Address = c.Address,
                    AccountManagerId = c.AccountManagerId,
                    AccountManagerName = c.AccountManager.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ClientDto> CreateClientAsync(CreateClientDto dto)
        {
            if (await _context.Clients.AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower()))
                throw new ArgumentException("A client with this email already exists.");

            var accountManager = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.AccountManagerId && u.Role == UserRole.AccountManager);
            if (accountManager == null)
                throw new ArgumentException("Invalid AccountManagerId. User must exist and have AccountManager role.");

            var client = new Client
            {
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Company = dto.Company,
                Address = dto.Address,
                AccountManagerId = dto.AccountManagerId
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return new ClientDto
            {
                ClientId = client.ClientId,
                Name = client.Name,
                Email = client.Email,
                ProjectCount = 0,
                PhoneNumber = client.PhoneNumber,
                Company = client.Company,
                Address = client.Address,
                AccountManagerId = client.AccountManagerId,
                AccountManagerName = accountManager.Name
            };
        }

        public async Task<bool> UpdateClientAsync(int id, UpdateClientDto dto)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return false;

            if (await _context.Clients.AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower() && c.ClientId != id))
                throw new ArgumentException("A client with this email already exists.");

            var accountManager = await _context.Users.FirstOrDefaultAsync(u => u.UserId == dto.AccountManagerId && u.Role == UserRole.AccountManager);
            if (accountManager == null)
                throw new ArgumentException("Invalid AccountManagerId. User must exist and have AccountManager role.");

            client.Name = dto.Name;
            client.Email = dto.Email;
            client.PhoneNumber = dto.PhoneNumber;
            client.Company = dto.Company;
            client.Address = dto.Address;
            client.AccountManagerId = dto.AccountManagerId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string? Error)> DeleteClientAsync(int id)
        {
            var client = await _context.Clients.Include(c => c.Projects).FirstOrDefaultAsync(c => c.ClientId == id);
            if (client == null)
                return (false, "notfound");
            if (client.Projects.Any())
                return (false, "Cannot delete client with existing projects.");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<IEnumerable<ProjectDto>> GetClientProjectsAsync(int id)
        {
            var exists = await _context.Clients.AnyAsync(c => c.ClientId == id);
            if (!exists)
                throw new KeyNotFoundException("Client not found");

            return await _context.Projects
                .Where(p => p.ClientId == id)
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
                .ToListAsync();
        }
    }
}
