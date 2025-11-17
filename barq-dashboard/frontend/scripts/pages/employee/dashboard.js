// Employee Dashboard Script

// Protect page - require Employee role
auth.requireRole([USER_ROLES.EMPLOYEE]);

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadDashboardData();
});

// Load all dashboard data
async function loadDashboardData() {
  try {
    utils.showLoading();

    // Fetch employee's tasks and projects
    const [tasks, projects] = await Promise.all([
      API.Tasks.getAll().catch(() => []),
      API.Projects.getAll().catch(() => []),
    ]);

    // Filter tasks assigned to current user
    const currentUser = auth.getCurrentUser();
    const myTasks = tasks.filter(
      (task) => task.AssignedTo === currentUser.UserId
    );

    // Update stats
    updateStats(myTasks, projects);

    // Render recent data
    renderRecentTasks(myTasks.slice(0, 10));
    renderRecentProjects(projects.slice(0, 5));
  } catch (error) {
    console.error("Error loading dashboard:", error);
    utils.showError("Failed to load dashboard data");
  } finally {
    utils.hideLoading();
  }
}

// Update statistics cards
function updateStats(tasks, projects) {
  const pendingTasks = tasks.filter(
    (t) => t.StatusId === 1 || t.StatusId === 2
  ).length;
  const completedTasks = tasks.filter((t) => t.StatusId === 3).length;

  document.getElementById("totalTasks").textContent = tasks.length;
  document.getElementById("pendingTasks").textContent = pendingTasks;
  document.getElementById("completedTasks").textContent = completedTasks;
  document.getElementById("totalProjects").textContent = projects.length;
}

// Render recent tasks
function renderRecentTasks(tasks) {
  const tbody = document.getElementById("recentTasksBody");

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
        <button class="btn btn-sm btn-primary" onclick="updateTaskStatus(${
          task.TaskId
        })">
          <i class="fa-solid fa-edit"></i>
        </button>
      </td>
    </tr>
  `
    )
    .join("");
}

// Render recent projects
function renderRecentProjects(projects) {
  const tbody = document.getElementById("recentProjectsBody");

  if (projects.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="5" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No projects found</h3>
            <p>You're not assigned to any projects yet</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = projects
    .map(
      (project) => `
    <tr>
      <td><strong>${project.ProjectName || "Untitled Project"}</strong></td>
      <td>${project.ClientName || "N/A"}</td>
      <td>${utils.formatDate(project.StartDate)}</td>
      <td>${utils.formatDate(project.EndDate)}</td>
      <td>${utils.getStatusBadge(project.StatusId || 1)}</td>
    </tr>
  `
    )
    .join("");
}

// Update task status
async function updateTaskStatus(taskId) {
  window.location.href = `my-tasks.html?taskId=${taskId}`;
}
