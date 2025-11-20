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
  // Team Leader's own tasks + tasks from supervised employees that need review
  const supervisedEmployeeIds = employees.map((e) => e.UserId || e.userId);
  
  const myTasks = allTasks.filter((t) => {
    const assignedTo = t.AssignedTo || t.assignedTo;
    const statusId = t.StatusId || t.statusId;
    
    // Include tasks assigned to me OR tasks from my team members that need review (StatusId = 3)
    return assignedTo === currentUser.UserId || 
           (supervisedEmployeeIds.includes(assignedTo) && statusId === 3);
  });

  const submissions = myTasks.filter((t) => t.StatusId === 3).length; // In Review
  const inProgress = myTasks.filter((t) => t.StatusId === 2).length;
  const received = submissions + inProgress; // Received = submissions + in progress
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

  // Team Leader's own tasks + tasks from supervised employees that need review
  const supervisedEmployeeIds = employees.map((e) => e.UserId || e.userId);
  
  const myTasks = allTasks.filter((t) => {
    const assignedTo = t.AssignedTo || t.assignedTo;
    const statusId = t.StatusId || t.statusId;
    
    // Include tasks assigned to me OR tasks from my team members that need review (StatusId = 3)
    return assignedTo === currentUser.UserId || 
           (supervisedEmployeeIds.includes(assignedTo) && statusId === 3);
  });

  if (!myTasks || myTasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="7" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No tasks found</h3>
            <p>You have no assigned tasks or team submissions to review</p>
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
      const isTeamMemberTask = supervisedEmployeeIds.includes(task.AssignedTo);

      return `
    <tr style="${needsReview ? "border-left: 4px solid #ff9800;" : ""}">
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
        <button class="btn btn-sm btn-primary" onclick="openTaskDetailsModal(${
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
          needsReview && (isTeamMemberTask || isAssignedToMe)
            ? `
          <button class="btn btn-sm btn-warning" onclick="openReviewModal(${task.TaskId})" title="Review Task">
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

// Open review modal for team member tasks
async function openReviewModal(taskId) {
  const task = allTasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  currentTaskForAction = task;

  try {
    utils.showLoading();

    // Populate modal with task details
    document.getElementById("reviewTaskTitle").textContent =
      task.Title || "Untitled";
    document.getElementById("reviewDescription").textContent =
      task.Description || "No description";
    document.getElementById("reviewAssignee").textContent =
      task.AssignedToName || "Unknown";
    document.getElementById("reviewCompletedDate").textContent =
      utils.formatDate(new Date());

    // Show/hide upload link
    const uploadLinkGroup = document.getElementById("reviewUploadLinkGroup");
    const uploadHref = task.DriveFolderLink || null;
    if (uploadHref) {
      uploadLinkGroup.style.display = "block";
      document.getElementById("reviewUploadLink").href = uploadHref;
    } else {
      uploadLinkGroup.style.display = "none";
    }

    document.getElementById("reviewAction").value = "approve";
    document.getElementById("reviewNotes").value = "";
    document.getElementById("reviewNewDueDate").value = "";

    // Show/hide notes and due date fields based on action
    toggleReviewFields();

    // Add event listener for action change
    document.getElementById("reviewAction").onchange = toggleReviewFields;

    document.getElementById("reviewModal").classList.remove("d-none");
  } catch (error) {
    console.error("Error loading review modal:", error);
    utils.showError("Failed to load task details for review");
  } finally {
    utils.hideLoading();
  }
}

// Toggle review fields based on action
function toggleReviewFields() {
  const action = document.getElementById("reviewAction").value;
  const notesGroup = document.getElementById("reviewNotesGroup");
  const dueDateGroup = document.getElementById("reviewNewDueDateGroup");

  if (action === "revise") {
    notesGroup.style.display = "block";
    dueDateGroup.style.display = "block";
  } else {
    notesGroup.style.display = "none";
    dueDateGroup.style.display = "none";
  }
}

// Close review modal
function closeReviewModal() {
  document.getElementById("reviewModal").classList.add("d-none");
  currentTaskForAction = null;
}

// Submit review
async function submitReview() {
  if (!currentTaskForAction) return;

  const action = document.getElementById("reviewAction").value;
  const notes = document.getElementById("reviewNotes").value;

  if (action === "revise" && !notes.trim()) {
    utils.showError("Please provide revision notes");
    return;
  }

  try {
    utils.showLoading();

    const newDueDate = document.getElementById("reviewNewDueDate").value;

    const reviewData = {
      approve: action === "approve",
      notes: notes || null,
      newDueDate: newDueDate || null,
    };

    await API.Tasks.reviewCompletion(currentTaskForAction.TaskId, reviewData);

    // Close modal first
    closeReviewModal();

    // Show success message
    utils.showSuccess(
      action === "approve"
        ? "Task approved successfully!"
        : "Revision request sent to employee with notes."
    );

    // Reload tasks to refresh the list
    await loadData();
  } catch (error) {
    console.error("Error submitting review:", error);
    utils.showError("Failed to submit review");
  } finally {
    utils.hideLoading();
  }
}

// Open task details modal
async function openTaskDetailsModal(taskId) {
  try {
    utils.showLoading();
    const task = await API.Tasks.getById(taskId);
    
    if (!task) {
      utils.showError("Task not found");
      return;
    }

    // Populate task details modal
    document.getElementById("detailsTaskTitle").textContent = task.Title || "Untitled Task";
    document.getElementById("detailsDescription").textContent = task.Description || "No description";
    document.getElementById("detailsProject").textContent = task.ProjectName || "N/A";
    document.getElementById("detailsPriority").innerHTML = utils.getPriorityBadge(task.PriorityId);
    document.getElementById("detailsStatus").innerHTML = utils.getStatusBadge(task.StatusId);
    document.getElementById("detailsAssignee").textContent = task.AssignedToName || "Unassigned";
    document.getElementById("detailsDueDate").textContent = utils.formatDate(task.DueDate);
    document.getElementById("detailsCreatedBy").textContent = task.CreatedByName || "Unknown";

    // Show/hide drive links
    const driveLinkGroup = document.getElementById("detailsDriveLinkGroup");
    if (task.DriveFolderLink) {
      driveLinkGroup.style.display = "block";
      document.getElementById("detailsDriveLink").href = task.DriveFolderLink;
    } else {
      driveLinkGroup.style.display = "none";
    }

    const materialLinkGroup = document.getElementById("detailsMaterialLinkGroup");
    if (task.MaterialDriveFolderLink) {
      materialLinkGroup.style.display = "block";
      document.getElementById("detailsMaterialLink").href = task.MaterialDriveFolderLink;
    } else {
      materialLinkGroup.style.display = "none";
    }

    document.getElementById("taskDetailsModal").classList.remove("d-none");
  } catch (error) {
    console.error("Failed to load task details:", error);
    utils.showError("Failed to load task details");
  } finally {
    utils.hideLoading();
  }
}

// Close task details modal
function closeTaskDetailsModal() {
  document.getElementById("taskDetailsModal").classList.add("d-none");
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
