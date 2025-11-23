// Manager Analytics Page Script
auth.requireRole([USER_ROLES.MANAGER]);

document.addEventListener("DOMContentLoaded", async () => {
  await loadAnalytics();
  await loadReportEntities();
});

async function loadAnalytics() {
  try {
    utils.showLoading();

    const [tasks, projects, users] = await Promise.all([
      API.Tasks.getAll().catch(() => []),
      API.Projects.getAll().catch(() => []),
      API.Users.getAll().catch(() => []),
    ]);

    calculateTaskStats(tasks);
    renderBudgetOverview(projects, tasks);
    renderTeamPerformance(tasks, users);
  } catch (error) {
    console.error("Error loading analytics:", error);
    utils.showError("Failed to load analytics data");
  } finally {
    utils.hideLoading();
  }
}

function calculateTaskStats(tasks) {
  const now = new Date();

  const completed = tasks.filter((t) => t.StatusId === 4).length;
  const pending = tasks.filter((t) => t.StatusId === 1).length;
  const inProgress = tasks.filter((t) => t.StatusId === 2).length;
  const overdue = tasks.filter((t) => {
    if (!t.DueDate || t.StatusId === 4 || t.StatusId === 5) return false;
    const dueDate = new Date(t.DueDate);
    dueDate.setHours(23, 59, 59, 999);
    return dueDate < now;
  }).length;

  document.getElementById("completedTasks").textContent = completed;
  document.getElementById("pendingTasks").textContent = pending;
  document.getElementById("inProgressTasks").textContent = inProgress;
  document.getElementById("overdueTasks").textContent = overdue;

  // Render chart
  renderTaskDistributionChart(
    pending,
    inProgress,
    completed,
    tasks.length - pending - inProgress - completed
  );
}

function renderBudgetOverview(projects, tasks) {
  const tbody = document.getElementById("budgetTableBody");

  if (projects.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="4" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No project data available</h3>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  // Calculate completion for each project based on tasks
  const projectsWithCompletion = projects.map((project) => {
    const projectTasks = tasks.filter((t) => t.ProjectId === project.ProjectId);
    const completedTasks = projectTasks.filter((t) => t.StatusId === 4).length;
    const totalTasks = projectTasks.length;
    const completion =
      totalTasks > 0 ? Math.round((completedTasks / totalTasks) * 100) : 0;

    return {
      ...project,
      totalTasks,
      completedTasks,
      completion,
    };
  });

  tbody.innerHTML = projectsWithCompletion
    .map((project) => {
      return `
      <tr>
        <td><strong>${project.ProjectName || "Untitled"}</strong></td>
        <td>${project.ClientName || "No Client"}</td>
        <td><span class="badge badge-info">${
          project.totalTasks
        } tasks</span> <span class="badge badge-success">${
        project.completedTasks
      } done</span></td>
        <td>
          <div style="display: flex; align-items: center; gap: 10px;">
            <div style="flex: 1; height: 8px; background: rgba(126, 45, 150, 0.2); border-radius: 4px; overflow: hidden;">
              <div style="width: ${
                project.completion
              }%; height: 100%; background: linear-gradient(90deg, var(--primary-color), var(--primary-light));"></div>
            </div>
            <span style="font-size: var(--text-sm); color: var(--text-secondary);">${
              project.completion
            }%</span>
          </div>
        </td>
      </tr>
    `;
    })
    .join("");
}

function renderTeamPerformance(tasks, users) {
  const tbody = document.getElementById("performanceTableBody");

  if (users.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="5" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No employee data available</h3>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = users
    .map((user) => {
      const userId = user.UserId;
      const assignedTasks = tasks.filter((t) => t.AssignedTo === userId);
      const completed = assignedTasks.filter((t) => t.StatusId === 4).length;
      const inProgress = assignedTasks.filter((t) => t.StatusId === 2).length;
      const completionRate =
        assignedTasks.length > 0
          ? Math.round((completed / assignedTasks.length) * 100)
          : 0;

      return `
      <tr>
        <td><strong>${user.Name || user.Username || "Unknown"}</strong></td>
        <td>${assignedTasks.length}</td>
        <td><span class="badge badge-success">${completed}</span></td>
        <td><span class="badge badge-info">${inProgress}</span></td>
        <td>
          <div style="display: flex; align-items: center; gap: 10px;">
            <div style="flex: 1; height: 8px; background: rgba(16, 185, 129, 0.2); border-radius: 4px; overflow: hidden;">
              <div style="width: ${completionRate}%; height: 100%; background: var(--success);"></div>
            </div>
            <span style="font-size: var(--text-sm); font-weight: 600;">${completionRate}%</span>
          </div>
        </td>
      </tr>
    `;
    })
    .join("");
}

// Chart rendering function
let taskChart = null;

function renderTaskDistributionChart(pending, inProgress, completed, others) {
  const canvas = document.getElementById("taskDistributionChart");

  // Destroy existing chart if it exists
  if (taskChart) {
    taskChart.destroy();
  }

  const ctx = canvas.getContext("2d");

  taskChart = new Chart(ctx, {
    type: "doughnut",
    data: {
      labels: ["Pending", "In Progress", "Completed", "Others"],
      datasets: [
        {
          data: [pending, inProgress, completed, others],
          backgroundColor: [
            "rgba(245, 158, 11, 0.8)", // Warning/Pending - Orange
            "rgba(59, 130, 246, 0.8)", // Info/In Progress - Blue
            "rgba(16, 185, 129, 0.8)", // Success/Completed - Green
            "rgba(156, 163, 175, 0.8)", // Secondary/Others - Gray
          ],
          borderColor: [
            "rgb(245, 158, 11)",
            "rgb(59, 130, 246)",
            "rgb(16, 185, 129)",
            "rgb(156, 163, 175)",
          ],
          borderWidth: 2,
        },
      ],
    },
    options: {
      responsive: true,
      maintainAspectRatio: true,
      plugins: {
        legend: {
          position: "bottom",
          labels: {
            color: "#f2f0d9",
            font: {
              size: 14,
            },
            padding: 15,
          },
        },
        tooltip: {
          backgroundColor: "rgba(30, 30, 46, 0.9)",
          titleColor: "#f2f0d9",
          bodyColor: "#f2f0d9",
          borderColor: "rgba(126, 45, 150, 0.5)",
          borderWidth: 1,
          padding: 12,
          displayColors: true,
          callbacks: {
            label: function (context) {
              const label = context.label || "";
              const value = context.parsed || 0;
              const total = context.dataset.data.reduce((a, b) => a + b, 0);
              const percentage =
                total > 0 ? ((value / total) * 100).toFixed(1) : 0;
              return `${label}: ${value} (${percentage}%)`;
            },
          },
        },
      },
    },
  });
}

// Report Generation Logic
let allUsers = [];
let allClients = [];

async function loadReportEntities() {
  try {
    const [users, clients] = await Promise.all([
      API.Users.getAll().catch(() => []),
      API.Clients.getAll().catch(() => [])
    ]);
    
    allUsers = users;
    allClients = clients;
    
    toggleReportEntitySelect(); // Initialize dropdown
  } catch (error) {
    console.error("Error loading report entities:", error);
  }
}

window.toggleReportEntitySelect = function() {
  const type = document.getElementById("reportType").value;
  const select = document.getElementById("reportEntity");
  select.innerHTML = '<option value="">Select...</option>';
  
  if (type === "employee") {
    allUsers.forEach(user => {
      const option = document.createElement("option");
      option.value = user.UserId;
      option.textContent = user.Name || user.Username;
      select.appendChild(option);
    });
  } else {
    allClients.forEach(client => {
      const option = document.createElement("option");
      option.value = client.ClientId;
      option.textContent = client.Name;
      select.appendChild(option);
    });
  }
};

window.generateReport = async function() {
  const type = document.getElementById("reportType").value;
  const entityId = document.getElementById("reportEntity").value;
  const startDate = document.getElementById("startDate").value;
  const endDate = document.getElementById("endDate").value;
  
  if (!entityId) {
    utils.showError("Please select an entity");
    return;
  }
  
  utils.showLoading();
  const resultsDiv = document.getElementById("reportResults");
  const contentDiv = document.getElementById("reportContent");
  const titleEl = document.getElementById("reportTitle");
  
  try {
    let data;
    if (type === "employee") {
      data = await API.Reporting.getEmployeeReport(entityId, startDate, endDate);
      titleEl.textContent = `Performance Report: ${data.UserName}`;
      renderEmployeeReport(data, contentDiv);
    } else {
      data = await API.Reporting.getClientReport(entityId, startDate, endDate);
      titleEl.textContent = `Client Report: ${data.ClientName}`;
      renderClientReport(data, contentDiv);
    }
    resultsDiv.style.display = "block";
  } catch (error) {
    console.error("Error generating report:", error);
    utils.showError("Failed to generate report");
    resultsDiv.style.display = "none";
  } finally {
    utils.hideLoading();
  }
};

function renderEmployeeReport(data, container) {
  container.innerHTML = `
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Completion Rate</span>
          <span class="stat-value">${data.CompletionRate.toFixed(1)}%</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Total Tasks</span>
          <span class="stat-value">${data.TotalTasksAssigned}</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Completed</span>
          <span class="stat-value">${data.CompletedTasks}</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Overdue</span>
          <span class="stat-value" style="color: var(--danger);">${data.OverdueTasks}</span>
        </div>
      </div>
    </div>
    <div style="margin-top: 20px;">
      <p><strong>Total Hours Logged:</strong> ${data.TotalHoursLogged.toFixed(2)} hrs</p>
      <p><strong>Projects Worked On:</strong> ${data.ProjectsWorkedOn}</p>
      <p><strong>Avg Completion Time:</strong> ${data.AverageTaskCompletionDays.toFixed(1)} days</p>
    </div>
  `;
}

function renderClientReport(data, container) {
  container.innerHTML = `
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Active Projects</span>
          <span class="stat-value">${data.ActiveProjects} / ${data.TotalProjects}</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Total Tasks</span>
          <span class="stat-value">${data.TotalTasks}</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Completion</span>
          <span class="stat-value">${data.CompletionPercentage.toFixed(1)}%</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Overdue</span>
          <span class="stat-value" style="color: var(--danger);">${data.OverdueTasks}</span>
        </div>
      </div>
    </div>
    <div style="margin-top: 20px;">
      <p><strong>Company:</strong> ${data.CompanyName}</p>
      <p><strong>Estimated Hours:</strong> ${data.TotalEstimatedHours}</p>
      <p><strong>Actual Hours:</strong> ${data.TotalActualHours}</p>
    </div>
  `;
}
