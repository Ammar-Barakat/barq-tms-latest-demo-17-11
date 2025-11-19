# Task Workflow Visual Guide

## Role Hierarchy and Task Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         TASK WORKFLOW                            │
└─────────────────────────────────────────────────────────────────┘

                          ┌─────────────┐
                          │   MANAGER   │
                          │  (Role: 1)  │
                          └──────┬──────┘
                                 │ Can assign to ↓
                                 ↓
                    ┌────────────┴────────────┐
                    │                         │
          ┌─────────▼────────┐    ┌──────────▼──────────┐
          │ ASSISTANT MANAGER│    │  ACCOUNT MANAGER   │
          │    (Role: 2)     │    │     (Role: 3)      │
          └─────────┬────────┘    └──────────┬──────────┘
                    │                        │
                    │ Can assign to ↓        │ Can PASS to ↓
                    ↓                        ↓
          ┌─────────────────────┬────────────────────────┐
          │                     │                        │
    ┌─────▼──────┐      ┌──────▼───────┐      ┌────────▼────────┐
    │TEAM LEADER │      │ TEAM LEADER  │      │    EMPLOYEE     │
    │ (Role: 4)  │      │  (Role: 4)   │      │   (Role: 5)     │
    └─────┬──────┘      └──────┬───────┘      └─────────────────┘
          │                    │
          │ Can PASS to ↓      │ Can PASS to ↓
          ↓                    ↓
    ┌──────────────────────────────┐
    │         EMPLOYEE             │
    │        (Role: 5)             │
    └──────────────────────────────┘
```

## Task States Flow

```
┌────────────┐
│  CREATED   │  Task is created and assigned
│ (Status 1) │
└─────┬──────┘
      │
      ↓
┌────────────┐
│IN PROGRESS │  User is working on it
│ (Status 2) │  OR
└─────┬──────┘  User passes it to someone else
      │
      ├─────────────┐ PASS TASK
      │             ↓
      │       ┌─────────────┐
      │       │  REASSIGNED │
      │       └──────┬──────┘
      │              ↓
      │         Back to IN PROGRESS
      │              for new assignee
      │
      ↓ COMPLETE
┌────────────┐
│ IN REVIEW  │  User requests completion
│ (Status 3) │  Waiting for approval
└─────┬──────┘
      │
      ├──────────┐
      │          │
      ↓          ↓ DECLINE
┌──────────┐  ┌────────────┐
│   DONE   │  │IN PROGRESS │  Task sent back
│(Status 4)│  │ (Status 2) │  with notes
└──────────┘  └────────────┘
 (Approved)     (Try again)
```

## Detailed Workflow Example

### Scenario: Manager → Account Manager → Team Leader → Employee

```
┌─────────────────────────────────────────────────────────────────┐
│                     STEP 1: CREATION                            │
└─────────────────────────────────────────────────────────────────┘

  Manager Creates Task
       │
       ├─ Title: "Design new logo"
       ├─ AssignedTo: Account Manager (Sarah)
       ├─ OriginalAssignerId: Manager ID
       └─ Status: Created

  → Notification sent to Sarah


┌─────────────────────────────────────────────────────────────────┐
│                  STEP 2: FIRST DELEGATION                       │
└─────────────────────────────────────────────────────────────────┘

  Account Manager (Sarah) receives task
       │
       ├─ Decision: I'll pass this to Team Leader
       │
       └─ Clicks "Delegate" → Selects Team Leader (John)

  Task Updated:
       ├─ AssignedTo: Team Leader (John)
       ├─ DelegatedBy: Account Manager (Sarah)
       ├─ OriginalAssignerId: Manager ID (unchanged)
       └─ Status: In Progress

  → Notification sent to John


┌─────────────────────────────────────────────────────────────────┐
│                 STEP 3: SECOND DELEGATION                       │
└─────────────────────────────────────────────────────────────────┘

  Team Leader (John) receives task
       │
       ├─ Decision: I'll pass this to an Employee
       │
       └─ Clicks "Pass" → Selects Employee (Mike)

  Task Updated:
       ├─ AssignedTo: Employee (Mike)
       ├─ DelegatedBy: Team Leader (John)
       ├─ OriginalAssignerId: Manager ID (unchanged)
       └─ Status: In Progress

  → Notification sent to Mike


┌─────────────────────────────────────────────────────────────────┐
│                    STEP 4: COMPLETION                           │
└─────────────────────────────────────────────────────────────────┘

  Employee (Mike) works on task
       │
       ├─ Completes the logo design
       │
       └─ Clicks "Mark as Done"

  Task Updated:
       ├─ Status: In Review

  → Notifications sent to:
       ├─ Team Leader (John) - because he passed it
       └─ Manager - because he created it


┌─────────────────────────────────────────────────────────────────┐
│                      STEP 5: REVIEW                             │
└─────────────────────────────────────────────────────────────────┘

  Team Leader (John) reviews task
       │
       ├─ Option A: APPROVE
       │    └─ Task Status: Done
       │       AssignedTo: NULL (cleared)
       │       → Notification to Mike: "Approved!"
       │
       └─ Option B: DECLINE
            └─ Task Status: In Progress
               Add notes: "Please use blue colors"
               New Due Date: Extended 2 days
               → Notification to Mike: "Needs revision"

  If Declined:
       Mike sees notes, makes changes,
       requests review again → Back to Step 4
```

## Permissions Matrix

```
┌──────────────────┬─────────┬──────────┬─────────┬──────────┬─────────┐
│     Action       │ Manager │ Asst Mgr │ Acct Mgr│Team Lead │Employee │
├──────────────────┼─────────┼──────────┼─────────┼──────────┼─────────┤
│ Create Task      │   ✓     │    ✓     │    ✓    │    ✓     │    ✗    │
├──────────────────┼─────────┼──────────┼─────────┼──────────┼─────────┤
│ Assign to        │         │          │         │          │         │
│  - Manager       │   ✗     │    ✗     │    ✗    │    ✗     │    ✗    │
│  - Asst Manager  │   ✓     │    ✗     │    ✗    │    ✗     │    ✗    │
│  - Acct Manager  │   ✓     │    ✓     │    ✗    │    ✗     │    ✗    │
│  - Team Leader   │   ✓     │    ✓     │    ✓    │    ✗     │    ✗    │
│  - Employee      │   ✓     │    ✓     │    ✓    │    ✓     │    ✗    │
├──────────────────┼─────────┼──────────┼─────────┼──────────┼─────────┤
│ Pass Task to     │         │          │         │          │         │
│  - Team Leader   │   N/A   │   N/A    │    ✓    │    ✗     │    ✗    │
│  - Employee      │   N/A   │   N/A    │    ✓    │    ✓     │    ✗    │
├──────────────────┼─────────┼──────────┼─────────┼──────────┼─────────┤
│ Do Task Self     │   ✓     │    ✓     │    ✓    │    ✓     │    ✓    │
├──────────────────┼─────────┼──────────┼─────────┼──────────┼─────────┤
│ Request Review   │   ✓     │    ✓     │    ✓    │    ✓     │    ✓    │
├──────────────────┼─────────┼──────────┼─────────┼──────────┼─────────┤
│ Approve/Decline  │   ✓     │    ✓     │    ✓*   │    ✓*    │    ✗    │
│                  │ (all)   │  (all)   │(passed) │(passed)  │         │
├──────────────────┼─────────┼──────────┼─────────┼──────────┼─────────┤
│ View Tasks       │   ALL   │   ALL    │  MINE   │  MINE+   │  MINE   │
│                  │         │          │         │  DEPT    │         │
└──────────────────┴─────────┴──────────┴─────────┴──────────┴─────────┘

* Can only approve/decline tasks they passed or created
```

## Notification Recipients

```
┌────────────────────────┬──────────────────────────────────┐
│        Event           │         Notification Sent To     │
├────────────────────────┼──────────────────────────────────┤
│ Task Created           │ → Assignee                       │
├────────────────────────┼──────────────────────────────────┤
│ Task Passed            │ → New Assignee                   │
├────────────────────────┼──────────────────────────────────┤
│ Request Completion     │ → Delegator (who passed it)      │
│                        │ → Original Assigner (if diff)    │
├────────────────────────┼──────────────────────────────────┤
│ Approved               │ → Employee (who completed it)    │
├────────────────────────┼──────────────────────────────────┤
│ Declined               │ → Employee (who completed it)    │
└────────────────────────┴──────────────────────────────────┘
```

## Database Fields

```
TASK Table
┌─────────────────────┬──────────┬─────────────────────────────┐
│      Field          │   Type   │         Description         │
├─────────────────────┼──────────┼─────────────────────────────┤
│ task_id             │   INT    │ Primary Key                 │
│ title               │  STRING  │ Task title                  │
│ created_by          │   INT    │ User who created task       │
│ assigned_to         │   INT    │ Current assignee            │
│ original_assigner_id│   INT    │ Initial creator/assigner    │
│ delegated_by        │   INT    │ Last person who passed it   │
│ status_id           │   INT    │ 1=Created,2=Progress,3=     │
│                     │          │ Review,4=Done               │
│ ...other fields...  │          │                             │
└─────────────────────┴──────────┴─────────────────────────────┘

Key Points:
- original_assigner_id: Never changes, always the initial creator
- delegated_by: Updates each time task is passed
- assigned_to: Changes to new person when passed
```

## API Endpoints Summary

```
POST   /api/tasks
  → Create new task
  → Auth: Manager, Asst Manager, Acct Manager, Team Leader

PUT    /api/tasks/{id}
  → Update task details
  → Auth: Manager, Asst Manager, Acct Manager, Team Leader

PUT    /api/tasks/{id}/pass
  → Pass task to another user
  → Auth: Account Manager, Team Leader
  → Body: { assignToUserId, notes }

PUT    /api/tasks/{id}/request-complete
  → Request review when task is done
  → Auth: Employee, Account Manager, Team Leader

PUT    /api/tasks/{id}/review-completion
  → Approve or decline completed task
  → Auth: Manager, Asst Manager, Account Manager, Team Leader
  → Body: { approve, notes, newDueDate }
```
