// Account Manager - My Tasks Page
auth.requireRole([USER_ROLES.ACCOUNTANT]);

let tasks = [];
let teamLeaders = [];
let currentFilter = "received";
let currentTaskForAction = null;
let currentUser = null;

document.addEventListener("DOMContentLoaded", async () => {
  currentUser = auth.getCurrentUser();
  await loadData();
  setupEventListeners();
});

async function loadData() {
  try {
    utils.showLoading();

    // Load tasks assigned to current user
    const allTasks = await API.Tasks.getAll().catch(() => []);

    // Load team leaders for delegation
    const allUsers = await API.Users.getAll().catch(() => []);
    teamLeaders = allUsers.filter(
      (u) =>
        u.roleId === 3 ||
        u.RoleId === 3 || // Team Leader role
        u.roleName === "Team Leader" ||
        u.RoleName === "Team Leader"
    );

    // Populate team leader dropdown
    populateTeamLeaderSelect();

    // Filter and normalize tasks
    tasks = allTasks
      .filter((task) => {
        const assignedToId = task.assignedToId || task.AssignedToId;
        const createdById = task.createdById || task.CreatedById;
        const delegatedById = task.delegatedById || task.DelegatedById;

        // Include tasks assigned to me OR delegated by me
        return (
          assignedToId === currentUser.userId ||
          delegatedById === currentUser.userId ||
          createdById === currentUser.userId
        );
      })
      .map((task) => ({
        TaskId: task.taskId || task.TaskId || task.id,
        Title: task.title || task.Title,
        Description: task.description || task.Description,
        ProjectId: task.projectId || task.ProjectId,
        ProjectName: task.projectName || task.ProjectName || "Unknown Project",
        Status: task.status || task.Status || "Pending",
        Priority: task.priority || task.Priority || "Medium",
        DueDate: task.dueDate || task.DueDate,
        AssignedToId: task.assignedToId || task.AssignedToId,
        AssignedToName:
          task.assignedToName || task.AssignedToName || "Unassigned",
        CreatedById: task.createdById || task.CreatedById,
        DelegatedById: task.delegatedById || task.DelegatedById,
        IsDelegated:
          (task.delegatedById || task.DelegatedById) === currentUser.userId,
        IsAssignedToMe:
          (task.assignedToId || task.AssignedToId) === currentUser.userId,
      }));

    updateStats();
    renderTasks();
  } catch (error) {
    console.error("Error loading data:", error);
    utils.showError("Failed to load tasks");
  } finally {
    utils.hideLoading();
  }
}

function populateTeamLeaderSelect() {
  const select = document.getElementById("teamLeaderSelect");
  select.innerHTML = '<option value="">Select a team leader...</option>';

  teamLeaders.forEach((leader) => {
    const option = document.createElement("option");
    option.value = leader.userId || leader.UserId || leader.id;
    option.textContent = `${leader.firstName || leader.FirstName} ${
      leader.lastName || leader.LastName
    }`;
    select.appendChild(option);
  });
}

function updateStats() {
  // Tasks received from manager (assigned to me, not delegated)
  const received = tasks.filter(
    (t) => t.IsAssignedToMe && !t.IsDelegated && t.Status !== "Completed"
  ).length;

  // My active tasks (working on personally)
  const myActive = tasks.filter(
    (t) => t.IsAssignedToMe && !t.IsDelegated && t.Status !== "Completed"
  ).length;

  // Delegated to team leaders
  const delegated = tasks.filter(
    (t) => t.IsDelegated && t.Status !== "Completed"
  ).length;

  // Completed tasks
  const completed = tasks.filter((t) => t.Status === "Completed").length;

  document.getElementById("receivedFromManager").textContent = received;
  document.getElementById("myActiveTasks").textContent = myActive;
  document.getElementById("delegatedToTeam").textContent = delegated;
  document.getElementById("completedTasks").textContent = completed;

  // Update badges
  document.getElementById("badge-received").textContent = received;
  document.getElementById("badge-assigned").textContent = myActive;
  document.getElementById("badge-delegated").textContent = delegated;
}

function filterTasks(filter) {
  currentFilter = filter;

  // Update active tab
  document.querySelectorAll(".tab-btn").forEach((btn) => {
    btn.classList.remove("active");
    if (btn.dataset.filter === filter) {
      btn.classList.add("active");
    }
  });

  // Update table title
  const titles = {
    received: "Tasks Received from Manager",
    "assigned-to-me": "Tasks Assigned to Me",
    delegated: "Tasks Delegated to Team",
    completed: "Completed Tasks",
  };
  document.getElementById("tableTitle").textContent = titles[filter];

  renderTasks();
}

function renderTasks() {
  const tbody = document.getElementById("tasksBody");

  // Filter tasks based on current filter
  let filteredTasks = [];

  switch (currentFilter) {
    case "received":
      filteredTasks = tasks.filter(
        (t) => t.IsAssignedToMe && !t.IsDelegated && t.Status !== "Completed"
      );
      break;
    case "assigned-to-me":
      filteredTasks = tasks.filter(
        (t) => t.IsAssignedToMe && t.Status !== "Completed"
      );
      break;
    case "delegated":
      filteredTasks = tasks.filter(
        (t) => t.IsDelegated && t.Status !== "Completed"
      );
      break;
    case "completed":
      filteredTasks = tasks.filter((t) => t.Status === "Completed");
      break;
  }

  if (filteredTasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="7" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No Tasks Found</h3>
            <p>There are no tasks in this category.</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = filteredTasks
    .map((task) => {
      const dueDate = task.DueDate
        ? new Date(task.DueDate).toLocaleDateString()
        : "-";

      const statusClass = getStatusClass(task.Status);
      const priorityClass = getPriorityClass(task.Priority);

      const delegatedInfo = task.IsDelegated
        ? `<span class="badge badge-info">${task.AssignedToName}</span>`
        : '<span style="color: var(--text-secondary);">Not delegated</span>';

      let actionButtons = "";

      if (currentFilter === "received" || currentFilter === "assigned-to-me") {
        actionButtons = `
          <button class="btn btn-sm btn-primary" onclick="viewTaskDetails(${task.TaskId})" title="View Details">
            <i class="fa-solid fa-eye"></i> View
          </button>
          <button class="btn btn-sm btn-secondary" onclick="showDelegateTaskModal(${task.TaskId})" title="Delegate">
            <i class="fa-solid fa-share"></i> Delegate
          </button>
          <button class="btn btn-sm btn-success" onclick="completeTask(${task.TaskId})" title="Mark Complete">
            <i class="fa-solid fa-check"></i>
          </button>
        `;
      } else if (currentFilter === "delegated") {
        actionButtons = `
          <button class="btn btn-sm btn-primary" onclick="viewTaskDetails(${task.TaskId})" title="View Details">
            <i class="fa-solid fa-eye"></i> View
          </button>
          <button class="btn btn-sm btn-info" onclick="followUpTask(${task.TaskId})" title="Follow Up">
            <i class="fa-solid fa-bell"></i> Follow Up
          </button>
        `;
      } else {
        actionButtons = `
          <button class="btn btn-sm btn-success" onclick="viewTaskDetails(${task.TaskId})" title="View Details">
            <i class="fa-solid fa-check-circle"></i> View
          </button>
        `;
      }

      return `
        <tr>
          <td>
            <strong>${task.Title}</strong>
            ${
              task.IsDelegated
                ? '<br><span class="badge badge-warning" style="margin-top: 4px;"><i class="fa-solid fa-share"></i> Delegated</span>'
                : ""
            }
          </td>
          <td>${task.ProjectName}</td>
          <td><span class="badge ${statusClass}">${task.Status}</span></td>
          <td><span class="badge ${priorityClass}">${task.Priority}</span></td>
          <td>${dueDate}</td>
          <td>${delegatedInfo}</td>
          <td>
            <div class="table-actions">
              ${actionButtons}
            </div>
          </td>
        </tr>
      `;
    })
    .join("");
}

function getStatusClass(status) {
  const statusMap = {
    Pending: "badge-secondary",
    "In Progress": "badge-info",
    Completed: "badge-success",
    "On Hold": "badge-warning",
    Cancelled: "badge-danger",
  };
  return statusMap[status] || "badge-secondary";
}

function getPriorityClass(priority) {
  const priorityMap = {
    Low: "badge-info",
    Medium: "badge-warning",
    High: "badge-danger",
    Urgent: "badge-danger",
  };
  return priorityMap[priority] || "badge-secondary";
}

// Delegate Modal Functions
function showDelegateModal() {
  document.getElementById("delegateModal").classList.remove("d-none");
  document.getElementById("delegateForm").reset();
}

function showDelegateTaskModal(taskId) {
  const task = tasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  currentTaskForAction = task;
  document.getElementById("taskIdToDelegate").value = taskId;
  document.getElementById("taskNameDisplay").textContent = task.Title;

  // Set current due date as default
  if (task.DueDate) {
    const dueDate = new Date(task.DueDate);
    const dateStr = dueDate.toISOString().split("T")[0];
    document.getElementById("delegateDueDate").value = dateStr;
  }

  document.getElementById("delegateModal").classList.remove("d-none");
}

function closeDelegateModal() {
  document.getElementById("delegateModal").classList.add("d-none");
  document.getElementById("delegateForm").reset();
  currentTaskForAction = null;
}

// Form submit handler
document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("delegateForm");
  if (form) {
    form.addEventListener("submit", handleDelegateSubmit);
  }
});

async function handleDelegateSubmit(e) {
  e.preventDefault();

  const taskId = document.getElementById("taskIdToDelegate").value;
  const teamLeaderId = document.getElementById("teamLeaderSelect").value;
  const notes = document.getElementById("delegateNotes").value;
  const dueDate = document.getElementById("delegateDueDate").value;

  if (!taskId || !teamLeaderId) {
    utils.showError("Please select a team leader");
    return;
  }

  try {
    utils.showLoading();

    // Update task assignment
    await API.Tasks.update(taskId, {
      AssignedToId: parseInt(teamLeaderId),
      DelegatedById: currentUser.userId,
      DueDate: dueDate || undefined,
    });

    // Add comment about delegation
    if (notes) {
      await API.Tasks.addComment(
        taskId,
        `Delegated to team leader with instructions: ${notes}`
      );
    }

    const teamLeader = teamLeaders.find(
      (l) => (l.userId || l.UserId) == teamLeaderId
    );
    const teamLeaderName = teamLeader
      ? `${teamLeader.firstName || teamLeader.FirstName} ${
          teamLeader.lastName || teamLeader.LastName
        }`
      : "team leader";

    console.log("Task delegated:", {
      taskId,
      teamLeaderId,
      notes,
      dueDate,
    });

    // TODO: Send notification to team leader
    utils.showSuccess(`Task delegated to ${teamLeaderName}`);

    closeDelegateModal();
    await loadData();
  } catch (error) {
    console.error("Error delegating task:", error);
    utils.showError("Failed to delegate task");
  } finally {
    utils.hideLoading();
  }
}

// Task Details Functions
async function viewTaskDetails(taskId) {
  const task = tasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  currentTaskForAction = task;

  try {
    const taskDetails = await API.Tasks.getById(taskId);
    const comments = await API.Tasks.getComments(taskId).catch(() => []);

    renderTaskDetails(taskDetails, comments);
    document.getElementById("taskDetailsModal").classList.remove("d-none");
  } catch (error) {
    console.error("Error loading task details:", error);
    utils.showError("Failed to load task details");
  }
}

function renderTaskDetails(task, comments) {
  const detailsContainer = document.getElementById("taskDetailsContent");

  const dueDate =
    task.dueDate || task.DueDate
      ? new Date(task.dueDate || task.DueDate).toLocaleDateString()
      : "Not set";

  detailsContainer.innerHTML = `
    <div class="details-grid" style="margin-bottom: var(--space-4);">
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-heading"></i> Task Title</label>
        <div class="detail-value">${task.title || task.Title}</div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-calendar"></i> Due Date</label>
        <div class="detail-value">${dueDate}</div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-user"></i> Assigned To</label>
        <div class="detail-value">${
          task.assignedToName || task.AssignedToName || "Unassigned"
        }</div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-flag"></i> Priority</label>
        <div class="detail-value"><span class="badge ${getPriorityClass(
          task.priority || task.Priority
        )}">${task.priority || task.Priority}</span></div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-info-circle"></i> Status</label>
        <div class="detail-value"><span class="badge ${getStatusClass(
          task.status || task.Status
        )}">${task.status || task.Status}</span></div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-folder"></i> Project</label>
        <div class="detail-value">${
          task.projectName || task.ProjectName || "Unknown"
        }</div>
      </div>
    </div>
    <div class="detail-item" style="margin-bottom: var(--space-4);">
      <label class="detail-label"><i class="fa-solid fa-align-left"></i> Description</label>
      <div class="detail-value">${
        task.description || task.Description || "No description"
      }</div>
    </div>
    ${
      comments && comments.length > 0
        ? `
      <div style="margin-top: var(--space-4);">
        <h4 style="margin-bottom: var(--space-3);"><i class="fa-solid fa-comments"></i> Comments</h4>
        <div style="max-height: 250px; overflow-y: auto; display: flex; flex-direction: column; gap: var(--space-2);">
          ${comments
            .map(
              (c) => `
            <div style="padding: var(--space-3); background: var(--surface-secondary); border-radius: var(--radius-md); border-left: 3px solid var(--primary-color);">
              <div style="display: flex; justify-content: space-between; margin-bottom: var(--space-2);">
                <strong>${c.userName || c.UserName || "User"}</strong>
                <span style="font-size: var(--text-sm); color: var(--text-secondary);">${new Date(
                  c.createdAt || c.CreatedAt
                ).toLocaleString()}</span>
              </div>
              <p style="margin: 0;">${c.comment || c.Comment}</p>
            </div>
          `
            )
            .join("")}
        </div>
      </div>
    `
        : ""
    }
  `;
}

function closeTaskDetailsModal() {
  document.getElementById("taskDetailsModal").classList.add("d-none");
  document.getElementById("taskComment").value = "";
  currentTaskForAction = null;
}

async function addTaskComment() {
  if (!currentTaskForAction) return;

  const comment = document.getElementById("taskComment").value;
  if (!comment) {
    utils.showError("Please enter a comment");
    return;
  }

  try {
    utils.showLoading();
    await API.Tasks.addComment(currentTaskForAction.TaskId, comment);
    utils.showSuccess("Comment added successfully");
    document.getElementById("taskComment").value = "";

    // Reload task details
    await viewTaskDetails(currentTaskForAction.TaskId);
  } catch (error) {
    console.error("Error adding comment:", error);
    utils.showError("Failed to add comment");
  } finally {
    utils.hideLoading();
  }
}

async function markTaskComplete() {
  if (!currentTaskForAction) return;

  if (!confirm("Are you sure you want to mark this task as complete?")) return;

  try {
    utils.showLoading();

    await API.Tasks.updateStatus(currentTaskForAction.TaskId, {
      Status: "Completed",
    });

    utils.showSuccess("Task marked as complete");
    closeTaskDetailsModal();
    await loadData();
  } catch (error) {
    console.error("Error completing task:", error);
    utils.showError("Failed to complete task");
  } finally {
    utils.hideLoading();
  }
}

async function completeTask(taskId) {
  if (!confirm("Mark this task as complete?")) return;

  try {
    utils.showLoading();

    await API.Tasks.updateStatus(taskId, {
      Status: "Completed",
    });

    utils.showSuccess("Task completed successfully");
    await loadData();
  } catch (error) {
    console.error("Error completing task:", error);
    utils.showError("Failed to complete task");
  } finally {
    utils.hideLoading();
  }
}

async function followUpTask(taskId) {
  const task = tasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  const message = prompt(`Send follow-up message to ${task.AssignedToName}:`);
  if (!message) return;

  try {
    utils.showLoading();

    await API.Tasks.addComment(
      taskId,
      `Follow-up from Account Manager: ${message}`
    );

    // TODO: Send notification to assigned team leader
    console.log("Follow-up sent:", { taskId, message });

    utils.showSuccess(`Follow-up sent to ${task.AssignedToName}`);
  } catch (error) {
    console.error("Error sending follow-up:", error);
    utils.showError("Failed to send follow-up");
  } finally {
    utils.hideLoading();
  }
}

function setupEventListeners() {
  document
    .getElementById("searchInput")
    .addEventListener("input", handleSearch);
}

function handleSearch(e) {
  const searchTerm = e.target.value.toLowerCase();
  const rows = document.querySelectorAll("#tasksBody tr");

  rows.forEach((row) => {
    const text = row.textContent.toLowerCase();
    row.style.display = text.includes(searchTerm) ? "" : "none";
  });
}
