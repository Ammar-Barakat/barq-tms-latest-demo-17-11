# Task Workflow Documentation

## Overview
This document describes the complete task lifecycle and approval workflow in the BarQ TMS system.

## Task Statuses
The system uses the following status progression:
1. **To Do** - Task created but not started
2. **In Progress** - Task being worked on
3. **In Review** - Task completed by employee, awaiting approval
4. **Done** - Task approved and completed
5. **Cancelled** - Task cancelled

## Workflow Steps

### 1. Task Creation (Manager/Assistant Manager/Team Leader)
- **Who**: Manager, Assistant Manager, or Team Leader
- **Action**: Creates a task and assigns it to an Employee
- **Status**: "To Do" or "In Progress"
- **Notification**: Employee receives notification about new task assignment

### 2. Task Execution (Employee)
- **Who**: Assigned Employee
- **Action**: Works on the task
- **Status**: Updates to "In Progress" (if not already)
- **Notes**: Employee can add comments and attachments

### 3. Request Completion (Employee)
- **Who**: Assigned Employee
- **Endpoint**: `PUT /api/tasks/{id}/request-complete`
- **Action**: Marks task as ready for review
- **Status**: Changes from "In Progress" → "In Review"
- **Notifications**: 
  - Task creator receives notification
  - Employee's Team Leader receives notification (if different from creator)

### 4. Review Task (Manager/Team Leader)
- **Who**: Task creator OR Team Leader of assigned employee
- **Endpoint**: `PUT /api/tasks/{id}/review-completion`
- **Action**: Reviews the completed task

#### 4a. Approve Task
- **Request Body**:
  ```json
  {
    "Approve": true,
    "Notes": "Great work!" (optional)
  }
  ```
- **Result**:
  - Status: "In Review" → "Done"
  - Task unassigned from employee (AssignedTo = null)
  - Employee receives approval notification
  - Task history updated: "Task approved and completed"

#### 4b. Reject Task
- **Request Body**:
  ```json
  {
    "Approve": false,
    "Notes": "Please fix the formatting issues and retest",
    "NewDueDate": "2025-11-25" (optional)
  }
  ```
- **Result**:
  - Status: "In Review" → "In Progress"
  - Notes added as task comment
  - Due date updated (if provided)
  - Employee receives notification with rejection notes
  - Task history updated: "Task review not approved"
  - Employee can rework and request completion again

## Authorization Rules

### Task Creation
- **Allowed**: Manager, Assistant Manager, Team Leader
- **Restrictions**: Cannot assign to other Managers or Assistant Managers

### Task Update
- **Allowed**: Manager, Assistant Manager, Team Leader
- **Restrictions**: Only task creator or authorized personnel

### Request Completion
- **Allowed**: Employee (only assigned employee)
- **Restrictions**: Must be assigned to the task

### Review Completion
- **Allowed**: 
  - Task creator (Manager, Assistant Manager, Team Leader)
  - Team Leader of assigned employee
- **Restrictions**: Must be creator or employee's team leader

### Task Deletion
- **Allowed**: Manager only
- **Restrictions**: Full delete permission

## Notifications

### Task Assignment
- **Recipient**: Assigned employee
- **Message**: "You have been assigned a new task: {TaskTitle}"

### Task Ready for Review
- **Recipients**: 
  - Task creator
  - Employee's team leader (if different)
- **Message**: 
  - Creator: "Task '{TaskTitle}' marked as ready for review."
  - Team Leader: "Task '{TaskTitle}' assigned to your team member is ready for review."

### Task Approved
- **Recipient**: Employee who completed the task
- **Message**: "Task '{TaskTitle}' has been approved and marked as done."

### Task Rejected
- **Recipient**: Employee who completed the task
- **Message**: "Task '{TaskTitle}' review not approved. Notes: {RejectionNotes}"
- **Additional**: Notes are added as a comment on the task

## Task History
All actions are logged in the audit system:
- Task creation
- Status changes
- Assignment changes
- Completion requests
- Approvals/Rejections
- Comments and attachments

## API Endpoints Summary

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `/api/tasks` | GET | All | Get tasks (filtered by role) |
| `/api/tasks/{id}` | GET | All | Get task details |
| `/api/tasks` | POST | Manager, AssistantManager, TeamLeader | Create task |
| `/api/tasks/{id}` | PUT | Manager, AssistantManager, TeamLeader | Update task |
| `/api/tasks/{id}` | DELETE | Manager | Delete task |
| `/api/tasks/{id}/request-complete` | PUT | Employee | Request task completion |
| `/api/tasks/{id}/review-completion` | PUT | Manager, AssistantManager, TeamLeader | Approve/Reject task |
| `/api/tasks/{id}/comments` | GET/POST | All | Get/Add comments |
| `/api/tasks/{id}/attachments` | GET/POST | All | Get/Add attachments |
| `/api/tasks/{id}/history` | GET | All | Get task history |

## Best Practices

1. **Clear Notes**: Always provide clear rejection notes explaining what needs to be fixed
2. **Due Date Adjustment**: Set realistic new due dates when rejecting tasks
3. **Team Leader Involvement**: Team Leaders should actively monitor their team's tasks
4. **Communication**: Use task comments for ongoing discussion
5. **Documentation**: Attach relevant files and documents to tasks

## Example Workflow

```
1. Manager creates task "Update User Interface" → Assigns to Employee John
   Status: To Do
   Notification: John

2. John starts working
   Status: In Progress

3. John finishes work and requests completion
   Status: In Review
   Notification: Manager + John's Team Leader

4a. Team Leader reviews and rejects
    Status: In Progress
    Notes: "Please add responsive design for mobile"
    Notification: John (with notes)

5. John fixes issues and requests completion again
   Status: In Review
   Notification: Manager + John's Team Leader

6b. Manager reviews and approves
    Status: Done
    AssignedTo: null
    Notification: John (approval)
```

## Technical Implementation

### Key Models
- `WorkTask`: Main task entity
- `Status`: Task status (To Do, In Progress, In Review, Done, Cancelled)
- `TaskComment`: Comments/notes on tasks
- `Notification`: Real-time notifications via SignalR
- `AuditLog`: Task history tracking

### Key Services
- `TaskService`: Business logic for task operations
- `RealTimeService`: SignalR notifications
- `AuthService`: Authentication and authorization

### Database Schema
- Tasks linked to: Priority, Status, Creator, AssignedUser, Department, Project
- User hierarchy: Employee → TeamLeader relationship
- Audit trail for all task actions
