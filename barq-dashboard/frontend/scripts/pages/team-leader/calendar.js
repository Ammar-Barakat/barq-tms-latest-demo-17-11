// Protect page - Manager only
auth.requireRole([USER_ROLES.TEAM_LEADER]);

// Page state
let currentDate = new Date();
let tasks = [];
let events = [];

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadCalendarData();
  renderCalendar();
  renderUpcomingEvents();
  setupEventListeners();
});

// Load data from API
async function loadCalendarData() {
  try {
    showLoading();
    const allTasks = await API.Tasks.getAll();
    const currentUser = auth.getUser();

    // Get team members (employees under this team leader)
    const allUsers = await API.Users.getAll();
    const teamMembers = allUsers.filter((u) => u.role === "EMPLOYEE");
    const teamMemberIds = teamMembers.map((m) => m.id);

    // Filter tasks to include:
    // 1. Tasks assigned to team leader
    // 2. Tasks assigned to team members
    tasks = allTasks
      .filter(
        (task) =>
          task.assignedTo === currentUser.id ||
          teamMemberIds.includes(task.assignedTo)
      )
      .map((task) => {
        // Mark if it's a team member's task
        task.isTeamTask = task.assignedTo !== currentUser.id;
        return task;
      });
  } catch (error) {
    console.error("Error loading calendar data:", error);
    showError("Failed to load calendar data");
  } finally {
    hideLoading();
  }
}

// Render calendar grid
function renderCalendar() {
  const year = currentDate.getFullYear();
  const month = currentDate.getMonth();

  // Update header
  const monthNames = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December",
  ];
  document.getElementById(
    "currentMonthYear"
  ).textContent = `${monthNames[month]} ${year}`;

  // Get first day of month and number of days
  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const daysInMonth = lastDay.getDate();
  const startingDayOfWeek = firstDay.getDay();

  // Get previous month's last days
  const prevMonth = new Date(year, month, 0);
  const daysInPrevMonth = prevMonth.getDate();

  const grid = document.getElementById("calendarGrid");
  grid.innerHTML = "";

  // Day headers
  const dayHeaders = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
  dayHeaders.forEach((day) => {
    const header = document.createElement("div");
    header.className = "calendar-day-header";
    header.textContent = day;
    grid.appendChild(header);
  });

  // Previous month days
  for (let i = startingDayOfWeek - 1; i >= 0; i--) {
    const day = daysInPrevMonth - i;
    const dayCell = createDayCell(day, true);
    grid.appendChild(dayCell);
  }

  // Current month days
  const today = new Date();
  for (let day = 1; day <= daysInMonth; day++) {
    const date = new Date(year, month, day);
    const isToday = date.toDateString() === today.toDateString();
    const dayCell = createDayCell(day, false, isToday, date);
    grid.appendChild(dayCell);
  }

  // Next month days to fill grid
  const totalCells = grid.children.length - 7; // Subtract headers
  const remainingCells = 42 - totalCells; // 6 rows * 7 days
  for (let day = 1; day <= remainingCells; day++) {
    const dayCell = createDayCell(day, true);
    grid.appendChild(dayCell);
  }
}

// Create day cell
function createDayCell(day, otherMonth = false, isToday = false, date = null) {
  const dayCell = document.createElement("div");
  dayCell.className = "calendar-day";
  if (otherMonth) dayCell.classList.add("other-month");
  if (isToday) dayCell.classList.add("today");

  const dayNumber = document.createElement("div");
  dayNumber.className = "calendar-day-number";
  dayNumber.textContent = day;
  dayCell.appendChild(dayNumber);

  // Add events/tasks for this day
  if (date && !otherMonth) {
    const dayTasks = tasks.filter((task) => {
      if (!task.DueDate && !task.dueDate) return false;
      const taskDate = new Date(task.DueDate || task.dueDate);
      return taskDate.toDateString() === date.toDateString();
    });

    dayTasks.forEach((task) => {
      const eventEl = document.createElement("div");
      // Different styling for team tasks vs personal tasks
      eventEl.className = task.isTeamTask
        ? "calendar-event team-task"
        : "calendar-event task";
      eventEl.textContent = task.Title || task.name;
      eventEl.title = `${task.Title || task.name}${
        task.isTeamTask ? " (Team)" : " (Personal)"
      }`;
      eventEl.style.background = task.isTeamTask
        ? "var(--color-info)"
        : "var(--color-primary)";
      dayCell.appendChild(eventEl);
    });
  }

  dayCell.onclick = () => {
    if (date) {
      showCreateEventModal(date);
    }
  };

  return dayCell;
}

// Render upcoming events
function renderUpcomingEvents() {
  const container = document.getElementById("upcomingEvents");

  // Get tasks with due dates in the next 30 days
  const today = new Date();
  const thirtyDaysFromNow = new Date(
    today.getTime() + 30 * 24 * 60 * 60 * 1000
  );

  const upcomingTasks = tasks
    .filter((task) => {
      if (!task.DueDate && !task.dueDate) return false;
      const dueDate = new Date(task.DueDate || task.dueDate);
      return dueDate >= today && dueDate <= thirtyDaysFromNow;
    })
    .sort(
      (a, b) =>
        new Date(a.DueDate || a.dueDate) - new Date(b.DueDate || b.dueDate)
    )
    .slice(0, 10);

  if (upcomingTasks.length === 0) {
    container.innerHTML =
      '<p style="color: var(--text-secondary); text-align: center;">No upcoming tasks</p>';
    return;
  }

  container.innerHTML = upcomingTasks
    .map((task) => {
      const dueDate = new Date(task.DueDate || task.dueDate);
      const dateStr = dueDate.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      });
      const timeStr = dueDate.toLocaleTimeString("en-US", {
        hour: "2-digit",
        minute: "2-digit",
      });

      const taskType = task.isTeamTask
        ? '<i class="fa-solid fa-users"></i> Team'
        : '<i class="fa-solid fa-user"></i> Personal';

      return `
      <div class="upcoming-event-item">
        <div class="upcoming-event-date">${dateStr}</div>
        <div class="upcoming-event-details">
          <div class="upcoming-event-title">
            ${task.Title || task.name}
            <span style="font-size: 12px; color: var(--text-secondary); margin-left: 8px;">${taskType}</span>
          </div>
          <div class="upcoming-event-time">${timeStr} - ${
        task.Description || task.description || "No description"
      }</div>
        </div>
        <span class="badge badge-${getStatusBadgeClass(
          task.StatusId || task.status
        )}">${getStatusText(task.StatusId || task.status)}</span>
      </div>
    `;
    })
    .join("");
}

// Calendar navigation
function previousMonth() {
  currentDate.setMonth(currentDate.getMonth() - 1);
  renderCalendar();
}

function nextMonth() {
  currentDate.setMonth(currentDate.getMonth() + 1);
  renderCalendar();
}

// Modal management
function showCreateEventModal(date = null) {
  document.getElementById("eventModal").classList.remove("d-none");
  document.getElementById("eventForm").reset();

  if (date) {
    const dateStr = date.toISOString().split("T")[0];
    document.getElementById("eventDate").value = dateStr;
  }
}

function closeEventModal() {
  document.getElementById("eventModal").classList.add("d-none");
  document.getElementById("eventForm").reset();
}

// Event handlers
function setupEventListeners() {
  document
    .getElementById("eventForm")
    ?.addEventListener("submit", handleEventSubmit);

  // Mobile menu toggle
  document.querySelector(".menu-toggle")?.addEventListener("click", () => {
    document.querySelector(".sidebar").classList.toggle("show");
  });
}

async function handleEventSubmit(e) {
  e.preventDefault();

  const formData = {
    Title: document.getElementById("eventTitle").value,
    Description: document.getElementById("eventDescription").value,
    DueDate:
      document.getElementById("eventDate").value +
      "T" +
      (document.getElementById("eventTime").value || "00:00"),
    StatusId: 1, // Pending
    PriorityId: 2, // Normal
    AssignedToId: auth.getCurrentUser()?.UserId,
  };

  try {
    showLoading();
    await API.Tasks.create(formData);
    await loadCalendarData();
    renderCalendar();
    renderUpcomingEvents();
    closeEventModal();
    showSuccess("Event added successfully");
  } catch (error) {
    console.error("Error creating event:", error);
    showError("Failed to create event");
  } finally {
    hideLoading();
  }
}

// Helper functions
function getStatusText(status) {
  const statuses = {
    1: "Pending",
    2: "In Progress",
    3: "Completed",
    4: "On Hold",
  };
  return statuses[status] || "Unknown";
}

function getStatusBadgeClass(status) {
  const classes = {
    1: "warning",
    2: "info",
    3: "success",
    4: "secondary",
  };
  return classes[status] || "secondary";
}

function showLoading() {
  document.body.classList.add("loading");
}

function hideLoading() {
  document.body.classList.remove("loading");
}

function showSuccess(message) {
  console.log("✓", message);
  // You can implement toast notifications here
}

function showError(message) {
  console.error("✗", message);
  alert(message);
}
