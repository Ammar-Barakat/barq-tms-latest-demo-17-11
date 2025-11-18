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

    console.log("[Manager] Loaded tasks from API:", tasks.length, "tasks");
    console.log("[Manager] Sample task structure:", tasks[0]);

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
      .map(
        (p) =>
          `<option value="${p.projectId || p.ProjectId}">${
            p.projectName || p.ProjectName || p.Name || "Unnamed"
          }</option>`
      )
      .join("");

  // API returns: UserId, Name for users
  employeeSelect.innerHTML =
    '<option value="">Select Employee</option>' +
    employees
      .map(
        (e) =>
          `<option value="${e.UserId || e.Id}">${
            e.Name || e.name || e.Username || e.username || "Unknown"
          }</option>`
      )
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
    .map((task) => {
      const taskId = task.taskId || task.TaskId || task.Id;
      const statusId = task.statusId || task.StatusId || task.Status || 1;
      const priorityId =
        task.priorityId || task.PriorityId || task.Priority || 1;

      // Check if task needs review
      // StatusId 3 = "In Review" (set by backend when employee requests completion)
      const needsReview = statusId === 3;

      // Debug: Log tasks with "In Review" status
      if (needsReview) {
        console.log("[Tasks] Task needs review (StatusId=3):", task);
      }

      const reviewBadge = needsReview
        ? '<span class="badge badge-warning" style="margin-left: 5px;">Needs Review</span>'
        : "";

      return `
    <tr style="${needsReview ? "border-left: 4px solid #ff9800;" : ""}">
      <td><strong>${
        task.Title || task.title || "Untitled"
      }</strong>${reviewBadge}</td>
      <td>${task.ProjectName || task.projectName || "N/A"}</td>
      <td>${task.AssignedToName || task.assignedToName || "Unassigned"}</td>
      <td>${utils.getStatusBadge(statusId)}</td>
      <td>${utils.getPriorityBadge(priorityId)}</td>
      <td>${utils.formatDate(task.DueDate || task.dueDate)}</td>
      <td>
        <div class="table-actions">
          ${
            needsReview
              ? `
          <button class="btn btn-sm btn-warning" onclick="openReviewModal(${taskId})" title="Review completed task">
            <i class="fa-solid fa-clipboard-check"></i>
          </button>
          `
              : ""
          }
          <button class="btn btn-sm btn-primary" onclick="editTask(${taskId})">
            <i class="fa-solid fa-pen"></i>
          </button>
          <button class="btn btn-sm btn-danger" onclick="deleteTask(${taskId})">
            <i class="fa-solid fa-trash"></i>
          </button>
        </div>
      </td>
    </tr>
  `;
    })
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
  clearFormErrors(document.getElementById("taskForm"));
  document.getElementById("taskId").value = "";
  document.getElementById("taskModal").classList.remove("d-none");
}

function closeModal() {
  document.getElementById("taskModal").classList.add("d-none");
  document.getElementById("taskForm").reset();
  currentEditId = null;
}

async function editTask(id) {
  const task = tasks.find((t) => (t.taskId || t.TaskId || t.Id) == id);
  if (!task) return;
  // Try to fetch full task details from the API to ensure optional fields
  // like Description and Drive links are available (list endpoints may omit them).
  try {
    const full = await API.Tasks.getById(id).catch(() => null);
    if (full) Object.assign(task, full);
  } catch (e) {
    // ignore and continue with available data
    console.warn("Failed to fetch full task details:", e);
  }

  currentEditId = id;
  document.getElementById("modalTitle").textContent = "Edit Task";
  document.getElementById("taskId").value = id;
  // Use detail DTO fields (PascalCase from getById)
  document.getElementById("title").value = task.Title || "";
  document.getElementById("description").value = task.Description || "";
  document.getElementById("projectId").value = task.ProjectId || "";
  document.getElementById("assignedToId").value = task.AssignedTo || "";
  document.getElementById("status").value = task.StatusId || 1;
  document.getElementById("priority").value = task.PriorityId || 1;
  document.getElementById("driveUploadLink").value = task.DriveFolderLink || "";
  document.getElementById("driveMaterialLink").value =
    task.MaterialDriveFolderLink || "";

  if (task.DueDate) {
    const date = new Date(task.DueDate);
    document.getElementById("dueDate").value = date.toISOString().split("T")[0];
  }

  document.getElementById("taskModal").classList.remove("d-none");
}

async function handleSubmit(e) {
  e.preventDefault();
  clearFormErrors(document.getElementById("taskForm"));
  // Build payload matching API `CreateTaskDto` / `UpdateTaskDto` (camelCase)
  const formData = {
    title: document.getElementById("title").value,
    description: document.getElementById("description").value || null,
    projectId: parseInt(document.getElementById("projectId").value) || null,
    assignedTo: parseInt(document.getElementById("assignedToId").value) || null,
    statusId: parseInt(document.getElementById("status").value) || 1,
    priorityId: parseInt(document.getElementById("priority").value) || 1,
    dueDate: document.getElementById("dueDate").value || null,
    driveFolderLink: document.getElementById("driveUploadLink").value || null,
    materialDriveFolderLink:
      document.getElementById("driveMaterialLink").value || null,
    deptId: parseInt(document.getElementById("deptId")?.value) || 1,
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
    // prefer showing server-provided message when available
    let msg = "Failed to save task";
    if (error && error.message) {
      const parts = error.message.split(":");
      msg = parts.length > 1 ? parts.slice(1).join(":").trim() : error.message;
      msg = msg.replace(/^\s*["']|["']\s*$/g, "");
    }
    // If there are field-level errors in the response attempt to show them
    if (typeof tryApplyFieldErrors === "function") {
      tryApplyFieldErrors(error, document.getElementById("taskForm"));
    }
    utils.showError(msg);
  } finally {
    utils.hideLoading();
  }
}

// --- Form error helpers ---
function clearFormErrors(form) {
  if (!form) return;
  form
    .querySelectorAll(".is-invalid")
    .forEach((el) => el.classList.remove("is-invalid"));
  form.querySelectorAll(".invalid-feedback").forEach((el) => el.remove());
}

function applyFieldErrors(form, fieldErrors) {
  if (!form || !fieldErrors) return;
  let firstEl = null;
  Object.keys(fieldErrors).forEach((field) => {
    const msg = Array.isArray(fieldErrors[field])
      ? fieldErrors[field].join(", ")
      : fieldErrors[field];
    // try several id/name variants
    const candidates = [
      field,
      field.charAt(0).toLowerCase() + field.slice(1),
      field.toLowerCase(),
      field + "Id",
      field.replace(/Id$/i, ""),
    ];
    let el = null;
    for (const c of candidates) {
      el = form.querySelector(`#${c}`) || form.querySelector(`[name="${c}"]`);
      if (el) break;
    }
    if (!el) return;
    el.classList.add("is-invalid");
    const feedback = document.createElement("div");
    feedback.className = "invalid-feedback";
    feedback.textContent = msg;
    if (el.parentNode) el.parentNode.appendChild(feedback);
    if (!firstEl) firstEl = el;
  });
  if (firstEl) firstEl.focus();
}

function tryApplyFieldErrors(error, form) {
  try {
    if (!error || !error.message) return false;
    // remove leading HTTP code if present
    let content = error.message.replace(/^HTTP\s*\d+\s*:\s*/i, "").trim();
    // if it's quoted JSON string, strip wrapping quotes
    if (
      (content.startsWith('"') && content.endsWith('"')) ||
      (content.startsWith("'") && content.endsWith("'"))
    ) {
      content = content.slice(1, -1);
    }
    // try parse JSON
    let parsed = null;
    try {
      parsed = JSON.parse(content);
    } catch (e) {
      parsed = null;
    }
    if (parsed) {
      // Common shapes: { errors: { field: [msg] } } or { field: [msg] }
      if (parsed.errors) {
        applyFieldErrors(form, parsed.errors);
        return true;
      }
      applyFieldErrors(form, parsed);
      return true;
    }
    return false;
  } catch (e) {
    return false;
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

// Open review modal
async function openReviewModal(taskId) {
  const task = tasks.find((t) => (t.taskId || t.TaskId || t.Id) == taskId);
  if (!task) return;

  currentEditId = taskId;

  try {
    utils.showLoading();

    // Populate modal with task details (tolerant to casing)
    document.getElementById("reviewTaskTitle").textContent =
      task.title || task.Title || "Untitled";
    document.getElementById("reviewDescription").textContent =
      task.description || task.Description || "No description";
    document.getElementById("reviewAssignee").textContent =
      task.assignedToName ||
      task.AssignedToName ||
      task.AssignedTo ||
      "Unknown";
    document.getElementById("reviewCompletedDate").textContent =
      utils.formatDate(
        task.completedDate ||
          task.CompletedDate ||
          task.CompletedAt ||
          new Date()
      );

    // Show/hide upload link (handle multiple possible property names)
    const uploadLinkGroup = document.getElementById("reviewUploadLinkGroup");
    const uploadHref =
      task.driveFolderLink ||
      task.DriveFolderLink ||
      task.DriveUploadLink ||
      task.driveUploadLink ||
      null;
    if (uploadHref) {
      uploadLinkGroup.style.display = "block";
      document.getElementById("reviewUploadLink").href = uploadHref;
    } else {
      uploadLinkGroup.style.display = "none";
    }

    // Hide employee notes section (not needed per requirements)
    const employeeNotesGroup = document.getElementById(
      "reviewEmployeeNotesGroup"
    );
    if (employeeNotesGroup) {
      employeeNotesGroup.style.display = "none";
    }

    document.getElementById("reviewAction").value = "approve";
    document.getElementById("managerNotes").value = "";
    document.getElementById("newDueDate").value = "";

    // Show/hide notes and due date fields based on action
    toggleReviewFields();

    // Add event listener for action change
    document.getElementById("reviewAction").onchange = toggleReviewFields;

    document.getElementById("reviewModal").classList.remove("d-none");
  } catch (error) {
    console.error("Error loading review modal:", error);
    utils.showError("Failed to load task details for review");
  } finally {
    utils.hideLoading();
  }
}

// Toggle review fields based on action
function toggleReviewFields() {
  const action = document.getElementById("reviewAction").value;
  const notesGroup = document.getElementById("managerNotesGroup");
  const dueDateGroup = document.getElementById("newDueDateGroup");

  if (action === "revise") {
    notesGroup.style.display = "block";
    dueDateGroup.style.display = "block";
  } else {
    notesGroup.style.display = "none";
    dueDateGroup.style.display = "none";
  }
}

// Close review modal
function closeReviewModal() {
  document.getElementById("reviewModal").classList.add("d-none");
  currentEditId = null;
}

// Submit review
async function submitReview() {
  if (!currentEditId) return;

  const action = document.getElementById("reviewAction").value;
  const notes = document.getElementById("managerNotes").value;

  if (action === "revise" && !notes.trim()) {
    utils.showError("Please provide revision notes");
    return;
  }

  try {
    utils.showLoading();

    // Use the new review-completion endpoint
    const newDueDate = document.getElementById("newDueDate").value;

    const reviewData = {
      approve: action === "approve",
      notes: notes || null,
      newDueDate: newDueDate || null,
    };

    console.log("[Manager Review] Submitting review:", {
      taskId: currentEditId,
      action,
      reviewData,
    });
    await API.Tasks.reviewCompletion(currentEditId, reviewData);
    console.log("[Manager Review] Review submitted successfully");

    // Close modal first
    closeReviewModal();

    // Show success message
    utils.showSuccess(
      action === "approve"
        ? "Task approved successfully! Task removed from employee's list."
        : "Revision request sent to employee with notes."
    );

    // Reload tasks to refresh the list and remove review flag
    await loadData();
  } catch (error) {
    console.error("Error submitting review:", error);
    utils.showError("Failed to submit review");
  } finally {
    utils.hideLoading();
  }
}
