# Task Workflow Implementation Guide

## Overview

This document describes the complete task workflow implementation for the Barq TMS system, supporting role-based task assignment, delegation, and approval processes.

## Roles and Permissions

### 1. Manager (Role ID: 1)

- **Can create tasks** for any role
- **Can assign tasks** to: Assistant Manager, Account Manager, Team Leader, Employee
- **Cannot assign tasks** to other Managers
- **Can review and approve** all tasks
- **Sees all tasks** in the system

### 2. Assistant Manager (Role ID: 2)

- **Can create tasks** for any role
- **Can assign tasks** to: Account Manager, Team Leader, Employee
- **Cannot assign tasks** to Manager or other Assistant Managers
- **Can review and approve** all tasks
- **Sees all tasks** in the system

### 3. Account Manager (Role ID: 3)

- **Can create tasks** for Team Leaders and Employees
- **Can receive tasks** from Manager or Assistant Manager
- **Can pass tasks** to Team Leader or Employee
- **Can do the task** themselves (request completion review)
- **Can review tasks** passed to Team Leaders
- **Sees**: Tasks assigned to them, tasks they created, tasks they delegated

### 4. Team Leader (Role ID: 4)

- **Can create tasks** for Employees
- **Can receive tasks** from Manager, Assistant Manager, or Account Manager
- **Can pass tasks** to Employees
- **Can do the task** themselves (request completion review)
- **Can review tasks** assigned to their team members
- **Sees**: Tasks assigned to them, tasks they created, tasks they delegated, tasks in their department

### 5. Employee (Role ID: 5)

- **Cannot create tasks**
- **Can receive tasks** from any higher role
- **Cannot pass tasks** to anyone
- **Can only do the task** (request completion review)
- **Sees**: Only tasks assigned to them

## Task Workflow

### Task Creation

1. Manager/Assistant Manager/Account Manager/Team Leader creates a task
2. Task is assigned to a user (can be any lower role)
3. `OriginalAssignerId` is set to the creator
4. Notification is sent to the assigned user

### Task Delegation (Passing)

1. **Account Manager** receives a task from Manager/Assistant Manager
   - Can choose to: Do it themselves OR pass to Team Leader OR pass to Employee
   - If passed: `DelegatedBy` is set to Account Manager ID, `AssignedTo` is updated
2. **Team Leader** receives a task (from Manager/Assistant Manager/Account Manager)

   - Can choose to: Do it themselves OR pass to Employee
   - If passed: `DelegatedBy` is set to Team Leader ID, `AssignedTo` is updated

3. **Employee** receives a task
   - Can only do the task, cannot pass it further

### Task Completion and Review

#### Request Completion

- When a user completes their task, they click "Request Completion" or "Mark as Done"
- Task status changes to "In Review" (StatusId = 3)
- Notification is sent to:
  - The person who passed the task (`DelegatedBy`)
  - The original assigner (`OriginalAssignerId`) if different

#### Review Process

- The reviewer (either the delegator or original assigner) receives notification
- Reviewer can:
  - **Approve**: Task marked as "Done" (StatusId = 4), assignee cleared
  - **Decline**: Task sent back to assignee with notes and optionally extended due date

## Database Schema Changes

### Task Table - New Fields

```sql
original_assigner_id INT NULL -- The person who originally assigned/created the task
delegated_by INT NULL         -- The person who last passed/delegated the task
```

### Migration

File: `Migrations/20251119120000_AddTaskDelegationFields.cs`

Run migration:

```bash
dotnet ef database update
```

## API Endpoints

### Pass Task

**Endpoint**: `PUT /api/tasks/{id}/pass`  
**Auth**: Account Manager, Team Leader  
**Body**:

```json
{
  "assignToUserId": 123,
  "notes": "Please complete this by end of week"
}
```

### Request Completion

**Endpoint**: `PUT /api/tasks/{id}/request-complete`  
**Auth**: Employee, Account Manager, Team Leader  
**Body**: None

### Review Completion

**Endpoint**: `PUT /api/tasks/{id}/review-completion`  
**Auth**: Manager, Assistant Manager, Account Manager, Team Leader  
**Body**:

```json
{
  "approve": true,
  "notes": "Great work!",
  "newDueDate": "2025-11-25T00:00:00Z"
}
```

## Frontend Implementation

### Account Manager - My Tasks

- **File**: `frontend/scripts/pages/accountant/my-tasks.js`
- **Features**:
  - View tasks assigned to them
  - Pass tasks to Team Leaders or Employees
  - Request completion for tasks they do themselves
  - View tasks they delegated

### Team Leader - My Tasks

- **File**: `frontend/scripts/pages/team-leader/my-tasks.js`
- **Features**:
  - View tasks assigned to them
  - Pass tasks to Employees
  - Request completion for tasks they do themselves
  - Review tasks from team members

### Employee - My Tasks

- **File**: `frontend/scripts/pages/employee/my-tasks.js`
- **Features**:
  - View tasks assigned to them
  - Request completion when done
  - See review notes from managers

### Manager/Assistant Manager - Tasks

- **File**: `frontend/scripts/pages/manager/tasks.js`
- **Features**:
  - Create and assign tasks
  - Review task completions
  - Approve or decline with notes

## Notification Flow

### Task Assignment

- Notification sent to assignee: "You have been assigned a new task: {TaskTitle}"

### Task Passed/Delegated

- Notification sent to new assignee: "Task '{TaskTitle}' has been passed to you by {DelegatorName}"

### Request Completion

- Notification sent to reviewer: "Task '{TaskTitle}' marked as ready for review"
- Also sent to original assigner if different

### Approval

- Notification sent to assignee: "Task '{TaskTitle}' has been approved and marked as done"

### Decline

- Notification sent to assignee: "Task '{TaskTitle}' review not approved. Notes: {ReviewNotes}"

## Testing Workflow

### Test Case 1: Manager → Account Manager → Team Leader → Employee

1. Manager creates task, assigns to Account Manager
2. Account Manager receives notification, passes to Team Leader
3. Team Leader receives notification, passes to Employee
4. Employee completes task, requests review
5. Team Leader reviews and approves
6. Task marked as done

### Test Case 2: Manager → Team Leader → Employee (Direct)

1. Manager creates task, assigns to Team Leader
2. Team Leader receives notification, passes to Employee
3. Employee completes task, requests review
4. Team Leader reviews and approves
5. Task marked as done

### Test Case 3: Account Manager Does Task Themselves

1. Manager assigns task to Account Manager
2. Account Manager does the task (doesn't pass it)
3. Account Manager requests completion
4. Manager reviews and approves
5. Task marked as done

## Important Notes

1. **Original Assigner Tracking**: The `OriginalAssignerId` field tracks who initially assigned the task, even after multiple delegations.

2. **Delegation Chain**: The `DelegatedBy` field tracks the most recent person who passed the task, which is who should review it.

3. **Role Restrictions**: The backend enforces role-based restrictions on who can pass tasks to whom.

4. **Notification Chain**: Both the delegator and original assigner receive notifications when a task is completed.

5. **Task Visibility**: Each role sees different sets of tasks based on their permissions and involvement.

## Future Enhancements

1. Add delegation history tracking
2. Add ability to "take back" a delegated task
3. Add bulk task assignment
4. Add task templates
5. Add automated task routing based on workload
