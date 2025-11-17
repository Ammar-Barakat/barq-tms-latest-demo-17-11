// Manager Dashboard Script

// Protect page - require Manager role
auth.requireRole([USER_ROLES.MANAGER]);

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadDashboardData();
});

// Load all dashboard data
async function loadDashboardData() {
  try {
    utils.showLoading();

    // Fetch dashboard stats from dedicated API
    const stats = await API.Dashboard.getStats().catch(() => null);

    if (stats) {
      // Update stats from Dashboard API
      updateStatsFromAPI(stats);
    }

    // Fetch recent data for tables
    const [tasks, projects] = await Promise.all([
      API.Tasks.getAll().catch(() => []),
      API.Projects.getAll().catch(() => []),
    ]);

    // Debug: Log the actual API response
    console.log("📋 Tasks from API:", tasks);
    console.log("📁 Projects from API:", projects);

    // Render recent data
    renderRecentTasks(tasks.slice(0, 10));
    renderRecentProjects(projects.slice(0, 5));
  } catch (error) {
    console.error("Error loading dashboard:", error);
    utils.showError("Failed to load dashboard data");
  } finally {
    utils.hideLoading();
  }
}

// Update statistics cards from Dashboard API
function updateStatsFromAPI(stats) {
  // Dashboard API returns: DashboardStats with PascalCase properties
  document.getElementById("totalTasks").textContent = stats.TotalTasks || 0;
  document.getElementById("totalProjects").textContent =
    stats.TotalProjects || 0;
  document.getElementById("totalEmployees").textContent = stats.TotalUsers || 0;
  document.getElementById("totalClients").textContent = stats.TotalClients || 0;
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
            <h3>No tasks found</h3>
            <p>Create your first task to get started</p>
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

// Render recent projects
function renderRecentProjects(projects) {
  const tbody = document.getElementById("recentProjectsBody");

  if (projects.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="6" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No projects found</h3>
            <p>Create your first project to get started</p>
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
      <td>${
        project.StartDate ? utils.formatDate(project.StartDate) : "N/A"
      }</td>
      <td>${project.EndDate ? utils.formatDate(project.EndDate) : "N/A"}</td>
      <td><span class="badge badge-info">${
        project.TaskCount || 0
      } tasks</span></td>
    </tr>
  `
    )
    .join("");
}
