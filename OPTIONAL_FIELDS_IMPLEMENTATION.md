# Optional Fields Implementation

## Overview
Made ProjectId, ClientId, and AccountManagerId optional throughout the system to allow flexible entity creation and relationship assignment.

## Changes Summary

### 1. Database Schema Changes (Migration: MakeFieldsOptional)
- **Task.ProjectId**: Changed from `int` (required) to `int?` (optional)
- **Project.ClientId**: Changed from `int` (required) to `int?` (optional)
- **Client.AccountManagerId**: Changed from `int` (required) to `int?` (optional)

### 2. Cascade Behavior Changes
Updated foreign key cascade behaviors to prevent orphaned records:
- **Task → Project**: `OnDelete(DeleteBehavior.SetNull)` - When a project is deleted, task.ProjectId is set to NULL
- **Project → Client**: `OnDelete(DeleteBehavior.SetNull)` - When a client is deleted, project.ClientId is set to NULL
- **Client → AccountManager**: `OnDelete(DeleteBehavior.SetNull)` - When an account manager is deleted, client.AccountManagerId is set to NULL

### 3. Model Updates
#### Task.cs (WorkTask)
```csharp
public int? ProjectId { get; set; }
public virtual Project? Project { get; set; }
```

#### Project.cs
```csharp
public int? ClientId { get; set; }
public virtual Client? Client { get; set; }
```

#### Client.cs
```csharp
public int? AccountManagerId { get; set; }
public virtual User? AccountManager { get; set; }
```

### 4. DTO Updates
#### CreateTaskDto & UpdateTaskDto
- `ProjectId` is now `int?` (nullable)
- No `[Required]` attribute

#### CreateProjectDto & UpdateProjectDto
- `ClientId` is now `int?` (nullable)
- No `[Required]` attribute

#### CreateClientDto & UpdateClientDto
- `AccountManagerId` is now `int?` (nullable)
- No `[Required]` attribute

#### Response DTOs
- `TaskListDto.ProjectName` changed to `string?` (nullable)
- `TaskDto.ProjectName` changed to `string?` (nullable)
- `ProjectDto.ClientName` changed to `string?` (nullable)

### 5. Service Layer Changes
#### TaskService.cs
- `CreateTaskAsync`: Only validates ProjectId if provided (`if (dto.ProjectId.HasValue)`)
- `UpdateTaskAsync`: Only validates ProjectId if provided
- `GetTasksAsync`: Added null checks for ProjectId in filtering logic
  ```csharp
  if (filters.ProjectId.HasValue) {
      query = query.Where(t => t.ProjectId == filters.ProjectId.Value);
  }
  ```

#### ProjectService.cs
- `CreateProjectAsync`: Only validates ClientId if provided
- `UpdateProjectAsync`: Only validates ClientId if provided

#### ClientService.cs
- `CreateClientAsync`: Only validates AccountManagerId if provided
  ```csharp
  if (dto.AccountManagerId.HasValue) {
      var accountManager = await _context.Users.FirstOrDefaultAsync(
          u => u.UserId == dto.AccountManagerId.Value && 
          u.Role == UserRole.AccountManager
      );
  }
  ```
- `UpdateClientAsync`: Only validates AccountManagerId if provided

### 6. Controller Updates
#### TasksController.cs
- Fixed nullable ProjectId handling in role-based access checks
- Updated `IsClientProject` call to check `HasValue` first

#### ProjectsController.cs
- Updated CreateProject to handle optional ClientId
- Conditional client name lookup only if ClientId provided

#### StatisticsController.cs
- Fixed three instances of nullable ProjectId filtering
- Added `.HasValue` checks and `.Value` accessors in Contains operations

### 7. Database Context (BarqTMSDbContext.cs)
Updated relationship configurations:
```csharp
// Task → Project relationship
modelBuilder.Entity<WorkTask>()
    .HasOne(t => t.Project)
    .WithMany(p => p.Tasks)
    .HasForeignKey(t => t.ProjectId)
    .OnDelete(DeleteBehavior.SetNull);

// Project → Client relationship
modelBuilder.Entity<Project>()
    .HasOne(p => p.Client)
    .WithMany(c => c.Projects)
    .HasForeignKey(p => p.ClientId)
    .OnDelete(DeleteBehavior.SetNull);

// Client → AccountManager relationship
modelBuilder.Entity<Client>()
    .HasOne(c => c.AccountManager)
    .WithMany()
    .HasForeignKey(c => c.AccountManagerId)
    .OnDelete(DeleteBehavior.SetNull);
```

## Usage Examples

### Create Task Without Project
```json
POST /api/tasks
{
    "title": "Research Market Trends",
    "description": "Analyze competitor data",
    "dueDate": "2025-12-01",
    "priorityId": 1,
    "statusId": 1,
    "deptId": 2,
    "assignedTo": 5
    // ProjectId is omitted - task created without project
}
```

### Add Project Later
```json
PUT /api/tasks/123
{
    "title": "Research Market Trends",
    "description": "Analyze competitor data",
    "dueDate": "2025-12-01",
    "priorityId": 1,
    "statusId": 1,
    "deptId": 2,
    "assignedTo": 5,
    "projectId": 10  // Now assigned to project
}
```

### Create Project Without Client
```json
POST /api/projects
{
    "projectName": "Internal Website Redesign",
    "description": "Modernize company website",
    "startDate": "2025-01-15"
    // ClientId is omitted - internal project
}
```

### Create Client Without Account Manager
```json
POST /api/clients
{
    "name": "Acme Corporation",
    "email": "contact@acme.com",
    "phoneNumber": "+1234567890",
    "company": "Acme Corp"
    // AccountManagerId is omitted - can be assigned later
}
```

## Benefits
1. **Flexible Workflow**: Create entities first, assign relationships later
2. **Internal Projects**: Projects without clients (internal initiatives)
3. **Unassigned Clients**: Clients can exist before account manager assignment
4. **Orphan Prevention**: SetNull cascade behavior prevents accidental deletions
5. **Gradual Data Entry**: Users can add information as it becomes available

## Migration Applied
✅ Migration `20251119071910_MakeFieldsOptional` successfully applied
✅ Database schema updated
✅ All compilation errors resolved
✅ Build successful

## Next Steps
- [ ] Update frontend forms to make project/client/account manager dropdowns optional
- [ ] Add UI indicators for entities without relationships
- [ ] Update API documentation to reflect optional fields
- [ ] Test all creation and update scenarios
- [ ] Test cascade delete behaviors
