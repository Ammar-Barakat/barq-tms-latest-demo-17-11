// Employee My Tasks Script

// Protect page - require Employee role
auth.requireRole([USER_ROLES.EMPLOYEE]);

let myTasks = [];
let currentTaskId = null;

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadMyTasks();
  setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
  document
    .getElementById("statusFilter")
    .addEventListener("change", filterTasks);
}

// Load my tasks
async function loadMyTasks() {
  try {
    utils.showLoading();
    const allTasks = await API.Tasks.getAll();

    // Filter tasks assigned to current user
    const currentUser = auth.getCurrentUser();
    myTasks = allTasks.filter((task) => task.AssignedTo === currentUser.UserId);

    renderTasks(myTasks);
  } catch (error) {
    console.error("Error loading my tasks:", error);
    utils.showError("Failed to load your tasks");
  } finally {
    utils.hideLoading();
  }
}

// Filter tasks
function filterTasks() {
  const statusFilter = document.getElementById("statusFilter").value;

  let filtered = myTasks;

  if (statusFilter) {
    filtered = filtered.filter((task) => task.StatusId == statusFilter);
  }

  renderTasks(filtered);
}

// Render tasks
function renderTasks(tasks) {
  const tbody = document.getElementById("myTasksBody");

  if (tasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="6" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No tasks assigned</h3>
            <p>You don't have any tasks assigned yet</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = tasks
    .map(
      (task) => `
    <tr>
      <td><strong>${task.Title || "Untitled Task"}</strong></td>
      <td>${task.ProjectName || "N/A"}</td>
      <td>${utils.getStatusBadge(task.StatusId || 1)}</td>
      <td>${utils.getPriorityBadge(task.PriorityId || 1)}</td>
      <td>${utils.formatDate(task.DueDate)}</td>
      <td>
        <button class="btn btn-sm btn-primary" onclick="openStatusModal(${
          task.TaskId
        }, '${task.Title}', ${task.StatusId})">
          <i class="fa-solid fa-edit"></i> Update
        </button>
      </td>
    </tr>
  `
    )
    .join("");
}

// Open status modal
function openStatusModal(taskId, taskTitle, currentStatus) {
  currentTaskId = taskId;
  document.getElementById("modalTaskTitle").textContent = taskTitle;
  document.getElementById("taskStatus").value = currentStatus;
  document.getElementById("taskNotes").value = "";
  document.getElementById("statusModal").style.display = "flex";
}

// Close status modal
function closeStatusModal() {
  document.getElementById("statusModal").style.display = "none";
  currentTaskId = null;
}

// Save task status
async function saveTaskStatus() {
  if (!currentTaskId) return;

  const status = parseInt(document.getElementById("taskStatus").value);
  const notes = document.getElementById("taskNotes").value;

  try {
    utils.showLoading();

    // Find the task
    const task = myTasks.find((t) => t.TaskId === currentTaskId);
    if (!task) throw new Error("Task not found");

    // Update task status
    const updatedTask = {
      ...task,
      StatusId: status,
      Notes: notes || task.Notes,
    };

    await API.Tasks.update(currentTaskId, updatedTask);
    utils.showSuccess("Task status updated successfully");

    closeStatusModal();
    await loadMyTasks();
  } catch (error) {
    console.error("Error updating task status:", error);
    utils.showError("Failed to update task status");
  } finally {
    utils.hideLoading();
  }
}
