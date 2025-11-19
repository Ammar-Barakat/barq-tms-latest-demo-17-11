# Quick Start Guide - Task Workflow Implementation

## Summary

I've implemented a complete role-based task workflow system that allows:

- **Managers & Assistant Managers** to assign tasks to any lower role
- **Account Managers** to pass tasks to Team Leaders or Employees (or do it themselves)
- **Team Leaders** to pass tasks to Employees (or do it themselves)
- **Employees** to only complete tasks (no delegation)
- Proper review workflow where tasks go back to the person who assigned/delegated them

## Steps to Run

### 1. Apply Database Migration

```bash
cd backend
dotnet ef database update
```

This will add two new columns to the TASK table:

- `original_assigner_id` - Tracks who originally created/assigned the task
- `delegated_by` - Tracks who last passed the task

### 2. Build and Run Backend

```bash
cd backend
dotnet build
dotnet run
```

The backend should start on `https://localhost:7XXX` (check `Properties/launchSettings.json`)

### 3. Open Frontend

Open `barq-dashboard/index.html` or navigate to the appropriate role dashboard.

## How the Workflow Works

### Scenario 1: Manager → Account Manager → Team Leader → Employee

1. **Manager** creates a task and assigns it to **Account Manager**
2. **Account Manager** receives notification and has 2 options:
   - Do the task themselves (click "Complete" → Request Review)
   - Pass to Team Leader or Employee (click "Delegate")
3. If passed to **Team Leader**, they receive notification and have 2 options:
   - Do the task themselves (click "Complete" → Request Review)
   - Pass to Employee (click "Pass")
4. **Employee** receives task, completes it, and clicks "Mark as Done"
5. Task goes "In Review" - notification sent to whoever passed it
6. Reviewer can Approve (task done) or Decline (send back with notes)

### Scenario 2: Account Manager Does Task Themselves

1. **Manager** assigns task to **Account Manager**
2. **Account Manager** works on it and clicks "Complete"
3. Task goes to **Manager** for review
4. **Manager** approves or declines with feedback

## Key Features Implemented

### Backend (C# / .NET)

✅ Added `OriginalAssignerId` and `DelegatedBy` fields to Task model  
✅ Created `PassTaskAsync` method with role-based validation  
✅ Updated `RequestTaskCompletionAsync` to notify correct reviewer  
✅ New API endpoint: `PUT /api/tasks/{id}/pass`  
✅ Updated authorization on all task endpoints  
✅ Created database migration

### Frontend (JavaScript)

✅ Account Manager can pass tasks to Team Leaders/Employees  
✅ Team Leader can pass tasks to Employees  
✅ Both can request completion for tasks they do themselves  
✅ Updated API client with `passTask()` method  
✅ Added pass task modal to Team Leader page  
✅ Updated Account Manager delegation to use new API

### Notifications

✅ Task assignment notifications  
✅ Task passed notifications  
✅ Request completion notifications (to delegator + original assigner)  
✅ Approval/decline notifications

## Testing the Workflow

### Test Users Needed

- 1 Manager
- 1 Account Manager
- 1 Team Leader
- 1 Employee

### Test Flow

1. Login as **Manager**
2. Go to Tasks → Create Task → Assign to Account Manager
3. Logout, Login as **Account Manager**
4. Go to My Tasks → See the task → Click "Delegate"
5. Select Team Leader → Add notes → Submit
6. Logout, Login as **Team Leader**
7. Go to My Tasks → See the task → Click "Pass"
8. Select Employee → Add notes → Submit
9. Logout, Login as **Employee**
10. Go to My Tasks → See the task → Click "Mark as Done"
11. Logout, Login as **Team Leader** (who passed it)
12. Go to My Tasks → See task with "Needs Review" badge → Click Review
13. Approve or Decline with notes

## Important Files Changed

### Backend

- `backend/Models/Task.cs` - Added delegation fields
- `backend/DTOs/TaskDtos.cs` - Added PassTaskDto
- `backend/Services/TaskService.cs` - Added PassTaskAsync method
- `backend/Controllers/TasksController.cs` - Added /pass endpoint
- `backend/Migrations/20251119120000_AddTaskDelegationFields.cs` - New migration

### Frontend

- `frontend/scripts/utils/api.js` - Added passTask() method
- `frontend/scripts/pages/accountant/my-tasks.js` - Use new API
- `frontend/scripts/pages/team-leader/my-tasks.js` - Complete rewrite with pass functionality
- `frontend/pages/team-leader/my-tasks.html` - Added pass task modal

## Troubleshooting

### Migration Issues

If migration fails, you may need to:

1. Check database connection string in `appsettings.json`
2. Manually run the SQL from the migration file
3. Or drop and recreate the database (if in development)

### Role Authorization Issues

Make sure user roles are correctly set in the database:

- Manager = 1
- AssistantManager = 2
- AccountManager = 3
- TeamLeader = 4
- Employee = 5
- Client = 6

### API Errors

Check browser console and backend logs for detailed error messages.

## Next Steps / Future Enhancements

1. Add delegation history view
2. Add ability to reassign/take back tasks
3. Add task filters (by status, priority, delegated, etc.)
4. Add bulk task operations
5. Add task analytics and reports
6. Add SLA tracking for task completion times

## Documentation

- **TASK_DELEGATION_WORKFLOW.md** - Complete workflow guide
- **IMPLEMENTATION_CHANGES.md** - Detailed list of all changes made

## Support

If you encounter any issues:

1. Check the browser console for JavaScript errors
2. Check the backend logs for API errors
3. Verify database migration was applied successfully
4. Ensure user roles are set correctly
5. Test with simple scenarios first before complex workflows
