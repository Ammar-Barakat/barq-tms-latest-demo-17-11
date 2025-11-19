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
            //await SeedTaskCategories(context);
            //await SeedClients(context);
            await SeedUsers(context, authService);
            await SeedUserSettings(context);
            //await SeedProjects(context);
            //await SeedProjectMilestones(context);
            //await SeedTasks(context);
            //await SeedTaskDependencies(context);
            //await SeedTimeLogs(context);
            //await SeedCalendarEvents(context);
            //await SeedCalendarAttendees(context);
            //await SeedCalendarReminders(context);
            //await SeedAttachments(context);
            //await SeedNotifications(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedPriorities(BarqTMSDbContext context)
        {
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

        private static async Task SeedClients(BarqTMSDbContext context)
        {
            var clients = new[]
            {
                new Client { Name = "TechCorp Solutions", Email = "contact@techcorp.com" },
                new Client { Name = "Global Industries", Email = "info@globalind.com" },
            };

            await context.Clients.AddRangeAsync(clients);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUsers(BarqTMSDbContext context, AuthService authService)
        {
            var users = new[]
            {
                // Default Admin Account
                new User
                {
                    Name = "System Administrator",
                    Username = "admin",
                    Email = "admin@barqtms.com",
                    Role = UserRole.Manager,
                    PasswordHash = authService.HashPassword("Admin@123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-400),
                    UpdatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsActive = true
                },
                // CEO
                new User
                {
                    Name = "Sarah Johnson",
                    Username = "sarah.johnson",
                    Email = "ceo@barqtms.com",
                    Role = UserRole.Manager,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-365),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30),
                    LastLogin = DateTime.UtcNow.AddHours(-2),
                    IsActive = true
                },
                // Owner
                new User
                {
                    Name = "Ahmed Al-Rashid",
                    Username = "ahmed.alrashid",
                    Email = "owner@barqtms.com",
                    Role = UserRole.AssistantManager,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-350),
                    UpdatedAt = DateTime.UtcNow.AddDays(-15),
                    LastLogin = DateTime.UtcNow.AddHours(-1),
                    IsActive = true
                },
                // Managers
                new User
                {
                    Name = "Michael Chen",
                    Username = "michael.chen",
                    Email = "m.chen@barqtms.com",
                    Role = UserRole.AccountManager,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-300),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10),
                    LastLogin = DateTime.UtcNow.AddMinutes(-30),
                    IsActive = true
                },
                new User
                {
                    Name = "Fatima Al-Zahra",
                    Username = "fatima.alzahra",
                    Email = "f.alzahra@barqtms.com",
                    Role = UserRole.TeamLeader,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-280),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5),
                    LastLogin = DateTime.UtcNow.AddHours(-3),
                    IsActive = true
                },
                // Employees
                new User
                {
                    Name = "John Smith",
                    Username = "john.smith",
                    Email = "j.smith@barqtms.com",
                    Role = UserRole.Employee,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-200),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    LastLogin = DateTime.UtcNow.AddMinutes(-15),
                    IsActive = true
                },
                new User
                {
                    Name = "Emily Davis",
                    Username = "emily.davis",
                    Email = null, // No email provided yet
                    Role = UserRole.Employee,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-180),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    LastLogin = DateTime.UtcNow.AddMinutes(-45),
                    IsActive = true
                },
                new User
                {
                    Name = "Omar Hassan",
                    Username = "omar.hassan",
                    Email = "o.hassan@barqtms.com",
                    Role = UserRole.Employee,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-150),
                    UpdatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow.AddMinutes(-10),
                    IsActive = true
                },
                new User
                {
                    Name = "Lisa Wang",
                    Username = "lisa.wang",
                    Email = null, // No email provided yet
                    Role = UserRole.Employee,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-120),
                    UpdatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow.AddMinutes(-5),
                    IsActive = true
                },
                // Client Users
                new User
                {
                    Name = "Robert Miller",
                    Username = "robert.miller",
                    Email = "r.miller@techcorp.com",
                    Role = UserRole.Client,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-100),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3),
                    LastLogin = DateTime.UtcNow.AddHours(-6),
                    IsActive = true
                },
                new User
                {
                    Name = "Jennifer Wilson",
                    Username = "jennifer.wilson",
                    Email = null, // No email provided yet
                    Role = UserRole.Client,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-90),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7),
                    LastLogin = DateTime.UtcNow.AddHours(-12),
                    IsActive = true
                },
                new User
                {
                    Name = "David Brown",
                    Username = "david.brown",
                    Email = "d.brown@startuphub.com",
                    Role = UserRole.Client,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-80),
                    UpdatedAt = DateTime.UtcNow.AddDays(-4),
                    LastLogin = DateTime.UtcNow.AddHours(-8),
                    IsActive = true
                },
                new User
                {
                    Name = "Maria Garcia",
                    Username = "maria.garcia",
                    Email = "m.garcia@enterprise.com",
                    Role = UserRole.Client,
                    PasswordHash = authService.HashPassword("password123"),
                    CreatedAt = DateTime.UtcNow.AddDays(-70),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    LastLogin = DateTime.UtcNow.AddHours(-4),
                    IsActive = true
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            // Assign departments to users (no more role assignments needed)
            var departmentAssignments = new List<UserDepartment>();

            // Get departments
            var itDept = await context.Departments.FirstAsync(d => d.DeptName == "Information Technology");
            var hrDept = await context.Departments.FirstAsync(d => d.DeptName == "Human Resources");
            var marketingDept = await context.Departments.FirstAsync(d => d.DeptName == "Marketing");
            var salesDept = await context.Departments.FirstAsync(d => d.DeptName == "Sales");
            var financeDept = await context.Departments.FirstAsync(d => d.DeptName == "Finance");

            // Admin (System Administrator - User ID 1)
            departmentAssignments.Add(new UserDepartment { UserId = 1, DeptId = itDept.DeptId });

            // CEO (Sarah Johnson - User ID 2)
            departmentAssignments.Add(new UserDepartment { UserId = 2, DeptId = itDept.DeptId });

            // Owner (Ahmed Al-Rashid - User ID 3)
            departmentAssignments.Add(new UserDepartment { UserId = 3, DeptId = itDept.DeptId });

            // Managers
            departmentAssignments.Add(new UserDepartment { UserId = 4, DeptId = itDept.DeptId });
            departmentAssignments.Add(new UserDepartment { UserId = 5, DeptId = hrDept.DeptId });

            // Employees
            departmentAssignments.Add(new UserDepartment { UserId = 6, DeptId = itDept.DeptId });
            departmentAssignments.Add(new UserDepartment { UserId = 7, DeptId = marketingDept.DeptId });
            departmentAssignments.Add(new UserDepartment { UserId = 8, DeptId = salesDept.DeptId });
            departmentAssignments.Add(new UserDepartment { UserId = 9, DeptId = financeDept.DeptId });

            // Clients
            for (int i = 10; i <= 13; i++)
            {
                departmentAssignments.Add(new UserDepartment { UserId = i, DeptId = salesDept.DeptId });
            }

            await context.UserDepartments.AddRangeAsync(departmentAssignments);
            await context.SaveChangesAsync();
        }

        //private static async Task SeedProjects(BarqTMSDbContext context)
        //{
        //    var projects = new[]
        //    {
        //        new Project
        //        {
        //            ProjectName = "E-Commerce Platform Development",
        //            Description = "Complete e-commerce solution with payment integration and inventory management",
        //            ClientId = 1,
        //            StartDate = DateTime.UtcNow.AddDays(-120),
        //            EndDate = DateTime.UtcNow.AddDays(60)
        //        },
        //        new Project
        //        {
        //            ProjectName = "Mobile App for Customer Service",
        //            Description = "iOS and Android app for customer support and ticket management",
        //            ClientId = 2,
        //            StartDate = DateTime.UtcNow.AddDays(-90),
        //            EndDate = DateTime.UtcNow.AddDays(90)
        //        },
        //        new Project
        //        {
        //            ProjectName = "Data Analytics Dashboard",
        //            Description = "Real-time analytics dashboard with advanced reporting features",
        //            ClientId = 3,
        //            StartDate = DateTime.UtcNow.AddDays(-60),
        //            EndDate = DateTime.UtcNow.AddDays(120)
        //        },
        //        new Project
        //        {
        //            ProjectName = "Enterprise Resource Planning System",
        //            Description = "Comprehensive ERP system for resource management and planning",
        //            ClientId = 4,
        //            StartDate = DateTime.UtcNow.AddDays(-150),
        //            EndDate = DateTime.UtcNow.AddDays(30)
        //        },
        //        new Project
        //        {
        //            ProjectName = "Cloud Migration Initiative",
        //            Description = "Migration of legacy systems to cloud infrastructure",
        //            ClientId = 5,
        //            StartDate = DateTime.UtcNow.AddDays(-30),
        //            EndDate = DateTime.UtcNow.AddDays(180)
        //        },
        //        new Project
        //        {
        //            ProjectName = "AI-Powered Recommendation Engine",
        //            Description = "Machine learning system for personalized product recommendations",
        //            ClientId = 6,
        //            StartDate = DateTime.UtcNow.AddDays(-45),
        //            EndDate = DateTime.UtcNow.AddDays(150)
        //        },
        //        new Project
        //        {
        //            ProjectName = "Security Audit and Compliance",
        //            Description = "Comprehensive security assessment and compliance implementation",
        //            ClientId = 7,
        //            StartDate = DateTime.UtcNow.AddDays(-75),
        //            EndDate = DateTime.UtcNow.AddDays(45)
        //        },
        //        new Project
        //        {
        //            ProjectName = "Digital Marketing Platform",
        //            Description = "Multi-channel digital marketing automation platform",
        //            ClientId = 8,
        //            StartDate = DateTime.UtcNow.AddDays(-100),
        //            EndDate = DateTime.UtcNow.AddDays(80)
        //        }
        //    };

        //    await context.Projects.AddRangeAsync(projects);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedTasks(BarqTMSDbContext context)
        //{
        //    var random = new Random();
        //    var priorities = await context.Priorities.ToListAsync();
        //    var statuses = await context.Statuses.ToListAsync();
        //    var projects = await context.Projects.ToListAsync();
        //    var departments = await context.Departments.ToListAsync();
        //    var employeeUserIds = new[] { 3, 4, 5, 6, 7, 8 }; // Managers and employees

        //    var tasks = new List<WorkTask>();

        //    var taskTemplates = new[]
        //    {
        //        ("Requirements Analysis", "Analyze and document system requirements"),
        //        ("Database Design", "Design database schema and relationships"),
        //        ("API Development", "Develop RESTful API endpoints"),
        //        ("Frontend Implementation", "Implement user interface components"),
        //        ("Testing and QA", "Conduct comprehensive testing"),
        //        ("Documentation", "Create technical and user documentation"),
        //        ("Deployment Setup", "Configure production deployment"),
        //        ("Performance Optimization", "Optimize system performance"),
        //        ("Security Implementation", "Implement security measures"),
        //        ("User Training", "Conduct user training sessions"),
        //        ("Code Review", "Review and approve code changes"),
        //        ("Bug Fixes", "Fix reported bugs and issues"),
        //        ("Feature Enhancement", "Enhance existing features"),
        //        ("Integration Testing", "Test system integrations"),
        //        ("System Monitoring", "Set up monitoring and alerts")
        //    };

        //    foreach (var project in projects)
        //    {
        //        var tasksForProject = random.Next(8, 15);
                
        //        for (int i = 0; i < tasksForProject; i++)
        //        {
        //            var template = taskTemplates[random.Next(taskTemplates.Length)];
        //            var createdBy = employeeUserIds[random.Next(employeeUserIds.Length)];
        //            var assignedTo = random.Next(10) < 8 ? employeeUserIds[random.Next(employeeUserIds.Length)] : (int?)null;
                    
        //            var task = new WorkTask
        //            {
        //                Title = $"{template.Item1} - {project.ProjectName}",
        //                Description = template.Item2,
        //                PriorityId = priorities[random.Next(priorities.Count)].PriorityId,
        //                StatusId = statuses[random.Next(statuses.Count)].StatusId,
        //                DueDate = DateTime.UtcNow.AddDays(random.Next(-30, 90)),
        //                CreatedBy = createdBy,
        //                AssignedTo = assignedTo,
        //                DeptId = departments[random.Next(departments.Count)].DeptId,
        //                ProjectId = project.ProjectId
        //            };

        //            tasks.Add(task);
        //        }
        //    }

        //    await context.Tasks.AddRangeAsync(tasks);
        //    await context.SaveChangesAsync();

        //    // Add some task comments and history
        //    await SeedTaskComments(context);
        //    await SeedTaskHistory(context);
        //}

        //private static async Task SeedTaskComments(BarqTMSDbContext context)
        //{
        //    var tasks = await context.Tasks.Take(20).ToListAsync();
        //    var userIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        //    var random = new Random();

        //    var commentTemplates = new[]
        //    {
        //        "Great progress on this task!",
        //        "Please review the implementation details.",
        //        "This looks good, ready for testing.",
        //        "I've updated the requirements document.",
        //        "Can we schedule a meeting to discuss this?",
        //        "The deadline might need to be extended.",
        //        "Excellent work, well done!",
        //        "Please add more error handling.",
        //        "The design needs some adjustments.",
        //        "Ready for deployment to staging."
        //    };

        //    var comments = new List<TaskComment>();

        //    foreach (var task in tasks)
        //    {
        //        var commentsCount = random.Next(1, 5);
                
        //        for (int i = 0; i < commentsCount; i++)
        //        {
        //            var comment = new TaskComment
        //            {
        //                TaskId = task.TaskId,
        //                UserId = userIds[random.Next(userIds.Length)],
        //                Comment = commentTemplates[random.Next(commentTemplates.Length)],
        //                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
        //            };

        //            comments.Add(comment);
        //        }
        //    }

        //    await context.TaskComments.AddRangeAsync(comments);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedTaskHistory(BarqTMSDbContext context)
        //{
        //    var tasks = await context.Tasks.Take(15).ToListAsync();
        //    var userIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        //    var random = new Random();

        //    var historyTemplates = new[]
        //    {
        //        "Task created",
        //        "Status changed to In Progress",
        //        "Priority updated",
        //        "Assigned to team member",
        //        "Description updated",
        //        "Due date modified",
        //        "Task completed",
        //        "Additional comments added"
        //    };

        //    var histories = new List<AuditLog>();

        //    foreach (var task in tasks)
        //    {
        //        var historyCount = random.Next(2, 6);
                
        //        for (int i = 0; i < historyCount; i++)
        //        {
        //            var history = new AuditLog
        //            {
        //                EntityType = "Task",
        //                EntityId = task.TaskId,
        //                UserId = userIds[random.Next(userIds.Length)],
        //                Action = historyTemplates[random.Next(historyTemplates.Length)],
        //                Timestamp = DateTime.UtcNow.AddDays(-random.Next(1, 60))
        //            };

        //            histories.Add(history);
        //        }
        //    }

        //    await context.AuditLogs.AddRangeAsync(histories);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedNotifications(BarqTMSDbContext context)
        //{
        //    var userIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        //    var tasks = await context.Tasks.Take(10).ToListAsync();
        //    var projects = await context.Projects.Take(5).ToListAsync();
        //    var random = new Random();

        //    var notificationTemplates = new[]
        //    {
        //        "You have been assigned a new task",
        //        "Task status has been updated",
        //        "Project deadline is approaching",
        //        "New comment added to your task",
        //        "Task has been completed",
        //        "Meeting scheduled for project review",
        //        "Document uploaded to project",
        //        "System maintenance scheduled"
        //    };

        //    var notifications = new List<Notification>();

        //    foreach (var userId in userIds)
        //    {
        //        var notificationCount = random.Next(5, 12);
                
        //        for (int i = 0; i < notificationCount; i++)
        //        {
        //            var task = tasks[random.Next(tasks.Count)];
        //            var project = projects[random.Next(projects.Count)];
                    
        //            var notification = new Notification
        //            {
        //                UserId = userId,
        //                Message = notificationTemplates[random.Next(notificationTemplates.Length)],
        //                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
        //                IsRead = random.Next(10) < 6, // 60% chance of being read
        //                TaskId = random.Next(10) < 7 ? task.TaskId : null, // 70% chance of having task
        //                ProjectId = random.Next(10) < 4 ? project.ProjectId : null // 40% chance of having project
        //            };

        //            notifications.Add(notification);
        //        }
        //    }

        //    await context.Notifications.AddRangeAsync(notifications);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedTaskCategories(BarqTMSDbContext context)
        //{
        //    var categories = new[]
        //    {
        //        new TaskCategory { Name = "Development", Description = "Software development tasks", Color = "#007bff" },
        //        new TaskCategory { Name = "Testing", Description = "Quality assurance and testing", Color = "#28a745" },
        //        new TaskCategory { Name = "Design", Description = "UI/UX design tasks", Color = "#e83e8c" },
        //        new TaskCategory { Name = "Documentation", Description = "Documentation and knowledge base", Color = "#6f42c1" },
        //        new TaskCategory { Name = "DevOps", Description = "Deployment and infrastructure", Color = "#fd7e14" },
        //        new TaskCategory { Name = "Research", Description = "Research and analysis", Color = "#20c997" },
        //        new TaskCategory { Name = "Meeting", Description = "Meetings and discussions", Color = "#6c757d" },
        //        new TaskCategory { Name = "Bug Fix", Description = "Bug fixes and issues", Color = "#dc3545" },
        //        new TaskCategory { Name = "Feature", Description = "New feature development", Color = "#17a2b8" },
        //        new TaskCategory { Name = "Maintenance", Description = "System maintenance tasks", Color = "#ffc107" }
        //    };

        //    await context.TaskCategories.AddRangeAsync(categories);
        //    await context.SaveChangesAsync();
        //}

        private static async Task SeedUserSettings(BarqTMSDbContext context)
        {
            var users = await context.Users.ToListAsync();
            var settings = new List<UserSettings>();
            var random = new Random();
            
            var themes = new[] { "light", "dark", "auto" };
            var languages = new[] { "en", "ar", "es", "fr" };
            var timezones = new[] { "UTC", "Asia/Dubai", "America/New_York", "Europe/London", "Asia/Tokyo" };

            foreach (var user in users)
            {
                var userSetting = new UserSettings
                {
                    UserId = user.UserId,
                    Theme = themes[random.Next(themes.Length)],
                    Language = languages[random.Next(languages.Length)],
                    Timezone = timezones[random.Next(timezones.Length)],
                    EmailNotifications = random.Next(10) < 8, // 80% enable email notifications
                    PushNotifications = random.Next(10) < 7, // 70% enable push notifications
                    TaskReminders = random.Next(10) < 9, // 90% enable task reminders
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                };

                settings.Add(userSetting);
            }

            await context.UserSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }

        //private static async Task SeedProjectMilestones(BarqTMSDbContext context)
        //{
        //    var projects = await context.Projects.ToListAsync();
        //    var milestones = new List<ProjectMilestone>();
        //    var random = new Random();

        //    var milestoneTemplates = new[]
        //    {
        //        ("Project Kickoff", "Initial project setup and team alignment"),
        //        ("Requirements Complete", "All requirements gathered and documented"),
        //        ("Design Phase Complete", "System design and architecture finalized"),
        //        ("Development Phase 1", "Core functionality implementation"),
        //        ("Alpha Release", "Internal testing version ready"),
        //        ("Beta Release", "External testing version ready"),
        //        ("User Acceptance Testing", "Client testing and feedback"),
        //        ("Go-Live Preparation", "Final deployment preparations"),
        //        ("Production Release", "System deployed to production"),
        //        ("Project Closure", "Project completion and handover")
        //    };

        //    foreach (var project in projects)
        //    {
        //        var milestoneCount = random.Next(4, 8);
        //        var startDate = project.StartDate ?? DateTime.UtcNow.AddDays(-100);
        //        var endDate = project.EndDate ?? DateTime.UtcNow.AddDays(100);
        //        var totalDays = (endDate - startDate).Days;

        //        for (int i = 0; i < milestoneCount; i++)
        //        {
        //            var template = milestoneTemplates[Math.Min(i, milestoneTemplates.Length - 1)];
        //            var progressRatio = (double)i / (milestoneCount - 1);
        //            var dueDate = startDate.AddDays(totalDays * progressRatio);
                    
        //            var milestone = new ProjectMilestone
        //            {
        //                ProjectId = project.ProjectId,
        //                Name = template.Item1,
        //                Description = template.Item2,
        //                DueDate = dueDate,
        //                IsCompleted = dueDate < DateTime.UtcNow && random.Next(10) < 7, // 70% chance if due date passed
        //                CompletionDate = dueDate < DateTime.UtcNow && random.Next(10) < 7 ? 
        //                    dueDate.AddDays(random.Next(-2, 5)) : null,
        //                CreatedAt = startDate.AddDays(-random.Next(1, 10))
        //            };

        //            milestones.Add(milestone);
        //        }
        //    }

        //    await context.ProjectMilestones.AddRangeAsync(milestones);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedTaskDependencies(BarqTMSDbContext context)
        //{
        //    var tasks = await context.Tasks.ToListAsync();
        //    var dependencies = new List<TaskDependency>();
        //    var random = new Random();

        //    // Create some logical dependencies
        //    var tasksByProject = tasks.GroupBy(t => t.ProjectId).ToList();

        //    foreach (var projectTasks in tasksByProject)
        //    {
        //        var projectTaskList = projectTasks.ToList();
        //        var dependencyCount = Math.Min(projectTaskList.Count / 2, random.Next(2, 6));

        //        for (int i = 0; i < dependencyCount; i++)
        //        {
        //            var dependentTask = projectTaskList[random.Next(projectTaskList.Count)];
        //            var prerequisiteTask = projectTaskList[random.Next(projectTaskList.Count)];

        //            // Ensure we don't create circular dependencies
        //            if (dependentTask.TaskId != prerequisiteTask.TaskId &&
        //                !dependencies.Any(d => d.TaskId == prerequisiteTask.TaskId && d.PrerequisiteTaskId == dependentTask.TaskId))
        //            {
        //                var dependency = new TaskDependency
        //                {
        //                    TaskId = dependentTask.TaskId,
        //                    PrerequisiteTaskId = prerequisiteTask.TaskId,
        //                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 60))
        //                };

        //                dependencies.Add(dependency);
        //            }
        //        }
        //    }

        //    await context.TaskDependencies.AddRangeAsync(dependencies);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedTimeLogs(BarqTMSDbContext context)
        //{
        //    var tasks = await context.Tasks.Take(30).ToListAsync();
        //    var users = await context.Users.Where(u => new[] { 3, 4, 5, 6, 7, 8 }.Contains(u.UserId)).ToListAsync();
        //    var timeLogs = new List<TimeLog>();
        //    var random = new Random();

        //    var workDescriptions = new[]
        //    {
        //        "Initial analysis and planning",
        //        "Code implementation",
        //        "Testing and debugging",
        //        "Code review and optimization",
        //        "Documentation updates",
        //        "Client meeting and discussion",
        //        "Research and investigation",
        //        "Bug fixing and troubleshooting",
        //        "Feature development",
        //        "System integration work"
        //    };

        //    foreach (var task in tasks)
        //    {
        //        var logCount = random.Next(1, 8);
                
        //        for (int i = 0; i < logCount; i++)
        //        {
        //            var user = users[random.Next(users.Count)];
        //            var workDate = DateTime.UtcNow.AddDays(-random.Next(1, 30));
        //            var startTime = workDate.Date.AddHours(8 + random.Next(0, 8)); // Work hours 8AM-4PM
        //            var durationMinutes = random.Next(15, 480); // 15 minutes to 8 hours
        //            var endTime = startTime.AddMinutes(durationMinutes);

        //            var timeLog = new TimeLog
        //            {
        //                TaskId = task.TaskId,
        //                UserId = user.UserId,
        //                StartTime = startTime,
        //                EndTime = endTime,
        //                DurationMinutes = durationMinutes,
        //                Description = workDescriptions[random.Next(workDescriptions.Length)],
        //                IsBillable = random.Next(10) < 8, // 80% billable
        //                CreatedAt = startTime.AddMinutes(durationMinutes + random.Next(1, 60))
        //            };

        //            timeLogs.Add(timeLog);
        //        }
        //    }

        //    await context.TimeLogs.AddRangeAsync(timeLogs);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedCalendarEvents(BarqTMSDbContext context)
        //{
        //    var users = await context.Users.ToListAsync();
        //    var tasks = await context.Tasks.Take(20).ToListAsync();
        //    var projects = await context.Projects.ToListAsync();
        //    var departments = await context.Departments.ToListAsync();
        //    var events = new List<CalendarEvent>();
        //    var random = new Random();

        //    var eventTitles = new[]
        //    {
        //        ("Team Standup", "Daily team synchronization meeting"),
        //        ("Project Review", "Weekly project progress review"),
        //        ("Client Meeting", "Meeting with client to discuss requirements"),
        //        ("Sprint Planning", "Plan upcoming sprint tasks and goals"),
        //        ("Code Review Session", "Review and discuss code changes"),
        //        ("Training Session", "Team training and skill development"),
        //        ("System Maintenance", "Scheduled system maintenance window"),
        //        ("Deadline Reminder", "Important project deadline approaching"),
        //        ("Team Building", "Team building and social activities"),
        //        ("Architecture Discussion", "Technical architecture planning"),
        //        ("User Demo", "Demonstrate new features to users"),
        //        ("Retrospective", "Sprint retrospective and improvement"),
        //        ("Technical Workshop", "Technical learning workshop"),
        //        ("Performance Review", "Individual performance review meeting"),
        //        ("Budget Meeting", "Project budget and resource planning")
        //    };

        //    var colors = new[] { "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8", "#6f42c1", "#e83e8c", "#fd7e14" };
        //    var eventTypes = Enum.GetValues<CalendarEventType>();

        //    foreach (var user in users)
        //    {
        //        var eventCount = random.Next(5, 15);
                
        //        for (int i = 0; i < eventCount; i++)
        //        {
        //            var eventData = eventTitles[random.Next(eventTitles.Length)];
        //            var startDate = DateTime.UtcNow.AddDays(random.Next(-30, 60)).Date
        //                .AddHours(random.Next(8, 17)) // Business hours
        //                .AddMinutes(random.Next(0, 4) * 15); // 15-minute intervals
                    
        //            var duration = random.Next(1, 5) * 30; // 30 minutes to 2.5 hours
        //            var endDate = startDate.AddMinutes(duration);
        //            var isAllDay = random.Next(10) < 2; // 20% chance all-day

        //            if (isAllDay)
        //            {
        //                startDate = startDate.Date;
        //                endDate = startDate.AddDays(1).AddSeconds(-1);
        //            }

        //            var calendarEvent = new CalendarEvent
        //            {
        //                Title = eventData.Item1,
        //                Description = eventData.Item2,
        //                StartDate = startDate,
        //                EndDate = endDate,
        //                IsAllDay = isAllDay,
        //                Color = colors[random.Next(colors.Length)],
        //                EventType = eventTypes[random.Next(eventTypes.Length)],
        //                UserId = user.UserId,
        //                CreatedByUserId = user.UserId,
        //                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 10))
        //            };

        //            // Randomly assign to task, project, or department
        //            var assignmentType = random.Next(4);
        //            switch (assignmentType)
        //            {
        //                case 0:
        //                    calendarEvent.TaskId = tasks[random.Next(tasks.Count)].TaskId;
        //                    break;
        //                case 1:
        //                    calendarEvent.ProjectId = projects[random.Next(projects.Count)].ProjectId;
        //                    break;
        //                case 2:
        //                    calendarEvent.DepartmentId = departments[random.Next(departments.Count)].DeptId;
        //                    break;
        //                // case 3: No assignment (personal event)
        //            }

        //            // Add recurrence for some events
        //            if (random.Next(10) < 3) // 30% recurring
        //            {
        //                calendarEvent.IsRecurring = true;
        //                calendarEvent.RecurrencePattern = (RecurrencePattern)random.Next(1, 6);
        //                calendarEvent.RecurrenceInterval = random.Next(1, 4);
        //                calendarEvent.RecurrenceEndDate = startDate.AddMonths(random.Next(3, 12));
        //            }

        //            events.Add(calendarEvent);
        //        }
        //    }

        //    await context.CalendarEvents.AddRangeAsync(events);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedCalendarAttendees(BarqTMSDbContext context)
        //{
        //    var events = await context.CalendarEvents.Take(50).ToListAsync();
        //    var users = await context.Users.ToListAsync();
        //    var attendees = new List<CalendarEventAttendee>();
        //    var random = new Random();

        //    var responseStatuses = new[] { "Pending", "Accepted", "Declined", "Tentative" };

        //    foreach (var calendarEvent in events)
        //    {
        //        var attendeeCount = random.Next(1, Math.Min(6, users.Count));
        //        var selectedUsers = users.OrderBy(x => random.Next()).Take(attendeeCount);

        //        foreach (var user in selectedUsers)
        //        {
        //            var attendee = new CalendarEventAttendee
        //            {
        //                CalendarEventId = calendarEvent.Id,
        //                UserId = user.UserId,
        //                Status = (AttendeeStatus)random.Next(1, 6), // 1-5 for enum values
        //                ResponseDate = random.Next(10) < 7 ? 
        //                    calendarEvent.CreatedAt.AddDays(random.Next(0, 3)) : null,
        //                IsOrganizer = user.UserId == calendarEvent.CreatedByUserId,
        //                CreatedAt = calendarEvent.CreatedAt
        //            };

        //            attendees.Add(attendee);
        //        }
        //    }

        //    await context.CalendarEventAttendees.AddRangeAsync(attendees);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedCalendarReminders(BarqTMSDbContext context)
        //{
        //    var events = await context.CalendarEvents.Take(40).ToListAsync();
        //    var reminders = new List<CalendarReminder>();
        //    var random = new Random();

        //    var reminderMinutes = new[] { 5, 10, 15, 30, 60, 120, 1440 }; // 5 min to 1 day

        //    foreach (var calendarEvent in events)
        //    {
        //        // 70% of events have reminders
        //        if (random.Next(10) < 7)
        //        {
        //            var reminderCount = random.Next(1, 3); // 1-2 reminders per event
                    
        //            for (int i = 0; i < reminderCount; i++)
        //            {
        //                var minutesBefore = reminderMinutes[random.Next(reminderMinutes.Length)];
        //                var reminderTime = calendarEvent.StartDate.AddMinutes(-minutesBefore);

        //                var reminder = new CalendarReminder
        //                {
        //                    CalendarEventId = calendarEvent.Id,
        //                    UserId = calendarEvent.UserId ?? calendarEvent.CreatedByUserId,
        //                    MinutesBefore = minutesBefore,
        //                    Type = (ReminderType)random.Next(1, 4),
        //                    SentAt = reminderTime < DateTime.UtcNow ? reminderTime : null,
        //                    CreatedAt = calendarEvent.CreatedAt
        //                };

        //                reminders.Add(reminder);
        //            }
        //        }
        //    }

        //    await context.CalendarReminders.AddRangeAsync(reminders);
        //    await context.SaveChangesAsync();
        //}

        //private static async Task SeedAttachments(BarqTMSDbContext context)
        //{
        //    var tasks = await context.Tasks.Take(25).ToListAsync();
        //    var users = await context.Users.ToListAsync();
        //    var attachments = new List<Attachment>();
        //    var random = new Random();

        //    var fileNames = new[]
        //    {
        //        "requirements.pdf",
        //        "design-mockup.png", 
        //        "technical-spec.docx",
        //        "database-schema.sql",
        //        "api-documentation.md",
        //        "test-results.xlsx",
        //        "architecture-diagram.png",
        //        "user-manual.pdf",
        //        "source-code.zip",
        //        "meeting-notes.txt"
        //    };

        //    // Task attachments only (based on the current Attachment model)
        //    foreach (var task in tasks)
        //    {
        //        if (random.Next(10) < 6) // 60% of tasks have attachments
        //        {
        //            var attachmentCount = random.Next(1, 4);
                    
        //            for (int i = 0; i < attachmentCount; i++)
        //            {
        //                var fileName = fileNames[random.Next(fileNames.Length)];
        //                var uploader = users[random.Next(users.Count)];
                        
        //                var attachment = new Attachment
        //                {
        //                    FileName = fileName,
        //                    FileUrl = $"/uploads/tasks/{task.TaskId}/{Guid.NewGuid()}-{fileName}",
        //                    TaskId = task.TaskId,
        //                    UploadedBy = uploader.UserId,
        //                    UploadedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
        //                };

        //                attachments.Add(attachment);
        //            }
        //        }
        //    }

        //    await context.Attachments.AddRangeAsync(attachments);
        //    await context.SaveChangesAsync();
        //}
    }
}