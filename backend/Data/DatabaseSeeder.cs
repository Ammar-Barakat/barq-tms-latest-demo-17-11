using Microsoft.EntityFrameworkCore;
using BarqTMS.API.Models;
using BarqTMS.API.Services;
using System.Threading.Tasks;

namespace BarqTMS.API.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabaseAsync(BarqTMSDbContext context, AuthService authService)
        {
            // Check if data already exists
            if (await context.Users.AnyAsync())
            {
                return; // Database has been seeded
            }

            await SeedPriorities(context);
            await SeedStatuses(context);
            await SeedRoles(context);
            await SeedDepartments(context);
            await SeedUsers(context, authService);
            await SeedClients(context);
            await SeedProjects(context);
            await SeedTasks(context);
            await SeedTaskComments(context);
            await SeedNotifications(context);
            await SeedUserSettings(context);
            
            await context.SaveChangesAsync();
        }

        private static async Task SeedPriorities(BarqTMSDbContext context)
        {
            if (await context.Priorities.AnyAsync())
            {
                return;
            }

            var priorities = new[]
            {
                new Priority { Level = "Critical" },
                new Priority { Level = "High" },
                new Priority { Level = "Medium" },
                new Priority { Level = "Low" }
            };

            await context.Priorities.AddRangeAsync(priorities);
            await context.SaveChangesAsync();
        }

        private static async Task SeedStatuses(BarqTMSDbContext context)
        {
            if (await context.Statuses.AnyAsync())
            {
                return;
            }

            var statuses = new[]
            {
                new Status { StatusName = "To Do" },
                new Status { StatusName = "In Progress" },
                new Status { StatusName = "In Review" },
                new Status { StatusName = "Done" },
                new Status { StatusName = "Cancelled" }
            };

            await context.Statuses.AddRangeAsync(statuses);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRoles(BarqTMSDbContext context)
        {
            if (await context.Roles.AnyAsync())
            {
                return;
            }

            var roles = new[]
            {
                new Role { RoleName = "Manager" },
                new Role { RoleName = "Assistant Manager" },
                new Role { RoleName = "Account Manager" },
                new Role { RoleName = "Team Leader" },
                new Role { RoleName = "Employee" },
                new Role { RoleName = "Client" }
            };

            await context.Roles.AddRangeAsync(roles);
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
                new Department { DeptName = "Information Technology" },
                new Department { DeptName = "Human Resources" },
                new Department { DeptName = "Marketing" },
                new Department { DeptName = "Sales" },
                new Department { DeptName = "Finance" },
                new Department { DeptName = "Operations" },
                new Department { DeptName = "Customer Service" },
                new Department { DeptName = "Research & Development" }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsers(BarqTMSDbContext context, AuthService authService)
    {
        if (await context.Users.AnyAsync()) return;

        // Get departments
        var itDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Information Technology");
        var hrDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Human Resources");
        var marketingDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Marketing");
        var salesDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Sales");
        var financeDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Finance");
        var operationsDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Operations");

        // 1. Manager (Admin) - Full access
        var admin = new User
        {
            Name = "System Administrator",
            Username = "admin",
            Email = "admin@barqtms.com",
            PasswordHash = authService.HashPassword("Admin@123"),
            Role = UserRole.Manager,
            IsActive = true,
            Position = "System Admin",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        if (itDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = admin.UserId, DeptId = itDept.DeptId });

        // 2. Account Manager - Should see all clients and their projects
        var accountManager1 = new User
        {
            Name = "Sarah Johnson",
            Username = "sarah.johnson",
            Email = "sarah.johnson@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.AccountManager,
            IsActive = true,
            Position = "Senior Account Manager",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(accountManager1);

        var accountManager2 = new User
        {
            Name = "Michael Chen",
            Username = "michael.chen",
            Email = "michael.chen@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.AccountManager,
            IsActive = true,
            Position = "Account Manager",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(accountManager2);
        await context.SaveChangesAsync();

        if (salesDept != null)
        {
            context.UserDepartments.Add(new UserDepartment { UserId = accountManager1.UserId, DeptId = salesDept.DeptId });
            context.UserDepartments.Add(new UserDepartment { UserId = accountManager2.UserId, DeptId = salesDept.DeptId });
        }

        // 3. Team Leaders - Should see their team's tasks and projects
        var teamLeader1 = new User
        {
            Name = "John Smith",
            Username = "john.smith",
            Email = "john.smith@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.TeamLeader,
            IsActive = true,
            Position = "IT Team Leader",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(teamLeader1);

        var teamLeader2 = new User
        {
            Name = "Emily Davis",
            Username = "emily.davis",
            Email = "emily.davis@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.TeamLeader,
            IsActive = true,
            Position = "Marketing Team Leader",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(teamLeader2);

        var teamLeader3 = new User
        {
            Name = "David Wilson",
            Username = "david.wilson",
            Email = "david.wilson@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.TeamLeader,
            IsActive = true,
            Position = "Operations Team Leader",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(teamLeader3);
        await context.SaveChangesAsync();

        if (itDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = teamLeader1.UserId, DeptId = itDept.DeptId });
        if (marketingDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = teamLeader2.UserId, DeptId = marketingDept.DeptId });
        if (operationsDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = teamLeader3.UserId, DeptId = operationsDept.DeptId });

        // 4. Assistant Managers - Should have elevated access
        var assistantManager1 = new User
        {
            Name = "Alice Brown",
            Username = "alice.brown",
            Email = "alice.brown@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.AssistantManager,
            IsActive = true,
            Position = "Assistant IT Manager",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(assistantManager1);

        var assistantManager2 = new User
        {
            Name = "Bob Martinez",
            Username = "bob.martinez",
            Email = "bob.martinez@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.AssistantManager,
            IsActive = true,
            Position = "Assistant Marketing Manager",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(assistantManager2);
        await context.SaveChangesAsync();

        if (itDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = assistantManager1.UserId, DeptId = itDept.DeptId });
        if (marketingDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = assistantManager2.UserId, DeptId = marketingDept.DeptId });

        // 5. Employees - Should only see their own tasks
        var employee1 = new User
        {
            Name = "Tom Anderson",
            Username = "tom.anderson",
            Email = "tom.anderson@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.Employee,
            IsActive = true,
            Position = "Junior Developer",
            TeamLeaderId = teamLeader1.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(employee1);

        var employee2 = new User
        {
            Name = "Lisa Thompson",
            Username = "lisa.thompson",
            Email = "lisa.thompson@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.Employee,
            IsActive = true,
            Position = "Content Writer",
            TeamLeaderId = teamLeader2.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(employee2);

        var employee3 = new User
        {
            Name = "Mark Roberts",
            Username = "mark.roberts",
            Email = "mark.roberts@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.Employee,
            IsActive = true,
            Position = "Operations Coordinator",
            TeamLeaderId = teamLeader3.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(employee3);
        await context.SaveChangesAsync();

        if (itDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = employee1.UserId, DeptId = itDept.DeptId });
        if (marketingDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = employee2.UserId, DeptId = marketingDept.DeptId });
        if (operationsDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = employee3.UserId, DeptId = operationsDept.DeptId });

        // 6. Client Users - Should only see their own projects
        var client1 = new User
        {
            Name = "Rachel Green",
            Username = "rachel.green",
            Email = "rachel.green@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.Client,
            IsActive = true,
            Position = "Client Contact",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(client1);

        var client2 = new User
        {
            Name = "James Taylor",
            Username = "james.taylor",
            Email = "james.taylor@barqtms.com",
            PasswordHash = authService.HashPassword("Password@123"),
            Role = UserRole.Client,
            IsActive = true,
            Position = "Client Contact",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(client2);
        await context.SaveChangesAsync();

        if (hrDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = client1.UserId, DeptId = hrDept.DeptId });
        if (financeDept != null)
            context.UserDepartments.Add(new UserDepartment { UserId = client2.UserId, DeptId = financeDept.DeptId });

        await context.SaveChangesAsync();
    }

    private static async Task SeedClients(BarqTMSDbContext context)
    {
        if (await context.Clients.AnyAsync()) return;

        var accountManager1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "sarah.johnson");
        var accountManager2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "michael.chen");

        var clients = new[]
        {
            new Client { Name = "TechCorp Solutions", Email = "contact@techcorp.com", PhoneNumber = "555-0101", Company = "TechCorp Solutions Inc.", Address = "123 Tech Street", AccountManagerId = accountManager1?.UserId },
            new Client { Name = "Global Industries", Email = "info@globalind.com", PhoneNumber = "555-0102", Company = "Global Industries Ltd.", Address = "456 Industry Ave", AccountManagerId = accountManager1?.UserId },
            new Client { Name = "StartupHub", Email = "hello@startuphub.com", PhoneNumber = "555-0103", Company = "StartupHub Co.", Address = "789 Innovation Blvd", AccountManagerId = accountManager2?.UserId },
            new Client { Name = "Enterprise Partners", Email = "contact@enterprise.com", PhoneNumber = "555-0104", Company = "Enterprise Partners LLC", Address = "321 Business Park", AccountManagerId = accountManager2?.UserId },
            new Client { Name = "Digital Dynamics", Email = "info@digitaldy.com", PhoneNumber = "555-0105", Company = "Digital Dynamics Corp.", Address = "555 Digital Way", AccountManagerId = accountManager1?.UserId }
        };

        await context.Clients.AddRangeAsync(clients);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProjects(BarqTMSDbContext context)
    {
        if (await context.Projects.AnyAsync()) return;

        var clients = await context.Clients.ToListAsync();
        if (clients.Count == 0) return;

        var teamLeader1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "john.smith");
        var teamLeader2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "emily.davis");
        var teamLeader3 = await context.Users.FirstOrDefaultAsync(u => u.Username == "david.wilson");

        var projects = new[]
        {
            new Project { ProjectName = "E-Commerce Platform", Description = "Build a modern e-commerce platform", ClientId = clients[0].ClientId, StartDate = DateTime.UtcNow.AddDays(-60), EndDate = DateTime.UtcNow.AddDays(90), TeamLeaderId = teamLeader1?.UserId },
            new Project { ProjectName = "Mobile App Development", Description = "iOS and Android app for client", ClientId = clients[0].ClientId, StartDate = DateTime.UtcNow.AddDays(-45), EndDate = DateTime.UtcNow.AddDays(75), TeamLeaderId = teamLeader1?.UserId },
            new Project { ProjectName = "Marketing Campaign Q1", Description = "Digital marketing campaign for Q1", ClientId = clients.Count > 1 ? clients[1].ClientId : clients[0].ClientId, StartDate = DateTime.UtcNow.AddDays(-30), EndDate = DateTime.UtcNow.AddDays(60), TeamLeaderId = teamLeader2?.UserId },
            new Project { ProjectName = "Brand Redesign", Description = "Complete brand identity redesign", ClientId = clients.Count > 2 ? clients[2].ClientId : clients[0].ClientId, StartDate = DateTime.UtcNow.AddDays(-20), EndDate = DateTime.UtcNow.AddDays(50), TeamLeaderId = teamLeader2?.UserId },
            new Project { ProjectName = "Operations Optimization", Description = "Improve operational efficiency", ClientId = clients.Count > 3 ? clients[3].ClientId : clients[0].ClientId, StartDate = DateTime.UtcNow.AddDays(-15), EndDate = DateTime.UtcNow.AddDays(45), TeamLeaderId = teamLeader3?.UserId },
            new Project { ProjectName = "Cloud Migration", Description = "Migrate infrastructure to cloud", ClientId = clients.Count > 4 ? clients[4].ClientId : clients[0].ClientId, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(120), TeamLeaderId = teamLeader1?.UserId }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTasks(BarqTMSDbContext context)
    {
        if (await context.Tasks.AnyAsync()) return;

        var projects = await context.Projects.ToListAsync();
        var priorities = await context.Priorities.ToListAsync();
        var statuses = await context.Statuses.ToListAsync();
        var itDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Information Technology");
        var marketingDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Marketing");
        var operationsDept = await context.Departments.FirstOrDefaultAsync(d => d.DeptName == "Operations");
        
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
        var teamLeader1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "john.smith");
        var teamLeader2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "emily.davis");
        var teamLeader3 = await context.Users.FirstOrDefaultAsync(u => u.Username == "david.wilson");
        var employee1 = await context.Users.FirstOrDefaultAsync(u => u.Username == "tom.anderson");
        var employee2 = await context.Users.FirstOrDefaultAsync(u => u.Username == "lisa.thompson");
        var employee3 = await context.Users.FirstOrDefaultAsync(u => u.Username == "mark.roberts");

        if (itDept == null || marketingDept == null || operationsDept == null || admin == null) return;

        var criticalPriority = priorities.FirstOrDefault(p => p.Level == "Critical");
        var highPriority = priorities.FirstOrDefault(p => p.Level == "High");
        var mediumPriority = priorities.FirstOrDefault(p => p.Level == "Medium");
        var lowPriority = priorities.FirstOrDefault(p => p.Level == "Low");

        var todoStatus = statuses.FirstOrDefault(s => s.StatusName == "To Do");
        var inProgressStatus = statuses.FirstOrDefault(s => s.StatusName == "In Progress");
        var inReviewStatus = statuses.FirstOrDefault(s => s.StatusName == "In Review");
        var doneStatus = statuses.FirstOrDefault(s => s.StatusName == "Done");

        if (criticalPriority == null || todoStatus == null || inProgressStatus == null || inReviewStatus == null || doneStatus == null ||
            highPriority == null || mediumPriority == null || lowPriority == null || teamLeader1 == null || teamLeader2 == null || teamLeader3 == null) return;

        var tasks = new List<WorkTask>
        {
            // IT Department Tasks
            new WorkTask { Title = "Database Schema Design", Description = "Design and implement database schema", PriorityId = criticalPriority.PriorityId, StatusId = doneStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(-5), CreatedBy = teamLeader1.UserId, AssignedTo = employee1?.UserId, DeptId = itDept.DeptId, ProjectId = projects.Count > 0 ? projects[0].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder1", CreatedAt = DateTime.UtcNow.AddDays(-60), UpdatedAt = DateTime.UtcNow.AddDays(-5) },
            new WorkTask { Title = "API Development", Description = "Develop RESTful APIs", PriorityId = highPriority.PriorityId, StatusId = inProgressStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(10), CreatedBy = teamLeader1.UserId, AssignedTo = employee1?.UserId, DeptId = itDept.DeptId, ProjectId = projects.Count > 0 ? projects[0].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder2", CreatedAt = DateTime.UtcNow.AddDays(-50), UpdatedAt = DateTime.UtcNow },
            new WorkTask { Title = "Frontend Development", Description = "Build responsive frontend", PriorityId = highPriority.PriorityId, StatusId = inProgressStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(15), CreatedBy = teamLeader1.UserId, AssignedTo = teamLeader1.UserId, DeptId = itDept.DeptId, ProjectId = projects.Count > 0 ? projects[0].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder3", CreatedAt = DateTime.UtcNow.AddDays(-45), UpdatedAt = DateTime.UtcNow },
            new WorkTask { Title = "Payment Gateway", Description = "Integrate payment processing", PriorityId = criticalPriority.PriorityId, StatusId = todoStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(20), CreatedBy = admin.UserId, AssignedTo = employee1?.UserId, DeptId = itDept.DeptId, ProjectId = projects.Count > 0 ? projects[0].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder4", CreatedAt = DateTime.UtcNow.AddDays(-40), UpdatedAt = DateTime.UtcNow },
            new WorkTask { Title = "iOS App Setup", Description = "Initialize iOS project", PriorityId = highPriority.PriorityId, StatusId = inReviewStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(-10), CreatedBy = teamLeader1.UserId, AssignedTo = teamLeader1.UserId, DeptId = itDept.DeptId, ProjectId = projects.Count > 1 ? projects[1].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder5", CreatedAt = DateTime.UtcNow.AddDays(-45), UpdatedAt = DateTime.UtcNow.AddDays(-10) },
            new WorkTask { Title = "Android App Setup", Description = "Initialize Android project", PriorityId = highPriority.PriorityId, StatusId = inReviewStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(-8), CreatedBy = teamLeader1.UserId, AssignedTo = employee1?.UserId, DeptId = itDept.DeptId, ProjectId = projects.Count > 1 ? projects[1].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder6", CreatedAt = DateTime.UtcNow.AddDays(-45), UpdatedAt = DateTime.UtcNow.AddDays(-8) },
            
            // Marketing Department Tasks
            new WorkTask { Title = "Social Media Strategy", Description = "Develop content strategy", PriorityId = highPriority.PriorityId, StatusId = inReviewStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(-7), CreatedBy = teamLeader2.UserId, AssignedTo = employee2?.UserId, DeptId = marketingDept.DeptId, ProjectId = projects.Count > 2 ? projects[2].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder8", CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow.AddDays(-7) },
            new WorkTask { Title = "Content Creation", Description = "Create marketing content", PriorityId = mediumPriority.PriorityId, StatusId = inProgressStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(8), CreatedBy = teamLeader2.UserId, AssignedTo = employee2?.UserId, DeptId = marketingDept.DeptId, ProjectId = projects.Count > 2 ? projects[2].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder9", CreatedAt = DateTime.UtcNow.AddDays(-25), UpdatedAt = DateTime.UtcNow },
            new WorkTask { Title = "Email Campaign", Description = "Configure email automation", PriorityId = mediumPriority.PriorityId, StatusId = todoStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(12), CreatedBy = admin.UserId, AssignedTo = teamLeader2.UserId, DeptId = marketingDept.DeptId, ProjectId = projects.Count > 2 ? projects[2].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder10", CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow },
            new WorkTask { Title = "Logo Design", Description = "Create new logo", PriorityId = criticalPriority.PriorityId, StatusId = inProgressStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(3), CreatedBy = teamLeader2.UserId, AssignedTo = employee2?.UserId, DeptId = marketingDept.DeptId, ProjectId = projects.Count > 3 ? projects[3].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder11", CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow },
            
            // Operations Department Tasks
            new WorkTask { Title = "Process Mapping", Description = "Map operational processes", PriorityId = highPriority.PriorityId, StatusId = inReviewStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(-3), CreatedBy = teamLeader3.UserId, AssignedTo = employee3?.UserId, DeptId = operationsDept.DeptId, ProjectId = projects.Count > 4 ? projects[4].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder13", CreatedAt = DateTime.UtcNow.AddDays(-15), UpdatedAt = DateTime.UtcNow.AddDays(-3) },
            new WorkTask { Title = "Efficiency Analysis", Description = "Analyze bottlenecks", PriorityId = highPriority.PriorityId, StatusId = inProgressStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(7), CreatedBy = teamLeader3.UserId, AssignedTo = employee3?.UserId, DeptId = operationsDept.DeptId, ProjectId = projects.Count > 4 ? projects[4].ProjectId : null, DriveFolderLink = "https://drive.google.com/folder14", CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow },
            
            // Cross-Department Tasks
            new WorkTask { Title = "Security Audit", Description = "Security audit", PriorityId = criticalPriority.PriorityId, StatusId = inProgressStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(4), CreatedBy = admin.UserId, AssignedTo = teamLeader1.UserId, DeptId = itDept.DeptId, ProjectId = null, DriveFolderLink = "https://drive.google.com/folder19", CreatedAt = DateTime.UtcNow.AddDays(-7), UpdatedAt = DateTime.UtcNow },
            new WorkTask { Title = "Team Training", Description = "Quarterly training", PriorityId = lowPriority.PriorityId, StatusId = todoStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(30), CreatedBy = admin.UserId, AssignedTo = null, DeptId = itDept.DeptId, ProjectId = null, DriveFolderLink = "https://drive.google.com/folder20", CreatedAt = DateTime.UtcNow.AddDays(-3), UpdatedAt = DateTime.UtcNow },
            
            // Overdue Tasks
            new WorkTask { Title = "Overdue IT Task", Description = "Overdue task", PriorityId = highPriority.PriorityId, StatusId = inProgressStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(-5), CreatedBy = teamLeader1.UserId, AssignedTo = employee1?.UserId, DeptId = itDept.DeptId, ProjectId = null, DriveFolderLink = "https://drive.google.com/folder22", CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow.AddDays(-5) },
            new WorkTask { Title = "Overdue Marketing Task", Description = "Overdue marketing", PriorityId = criticalPriority.PriorityId, StatusId = todoStatus.StatusId, DueDate = DateTime.UtcNow.AddDays(-3), CreatedBy = teamLeader2.UserId, AssignedTo = employee2?.UserId, DeptId = marketingDept.DeptId, ProjectId = null, DriveFolderLink = "https://drive.google.com/folder23", CreatedAt = DateTime.UtcNow.AddDays(-15), UpdatedAt = DateTime.UtcNow.AddDays(-3) }
        };

        await context.Tasks.AddRangeAsync(tasks);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTaskComments(BarqTMSDbContext context)
    {
        if (await context.TaskComments.AnyAsync()) return;

        var tasks = await context.Tasks.Take(5).ToListAsync();
        var users = await context.Users.ToListAsync();

        var comments = new List<TaskComment>();
        
        if (tasks.Count > 0 && users.Count > 1)
        {
            comments.Add(new TaskComment { TaskId = tasks[0].TaskId, UserId = users[1].UserId, Comment = "Great progress on the database design!", CreatedAt = DateTime.UtcNow.AddDays(-4) });
        }
        
        if (tasks.Count > 1 && users.Count > 1)
        {
            comments.Add(new TaskComment { TaskId = tasks[1].TaskId, UserId = users[0].UserId, Comment = "Make sure to follow REST best practices.", CreatedAt = DateTime.UtcNow.AddDays(-2) });
        }

        if (comments.Any())
        {
            await context.TaskComments.AddRangeAsync(comments);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedNotifications(BarqTMSDbContext context)
    {
        if (await context.Notifications.AnyAsync()) return;

        var users = await context.Users.ToListAsync();
        var tasks = await context.Tasks.Take(10).ToListAsync();

        var notifications = new List<Notification>();

        foreach (var user in users.Take(5))
        {
            notifications.Add(new Notification
            {
                UserId = user.UserId,
                Message = "Welcome to BarqTMS!",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsRead = false
            });
        }

        if (tasks.Any())
        {
            foreach (var task in tasks.Take(5))
            {
                if (task.AssignedTo.HasValue)
                {
                    notifications.Add(new Notification
                    {
                        UserId = task.AssignedTo.Value,
                        Message = $"You have been assigned to task: {task.Title}",
                        TaskId = task.TaskId,
                        CreatedAt = DateTime.UtcNow.AddHours(-6),
                        IsRead = false
                    });
                }
            }
        }

        await context.Notifications.AddRangeAsync(notifications);
        await context.SaveChangesAsync();
    }

        private static async Task SeedUserSettings(BarqTMSDbContext context)
        {
            var users = await context.Users.ToListAsync();
            var settings = new List<UserSettings>();

            foreach (var user in users)
            {
                var userSetting = new UserSettings
                {
                    UserId = user.UserId,
                    Theme = "light",
                    Language = "en",
                    Timezone = "UTC",
                    EmailNotifications = true,
                    PushNotifications = true,
                    TaskReminders = true,
                    UpdatedAt = DateTime.UtcNow
                };

                settings.Add(userSetting);
            }

            await context.UserSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }
    }
}
