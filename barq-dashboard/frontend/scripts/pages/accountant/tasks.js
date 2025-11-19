// Account Manager Tasks Review Page
auth.requireRole([USER_ROLES.ACCOUNTANT]);

let tasks = [];
let currentFilter = "pending-review";
let currentTaskForReview = null;

document.addEventListener("DOMContentLoaded", async () => {
  await loadTasks();
  setupEventListeners();
});

async function loadTasks() {
  try {
    utils.showLoading();

    // Load all tasks
    const allTasks = await API.Tasks.getAll().catch(() => []);

    tasks = allTasks.map((task) => ({
      TaskId: task.taskId || task.TaskId || task.id,
      Title: task.title || task.Title,
      Description: task.description || task.Description,
      ProjectId: task.projectId || task.ProjectId,
      ProjectName: task.projectName || task.ProjectName || "Unknown Project",
      ClientId: task.clientId || task.ClientId,
      ClientName: task.clientName || task.ClientName || "Unknown Client",
      AssignedToId: task.assignedToId || task.AssignedToId,
      AssignedToName:
        task.assignedToName || task.AssignedToName || "Unassigned",
      DueDate: task.dueDate || task.DueDate,
      Status: task.status || task.Status || "Pending",
      Priority: task.priority || task.Priority || "Medium",
      // Review-specific fields
      ReviewStatus: task.reviewStatus || task.ReviewStatus || "Not Submitted",
      SubmittedForReview:
        task.submittedForReview || task.SubmittedForReview || false,
      AccountManagerApproved:
        task.accountManagerApproved || task.AccountManagerApproved || false,
      SentToClient: task.sentToClient || task.SentToClient || false,
      ClientApproved: task.clientApproved || task.ClientApproved || null,
      ClientFeedback: task.clientFeedback || task.ClientFeedback || "",
    }));

    updateStats();
    renderTasks();
  } catch (error) {
    console.error("Error loading tasks:", error);
    utils.showError("Failed to load tasks");
  } finally {
    utils.hideLoading();
  }
}

function updateStats() {
  // Pending account manager review
  const pendingReview = tasks.filter(
    (t) => t.SubmittedForReview && !t.AccountManagerApproved && !t.SentToClient
  ).length;

  // Sent to client (approved by account manager, waiting for client)
  const sentToClient = tasks.filter(
    (t) =>
      t.AccountManagerApproved && t.SentToClient && t.ClientApproved === null
  ).length;

  // Approved by both
  const approved = tasks.filter((t) => t.ClientApproved === true).length;

  // Rejected by client (needs rework)
  const rejected = tasks.filter((t) => t.ClientApproved === false).length;

  document.getElementById("pendingMyReview").textContent = pendingReview;
  document.getElementById("sentToClient").textContent = sentToClient;
  document.getElementById("approvedCount").textContent = approved;
  document.getElementById("rejectedCount").textContent = rejected;

  // Update badges
  document.getElementById("badge-pending-review").textContent = pendingReview;
  document.getElementById("badge-sent-to-client").textContent = sentToClient;
  document.getElementById("badge-client-feedback").textContent = rejected;
}

function filterTasks(filter) {
  currentFilter = filter;

  // Update active tab
  document.querySelectorAll(".tab-btn").forEach((btn) => {
    btn.classList.remove("active");
    if (btn.dataset.filter === filter) {
      btn.classList.add("active");
    }
  });

  // Update table title
  const titles = {
    "pending-review": "Tasks Pending Your Review",
    "sent-to-client": "Tasks Sent to Client",
    "client-feedback": "Tasks with Client Feedback",
    approved: "Approved Tasks",
  };
  document.getElementById("tableTitle").textContent = titles[filter];

  renderTasks();
}

function renderTasks() {
  const tbody = document.getElementById("tasksBody");

  // Filter tasks based on current filter
  let filteredTasks = [];

  switch (currentFilter) {
    case "pending-review":
      filteredTasks = tasks.filter(
        (t) =>
          t.SubmittedForReview && !t.AccountManagerApproved && !t.SentToClient
      );
      break;
    case "sent-to-client":
      filteredTasks = tasks.filter(
        (t) =>
          t.AccountManagerApproved &&
          t.SentToClient &&
          t.ClientApproved === null
      );
      break;
    case "client-feedback":
      filteredTasks = tasks.filter((t) => t.ClientApproved === false);
      break;
    case "approved":
      filteredTasks = tasks.filter((t) => t.ClientApproved === true);
      break;
  }

  if (filteredTasks.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="7" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No Tasks Found</h3>
            <p>There are no tasks in this category.</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = filteredTasks
    .map((task) => {
      const dueDate = task.DueDate
        ? new Date(task.DueDate).toLocaleDateString()
        : "-";

      const statusBadge = getStatusBadge(task);
      const priorityClass = getPriorityClass(task.Priority);

      let actionButtons = "";

      if (currentFilter === "pending-review") {
        actionButtons = `
          <button class="btn btn-sm btn-primary" onclick="reviewTask(${task.TaskId})" title="Review Task">
            <i class="fa-solid fa-clipboard-check"></i> Review
          </button>
        `;
      } else if (currentFilter === "sent-to-client") {
        actionButtons = `
          <button class="btn btn-sm btn-info" onclick="viewTask(${task.TaskId})" title="View Task">
            <i class="fa-solid fa-eye"></i> View
          </button>
        `;
      } else if (currentFilter === "client-feedback") {
        actionButtons = `
          <button class="btn btn-sm btn-warning" onclick="viewClientFeedback(${task.TaskId})" title="View Feedback">
            <i class="fa-solid fa-comment-dots"></i> View Feedback
          </button>
          <button class="btn btn-sm btn-primary" onclick="sendBackToEmployee(${task.TaskId})" title="Send to Employee">
            <i class="fa-solid fa-reply"></i> Send Back
          </button>
        `;
      } else {
        actionButtons = `
          <button class="btn btn-sm btn-success" onclick="viewTask(${task.TaskId})" title="View Task">
            <i class="fa-solid fa-check-circle"></i> Completed
          </button>
        `;
      }

      return `
        <tr>
          <td>
            <strong>${task.Title}</strong>
            <div style="font-size: var(--text-sm); color: var(--text-secondary);">
              <span class="badge ${priorityClass}">${task.Priority}</span>
            </div>
          </td>
          <td>${task.ProjectName}</td>
          <td>${task.ClientName}</td>
          <td>${task.AssignedToName}</td>
          <td>${dueDate}</td>
          <td>${statusBadge}</td>
          <td>
            <div class="table-actions">
              ${actionButtons}
            </div>
          </td>
        </tr>
      `;
    })
    .join("");
}

function getStatusBadge(task) {
  if (task.ClientApproved === true) {
    return '<span class="badge badge-success"><i class="fa-solid fa-check"></i> Client Approved</span>';
  } else if (task.ClientApproved === false) {
    return '<span class="badge badge-danger"><i class="fa-solid fa-times"></i> Client Rejected</span>';
  } else if (task.SentToClient) {
    return '<span class="badge badge-info"><i class="fa-solid fa-paper-plane"></i> With Client</span>';
  } else if (task.AccountManagerApproved) {
    return '<span class="badge badge-success"><i class="fa-solid fa-check"></i> Approved</span>';
  } else if (task.SubmittedForReview) {
    return '<span class="badge badge-warning"><i class="fa-solid fa-clock"></i> Pending Review</span>';
  }
  return '<span class="badge badge-secondary">In Progress</span>';
}

function getPriorityClass(priority) {
  const priorityMap = {
    Low: "badge-info",
    Medium: "badge-warning",
    High: "badge-danger",
    Urgent: "badge-danger",
  };
  return priorityMap[priority] || "badge-secondary";
}

async function reviewTask(taskId) {
  currentTaskForReview = tasks.find((t) => t.TaskId === taskId);
  if (!currentTaskForReview) return;

  // Load task details with comments
  try {
    const taskDetails = await API.Tasks.getById(taskId);
    const comments = await API.Tasks.getComments(taskId).catch(() => []);

    renderTaskDetailsInModal(taskDetails, comments);
    document.getElementById("reviewModal").classList.remove("d-none");
  } catch (error) {
    console.error("Error loading task details:", error);
    utils.showError("Failed to load task details");
  }
}

function renderTaskDetailsInModal(task, comments) {
  const detailsContainer = document.getElementById("taskDetails");

  const dueDate =
    task.dueDate || task.DueDate
      ? new Date(task.dueDate || task.DueDate).toLocaleDateString()
      : "Not set";

  detailsContainer.innerHTML = `
    <div class="details-grid" style="margin-bottom: var(--space-4);">
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-heading"></i> Task Title</label>
        <div class="detail-value">${task.title || task.Title}</div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-calendar"></i> Due Date</label>
        <div class="detail-value">${dueDate}</div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-user"></i> Assigned To</label>
        <div class="detail-value">${
          task.assignedToName || task.AssignedToName || "Unassigned"
        }</div>
      </div>
      <div class="detail-item">
        <label class="detail-label"><i class="fa-solid fa-flag"></i> Priority</label>
        <div class="detail-value"><span class="badge ${getPriorityClass(
          task.priority || task.Priority
        )}">${task.priority || task.Priority}</span></div>
      </div>
    </div>
    <div class="detail-item" style="margin-bottom: var(--space-4);">
      <label class="detail-label"><i class="fa-solid fa-align-left"></i> Description</label>
      <div class="detail-value">${
        task.description || task.Description || "No description"
      }</div>
    </div>
    ${
      comments && comments.length > 0
        ? `
      <div style="margin-top: var(--space-4);">
        <h4 style="margin-bottom: var(--space-3);"><i class="fa-solid fa-comments"></i> Comments</h4>
        <div style="max-height: 200px; overflow-y: auto; display: flex; flex-direction: column; gap: var(--space-2);">
          ${comments
            .map(
              (c) => `
            <div style="padding: var(--space-3); background: var(--surface-secondary); border-radius: var(--radius-md); border-left: 3px solid var(--primary-color);">
              <div style="display: flex; justify-content: space-between; margin-bottom: var(--space-2);">
                <strong>${c.userName || c.UserName || "User"}</strong>
                <span style="font-size: var(--text-sm); color: var(--text-secondary);">${new Date(
                  c.createdAt || c.CreatedAt
                ).toLocaleString()}</span>
              </div>
              <p style="margin: 0;">${c.comment || c.Comment}</p>
            </div>
          `
            )
            .join("")}
        </div>
      </div>
    `
        : ""
    }
  `;
}

function closeReviewModal() {
  document.getElementById("reviewModal").classList.add("d-none");
  document.getElementById("reviewNotes").value = "";
  currentTaskForReview = null;
}

async function approveAndSendToClient() {
  if (!currentTaskForReview) return;

  const notes = document.getElementById("reviewNotes").value;

  try {
    utils.showLoading();

    // Add comment if provided
    if (notes) {
      await API.Tasks.addComment(currentTaskForReview.TaskId, notes);
    }

    // Placeholder: Approve and send to client
    // await API.Tasks.approveForClient(currentTaskForReview.TaskId);

    console.log("Approving task and sending to client:", {
      taskId: currentTaskForReview.TaskId,
      notes,
    });

    // TODO: Send notification to client
    utils.showSuccess(
      `Task approved and sent to ${currentTaskForReview.ClientName}`
    );

    closeReviewModal();
    await loadTasks();
  } catch (error) {
    console.error("Error approving task:", error);
    utils.showError("Failed to approve task");
  } finally {
    utils.hideLoading();
  }
}

async function rejectTask() {
  if (!currentTaskForReview) return;

  const notes = document.getElementById("reviewNotes").value;

  if (!notes) {
    utils.showError("Please provide feedback for rejection");
    return;
  }

  try {
    utils.showLoading();

    // Use existing review completion API with rejection
    await API.Tasks.reviewCompletion(currentTaskForReview.TaskId, {
      Approved: false,
      Comment: notes,
    });

    console.log("Rejecting task and sending back:", {
      taskId: currentTaskForReview.TaskId,
      notes,
    });

    // TODO: Send notification to assigned employee
    utils.showSuccess(
      `Task rejected and sent back to ${currentTaskForReview.AssignedToName}`
    );

    closeReviewModal();
    await loadTasks();
  } catch (error) {
    console.error("Error rejecting task:", error);
    utils.showError("Failed to reject task");
  } finally {
    utils.hideLoading();
  }
}

async function viewClientFeedback(taskId) {
  const task = tasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  // Show client feedback in a modal or alert
  alert(
    `Client Feedback for: ${task.Title}\n\n${
      task.ClientFeedback || "No feedback provided"
    }`
  );
}

async function sendBackToEmployee(taskId) {
  const task = tasks.find((t) => t.TaskId === taskId);
  if (!task) return;

  const feedback = prompt(
    `Send task back to ${task.AssignedToName} with client feedback:\n\n${task.ClientFeedback}\n\nAdd your instructions:`
  );

  if (!feedback) return;

  try {
    utils.showLoading();

    await API.Tasks.addComment(
      taskId,
      `Client Feedback: ${task.ClientFeedback}\n\nAccount Manager: ${feedback}`
    );

    // Placeholder: Reset task for rework
    // await API.Tasks.sendBackForRework(taskId, feedback);

    console.log("Sending task back to employee:", {
      taskId,
      feedback,
    });

    // TODO: Send notification to employee
    utils.showSuccess(`Task sent back to ${task.AssignedToName}`);

    await loadTasks();
  } catch (error) {
    console.error("Error sending task back:", error);
    utils.showError("Failed to send task back");
  } finally {
    utils.hideLoading();
  }
}

function viewTask(taskId) {
  window.open(`../manager/tasks.html?id=${taskId}`, "_blank");
}

function setupEventListeners() {
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
