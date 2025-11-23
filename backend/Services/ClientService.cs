using BarqTMS.API.Data;
using BarqTMS.API.DTOs;
using BarqTMS.API.Models;
using BarqTMS.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BarqTMS.API.Services
{
    public interface IClientService
    {
        Task<IEnumerable<ClientDto>> GetAllClientsAsync();
        Task<ClientDto?> GetClientByIdAsync(int id);
        Task<ClientDto> CreateClientAsync(CreateClientDto clientDto);
        Task<ClientDto?> UpdateClientAsync(int id, UpdateClientDto clientDto);
        Task<bool> DeleteClientAsync(int id);
        Task<IEnumerable<ProjectDto>> GetClientProjectsAsync(int clientId);
    }

    public class ClientService : IClientService
    {
        private readonly BarqTMSDbContext _context;
        private readonly AuthService _authService;
        private readonly ILogger<ClientService> _logger;

        public ClientService(BarqTMSDbContext context, AuthService authService, ILogger<ClientService> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        public async Task<IEnumerable<ClientDto>> GetAllClientsAsync()
        {
            var companies = await _context.Companies
                .Include(c => c.Owner)
                .Include(c => c.AccountManager)
                .Include(c => c.Projects)
                .ToListAsync();

            return companies.Select(MapToDto);
        }

        public async Task<ClientDto?> GetClientByIdAsync(int id)
        {
            var company = await _context.Companies
                .Include(c => c.Owner)
                .Include(c => c.AccountManager)
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.CompanyId == id);

            return company == null ? null : MapToDto(company);
        }

        public async Task<ClientDto> CreateClientAsync(CreateClientDto clientDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int ownerId;

                // 1. Handle Owner Creation or Selection
                if (clientDto.OwnerUserId.HasValue)
                {
                    // Use existing user
                    var existingUser = await _context.Users.FindAsync(clientDto.OwnerUserId.Value);
                    if (existingUser == null)
                        throw new ArgumentException("Selected owner user does not exist.");
                    
                    ownerId = existingUser.UserId;
                }
                else
                {
                    // Create new user
                    if (string.IsNullOrEmpty(clientDto.Username) || string.IsNullOrEmpty(clientDto.Password))
                        throw new ArgumentException("Username and Password are required for new client users.");

                    if (await _context.Users.AnyAsync(u => u.Username == clientDto.Username))
                        throw new ArgumentException("Username already exists.");

                    var newUser = new User
                    {
                        FullName = clientDto.OwnerName ?? clientDto.Name, // Fallback to company name if owner name missing
                        Username = clientDto.Username,
                        Email = clientDto.Email,
                        PasswordHash = _authService.HashPassword(clientDto.Password),
                        Role = UserRole.Client,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    ownerId = newUser.UserId;
                }

                // 2. Create Company
                var company = new Company
                {
                    Name = clientDto.Name,
                    Email = clientDto.Email,
                    Phone = clientDto.PhoneNumber,
                    Address = clientDto.Address,
                    Type = "Client", // Default type
                    OwnerUserId = ownerId,
                    AccountManagerId = clientDto.AccountManagerId
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Reload to get navigation properties
                return (await GetClientByIdAsync(company.CompanyId))!;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating client");
                throw;
            }
        }

        public async Task<ClientDto?> UpdateClientAsync(int id, UpdateClientDto clientDto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return null;

            company.Name = clientDto.Name;
            company.Email = clientDto.Email;
            company.Phone = clientDto.PhoneNumber;
            company.Address = clientDto.Address;
            company.AccountManagerId = clientDto.AccountManagerId;

            await _context.SaveChangesAsync();
            return (await GetClientByIdAsync(id))!;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return false;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ProjectDto>> GetClientProjectsAsync(int clientId)
        {
             var projects = await _context.Projects
                .Include(p => p.Company)
                .Where(p => p.CompanyId == clientId)
                .ToListAsync();
            
            return projects.Select(p => new ProjectDto
            {
                ProjectId = p.ProjectId,
                ProjectName = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.DueDate,
                ClientId = p.CompanyId,
                ClientName = p.Company.Name
            });
        }

        private ClientDto MapToDto(Company company)
        {
            return new ClientDto
            {
                ClientId = company.CompanyId,
                Name = company.Name,
                Email = company.Email ?? string.Empty,
                PhoneNumber = company.Phone,
                Address = company.Address,
                ProjectCount = company.Projects.Count,
                AccountManagerId = company.AccountManagerId,
                AccountManagerName = company.AccountManager?.FullName,
                OwnerUserId = company.OwnerUserId,
                OwnerName = company.Owner?.FullName
            };
        }
    }
}

