# Task Workflow Implementation - Changes Summary

## Backend Changes

### 1. Models/Task.cs

**Added Fields**:

- `OriginalAssignerId` (int?) - Tracks who originally assigned the task
- `DelegatedBy` (int?) - Tracks who last passed/delegated the task
- Navigation properties: `OriginalAssigner`, `Delegator`

### 2. DTOs/TaskDtos.cs

**Updated TaskDto**:

- Added `OriginalAssignerId`, `OriginalAssignerName`
- Added `DelegatedBy`, `DelegatedByName`

**New DTO**:

- `PassTaskDto` - For passing tasks to other users
  - `AssignToUserId` (int, required)
  - `Notes` (string, optional)

### 3. Services/TaskService.cs

**Updated Methods**:

- `GetTasksAsync()`: Enhanced filtering for Account Manager and Team Leader roles
- `GetTaskByIdAsync()`: Include new delegation fields in query
- `CreateTaskAsync()`: Set `OriginalAssignerId` when creating tasks
- `RequestTaskCompletionAsync()`: Notify both delegator and original assigner

**New Method**:

- `PassTaskAsync()`: Handle task passing with role-based validation
  - Validates current user can pass tasks
  - Enforces role hierarchy (Account Manager → Team Leader/Employee, Team Leader → Employee)
  - Sets delegation tracking fields
  - Sends notifications
  - Creates audit log

### 4. Controllers/TasksController.cs

**Updated Endpoints**:

- `POST /api/tasks`: Added AccountManager to allowed roles
- `PUT /api/tasks/{id}`: Added AccountManager to allowed roles
- `PUT /api/tasks/{id}/request-complete`: Added AccountManager and TeamLeader to allowed roles
- `PUT /api/tasks/{id}/review-completion`: Updated authorization to check delegator or original assigner

**New Endpoint**:

- `PUT /api/tasks/{id}/pass`: Pass task to another user (AccountManager, TeamLeader only)

### 5. Migrations/

**New Migration**: `20251119120000_AddTaskDelegationFields.cs`

- Adds `original_assigner_id` column to TASK table
- Adds `delegated_by` column to TASK table
- Creates foreign key constraints
- Creates indexes for performance

## Frontend Changes

### 1. scripts/utils/api.js

**New Method**:

- `API.Tasks.passTask(id, passData)`: Call the pass task endpoint

### 2. scripts/pages/accountant/my-tasks.js

**Updated**:

- `handleDelegateSubmit()`: Changed to use new `API.Tasks.passTask()` endpoint
- `completeTask()`: Changed to use `API.Tasks.requestComplete()` for proper workflow
- Removed workaround that used `updateStatus`

### 3. scripts/pages/team-leader/my-tasks.js

**Complete Rewrite**:

- Added employee loading and filtering
- Added `showPassTaskModal()`: Display modal to pass task to employee
- Added `handlePassTask()`: Pass task using API
- Added `requestCompletion()`: Request task review
- Enhanced `renderTasks()`: Show pass and complete buttons based on task state
- Added support for reviewing tasks that need approval

### 4. pages/team-leader/my-tasks.html

**New Modal**:

- Added "Pass Task Modal" with employee selection dropdown
- Includes notes/instructions field

## Role-Based Workflow

### Manager & Assistant Manager

- Create and assign tasks to any lower role
- Review all task completions
- See all tasks in system
- Cannot assign to each other or to higher roles

### Account Manager

- Receives tasks from Manager/Assistant Manager
- **Option 1**: Do the task themselves → Request completion
- **Option 2**: Pass to Team Leader → Track delegation
- **Option 3**: Pass directly to Employee → Track delegation
- Can review tasks they passed to Team Leaders
- See: Tasks assigned to them, created by them, or delegated by them

### Team Leader

- Receives tasks from Manager/Assistant Manager/Account Manager
- **Option 1**: Do the task themselves → Request completion
- **Option 2**: Pass to Employee → Track delegation
- Can review tasks assigned to their team
- See: Tasks assigned to them, created by them, delegated by them, or in their department

### Employee

- Receives tasks from any higher role
- Can only complete tasks (no delegation)
- Request completion → Sends to reviewer
- See: Only tasks assigned to them

## Task States and Workflow

1. **Created** → Task assigned to user (StatusId = 1)
2. **In Progress** → User working on task (StatusId = 2)
3. **In Review** → User requested completion (StatusId = 3)
4. **Done** → Reviewer approved task (StatusId = 4)
5. **Back to In Progress** → Reviewer declined with notes

## Notification Flow

| Event              | Recipient                     | Message                                                |
| ------------------ | ----------------------------- | ------------------------------------------------------ |
| Task Created       | Assignee                      | "You have been assigned a new task: {Title}"           |
| Task Passed        | New Assignee                  | "Task '{Title}' has been passed to you by {Delegator}" |
| Request Completion | Delegator + Original Assigner | "Task '{Title}' marked as ready for review"            |
| Approved           | Assignee                      | "Task '{Title}' has been approved and marked as done"  |
| Declined           | Assignee                      | "Task '{Title}' review not approved. Notes: {Notes}"   |

## Database Migration Required

Before running the application, execute the migration:

```bash
cd backend
dotnet ef database update
```

Or manually run the migration SQL if needed.

## Testing Checklist

- [ ] Manager creates task and assigns to Account Manager
- [ ] Account Manager receives notification
- [ ] Account Manager passes task to Team Leader
- [ ] Team Leader receives notification
- [ ] Team Leader passes task to Employee
- [ ] Employee receives notification
- [ ] Employee completes task and requests review
- [ ] Team Leader receives review notification
- [ ] Team Leader approves/declines task
- [ ] Employee receives approval/decline notification
- [ ] Account Manager completes task themselves (no passing)
- [ ] Account Manager requests review
- [ ] Manager reviews and approves
- [ ] All role permissions enforced correctly

## Files Modified

### Backend

1. `backend/Models/Task.cs`
2. `backend/DTOs/TaskDtos.cs`
3. `backend/Services/TaskService.cs`
4. `backend/Controllers/TasksController.cs`
5. `backend/Migrations/20251119120000_AddTaskDelegationFields.cs` (new)

### Frontend

1. `barq-dashboard/frontend/scripts/utils/api.js`
2. `barq-dashboard/frontend/scripts/pages/accountant/my-tasks.js`
3. `barq-dashboard/frontend/scripts/pages/team-leader/my-tasks.js`
4. `barq-dashboard/frontend/pages/team-leader/my-tasks.html`

### Documentation

1. `TASK_DELEGATION_WORKFLOW.md` (new)
2. `IMPLEMENTATION_CHANGES.md` (this file, new)

## Notes

1. The `Client` role (Role ID: 6) is not part of the task workflow and maintains its existing behavior.

2. All task passing operations are logged in the audit log for tracking.

3. The original task creator remains the same even after multiple delegations.

4. Review authorization checks both the delegator and original assigner to allow either to approve/decline.

5. Task visibility is role-based and enforced at the service layer.
