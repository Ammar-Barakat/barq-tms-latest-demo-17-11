# Account Manager API Integration Analysis

## Overview

This document analyzes the API endpoints available for the Account Manager role and provides recommendations for implementing the required features.

---

## Required Features for Account Manager

### 1. Upload Files

### 2. Dashboard: Display Total Clients / Recent Clients

### 3. Review Team Leader's Tasks Before Sending to Client

### 4. Handle Client Feedback and Send Back to Team Leader

### 5. All Actions Scoped to Assigned Projects

---

## API Endpoints Analysis

### ✅ **1. File Upload Feature**

#### Available Endpoint:

```
POST /api/Files/upload/{taskId}
```

**Current Implementation:**

- Location: `frontend/scripts/utils/api.js` (Lines 535-551)
- Method: `API.Files.upload(taskId, formData)`

**Status:** ✅ **FULLY SUPPORTED**

**Usage Example:**

```javascript
const formData = new FormData();
formData.append("file", fileInput.files[0]);
await API.Files.upload(taskId, formData);
```

**Related Endpoints:**

- `GET /api/Files/download/{fileId}` - Download uploaded files
- `DELETE /api/Files/{fileId}` - Delete files
- `GET /api/Tasks/{taskId}/attachments` - List task attachments

---

### ✅ **2. Dashboard: Total Clients / Recent Clients**

#### Available Endpoints:

```
GET /api/Clients
GET /api/Clients/{id}
GET /api/Clients/{id}/projects
```

**Current Implementation:**

- Location: `frontend/scripts/pages/accountant/dashboard.js`
- Methods:
  - `API.Clients.getAll()` - Get all clients
  - `API.Clients.getById(id)` - Get specific client details
  - `API.Clients.getProjects(clientId)` - Get client's projects

**Status:** ✅ **FULLY SUPPORTED**

**Current Dashboard Stats:**

```javascript
// From dashboard.js (Line 42-46)
document.getElementById("totalTasks").textContent = data.tasks.length;
document.getElementById("totalProjects").textContent = data.projects.length;
document.getElementById("totalEmployees").textContent = "0";
document.getElementById("totalClients").textContent = data.clients.length;
```

**Filtering by Account Manager's Assigned Projects:**
To show only clients for projects where the Account Manager is assigned, you need to:

1. Get current user from `auth.getCurrentUser()`
2. Filter projects by `accountManagerId` field
3. Extract unique clients from filtered projects

**Recommended Implementation:**

```javascript
async function loadDashboardData() {
  const currentUser = auth.getCurrentUser();
  const [allProjects, allTasks, allClients] = await Promise.all([
    API.Projects.getAll(),
    API.Tasks.getAll(),
    API.Clients.getAll(),
  ]);

  // Filter projects where current user is Account Manager
  const myProjects = allProjects.filter(
    (p) =>
      p.accountManagerId === currentUser.userId ||
      p.AccountManagerId === currentUser.userId
  );

  // Get unique clients from my projects
  const myClientIds = new Set(myProjects.map((p) => p.ClientId || p.clientId));
  const myClients = allClients.filter((c) =>
    myClientIds.has(c.ClientId || c.clientId)
  );

  // Filter tasks from my projects
  const myProjectIds = new Set(
    myProjects.map((p) => p.ProjectId || p.projectId)
  );
  const myTasks = allTasks.filter((t) =>
    myProjectIds.has(t.ProjectId || t.projectId)
  );

  updateStats({
    projects: myProjects,
    clients: myClients,
    tasks: myTasks,
  });
}
```

---

### ⚠️ **3. Review Team Leader's Tasks (Task Review Workflow)**

#### Available Endpoints:

```
GET /api/Tasks
GET /api/Tasks/{id}
GET /api/Tasks/{id}/comments
POST /api/Tasks/{id}/comments
PUT /api/Tasks/{id}/review-completion
GET /api/Tasks/{id}/history
```

**Current Implementation:**

- Location: `frontend/scripts/pages/accountant/tasks.js`
- Uses: `API.Tasks.reviewCompletion(id, { Approved: false, Comment: notes })`

**Status:** ⚠️ **PARTIALLY SUPPORTED**

**Gap Analysis:**

The API provides `PUT /api/Tasks/{id}/review-completion` but this endpoint is designed for **manager approval of task completion requests**, not for Account Manager review workflow.

**Missing Endpoints Needed:**

```
PUT /api/Tasks/{id}/approve-for-client
PUT /api/Tasks/{id}/send-back-rework
PUT /api/Tasks/{id}/client-approve
PUT /api/Tasks/{id}/client-reject
```

**Current Placeholder Implementation:**

```javascript
// From api.js (Lines 225-243)
async approveForClient(id, notes) {
  const client = new APIClient();
  // Placeholder endpoint
  return client.put(`/Tasks/${id}/approve-for-client`, { notes });
},
async sendBackForRework(id, feedback) {
  const client = new APIClient();
  // Placeholder endpoint
  return client.put(`/Tasks/${id}/send-back-rework`, { feedback });
},
async clientApprove(id, notes) {
  const client = new APIClient();
  // Placeholder endpoint
  return client.put(`/Tasks/${id}/client-approve`, { notes });
},
async clientReject(id, feedback) {
  const client = new APIClient();
  // Placeholder endpoint
  return client.put(`/Tasks/${id}/client-reject`, { feedback });
}
```

**Workaround Solution:**

Use existing endpoints creatively:

1. **Add Custom Fields to Task Model** (Backend Change Required):

   ```csharp
   public bool SubmittedForAccountManagerReview { get; set; }
   public bool AccountManagerApproved { get; set; }
   public bool SentToClient { get; set; }
   public bool? ClientApproved { get; set; }  // null, true, false
   public string ClientFeedback { get; set; }
   public string ReviewWorkflow { get; set; } // JSON workflow state
   ```

2. **Use Existing Update Endpoint**:

   ```javascript
   // Approve and send to client
   await API.Tasks.update(taskId, {
     AccountManagerApproved: true,
     SentToClient: true,
     // ... other fields
   });

   // Add comment for audit trail
   await API.Tasks.addComment(
     taskId,
     "Account Manager approved and sent to client"
   );
   ```

3. **Alternative: Use Task Status and Comments**:

   ```javascript
   // Use existing status system
   const STATUS_IDS = {
     PENDING_AM_REVIEW: 6,
     SENT_TO_CLIENT: 7,
     CLIENT_APPROVED: 8,
     CLIENT_REJECTED: 9,
   };

   await API.Tasks.update(taskId, {
     StatusId: STATUS_IDS.SENT_TO_CLIENT,
   });
   await API.Tasks.addComment(taskId, notes);
   ```

---

### ⚠️ **4. Client Feedback Handling**

#### Available Endpoints:

```
GET /api/Tasks/{id}/comments
POST /api/Tasks/{id}/comments
PUT /api/Tasks/{id}
```

**Status:** ⚠️ **WORKAROUND AVAILABLE**

**Current Implementation:**
Uses comment system to store feedback, but lacks dedicated client review endpoints.

**Recommended Implementation:**

**Option A: Use Comments System** (Current Approach)

```javascript
async function sendBackToEmployee(taskId) {
  const task = tasks.find((t) => t.TaskId === taskId);

  // Add comment with client feedback
  await API.Tasks.addComment(
    taskId,
    `[CLIENT FEEDBACK] ${task.ClientFeedback}\n\n` +
      `[ACCOUNT MANAGER] Please address the client's feedback and resubmit.`
  );

  // Reset task status
  await API.Tasks.update(taskId, {
    StatusId: STATUS_IN_PROGRESS,
    AccountManagerApproved: false,
    SentToClient: false,
    ClientApproved: null,
  });

  // Notify team leader
  await API.Notifications.create({
    UserId: task.AssignedToId,
    Message: `Client provided feedback on task: ${task.Title}`,
    TaskId: taskId,
  });
}
```

**Option B: Request Backend Enhancement**

Add dedicated endpoints:

```
POST /api/Tasks/{id}/client-review
  Body: { approved: boolean, feedback: string }

PUT /api/Tasks/{id}/return-to-team-leader
  Body: { accountManagerNotes: string }
```

---

### ⚠️ **5. Project-Based Access Control**

#### Available Endpoints:

```
GET /api/Projects
GET /api/Clients/{id}/projects
```

**Status:** ⚠️ **REQUIRES FILTERING LOGIC**

**Challenge:**
The API doesn't have built-in filtering for "projects where current user is account manager". You need to filter on the frontend.

**Current Implementation Gap:**
No `accountManagerId` field is documented in the Project schema in the OpenAPI spec.

**Recommended Solutions:**

**Option A: Add Query Parameter to Projects Endpoint** (Backend Change)

```
GET /api/Projects?accountManagerId={userId}
```

**Option B: Frontend Filtering** (Current Workaround)

```javascript
async function getMyProjects() {
  const currentUser = auth.getCurrentUser();
  const allProjects = await API.Projects.getAll();

  // Filter projects where I'm the account manager
  return allProjects.filter(
    (project) =>
      project.accountManagerId === currentUser.userId ||
      project.AccountManagerId === currentUser.userId
  );
}

async function getMyClients() {
  const myProjects = await getMyProjects();
  const clientIds = new Set(myProjects.map((p) => p.ClientId || p.clientId));

  const allClients = await API.Clients.getAll();
  return allClients.filter((c) => clientIds.has(c.ClientId || c.clientId));
}

async function getMyTasks() {
  const myProjects = await getMyProjects();
  const projectIds = new Set(myProjects.map((p) => p.ProjectId || p.projectId));

  const allTasks = await API.Tasks.getAll();
  return allTasks.filter((t) => projectIds.has(t.ProjectId || t.projectId));
}
```

---

## Implementation Recommendations

### Priority 1: Immediate Implementation (Using Existing APIs)

#### ✅ 1. File Upload - Already Working

No changes needed. Feature is fully implemented.

#### ✅ 2. Dashboard Client Stats - Needs Filtering

**File:** `frontend/scripts/pages/accountant/dashboard.js`

**Changes Required:**

```javascript
async function loadDashboardData() {
  try {
    utils.showLoading();
    const currentUser = auth.getCurrentUser();

    const [projects, tasks, clients] = await Promise.all([
      API.Projects.getAll(),
      API.Tasks.getAll(),
      API.Clients.getAll(),
    ]);

    // Filter by projects where user is account manager
    const myProjects = projects.filter(
      (p) => (p.accountManagerId || p.AccountManagerId) === currentUser.userId
    );

    // Get unique clients from my projects
    const myClientIds = new Set(
      myProjects
        .filter((p) => p.ClientId || p.clientId)
        .map((p) => p.ClientId || p.clientId)
    );

    const myClients = clients.filter((c) =>
      myClientIds.has(c.ClientId || c.clientId)
    );

    // Filter tasks from my projects
    const myProjectIds = new Set(
      myProjects.map((p) => p.ProjectId || p.projectId)
    );

    const myTasks = tasks.filter((t) =>
      myProjectIds.has(t.ProjectId || t.projectId)
    );

    updateStats({ projects: myProjects, clients: myClients, tasks: myTasks });
    renderRecentTasks(myTasks.slice(0, 5));
    renderRecentProjects(myProjects.slice(0, 5));
    renderRecentClients(myClients.slice(0, 5)); // NEW
  } catch (error) {
    console.error("Error loading dashboard:", error);
    utils.showError("Failed to load dashboard data");
  } finally {
    utils.hideLoading();
  }
}

// NEW: Add function to render recent clients
function renderRecentClients(clients) {
  const container = document.getElementById("recentClientsContainer");
  if (!container) return;

  if (clients.length === 0) {
    container.innerHTML = '<p class="text-secondary">No clients found</p>';
    return;
  }

  container.innerHTML = clients
    .map(
      (client) => `
    <div class="client-card">
      <h4>${client.ClientName || client.Name}</h4>
      <p>${client.Company || "N/A"}</p>
      <span class="badge badge-info">${client.ProjectCount || 0} projects</span>
    </div>
  `
    )
    .join("");
}
```

#### ⚠️ 3. Task Review Workflow - Use Existing Status + Comments

**File:** `frontend/scripts/pages/accountant/tasks.js`

**Define Custom Status IDs:**

```javascript
// Add at top of file
const TASK_REVIEW_STATUS = {
  IN_PROGRESS: 2,
  PENDING_AM_REVIEW: 6, // Configure these IDs based on your DB
  SENT_TO_CLIENT: 7,
  CLIENT_REVIEW: 8,
  CLIENT_APPROVED: 9,
  CLIENT_REJECTED: 10,
  COMPLETED: 4,
};
```

**Update Review Functions:**

```javascript
async function approveAndSendToClient() {
  if (!currentTaskForReview) return;
  const notes = document.getElementById("reviewNotes").value;

  try {
    utils.showLoading();

    // Update task status
    await API.Tasks.update(currentTaskForReview.TaskId, {
      StatusId: TASK_REVIEW_STATUS.SENT_TO_CLIENT,
    });

    // Add comment
    if (notes) {
      await API.Tasks.addComment(
        currentTaskForReview.TaskId,
        `[ACCOUNT MANAGER APPROVED] ${notes}`
      );
    }

    // Notify client (if client user exists)
    // Assuming client has a user account
    if (currentTaskForReview.ClientUserId) {
      await API.Notifications.create({
        UserId: currentTaskForReview.ClientUserId,
        Message: `Task ready for your review: ${currentTaskForReview.Title}`,
        TaskId: currentTaskForReview.TaskId,
        ProjectId: currentTaskForReview.ProjectId,
      });
    }

    utils.showSuccess(`Task approved and sent to client`);
    closeReviewModal();
    await loadTasks();
  } catch (error) {
    console.error("Error approving task:", error);
    utils.showError("Failed to approve task");
  } finally {
    utils.hideLoading();
  }
}

async function rejectTask() {
  if (!currentTaskForReview) return;
  const notes = document.getElementById("reviewNotes").value;

  if (!notes) {
    utils.showError("Please provide feedback for rejection");
    return;
  }

  try {
    utils.showLoading();

    // Update task status back to in progress
    await API.Tasks.update(currentTaskForReview.TaskId, {
      StatusId: TASK_REVIEW_STATUS.IN_PROGRESS,
    });

    // Add rejection comment
    await API.Tasks.addComment(
      currentTaskForReview.TaskId,
      `[ACCOUNT MANAGER REJECTED] ${notes}`
    );

    // Notify team leader/employee
    await API.Notifications.create({
      UserId: currentTaskForReview.AssignedToId,
      Message: `Task needs revision: ${currentTaskForReview.Title}`,
      TaskId: currentTaskForReview.TaskId,
    });

    utils.showSuccess(`Task rejected and sent back to team`);
    closeReviewModal();
    await loadTasks();
  } catch (error) {
    console.error("Error rejecting task:", error);
    utils.showError("Failed to reject task");
  } finally {
    utils.hideLoading();
  }
}

async function sendBackToEmployee(taskId) {
  const task = tasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  const feedback = prompt(
    `Client Feedback:\n${task.ClientFeedback}\n\n` +
      `Add instructions for ${task.AssignedToName}:`
  );

  if (!feedback) return;

  try {
    utils.showLoading();

    // Update status back to in progress
    await API.Tasks.update(taskId, {
      StatusId: TASK_REVIEW_STATUS.IN_PROGRESS,
    });

    // Add combined feedback comment
    await API.Tasks.addComment(
      taskId,
      `[CLIENT FEEDBACK] ${task.ClientFeedback}\n\n` +
        `[ACCOUNT MANAGER INSTRUCTIONS] ${feedback}`
    );

    // Notify team leader
    await API.Notifications.create({
      UserId: task.AssignedToId,
      Message: `Client provided feedback on: ${task.Title}`,
      TaskId: taskId,
    });

    utils.showSuccess(`Task sent back to ${task.AssignedToName}`);
    await loadTasks();
  } catch (error) {
    console.error("Error sending task back:", error);
    utils.showError("Failed to send task back");
  } finally {
    utils.hideLoading();
  }
}
```

### Priority 2: Backend Enhancements (Recommended)

To properly support the Account Manager workflow, recommend these backend API additions:

#### 1. Add Project Filtering Endpoint

```
GET /api/Projects/by-account-manager/{userId}
```

#### 2. Add Task Review Workflow Endpoints

```
PUT /api/Tasks/{id}/account-manager-approve
  Body: { notes: string }

PUT /api/Tasks/{id}/account-manager-reject
  Body: { feedback: string }

PUT /api/Tasks/{id}/send-to-client
  Body: { notes: string }

PUT /api/Tasks/{id}/client-review
  Body: { approved: boolean, feedback: string, clientUserId: int }

GET /api/Tasks/pending-account-manager-review
  Returns: List of tasks submitted by team leaders

GET /api/Tasks/pending-client-review
  Returns: List of tasks waiting for client approval
```

#### 3. Add Task Model Fields

```csharp
public class Task {
  // ... existing fields ...

  // Review Workflow
  public bool SubmittedForAccountManagerReview { get; set; }
  public int? AccountManagerId { get; set; }
  public bool? AccountManagerApproved { get; set; }
  public DateTime? AccountManagerReviewDate { get; set; }
  public string AccountManagerNotes { get; set; }

  public bool SentToClient { get; set; }
  public DateTime? SentToClientDate { get; set; }

  public bool? ClientApproved { get; set; }
  public DateTime? ClientReviewDate { get; set; }
  public string ClientFeedback { get; set; }

  public string ReviewWorkflowStatus { get; set; }
  // "TeamLeaderDraft", "PendingAMReview", "SentToClient",
  // "ClientApproved", "ClientRejected", "Completed"
}
```

#### 4. Add Client User Association

```
GET /api/Clients/{id}/users
  Returns: List of user accounts associated with client

POST /api/Clients/{id}/users
  Body: { userId: int, role: string }
```

---

## Summary & Action Items

### ✅ What Works Now:

1. **File Upload** - Fully functional
2. **Client Data Access** - API endpoints exist
3. **Task Comments** - Can be used for feedback
4. **Notifications** - Can notify users

### ⚠️ What Needs Frontend Changes:

1. **Dashboard filtering** - Filter by account manager's projects
2. **Clients page filtering** - Show only assigned clients
3. **Task review logic** - Use status + comments workflow
4. **Recent clients section** - Add to dashboard

### ❌ What Needs Backend Support:

1. **Dedicated review workflow endpoints**
2. **Project filtering by account manager**
3. **Task review status fields**
4. **Client user associations**
5. **Review workflow state management**

### Recommended Implementation Path:

**Phase 1 (Immediate - Frontend Only):**

1. ✅ Implement file upload UI (already exists)
2. ⚠️ Add filtering logic to dashboard for assigned projects
3. ⚠️ Add "Recent Clients" section to dashboard
4. ⚠️ Update task review to use status codes + comments
5. ⚠️ Filter clients page by assigned projects

**Phase 2 (Backend Enhancement):**

1. ❌ Add account manager fields to Project/Task models
2. ❌ Create review workflow endpoints
3. ❌ Add project filtering by account manager
4. ❌ Create client user association system
5. ❌ Add review workflow status tracking

**Phase 3 (Frontend Integration):**

1. Update API calls to use new endpoints
2. Remove workarounds with proper API calls
3. Add workflow visualizations
4. Enhanced error handling

---

## Code Examples for Immediate Implementation

See the detailed code examples in the Priority 1 section above for:

- Dashboard filtering by account manager
- Task review using status + comments
- Client feedback handling
- Project-scoped data access

These can be implemented immediately using existing API endpoints with minimal changes.

---

## Conclusion

**The current API supports 60-70% of the required functionality** through workarounds. The file upload feature is complete, and the basic data access is available. However, for a production-ready Account Manager workflow, backend enhancements are strongly recommended to provide proper state management, clear workflow states, and better data filtering.

The recommended approach is to implement Phase 1 (frontend workarounds) immediately to get the feature working, then plan Phase 2 (backend enhancements) for a more robust long-term solution.
