using Microsoft.EntityFrameworkCore;
using BarqTMS.API.Models;

namespace BarqTMS.API.Data
{
    public class BarqTMSDbContext : DbContext
    {
        public BarqTMSDbContext(DbContextOptions<BarqTMSDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserDepartment> UserDepartments { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<WorkTask> Tasks { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }
        public DbSet<RecurringTask> RecurringTasks { get; set; }
        public DbSet<ProjectMilestone> ProjectMilestones { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<CalendarEvent> CalendarEvents { get; set; }
        public DbSet<CalendarEventAttendee> CalendarEventAttendees { get; set; }
        public DbSet<CalendarReminder> CalendarReminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicit table names for calendar entities (consistency with other uppercase table names)
            modelBuilder.Entity<CalendarEvent>().ToTable("CALENDAR_EVENT");
            modelBuilder.Entity<CalendarEventAttendee>().ToTable("CALENDAR_EVENT_ATTENDEE");
            modelBuilder.Entity<CalendarReminder>().ToTable("CALENDAR_REMINDER");

            // Configure UserDepartment relationships
            modelBuilder.Entity<UserDepartment>()
                .HasOne(ud => ud.User)
                .WithMany(u => u.UserDepartments)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDepartment>()
                .HasOne(ud => ud.Department)
                .WithMany(d => d.UserDepartments)
                .HasForeignKey(ud => ud.DeptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure User self-referencing relationship for TeamLeader-Employee
            modelBuilder.Entity<User>()
                .HasOne(u => u.TeamLeader)
                .WithMany(u => u.ManagedEmployees)
                .HasForeignKey(u => u.TeamLeaderId)
                .OnDelete(DeleteBehavior.SetNull); // Set to NULL when team leader is deleted

            // Configure Client-AccountManager relationship
            modelBuilder.Entity<Client>()
                .HasOne(c => c.AccountManager)
                .WithMany(u => u.ManagedClients)
                .HasForeignKey(c => c.AccountManagerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of account manager if they have clients

            // Configure Project relationships
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure AuditLog relationships
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure WorkTask relationships
            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Department)
                .WithMany(d => d.Tasks)
                .HasForeignKey(t => t.DeptId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Priority)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Status)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TaskComment relationships
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.TaskComments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.User)
                .WithMany(u => u.TaskComments)
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TaskDependency relationships
            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.Task)
                .WithMany(t => t.Dependencies)
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskDependency>()
                .HasOne(td => td.PrerequisiteTask)
                .WithMany(t => t.PrerequisiteFor)
                .HasForeignKey(td => td.PrerequisiteTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent duplicate and self dependencies
            modelBuilder.Entity<TaskDependency>()
                .HasIndex(td => new { td.TaskId, td.PrerequisiteTaskId }).IsUnique();
            modelBuilder.Entity<TaskDependency>()
                .HasCheckConstraint("CK_TaskDependency_NoSelf", "[prerequisite_task_id] <> [task_id]");

            // Configure TimeLog relationships
            modelBuilder.Entity<TimeLog>()
                .HasOne(tl => tl.Task)
                .WithMany(t => t.TimeLogs)
                .HasForeignKey(tl => tl.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TimeLog>()
                .HasOne(tl => tl.User)
                .WithMany(u => u.TimeLogs)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TaskCategory relationships
            modelBuilder.Entity<WorkTask>()
                .HasOne(t => t.Category)
                .WithMany(tc => tc.Tasks)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure RecurringTask relationships
            modelBuilder.Entity<RecurringTask>()
                .HasOne(rt => rt.TemplateTask)
                .WithMany()
                .HasForeignKey(rt => rt.TemplateTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProjectMilestone relationships
            modelBuilder.Entity<ProjectMilestone>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure UserSettings relationships
            modelBuilder.Entity<UserSettings>()
                .HasOne(us => us.User)
                .WithOne(u => u.Settings)
                .HasForeignKey<UserSettings>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Attachment relationships
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.UploadedByUser)
                .WithMany(u => u.UploadedAttachments)
                .HasForeignKey(a => a.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Notification relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Task)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Project)
                .WithMany(p => p.Notifications)
                .HasForeignKey(n => n.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes & constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[email] IS NOT NULL"); // allow multiple NULLs
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<UserDepartment>()
                .HasIndex(ud => new { ud.UserId, ud.DeptId })
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.DeptName)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName)
                .IsUnique();

            modelBuilder.Entity<Client>()
                .HasIndex(c => c.Email)
                .IsUnique();

            // Configure Calendar relationships
            modelBuilder.Entity<CalendarEvent>()
                .HasOne(ce => ce.Task)
                .WithMany()
                .HasForeignKey(ce => ce.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CalendarEvent>()
                .HasOne(ce => ce.Project)
                .WithMany()
                .HasForeignKey(ce => ce.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CalendarEvent>()
                .HasOne(ce => ce.User)
                .WithMany()
                .HasForeignKey(ce => ce.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CalendarEvent>()
                .HasOne(ce => ce.Department)
                .WithMany()
                .HasForeignKey(ce => ce.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CalendarEvent>()
                .HasOne(ce => ce.CreatedByUser)
                .WithMany()
                .HasForeignKey(ce => ce.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalendarEventAttendee>()
                .HasOne(cea => cea.CalendarEvent)
                .WithMany(ce => ce.Attendees)
                .HasForeignKey(cea => cea.CalendarEventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CalendarEventAttendee>()
                .HasOne(cea => cea.User)
                .WithMany()
                .HasForeignKey(cea => cea.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CalendarReminder>()
                .HasOne(cr => cr.CalendarEvent)
                .WithMany(ce => ce.Reminders)
                .HasForeignKey(cr => cr.CalendarEventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CalendarReminder>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CalendarEvent>()
                .HasIndex(ce => ce.StartDate);
            modelBuilder.Entity<CalendarEvent>()
                .HasIndex(ce => ce.EndDate);
            modelBuilder.Entity<CalendarEvent>()
                .HasIndex(ce => ce.EventType);
        }
    }
}