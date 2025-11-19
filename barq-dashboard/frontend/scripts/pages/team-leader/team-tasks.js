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
  currentUser = auth.getUser();
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

    // Filter tasks from projects where this team leader is assigned
    const myProjectIds = projects
      .filter((p) => (p.TeamLeaderId || p.teamLeaderId) === currentUser.UserId)
      .map((p) => p.ProjectId || p.projectId);

    const teamTasks = allTasks.filter((t) => {
      const taskProjectId = t.ProjectId || t.projectId;
      return myProjectIds.includes(taskProjectId);
    });

    populateDropdowns();
    renderTasks(teamTasks);
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
        <td colspan="6" class="text-center" style="padding: 40px;">
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

function showAddModal() {
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

  const formData = {
    Title: document.getElementById("title").value,
    Description: document.getElementById("description").value,
    PriorityId: parseInt(document.getElementById("priorityId").value),
    StatusId: parseInt(document.getElementById("statusId").value),
    DueDate: document.getElementById("dueDate").value || null,
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
