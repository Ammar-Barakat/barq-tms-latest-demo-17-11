// Team Leader My Tasks Page
auth.requireRole([USER_ROLES.TEAM_LEADER]);

let allTasks = [];
let projects = [];
let employees = [];
let currentUser = null;
let currentTaskForAction = null;

// Initialize
document.addEventListener("DOMContentLoaded", async () => {
  currentUser = auth.getCurrentUser();
  await loadData();
});

// Load tasks and employees
async function loadData() {
  try {
    utils.showLoading();
    const [tasksResponse, projectsResponse, usersResponse] = await Promise.all([
      API.Tasks.getAll(),
      API.Projects.getAll().catch(() => []),
      API.Users.getAll().catch(() => []),
    ]);

    allTasks = tasksResponse || [];
    projects = projectsResponse || [];

    // Filter employees (Role 5) who are in this team leader's team
    employees = usersResponse.filter((u) => {
      const roleId = u.Role || u.RoleId || u.role;
      const teamLeaderId = u.TeamLeaderId || u.teamLeaderId;
      return roleId === 5 && teamLeaderId === currentUser.UserId;
    });

    updateStats();
    renderTasks();
  } catch (error) {
    console.error("Failed to load data:", error);
    utils.showError("Failed to load tasks");
  } finally {
    utils.hideLoading();
  }
}

// Update stats
function updateStats() {
  // Get project IDs where this team leader is assigned
  const myProjectIds = projects
    .filter((p) => (p.TeamLeaderId || p.teamLeaderId) === currentUser.UserId)
    .map((p) => p.ProjectId || p.projectId);

  // Filter tasks from projects assigned to this team leader
  const myTasks = allTasks.filter((t) => {
    const taskProjectId = t.ProjectId || t.projectId;
    return myProjectIds.includes(taskProjectId);
  });

  const received = myTasks.filter(
    (t) => t.StatusId === 1 || t.StatusId === 2
  ).length;
  const submissions = myTasks.filter((t) => t.StatusId === 3).length; // In Review
  const inProgress = myTasks.filter((t) => t.StatusId === 2).length;
  const completed = myTasks.filter((t) => t.StatusId === 4).length;

  const receivedEl = document.getElementById("receivedTasks");
  const submissionsEl = document.getElementById("employeeSubmissions");
  const inProgressEl = document.getElementById("inProgress");
  const completedEl = document.getElementById("completedCount");

  if (receivedEl) receivedEl.textContent = received;
  if (submissionsEl) submissionsEl.textContent = submissions;
  if (inProgressEl) inProgressEl.textContent = inProgress;
  if (completedEl) completedEl.textContent = completed;
}

// Render tasks
function renderTasks() {
  const tbody = document.getElementById("tasksBody");
  if (!tbody) return;

  // Get project IDs where this team leader is assigned
  const myProjectIds = projects
    .filter((p) => (p.TeamLeaderId || p.teamLeaderId) === currentUser.UserId)
    .map((p) => p.ProjectId || p.projectId);

  // Show tasks from projects where this team leader is assigned
  // This includes tasks assigned to the team leader AND tasks assigned to their team members
  const myTasks = allTasks.filter((t) => {
    const taskProjectId = t.ProjectId || t.projectId;
    return myProjectIds.includes(taskProjectId);
  });

  if (!myTasks || myTasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="7" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No tasks in your projects</h3>
            <p>There are no tasks in projects assigned to you</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = myTasks
    .map((task) => {
      const needsReview = task.StatusId === 3;
      const isAssignedToMe = task.AssignedTo === currentUser.UserId;

      return `
    <tr>
      <td><strong>${task.Title || "Untitled Task"}</strong>${
        needsReview
          ? '<span class="badge badge-warning" style="margin-left: 8px;">Needs Review</span>'
          : ""
      }</td>
      <td>${task.ProjectName || "N/A"}</td>
      <td>${utils.getPriorityBadge(task.PriorityId)}</td>
      <td>${utils.getStatusBadge(task.StatusId)}</td>
      <td>${task.AssignedToName || "Unassigned"}</td>
      <td>${utils.formatDate(task.DueDate)}</td>
      <td>
        <button class="btn btn-sm btn-primary" onclick="viewTaskDetails(${
          task.TaskId
        })" title="View Details">
          <i class="fa-solid fa-eye"></i>
        </button>
        ${
          isAssignedToMe && !needsReview
            ? `
          <button class="btn btn-sm btn-secondary" onclick="showPassTaskModal(${task.TaskId})" title="Pass to Employee">
            <i class="fa-solid fa-share"></i>
          </button>
          <button class="btn btn-sm btn-success" onclick="requestCompletion(${task.TaskId})" title="Request Completion">
            <i class="fa-solid fa-check"></i>
          </button>
        `
            : ""
        }
        ${
          needsReview
            ? `
          <button class="btn btn-sm btn-warning" onclick="reviewTask(${task.TaskId})" title="Review Task">
            <i class="fa-solid fa-clipboard-check"></i>
          </button>
        `
            : ""
        }
      </td>
    </tr>
  `;
    })
    .join("");
}

// Show pass task modal
function showPassTaskModal(taskId) {
  const task = allTasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  currentTaskForAction = task;

  // Populate employee dropdown
  const employeeSelect = document.getElementById("employeeSelect");
  if (employeeSelect) {
    employeeSelect.innerHTML =
      '<option value="">Select an employee...</option>' +
      employees
        .map(
          (emp) => `
        <option value="${emp.UserId || emp.userId}">${
            emp.Name || emp.name
          }</option>
      `
        )
        .join("");
  }

  document.getElementById("passTaskTitle").textContent = task.Title;
  document.getElementById("passTaskModal").classList.remove("d-none");
}

// Close pass task modal
function closePassTaskModal() {
  document.getElementById("passTaskModal").classList.add("d-none");
  document.getElementById("passTaskNotes").value = "";
  currentTaskForAction = null;
}

// Handle pass task
async function handlePassTask() {
  if (!currentTaskForAction) return;

  const employeeId = document.getElementById("employeeSelect").value;
  const notes = document.getElementById("passTaskNotes").value;

  if (!employeeId) {
    utils.showError("Please select an employee");
    return;
  }

  try {
    utils.showLoading();

    await API.Tasks.passTask(currentTaskForAction.TaskId, {
      assignToUserId: parseInt(employeeId),
      notes: notes || null,
    });

    const employee = employees.find(
      (e) => (e.UserId || e.userId) == employeeId
    );
    const employeeName = employee ? employee.Name || employee.name : "employee";

    utils.showSuccess(`Task passed to ${employeeName}`);
    closePassTaskModal();
    await loadData();
  } catch (error) {
    console.error("Error passing task:", error);
    utils.showError(error.message || "Failed to pass task");
  } finally {
    utils.hideLoading();
  }
}

// Request completion for task
async function requestCompletion(taskId) {
  if (
    !confirm(
      "Request review for this task? It will be sent to the manager/account manager for approval."
    )
  )
    return;

  try {
    utils.showLoading();
    await API.Tasks.requestComplete(taskId);
    utils.showSuccess("Task sent for review");
    await loadData();
  } catch (error) {
    console.error("Error requesting completion:", error);
    utils.showError("Failed to request task completion");
  } finally {
    utils.hideLoading();
  }
}

// Review task (placeholder - implement review modal if needed)
async function reviewTask(taskId) {
  try {
    const task = await API.Tasks.getById(taskId);
    // For now, just show task details
    utils.showSuccess("Task review functionality - implement modal if needed");
    console.log("Task to review:", task);
  } catch (error) {
    console.error("Failed to load task for review:", error);
    utils.showError("Failed to load task details");
  }
}

// View task details
async function viewTaskDetails(taskId) {
  try {
    const task = await API.Tasks.getById(taskId);
    utils.showSuccess("Task details loaded");
    console.log("Task details:", task);
  } catch (error) {
    console.error("Failed to load task details:", error);
    utils.showError("Failed to load task details");
  }
}

// Search functionality
const searchInput = document.getElementById("searchInput");
if (searchInput) {
  searchInput.addEventListener("input", (e) => {
    const searchTerm = e.target.value.toLowerCase();
    const rows = document.querySelectorAll("#tasksBody tr");

    rows.forEach((row) => {
      const text = row.textContent.toLowerCase();
      row.style.display = text.includes(searchTerm) ? "" : "none";
    });
  });
}
