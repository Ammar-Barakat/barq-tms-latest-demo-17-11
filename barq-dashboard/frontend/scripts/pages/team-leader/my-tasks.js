// Team Leader My Tasks Page
let currentFilter = "received";
let allTasks = [];
let currentReviewTask = null;

// Initialize
auth.requireRole([USER_ROLES.TEAM_LEADER]);

// Load initial data
loadTasks();
loadEmployees();

// Load tasks
async function loadTasks() {
  try {
    const data = await API.Tasks.getAll();
    allTasks = data.tasks || data || [];

    // Get current user
    const currentUser = auth.getUser();

    // Filter tasks relevant to team leader
    allTasks = allTasks.map((task) => {
      // Categorize tasks
      if (
        task.status === "SubmittedForReview" &&
        task.assignedTo === currentUser.id
      ) {
        task.category = "employee-review";
      } else if (
        task.assignedTo === currentUser.id &&
        task.status !== "Completed"
      ) {
        task.category = "received";
      } else if (
        task.status === "InProgress" &&
        task.assignedTo === currentUser.id
      ) {
        task.category = "in-progress";
      } else if (
        task.status === "Completed" &&
        task.assignedTo === currentUser.id
      ) {
        task.category = "completed";
      }

      return task;
    });

    updateStatistics();
    renderTasks();
  } catch (error) {
    console.error("Failed to load tasks:", error);
    showNotification("Failed to load tasks", "error");
  }
}

// Update statistics
function updateStatistics() {
  const currentUser = auth.getUser();

  const received = allTasks.filter(
    (t) =>
      t.assignedTo === currentUser.id &&
      t.status !== "Completed" &&
      t.status !== "SubmittedForReview"
  ).length;

  const employeeSubmissions = allTasks.filter(
    (t) => t.status === "SubmittedForReview" && t.assignedTo === currentUser.id
  ).length;

  const inProgress = allTasks.filter(
    (t) => t.assignedTo === currentUser.id && t.status === "InProgress"
  ).length;

  const completed = allTasks.filter(
    (t) => t.assignedTo === currentUser.id && t.status === "Completed"
  ).length;

  document.getElementById("receivedTasks").textContent = received;
  document.getElementById("employeeSubmissions").textContent =
    employeeSubmissions;
  document.getElementById("inProgress").textContent = inProgress;
  document.getElementById("completedCount").textContent = completed;

  // Update badges
  document.getElementById("badge-received").textContent = received;
  document.getElementById("badge-employee-review").textContent =
    employeeSubmissions;
  document.getElementById("badge-in-progress").textContent = inProgress;
}

// Filter tasks
function filterTasks(filter) {
  currentFilter = filter;

  // Update active tab
  document.querySelectorAll(".tab-btn").forEach((btn) => {
    btn.classList.remove("active");
  });
  document.querySelector(`[data-filter="${filter}"]`).classList.add("active");

  // Update table title
  const titles = {
    received: "Received Tasks from Managers",
    "employee-review": "Employee Submissions for Review",
    "in-progress": "Tasks In Progress",
    completed: "Completed Tasks",
  };
  document.getElementById("tableTitle").textContent = titles[filter];

  renderTasks();
}

// Render tasks
function renderTasks() {
  const tbody = document.getElementById("tasksBody");
  const currentUser = auth.getUser();

  let filteredTasks = [];

  if (currentFilter === "received") {
    filteredTasks = allTasks.filter(
      (t) =>
        t.assignedTo === currentUser.id &&
        t.status !== "Completed" &&
        t.status !== "SubmittedForReview"
    );
  } else if (currentFilter === "employee-review") {
    filteredTasks = allTasks.filter(
      (t) =>
        t.status === "SubmittedForReview" && t.assignedTo === currentUser.id
    );
  } else if (currentFilter === "in-progress") {
    filteredTasks = allTasks.filter(
      (t) => t.assignedTo === currentUser.id && t.status === "InProgress"
    );
  } else if (currentFilter === "completed") {
    filteredTasks = allTasks.filter(
      (t) => t.assignedTo === currentUser.id && t.status === "Completed"
    );
  }

  if (filteredTasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="7" class="text-center" style="padding: 40px; color: var(--text-secondary)">
          <i class="fa-solid fa-inbox" style="font-size: 48px; opacity: 0.3; display: block; margin-bottom: 16px"></i>
          No tasks found
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = filteredTasks
    .map(
      (task) => `
    <tr>
      <td>
        <strong>${task.name}</strong>
        ${
          task.description
            ? `<br><small style="color: var(--text-secondary)">${task.description.substring(
                0,
                80
              )}...</small>`
            : ""
        }
      </td>
      <td>${task.projectName || "N/A"}</td>
      <td>
        <span class="badge badge-${getPriorityClass(task.priority)}">
          ${task.priority || "Medium"}
        </span>
      </td>
      <td>
        <span class="badge badge-${getStatusClass(task.status)}">
          ${formatStatus(task.status)}
        </span>
      </td>
      <td>${task.assignedToName || "Unassigned"}</td>
      <td>
        ${
          task.dueDate
            ? `
          <span style="color: ${
            isOverdue(task.dueDate) ? "var(--color-danger)" : "inherit"
          }">
            ${formatDate(task.dueDate)}
          </span>
        `
            : "N/A"
        }
      </td>
      <td>
        <div style="display: flex; gap: var(--space-2)">
          ${
            task.status === "SubmittedForReview"
              ? `
            <button class="btn btn-sm btn-primary" onclick="reviewTask(${task.id})">
              <i class="fa-solid fa-clipboard-check"></i> Review
            </button>
          `
              : currentFilter === "received"
              ? `
            <button class="btn btn-sm btn-primary" onclick="assignTask(${task.id})">
              <i class="fa-solid fa-user-plus"></i> Assign
            </button>
          `
              : currentFilter === "in-progress"
              ? `
            <button class="btn btn-sm btn-success" onclick="markComplete(${task.id})">
              <i class="fa-solid fa-check"></i> Complete
            </button>
          `
              : ""
          }
          <button class="btn btn-sm btn-secondary" onclick="viewTaskDetails(${
            task.id
          })">
            <i class="fa-solid fa-eye"></i>
          </button>
        </div>
      </td>
    </tr>
  `
    )
    .join("");
}

// Review employee task submission
function reviewTask(taskId) {
  currentReviewTask = allTasks.find((t) => t.id === taskId);
  if (!currentReviewTask) return;

  document.getElementById("taskDetailsReview").innerHTML = `
    <div class="details-grid">
      <div class="detail-item">
        <span class="detail-label">Task Name</span>
        <span class="detail-value">${currentReviewTask.name}</span>
      </div>
      <div class="detail-item">
        <span class="detail-label">Project</span>
        <span class="detail-value">${
          currentReviewTask.projectName || "N/A"
        }</span>
      </div>
      <div class="detail-item">
        <span class="detail-label">Assigned To</span>
        <span class="detail-value">${
          currentReviewTask.assignedToName || "N/A"
        }</span>
      </div>
      <div class="detail-item">
        <span class="detail-label">Status</span>
        <span class="detail-value">
          <span class="badge badge-${getStatusClass(currentReviewTask.status)}">
            ${formatStatus(currentReviewTask.status)}
          </span>
        </span>
      </div>
      <div class="detail-item" style="grid-column: 1 / -1">
        <span class="detail-label">Description</span>
        <span class="detail-value">${
          currentReviewTask.description || "No description"
        }</span>
      </div>
      <div class="detail-item" style="grid-column: 1 / -1">
        <span class="detail-label">Employee Notes</span>
        <span class="detail-value" style="background: var(--surface-secondary); padding: var(--space-3); border-radius: var(--radius-md);">
          ${currentReviewTask.completionNotes || "No notes provided"}
        </span>
      </div>
    </div>
  `;

  document.getElementById("reviewModal").classList.remove("d-none");
}

// Close review modal
function closeReviewModal() {
  document.getElementById("reviewModal").classList.add("d-none");
  document.getElementById("reviewNotes").value = "";
  currentReviewTask = null;
}

// Approve submission
async function approveSubmission() {
  if (!currentReviewTask) return;

  const notes = document.getElementById("reviewNotes").value;

  try {
    // Update task status to completed
    await API.Tasks.update(currentReviewTask.id, {
      status: "Completed",
      reviewNotes: notes,
    });

    // Add comment
    if (notes) {
      await API.Tasks.addComment(currentReviewTask.id, {
        comment: `Task approved by Team Leader: ${notes}`,
      });
    }

    showNotification("Task approved successfully", "success");
    closeReviewModal();
    loadTasks();
  } catch (error) {
    console.error("Failed to approve task:", error);
    showNotification("Failed to approve task", "error");
  }
}

// Reject submission
async function rejectSubmission() {
  if (!currentReviewTask) return;

  const notes = document.getElementById("reviewNotes").value;

  if (!notes.trim()) {
    showNotification("Please provide feedback for rejection", "warning");
    return;
  }

  try {
    // Update task status back to in progress
    await API.Tasks.update(currentReviewTask.id, {
      status: "InProgress",
      reviewNotes: notes,
    });

    // Add comment
    await API.Tasks.addComment(currentReviewTask.id, {
      comment: `Task rejected by Team Leader - Needs rework: ${notes}`,
    });

    showNotification("Task sent back for rework", "info");
    closeReviewModal();
    loadTasks();
  } catch (error) {
    console.error("Failed to reject task:", error);
    showNotification("Failed to reject task", "error");
  }
}

// Assign task to employee
let currentAssignTask = null;

function assignTask(taskId) {
  currentAssignTask = allTasks.find((t) => t.id === taskId);
  if (!currentAssignTask) return;

  document.getElementById("taskIdToAssign").value = taskId;
  document.getElementById("assignTaskName").textContent =
    currentAssignTask.name;
  document.getElementById("assignModal").classList.remove("d-none");
}

function closeAssignModal() {
  document.getElementById("assignModal").classList.add("d-none");
  document.getElementById("assignForm").reset();
  currentAssignTask = null;
}

// Load employees (team members)
async function loadEmployees() {
  try {
    const users = await API.Users.getAll();
    const employees = users.filter((u) => u.role === "EMPLOYEE");

    const select = document.getElementById("employeeSelect");
    select.innerHTML =
      '<option value="">Select an employee...</option>' +
      employees
        .map(
          (emp) => `
        <option value="${emp.id}">${emp.fullName || emp.email}</option>
      `
        )
        .join("");
  } catch (error) {
    console.error("Failed to load employees:", error);
  }
}

// Handle assign form submission
document.getElementById("assignForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  const taskId = document.getElementById("taskIdToAssign").value;
  const employeeId = document.getElementById("employeeSelect").value;
  const notes = document.getElementById("assignNotes").value;

  if (!employeeId) {
    showNotification("Please select an employee", "warning");
    return;
  }

  try {
    await API.Tasks.update(taskId, {
      assignedTo: employeeId,
      status: "InProgress",
    });

    if (notes) {
      await API.Tasks.addComment(taskId, {
        comment: `Task assigned by Team Leader: ${notes}`,
      });
    }

    showNotification("Task assigned successfully", "success");
    closeAssignModal();
    loadTasks();
  } catch (error) {
    console.error("Failed to assign task:", error);
    showNotification("Failed to assign task", "error");
  }
});

// Mark task as complete
async function markComplete(taskId) {
  if (!confirm("Mark this task as complete?")) return;

  try {
    await API.Tasks.update(taskId, {
      status: "Completed",
    });

    showNotification("Task marked as complete", "success");
    loadTasks();
  } catch (error) {
    console.error("Failed to complete task:", error);
    showNotification("Failed to complete task", "error");
  }
}

// View task details
function viewTaskDetails(taskId) {
  const task = allTasks.find((t) => t.id === taskId);
  if (!task) return;

  alert(
    `Task Details:\n\nName: ${task.name}\nDescription: ${
      task.description || "N/A"
    }\nStatus: ${formatStatus(task.status)}\nPriority: ${
      task.priority
    }\nDue Date: ${task.dueDate ? formatDate(task.dueDate) : "N/A"}`
  );
}

// Helper functions
function getPriorityClass(priority) {
  const classes = {
    Low: "success",
    Medium: "warning",
    High: "danger",
    Critical: "danger",
  };
  return classes[priority] || "secondary";
}

function getStatusClass(status) {
  const classes = {
    NotStarted: "secondary",
    InProgress: "primary",
    SubmittedForReview: "warning",
    Completed: "success",
    Cancelled: "danger",
  };
  return classes[status] || "secondary";
}

function formatStatus(status) {
  return status.replace(/([A-Z])/g, " $1").trim();
}

function formatDate(dateString) {
  const date = new Date(dateString);
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

function isOverdue(dateString) {
  return new Date(dateString) < new Date();
}

// Search functionality
document.getElementById("searchInput").addEventListener("input", (e) => {
  const searchTerm = e.target.value.toLowerCase();
  const rows = document.querySelectorAll("#tasksBody tr");

  rows.forEach((row) => {
    const text = row.textContent.toLowerCase();
    row.style.display = text.includes(searchTerm) ? "" : "none";
  });
});
