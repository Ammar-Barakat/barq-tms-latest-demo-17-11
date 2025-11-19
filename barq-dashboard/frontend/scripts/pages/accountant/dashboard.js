// Account Manager Dashboard Script
auth.requireRole([USER_ROLES.ACCOUNTANT]);

document.addEventListener("DOMContentLoaded", async () => {
  await loadDashboardData();
});

async function loadDashboardData() {
  try {
    utils.showLoading();

    const [projects, tasks] = await Promise.all([
      API.Projects.getAll().catch(() => []),
      API.Tasks.getAll().catch(() => []),
    ]);

    // Extract unique clients from projects
    const clientMap = new Map();
    projects.forEach((p) => {
      if (p.ClientId && p.ClientName) {
        clientMap.set(p.ClientId, {
          ClientId: p.ClientId,
          ClientName: p.ClientName,
        });
      }
    });
    const clients = Array.from(clientMap.values());

    updateStats({ projects, clients, tasks });
    renderRecentTasks(tasks.slice(0, 5));
    renderRecentProjects(projects.slice(0, 5));
  } catch (error) {
    console.error("Error loading dashboard:", error);
    utils.showError("Failed to load dashboard data");
  } finally {
    utils.hideLoading();
  }
}

function updateStats(data) {
  document.getElementById("totalTasks").textContent = data.tasks.length;
  document.getElementById("totalProjects").textContent = data.projects.length;
  document.getElementById("totalEmployees").textContent = "0";
  document.getElementById("totalClients").textContent = data.clients.length;
}

function renderRecentTasks(tasks) {
  const tbody = document.getElementById("recentTasksBody");

  if (tasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="6" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No tasks found</h3>
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

function renderRecentProjects(projects) {
  const tbody = document.getElementById("recentProjectsBody");

  if (projects.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="4" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No projects found</h3>
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
