// Manager Tasks Page Script
auth.requireRole([USER_ROLES.MANAGER]);

let tasks = [];
let projects = [];
let employees = [];
let currentEditId = null;

document.addEventListener("DOMContentLoaded", async () => {
  await loadData();
  setupEventListeners();
});

async function loadData() {
  try {
    utils.showLoading();

    [tasks, projects, employees] = await Promise.all([
      API.Tasks.getAll().catch(() => []),
      API.Projects.getAll().catch(() => []),
      API.Employees.getAll().catch(() => []),
    ]);

    populateDropdowns();
    renderTasks();
  } catch (error) {
    console.error("Error loading data:", error);
    utils.showError("Failed to load tasks");
  } finally {
    utils.hideLoading();
  }
}

function populateDropdowns() {
  const projectSelect = document.getElementById("projectId");
  const employeeSelect = document.getElementById("assignedToId");

  // API returns: ProjectId, ProjectName for projects
  projectSelect.innerHTML =
    '<option value="">Select Project</option>' +
    projects
      .map((p) => `<option value="${p.ProjectId}">${p.ProjectName}</option>`)
      .join("");

  // API returns: UserId, Name for users
  employeeSelect.innerHTML =
    '<option value="">Select Employee</option>' +
    employees
      .map((e) => `<option value="${e.UserId}">${e.Name}</option>`)
      .join("");
}

function renderTasks() {
  const tbody = document.getElementById("tasksBody");

  if (tasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="8" class="text-center" style="padding: 40px;">
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
      <td><strong>${task.Title || "Untitled"}</strong></td>
      <td>${utils.truncateText(task.Description || "No description", 50)}</td>
      <td>${task.ProjectName || "N/A"}</td>
      <td>${task.AssignedToName || "Unassigned"}</td>
      <td>${utils.getStatusBadge(task.StatusId || 1)}</td>
      <td>${utils.getPriorityBadge(task.PriorityId || 1)}</td>
      <td>${utils.formatDate(task.DueDate)}</td>
      <td>
        <div class="table-actions">
          <button class="btn btn-sm btn-primary" onclick="editTask(${
            task.TaskId
          })">
            <i class="fa-solid fa-pen"></i>
          </button>
          <button class="btn btn-sm btn-danger" onclick="deleteTask(${
            task.TaskId
          })">
            <i class="fa-solid fa-trash"></i>
          </button>
        </div>
      </td>
    </tr>
  `
    )
    .join("");
}

function setupEventListeners() {
  document.getElementById("taskForm").addEventListener("submit", handleSubmit);
  document
    .getElementById("searchInput")
    .addEventListener("input", handleSearch);
}

function handleSearch(e) {
  const searchTerm = e.target.value.toLowerCase();
  const rows = document.querySelectorAll("#tasksBody tr");

  rows.forEach((row) => {
    const text = row.textContent.toLowerCase();
    row.style.display = text.includes(searchTerm) ? "" : "none";
  });
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

async function editTask(id) {
  const task = tasks.find((t) => t.TaskId === id);
  if (!task) return;

  currentEditId = id;
  document.getElementById("modalTitle").textContent = "Edit Task";
  document.getElementById("taskId").value = id;
  document.getElementById("title").value = task.Title || "";
  document.getElementById("description").value = task.Description || "";
  document.getElementById("projectId").value = task.ProjectId || "";
  document.getElementById("assignedToId").value = task.AssignedTo || "";
  document.getElementById("status").value = task.StatusId || 1;
  document.getElementById("priority").value = task.PriorityId || 1;
  document.getElementById("driveUploadLink").value = task.DriveUploadLink || "";
  document.getElementById("driveMaterialLink").value =
    task.DriveMaterialLink || "";

  if (task.DueDate) {
    const date = new Date(task.DueDate);
    document.getElementById("dueDate").value = date.toISOString().split("T")[0];
  }

  document.getElementById("taskModal").classList.remove("d-none");
}

async function handleSubmit(e) {
  e.preventDefault();

  // API expects PascalCase for request body
  const formData = {
    Title: document.getElementById("title").value,
    Description: document.getElementById("description").value,
    ProjectId: parseInt(document.getElementById("projectId").value),
    AssignedTo: parseInt(document.getElementById("assignedToId").value) || null,
    StatusId: parseInt(document.getElementById("status").value),
    PriorityId: parseInt(document.getElementById("priority").value),
    DueDate: document.getElementById("dueDate").value || null,
    DriveUploadLink: document.getElementById("driveUploadLink").value || null,
    DriveMaterialLink:
      document.getElementById("driveMaterialLink").value || null,
    DeptId: 1, // Default department - should be selected from form
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
    await loadData();
  } catch (error) {
    console.error("Error saving task:", error);
    utils.showError("Failed to save task");
  } finally {
    utils.hideLoading();
  }
}

async function deleteTask(id) {
  if (!utils.confirmAction("Are you sure you want to delete this task?"))
    return;

  try {
    utils.showLoading();
    await API.Tasks.delete(id);
    utils.showSuccess("Task deleted successfully");
    await loadData();
  } catch (error) {
    console.error("Error deleting task:", error);
    utils.showError("Failed to delete task");
  } finally {
    utils.hideLoading();
  }
}
