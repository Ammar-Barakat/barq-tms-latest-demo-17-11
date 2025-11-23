using Microsoft.EntityFrameworkCore;
using BarqTMS.API.Models;
using BarqTMS.API.Models.Enums;
using BarqTMS.API.Services;
using System.Threading.Tasks;

namespace BarqTMS.API.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabaseAsync(BarqTMSDbContext context, AuthService authService)
        {
            await SeedDepartments(context);
            await SeedUsers(context, authService);
            await SeedProjects(context);
            
            await context.SaveChangesAsync();
        }

        private static async Task SeedDepartments(BarqTMSDbContext context)
        {
            if (await context.Departments.AnyAsync())
            {
                return;
            }

            var departments = new[]
            {
                new Department { Name = "Management", Description = "Executive Management" },
                new Department { Name = "Marketing", Description = "Marketing Department" },
                new Department { Name = "Sales", Description = "Sales Department" },
                new Department { Name = "Graphic Design", Description = "Design Team" },
                new Department { Name = "Software Development", Description = "Tech Team" },
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsers(BarqTMSDbContext context, AuthService authService)
        {
            if (await context.Users.AnyAsync()) return;

            var manDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Management");

            var admin = new User
            {
                FullName = "System Administrator",
                Username = "admin",
                Email = "admin@barqtms.com",
                PasswordHash = authService.HashPassword("Admin@123"),
                Role = UserRole.Manager,
                IsActive = true,
                DepartmentId = manDept?.DeptId,
                CreatedAt = DateTime.UtcNow
            };

            var manager = new User
            {
                FullName = "Mohamed Elbadry",
                Username = "elbadry0",
                Email = "mohammed.elbadry0@gmail.com",
                PasswordHash = authService.HashPassword("Mohamed@80"),
                Role = UserRole.Manager,
                IsActive = true,
                DepartmentId = manDept?.DeptId,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            context.Users.Add(manager);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProjects(BarqTMSDbContext context)
        {
            if (await context.Projects.AnyAsync()) return;

            // Ensure we have a company
            var admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
            if (admin == null) return;

            var company = await context.Companies.FirstOrDefaultAsync();
            if (company == null)
            {
                company = new Company
                {
                    Name = "Barq TMS Internal",
                    Type = "Internal",
                    Description = "Internal Company",
                    OwnerUserId = admin.UserId
                };
                context.Companies.Add(company);
                await context.SaveChangesAsync();
            }

            var project = new Project
            {
                Name = "Internal System Development",
                Description = "Development of the internal TMS system",
                StartDate = DateTime.UtcNow,
                Status = Models.Enums.ProjectStatus.Active,
                CompanyId = company.CompanyId
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();
        }
    }
}
