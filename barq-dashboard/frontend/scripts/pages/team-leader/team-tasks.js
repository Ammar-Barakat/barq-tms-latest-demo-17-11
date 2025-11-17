// Team Leader Team Tasks Script
auth.requireRole([USER_ROLES.TEAM_LEADER]);

let allTasks = [];
let projects = [];
let employees = [];
let currentEditId = null;

document.addEventListener("DOMContentLoaded", async () => {
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
    [allTasks, projects, employees] = await Promise.all([
      API.Tasks.getAll(),
      API.Projects.getAll().catch(() => []),
      API.Users.getAll().catch(() => []),
    ]);

    populateDropdowns();
    // TODO: Filter by team members - for now showing all tasks
    renderTasks(allTasks);
  } catch (error) {
    console.error("Error loading team tasks:", error);
    utils.showError("Failed to load team tasks");
  } finally {
    utils.hideLoading();
  }
}

function populateDropdowns() {
  const projectSelect = document.getElementById("projectId");
  const employeeSelect = document.getElementById("assignedToId");

  projectSelect.innerHTML = '<option value="">Select Project</option>';
  projects.forEach((project) => {
    const option = document.createElement("option");
    option.value = project.ProjectId;
    option.textContent = project.ProjectName;
    projectSelect.appendChild(option);
  });

  employeeSelect.innerHTML = '<option value="">Select Employee</option>';
  employees.forEach((emp) => {
    const option = document.createElement("option");
    option.value = emp.UserId;
    option.textContent = emp.Name || emp.Username;
    employeeSelect.appendChild(option);
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

function showCreateModal() {
  currentEditId = null;
  document.getElementById("modalTitle").textContent = "Create Task";
  document.getElementById("taskForm").reset();
  document.getElementById("taskId").value = "";
  document.getElementById("taskModal").classList.remove("d-none");
}

function closeModal() {
  document.getElementById("taskModal").classList.add("d-none");
  document.getElementById("taskForm").reset();
  currentEditId = null;
}

async function handleSubmit(e) {
  e.preventDefault();

  const formData = {
    Title: document.getElementById("title").value,
    Description: document.getElementById("description").value,
    ProjectId: parseInt(document.getElementById("projectId").value) || null,
    AssignedTo: parseInt(document.getElementById("assignedToId").value) || null,
    StatusId: parseInt(document.getElementById("status").value),
    PriorityId: parseInt(document.getElementById("priority").value),
    DueDate: document.getElementById("dueDate").value || null,
    DeptId: 1,
  };

  try {
    utils.showLoading();

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
    utils.showError("Failed to save task");
  } finally {
    utils.hideLoading();
  }
}
