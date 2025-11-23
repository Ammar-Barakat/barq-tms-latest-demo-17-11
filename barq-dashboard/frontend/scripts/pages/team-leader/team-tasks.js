// Team Leader Team Tasks
auth.requireRole([USER_ROLES.TEAM_LEADER]);

let allTasks = [];
let projects = [];
let employees = []; // Will contain only supervised employees
let priorities = [];
let statuses = [];
let departments = [];
let currentUser = null;

document.addEventListener("DOMContentLoaded", async () => {
  currentUser = auth.getCurrentUser();
  await loadTeamTasks();
  setupEventListeners();
});

function setupEventListeners() {
  document
    .getElementById("statusFilter")
    .addEventListener("change", filterTasks);
  document.getElementById("taskForm").addEventListener("submit", handleSubmit);
}

async function loadTeamTasks() {
  try {
    utils.showLoading();

    [allTasks, projects, allUsers, priorities, statuses, departments] =
      await Promise.all([
        API.Tasks.getAll(),
        API.Projects.getAll().catch(() => []),
        API.Users.getAll().catch(() => []),
        fetch(`${API_CONFIG.BASE_URL}/priorities`, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem(
              API_CONFIG.TOKEN_KEY
            )}`,
          },
        })
          .then((r) => r.json())
          .catch(() => []),
        fetch(`${API_CONFIG.BASE_URL}/statuses`, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem(
              API_CONFIG.TOKEN_KEY
            )}`,
          },
        })
          .then((r) => r.json())
          .catch(() => []),
        fetch(`${API_CONFIG.BASE_URL}/departments`, {
          headers: {
            Authorization: `Bearer ${localStorage.getItem(
              API_CONFIG.TOKEN_KEY
            )}`,
          },
        })
          .then((r) => r.json())
          .catch(() => []),
      ]);

    // Filter employees to only those under this team leader
    employees = allUsers.filter((u) => {
      const roleId = u.Role || u.RoleId;
      const teamLeaderId = u.TeamLeaderId || u.teamLeaderId;
      return roleId === 5 && teamLeaderId === currentUser.UserId;
    });

    // Fallback for Priorities if API fails
    if (!priorities || priorities.length === 0) {
      priorities = [
        { PriorityId: 1, PriorityLevel: "Low" },
        { PriorityId: 2, PriorityLevel: "Medium" },
        { PriorityId: 3, PriorityLevel: "High" }
      ];
    }

    // Fallback for Statuses if API fails
    if (!statuses || statuses.length === 0) {
      statuses = [
        { StatusId: 1, StatusName: "To Do" },
        { StatusId: 2, StatusName: "In Progress" },
        { StatusId: 3, StatusName: "In Review" },
        { StatusId: 4, StatusName: "Completed" },
        { StatusId: 5, StatusName: "Cancelled" }
      ];
    }

    // Backend already filters tasks correctly, just render them
    populateDropdowns();
    renderTasks(allTasks);
  } catch (error) {
    console.error("Error loading team tasks:", error);
    utils.showError("Failed to load team tasks");
  } finally {
    utils.hideLoading();
  }
}

function populateDropdowns() {
  // Projects
  const projectSelect = document.getElementById("projectId");
  projectSelect.innerHTML =
    '<option value="">Select Project (Optional)</option>';
  projects.forEach((project) => {
    const option = document.createElement("option");
    option.value = project.ProjectId;
    option.textContent = project.ProjectName;
    projectSelect.appendChild(option);
  });

  // Employees (only supervised)
  const employeeSelect = document.getElementById("assignedToId");
  employeeSelect.innerHTML = '<option value="">Select Employee</option>';
  employees.forEach((emp) => {
    const option = document.createElement("option");
    option.value = emp.UserId;
    option.textContent = emp.Name || emp.Username;
    employeeSelect.appendChild(option);
  });

  // Priorities
  const prioritySelect = document.getElementById("priorityId");
  prioritySelect.innerHTML = '<option value="">Select Priority</option>';
  priorities.forEach((priority) => {
    const option = document.createElement("option");
    option.value = priority.PriorityId || priority.priorityId;
    option.textContent = priority.PriorityLevel || priority.priorityLevel;
    prioritySelect.appendChild(option);
  });

  // Statuses
  const statusSelect = document.getElementById("statusId");
  statusSelect.innerHTML = '<option value="">Select Status</option>';
  statuses.forEach((status) => {
    const option = document.createElement("option");
    option.value = status.StatusId || status.statusId;
    option.textContent = status.StatusName || status.statusName;
    statusSelect.appendChild(option);
  });

  // Departments
  const deptSelect = document.getElementById("deptId");
  deptSelect.innerHTML = '<option value="">Select Department</option>';
  departments.forEach((dept) => {
    const option = document.createElement("option");
    option.value = dept.DeptId || dept.deptId;
    option.textContent = dept.DeptName || dept.deptName;
    deptSelect.appendChild(option);
  });
}

function filterTasks() {
  const statusFilter = document.getElementById("statusFilter").value;
  let filtered = allTasks;

  if (statusFilter) {
    filtered = filtered.filter((task) => task.StatusId == statusFilter);
  }

  renderTasks(filtered);
}

function renderTasks(tasks) {
  const tbody = document.getElementById("teamTasksBody");

  if (tasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="7" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No team tasks found</h3>
            <p>No tasks assigned to your team members</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = tasks
    .map((task) => {
      const taskId = task.taskId || task.TaskId || task.Id;
      const statusId = task.statusId || task.StatusId;
      
      // Check if task needs review (StatusId 3 = "In Review")
      const needsReview = statusId === 3;
      
      const reviewBadge = needsReview
        ? '<span class="badge badge-warning" style="margin-left: 5px;">Needs Review</span>'
        : "";
      
      return `
    <tr style="${needsReview ? "border-left: 4px solid #ff9800;" : ""}">
      <td><strong>${task.Title || "Untitled Task"}</strong>${reviewBadge}</td>
      <td>${task.ProjectName || "N/A"}</td>
      <td>${task.AssignedToName || "Unassigned"}</td>
      <td>${utils.getStatusBadge(task.StatusId)}</td>
      <td>${utils.getPriorityBadge(task.PriorityId)}</td>
      <td>${utils.formatDate(task.DueDate)}</td>
      <td>
        <div class="btn-group">
          ${
            needsReview
              ? `
          <button class="btn btn-sm btn-warning" onclick="openReviewModal(${taskId})" title="Review completed task">
            <i class="fa-solid fa-clipboard-check"></i>
          </button>
          `
              : ""
          }
          <button class="btn btn-sm btn-primary" onclick="viewTask(${taskId})" title="View details">
            <i class="fa-solid fa-eye"></i>
          </button>
        </div>
      </td>
    </tr>
  `;
    })
    .join("");
}

function showCreateModal() {
  currentEditId = null;
  document.getElementById("taskForm").reset();
  document.getElementById("modalTitle").textContent = "Create New Task";
  document.getElementById("taskModal").classList.remove("d-none");
}

function closeModal() {
  document.getElementById("taskModal").classList.add("d-none");
  currentEditId = null;
}

async function handleSubmit(e) {
  e.preventDefault();

  const dueDateVal = document.getElementById("dueDate").value;
  
  // Validate Due Date
  if (dueDateVal) {
    const selectedDate = new Date(dueDateVal);
    const today = new Date();
    today.setHours(0, 0, 0, 0); // Reset time to start of day for comparison

    if (selectedDate < today) {
      utils.showError("Due date cannot be in the past. Please select today or a future date.");
      return;
    }
  }

  const formData = {
    Title: document.getElementById("title").value,
    Description: document.getElementById("description").value,
    PriorityId: parseInt(document.getElementById("priorityId").value),
    StatusId: parseInt(document.getElementById("statusId").value),
    DueDate: dueDateVal || null,
    AssignedTo: parseInt(document.getElementById("assignedToId").value) || null,
    DeptId: parseInt(document.getElementById("deptId").value),
    ProjectId: parseInt(document.getElementById("projectId").value) || null,
    DriveFolderLink: document.getElementById("driveFolderLink").value,
    MaterialDriveFolderLink:
      document.getElementById("materialDriveFolderLink").value || null,
  };

  try {
    if (currentEditId) {
      await API.Tasks.update(currentEditId, formData);
      utils.showSuccess("Task updated successfully");
    } else {
      await API.Tasks.create(formData);
      utils.showSuccess("Task created successfully");
    }

    closeModal();
    await loadTeamTasks();
  } catch (error) {
    console.error("Error saving task:", error);
    utils.showError(error.message || "Failed to save task");
  }
}

// View task details
async function viewTask(taskId) {
  try {
    utils.showLoading();
    const task = await API.Tasks.getById(taskId);
    
    if (!task) {
      utils.showError("Task not found");
      return;
    }

    const driveFolderLink = task.driveFolderLink || task.DriveFolderLink || "";
    const materialDriveFolderLink = task.materialDriveFolderLink || task.MaterialDriveFolderLink || "";

    const detailsContainer = document.getElementById("taskDetailsContent");
    detailsContainer.innerHTML = `
      <div class="details-grid" style="margin-bottom: var(--space-4);">
        <div class="detail-item">
          <label class="detail-label"><i class="fa-solid fa-heading"></i> Task Title</label>
          <div class="detail-value">${task.title || task.Title}</div>
        </div>
        <div class="detail-item">
          <label class="detail-label"><i class="fa-solid fa-calendar"></i> Due Date</label>
          <div class="detail-value">${utils.formatDate(task.dueDate || task.DueDate)}</div>
        </div>
        <div class="detail-item">
          <label class="detail-label"><i class="fa-solid fa-user"></i> Assigned To</label>
          <div class="detail-value">${task.assignedToName || task.AssignedToName || "Unassigned"}</div>
        </div>
        <div class="detail-item">
          <label class="detail-label"><i class="fa-solid fa-flag"></i> Priority</label>
          <div class="detail-value">${utils.getPriorityBadge(task.priorityId || task.PriorityId)}</div>
        </div>
        <div class="detail-item">
          <label class="detail-label"><i class="fa-solid fa-info-circle"></i> Status</label>
          <div class="detail-value">${utils.getStatusBadge(task.statusId || task.StatusId)}</div>
        </div>
        <div class="detail-item">
          <label class="detail-label"><i class="fa-solid fa-folder"></i> Project</label>
          <div class="detail-value">${task.projectName || task.ProjectName || "No Project"}</div>
        </div>
        <div class="detail-item">
          <label class="detail-label"><i class="fa-solid fa-user-pen"></i> Created By</label>
          <div class="detail-value">${task.createdByName || task.CreatedByName || "Unknown"}</div>
        </div>
      </div>
      
      <div class="detail-item" style="margin-bottom: var(--space-4);">
        <label class="detail-label"><i class="fa-solid fa-align-left"></i> Description</label>
        <div class="detail-value">${task.description || task.Description || "No description"}</div>
      </div>

      ${(driveFolderLink || materialDriveFolderLink) ? `
      <div class="detail-item" style="margin-bottom: var(--space-4);">
        <label class="detail-label"><i class="fa-solid fa-link"></i> Resources</label>
        <div class="detail-value" style="display: flex; gap: var(--space-3); flex-wrap: wrap;">
          ${driveFolderLink ? `
          <a href="${driveFolderLink}" target="_blank" class="btn btn-primary" style="text-decoration: none; flex: 1;">
            <i class="fa-brands fa-google-drive"></i> Open Task Folder
          </a>
          ` : ''}
          ${materialDriveFolderLink ? `
          <a href="${materialDriveFolderLink}" target="_blank" class="btn btn-secondary" style="text-decoration: none; flex: 1;">
            <i class="fa-solid fa-folder-open"></i> Open Material Folder
          </a>
          ` : ''}
        </div>
      </div>
      ` : ''}
    `;

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

// Open review modal
async function openReviewModal(taskId) {
  const task = allTasks.find((t) => (t.taskId || t.TaskId || t.Id) == taskId);
  if (!task) return;

  currentEditId = taskId;

  try {
    utils.showLoading();

    // Populate modal with task details
    document.getElementById("reviewTaskTitle").textContent =
      task.title || task.Title || "Untitled";
    document.getElementById("reviewDescription").textContent =
      task.description || task.Description || "No description";
    document.getElementById("reviewAssignee").textContent =
      task.assignedToName ||
      task.AssignedToName ||
      task.AssignedTo ||
      "Unknown";
    document.getElementById("reviewCompletedDate").textContent =
      utils.formatDate(
        task.completedDate ||
          task.CompletedDate ||
          task.CompletedAt ||
          new Date()
      );

    // Show/hide upload link
    const uploadLinkGroup = document.getElementById("reviewUploadLinkGroup");
    const uploadHref =
      task.driveFolderLink ||
      task.DriveFolderLink ||
      task.DriveUploadLink ||
      task.driveUploadLink ||
      null;
    if (uploadHref) {
      uploadLinkGroup.style.display = "block";
      document.getElementById("reviewUploadLink").href = uploadHref;
    } else {
      uploadLinkGroup.style.display = "none";
    }

    // Hide employee notes section
    const employeeNotesGroup = document.getElementById(
      "reviewEmployeeNotesGroup"
    );
    if (employeeNotesGroup) {
      employeeNotesGroup.style.display = "none";
    }

    document.getElementById("reviewAction").value = "approve";
    document.getElementById("managerNotes").value = "";
    document.getElementById("newDueDate").value = "";

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
  const notesGroup = document.getElementById("managerNotesGroup");
  const dueDateGroup = document.getElementById("newDueDateGroup");

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
  currentEditId = null;
}

// Submit review
async function submitReview() {
  if (!currentEditId) return;

  const action = document.getElementById("reviewAction").value;
  const notes = document.getElementById("managerNotes").value;

  if (action === "revise" && !notes.trim()) {
    utils.showError("Please provide revision notes");
    return;
  }

  try {
    utils.showLoading();

    const newDueDate = document.getElementById("newDueDate").value;

    const reviewData = {
      approve: action === "approve",
      notes: notes || null,
      newDueDate: newDueDate || null,
    };

    await API.Tasks.reviewCompletion(currentEditId, reviewData);

    // Close modal first
    closeReviewModal();

    // Show success message
    utils.showSuccess(
      action === "approve"
        ? "Task approved successfully! Task marked as done."
        : "Revision request sent to employee with notes."
    );

    // Reload tasks to refresh the list
    await loadTeamTasks();
  } catch (error) {
    console.error("Error submitting review:", error);
    utils.showError("Failed to submit review");
  } finally {
    utils.hideLoading();
  }
}
