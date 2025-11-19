// Team Leader Dashboard Script

// Protect page - require Manager role
auth.requireRole([USER_ROLES.TEAM_LEADER]);

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadDashboardData();
});

// Load all dashboard data
async function loadDashboardData() {
  try {
    utils.showLoading();

    // Fetch all data in parallel
    const [tasks, projects, employees] = await Promise.all([
      API.Tasks.getAll().catch(() => []),
      API.Projects.getAll().catch(() => []),
      API.Users.getAll().catch(() => []), // This will return only supervised employees for team leader
    ]);

    // Update stats
    updateStats({ tasks, projects, employees });

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

// Update statistics cards
function updateStats(data) {
  document.getElementById("totalTasks").textContent = data.tasks.length;
  document.getElementById("totalProjects").textContent = data.projects.length;
  document.getElementById("totalEmployees").textContent = data.employees.length;
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
      <td>${utils.getStatusBadge(task.StatusId)}</td>
      <td>${utils.getPriorityBadge(task.PriorityId)}</td>
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
      <td><span class="badge badge-info">${
        project.TaskCount || 0
      } tasks</span></td>
      <td>${utils.formatDate(project.StartDate)} - ${utils.formatDate(
        project.EndDate
      )}</td>
    </tr>
  `
    )
    .join("");
}
