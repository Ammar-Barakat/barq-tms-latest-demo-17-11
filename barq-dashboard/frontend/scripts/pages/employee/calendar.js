// Employee Calendar Script

// Protect page - require Employee role
auth.requireRole([USER_ROLES.EMPLOYEE]);

// Page state
let currentDate = new Date();
let tasks = [];

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadCalendarData();
  renderCalendar();
  renderUpcomingEvents();
});

// Load calendar data
async function loadCalendarData() {
  try {
    utils.showLoading();

    const allTasks = await API.Tasks.getAll();

    // Filter tasks assigned to current user
    const currentUser = auth.getCurrentUser();
    tasks = allTasks.filter((task) => task.AssignedTo === currentUser.UserId);
  } catch (error) {
    console.error("Error loading calendar:", error);
    utils.showError("Failed to load calendar data");
  } finally {
    utils.hideLoading();
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
  for (let i = 0; i < startingDayOfWeek; i++) {
    const day = daysInPrevMonth - startingDayOfWeek + i + 1;
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
  const totalDayCells = startingDayOfWeek + daysInMonth;
  const remainingCells = Math.ceil(totalDayCells / 7) * 7 - totalDayCells;
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

  // Create scrollable events container
  const eventsContainer = document.createElement("div");
  eventsContainer.className = "calendar-day-events";

  // Add events/tasks for this day
  if (date && !otherMonth) {
    const dayTasks = tasks.filter((task) => {
      if (!task.DueDate) return false;
      const taskDate = new Date(task.DueDate);
      return taskDate.toDateString() === date.toDateString();
    });

    dayTasks.forEach((task) => {
      const eventEl = document.createElement("div");
      eventEl.className = "calendar-event task";
      eventEl.textContent = task.Title;
      eventEl.title = task.Title;
      eventsContainer.appendChild(eventEl);
    });
  }

  dayCell.appendChild(eventsContainer);
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
      if (!task.DueDate) return false;
      const dueDate = new Date(task.DueDate);
      return dueDate >= today && dueDate <= thirtyDaysFromNow;
    })
    .sort((a, b) => new Date(a.DueDate) - new Date(b.DueDate))
    .slice(0, 10);

  if (upcomingTasks.length === 0) {
    container.innerHTML =
      '<p style="color: var(--text-secondary); text-align: center;">No upcoming tasks</p>';
    return;
  }

  container.innerHTML = upcomingTasks
    .map((task) => {
      const dueDate = new Date(task.DueDate);
      const dateStr = dueDate.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      });

      return `
      <div class="upcoming-event-item">
        <div class="upcoming-event-date">${dateStr}</div>
        <div class="upcoming-event-details">
          <div class="upcoming-event-title">${task.Title}</div>
          <div class="upcoming-event-time">${
            task.ProjectName || "No project"
          }</div>
        </div>
        ${utils.getStatusBadge(task.StatusId || 1)}
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
