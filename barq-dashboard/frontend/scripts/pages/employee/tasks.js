// Employee Tasks Script

// Protect page - require Employee role
auth.requireRole([USER_ROLES.EMPLOYEE]);

let allTasks = [];

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadTasks();
  setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
  document
    .getElementById("statusFilter")
    .addEventListener("change", filterTasks);
}

// Load all tasks
async function loadTasks() {
  try {
    utils.showLoading();
    allTasks = await API.Tasks.getAll();
    renderTasks(allTasks);
  } catch (error) {
    console.error("Error loading tasks:", error);
    utils.showError("Failed to load tasks");
  } finally {
    utils.hideLoading();
  }
}

// Filter tasks
function filterTasks() {
  const statusFilter = document.getElementById("statusFilter").value;

  let filtered = allTasks;

  if (statusFilter) {
    filtered = filtered.filter((task) => task.StatusId == statusFilter);
  }

  renderTasks(filtered);
}

// Render tasks
function renderTasks(tasks) {
  const tbody = document.getElementById("tasksBody");

  if (tasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="6" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No tasks found</h3>
            <p>No tasks match your criteria</p>
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
      <td>${task.AssignedToName || "Unassigned"}</td>
      <td>${utils.getStatusBadge(task.StatusId || 1)}</td>
      <td>${utils.getPriorityBadge(task.PriorityId || 1)}</td>
      <td>${utils.formatDate(task.DueDate)}</td>
    </tr>
  `
    )
    .join("");
}
