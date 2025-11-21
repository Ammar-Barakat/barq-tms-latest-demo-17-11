// Employee My Tasks Script

// Protect page - require Employee role
auth.requireRole([USER_ROLES.EMPLOYEE]);

let myTasks = [];
let currentTaskId = null;

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadMyTasks();
  setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
  document
    .getElementById("statusFilter")
    .addEventListener("change", filterTasks);
}

// Load my tasks
async function loadMyTasks() {
  try {
    utils.showLoading();
    const allTasks = await API.Tasks.getAll();

    // Filter tasks assigned to current user (tolerate PascalCase/camelCase)
    const currentUser = auth.getCurrentUser();
    myTasks = allTasks.filter(
      (task) => (task.AssignedTo || task.assignedTo) == currentUser.UserId
    );

    renderTasks(myTasks);
  } catch (error) {
    console.error("Error loading my tasks:", error);
    utils.showError("Failed to load your tasks");
  } finally {
    utils.hideLoading();
  }
}

// Filter tasks
function filterTasks() {
  const statusFilter = document.getElementById("statusFilter").value;

  let filtered = myTasks;

  if (statusFilter) {
    filtered = filtered.filter((task) => task.StatusId == statusFilter);
  }

  renderTasks(filtered);
}

// Render tasks
function renderTasks(tasks) {
  const tbody = document.getElementById("myTasksBody");

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
        <button class="btn btn-sm btn-primary" onclick="viewTaskDetails(${task.TaskId
        })">
          <i class="fa-solid fa-eye"></i> View Details
        </button>
      </td>
    </tr>
  `
    )
    .join("");
}

// View task details
async function viewTaskDetails(taskId) {
  const task = myTasks.find((t) => (t.TaskId || t.taskId) == taskId);
  if (!task) return;

  currentTaskId = taskId;

  // Populate modal with task details
  document.getElementById("detailsTaskTitle").textContent =
    task.Title || "Task Details";
  document.getElementById("detailsDescription").textContent =
    task.Description || task.description || "No description";
  document.getElementById("detailsProject").textContent =
    task.ProjectName || "N/A";
  document.getElementById("detailsStatus").innerHTML = utils.getStatusBadge(
    task.StatusId || 1
  );
  document.getElementById("detailsPriority").innerHTML = utils.getPriorityBadge(
    task.PriorityId || 1
  );
  document.getElementById("detailsDueDate").textContent = utils.formatDate(
    task.DueDate
  );

  // Clear any previous field-level errors
  clearFormErrors(document.getElementById("taskDetailsForm"));

  // Show/hide Drive links
  const uploadLinkGroup = document.getElementById("uploadLinkGroup");
  const materialLinkGroup = document.getElementById("materialLinkGroup");

  // Show/hide Drive links (support multiple property names returned by API)
  const uploadHref =
    task.DriveFolderLink ||
    task.driveFolderLink ||
    task.DriveUploadLink ||
    task.driveUploadLink ||
    null;
  if (uploadHref) {
    uploadLinkGroup.style.display = "block";
    document.getElementById("detailsUploadLink").href = uploadHref;
  } else {
    uploadLinkGroup.style.display = "none";
  }

  const materialHref =
    task.MaterialDriveFolderLink ||
    task.materialDriveFolderLink ||
    task.DriveMaterialLink ||
    task.driveMaterialLink ||
    null;
  if (materialHref) {
    materialLinkGroup.style.display = "block";
    document.getElementById("detailsMaterialLink").href = materialHref;
  } else {
    materialLinkGroup.style.display = "none";
  }

  // Load and show latest manager review comments
  await loadLatestManagerNotes(taskId);

  // Load and display all comments
  await loadTaskComments(taskId);

  // Show/hide Mark as Done button based on status
  const markDoneBtn = document.getElementById("markDoneBtn");
  const statusVal = task.StatusId || task.statusId || 1;
  if (statusVal === 3) {
    markDoneBtn.style.display = "none";
  } else {
    markDoneBtn.style.display = "inline-block";
  }

  document.getElementById("taskDetailsModal").classList.remove("d-none");
}

// Close details modal
function closeDetailsModal() {
  document.getElementById("taskDetailsModal").classList.add("d-none");
  currentTaskId = null;
}

// --- Form error helpers (same shape used in manager/assistant-manager tasks) ---
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
    let content = error.message.replace(/^HTTP\s*\d+\s*:\s*/i, "").trim();
    if (
      (content.startsWith('"') && content.endsWith('"')) ||
      (content.startsWith("'") && content.endsWith("'"))
    ) {
      content = content.slice(1, -1);
    }
    let parsed = null;
    try {
      parsed = JSON.parse(content);
    } catch (e) {
      parsed = null;
    }
    if (parsed) {
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

// Update task status
// Load latest manager review notes from task comments
async function loadLatestManagerNotes(taskId) {
  const managerNotesGroup = document.getElementById("managerNotesGroup");

  try {
    // Get all comments for this task
    const comments = await API.Tasks.getComments(taskId);

    // Order by createdAt descending and get the top comment
    if (comments && comments.length > 0) {
      const latestComment = comments.sort((a, b) => {
        const dateA = new Date(a.CreatedAt || a.createdAt);
        const dateB = new Date(b.CreatedAt || b.createdAt);
        return dateB - dateA; // Descending order
      })[0];

      if (latestComment) {
        managerNotesGroup.style.display = "block";
        const commentText =
          latestComment.Comment || latestComment.comment || "";
        document.getElementById("detailsManagerNotes").innerHTML =
          commentText.replace(/\n/g, "<br>");
      } else {
        managerNotesGroup.style.display = "none";
      }
    } else {
      managerNotesGroup.style.display = "none";
    }
  } catch (error) {
    console.error("Error loading manager review notes:", error);
    managerNotesGroup.style.display = "none";
  }
}

// Load and display task comments
async function loadTaskComments(taskId) {
  const commentsContainer = document.getElementById("commentsContainer");
  try {
    const comments = await API.Tasks.getComments(taskId);
    if (comments && comments.length > 0) {
      const sortedComments = comments.sort((a, b) => {
        const dateA = new Date(a.CreatedAt || a.createdAt);
        const dateB = new Date(b.CreatedAt || b.createdAt);
        return dateB - dateA;
      });
      commentsContainer.innerHTML = sortedComments
        .map(
          (comment) => `
        <div class="comment-item" style="
          padding: var(--space-3);
          background: var(--bg-secondary);
          border-radius: var(--radius-md);
          margin-bottom: var(--space-3);
          border-left: 3px solid var(--primary-color);
        ">
          <div style="display: flex; justify-content: space-between; margin-bottom: var(--space-2);">
            <strong style="color: var(--primary-color);">
              ${comment.UserName || comment.userName || "User"}
            </strong>
            <small style="color: var(--text-secondary);">
              ${utils.formatDate(comment.CreatedAt || comment.createdAt)}
            </small>
          </div>
          <div style="color: var(--text-primary);">
            ${(comment.Comment || comment.comment || "").replace(/\n/g, "<br>")}
          </div>
        </div>
      `
        )
        .join("");
    } else {
      commentsContainer.innerHTML = `<p style="color: var(--text-secondary); font-style: italic;">No comments yet. Be the first to add an update!</p>`;
    }
  } catch (error) {
    console.error("Error loading comments:", error);
    commentsContainer.innerHTML = `<p style="color: var(--text-secondary);">Unable to load comments.</p>`;
  }
}

// Add a comment to the current task
async function addTaskComment() {
  if (!currentTaskId) {
    utils.showError("No task selected");
    return;
  }
  const commentInput = document.getElementById("taskComment");
  const comment = commentInput.value.trim();
  if (!comment) {
    utils.showError("Please enter a comment");
    return;
  }
  try {
    utils.showLoading();
    await API.Tasks.addComment(currentTaskId, comment);
    utils.showSuccess("Comment added successfully");
    commentInput.value = "";
    await loadTaskComments(currentTaskId);
  } catch (error) {
    console.error("Error adding comment:", error);
    utils.showError("Failed to add comment");
  } finally {
    utils.hideLoading();
  }
}


// Mark task as done and notify manager
async function markTaskAsDone() {
  if (!currentTaskId) return;

  if (
    !utils.confirmAction(
      "Are you sure you want to request completion for this task? The manager will review it."
    )
  ) {
    return;
  }

  try {
    utils.showLoading();

    // Use the new request-complete endpoint
    console.log("[Employee] Requesting completion for task:", currentTaskId);
    await API.Tasks.requestComplete(currentTaskId);

    console.log(
      "[Employee] Completion requested successfully, reloading tasks..."
    );
    utils.showSuccess(
      "Completion request sent! The manager will review your work."
    );

    closeDetailsModal();
    await loadMyTasks();
  } catch (error) {
    console.error("Error requesting task completion:", error);
    utils.showError("Failed to request task completion");
  } finally {
    utils.hideLoading();
  }
}
