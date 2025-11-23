// Protect page - Employee only
auth.requireRole([USER_ROLES.EMPLOYEE]);

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
    // Load calendar events
    const startDate = new Date(
      currentDate.getFullYear(),
      currentDate.getMonth() - 1,
      1
    );
    const endDate = new Date(
      currentDate.getFullYear(),
      currentDate.getMonth() + 2,
      0
    );

    const currentUser = auth.getCurrentUser();

    const [calendarResponse, tasksResponse] = await Promise.all([
      API.Calendar.getEvents({
        StartDate: startDate.toISOString(),
        EndDate: endDate.toISOString(),
      }).catch(() => ({ Events: [] })),
      API.Tasks.getAll().catch(() => []),
    ]);

    // Extract events array from CalendarViewDto response
    // Ideally backend filters events for employee, but we can filter if needed
    events = calendarResponse.Events || calendarResponse.events || [];

    // Add tasks as calendar events (type 3 = task)
    // Filter tasks assigned to current user
    tasks = tasksResponse.filter(task => task.AssignedTo === currentUser.UserId);
    
    const taskEvents = tasks.map((task) => ({
      EventId: `task-${task.TaskId}`,
      Title: task.Title,
      StartDate: task.DueDate,
      EventType: 3,
      Description: task.Description,
      isTask: true,
      taskData: task,
    }));

    events = [...events, ...taskEvents];
  } catch (error) {
    console.error("Error loading calendar data:", error);
    showError("Failed to load calendar data");
    events = [];
    tasks = [];
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

  // Previous month days (fill days before the 1st of current month)
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

  // Next month days to fill grid (6 weeks total = 42 days)
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
    const dayEvents = events.filter((event) => {
      if (!event.StartDate) return false;
      const eventDate = new Date(event.StartDate);
      return eventDate.toDateString() === date.toDateString();
    });

    dayEvents.forEach((event) => {
      const eventEl = document.createElement("div");
      // Map EventType enum to CSS class
      const eventTypeClass =
        {
          1: "meeting",
          2: "deadline",
          3: "task",
          4: "reminder",
        }[event.EventType] || "task";

      eventEl.className = `calendar-event ${eventTypeClass}`;
      eventEl.textContent = event.Title;
      eventEl.title = event.Title;

      // Add click handler to edit event
      eventEl.onclick = (e) => {
        e.stopPropagation(); // Prevent day cell click
        showEditEventModal(event);
      };

      eventsContainer.appendChild(eventEl);
    });
  }

  dayCell.appendChild(eventsContainer);

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

  // Get events in the next 30 days
  const today = new Date();
  const thirtyDaysFromNow = new Date(
    today.getTime() + 30 * 24 * 60 * 60 * 1000
  );

  const upcomingEvents = events
    .filter((event) => {
      if (!event.StartDate) return false;
      const startDate = new Date(event.StartDate);
      return startDate >= today && startDate <= thirtyDaysFromNow;
    })
    .sort((a, b) => new Date(a.StartDate) - new Date(b.StartDate))
    .slice(0, 10);

  if (upcomingEvents.length === 0) {
    container.innerHTML =
      '<p style="color: var(--text-secondary); text-align: center;">No upcoming events</p>';
    return;
  }

  container.innerHTML = upcomingEvents
    .map((event, index) => {
      const startDate = new Date(event.StartDate);
      const dateStr = startDate.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      });
      const timeStr = startDate.toLocaleTimeString("en-US", {
        hour: "2-digit",
        minute: "2-digit",
      });

      // Map EventType enum to label and class
      const eventTypeMap = {
        1: { label: "Meeting", class: "meeting" },
        2: { label: "Deadline", class: "deadline" },
        3: { label: "Task", class: "task" },
        4: { label: "Reminder", class: "reminder" },
      };
      
      const typeInfo = eventTypeMap[event.EventType] || { label: "Event", class: "task" };

      return `
      <div class="upcoming-event-item ${typeInfo.class}" data-event-index="${index}" style="cursor: pointer;">
        <div class="upcoming-event-date">${dateStr}</div>
        <div class="upcoming-event-details">
          <div class="upcoming-event-title">${event.Title}</div>
          <div class="upcoming-event-time">${timeStr} - ${typeInfo.label}</div>
        </div>
      </div>
    `;
    })
    .join("");

  // Add click handlers to upcoming event items
  container.querySelectorAll(".upcoming-event-item").forEach((item, index) => {
    item.addEventListener("click", () => {
      showEditEventModal(upcomingEvents[index]);
    });
  });
}

// Calendar navigation
async function previousMonth() {
  currentDate.setMonth(currentDate.getMonth() - 1);
  await loadCalendarData();
  renderCalendar();
  renderUpcomingEvents();
}

async function nextMonth() {
  currentDate.setMonth(currentDate.getMonth() + 1);
  await loadCalendarData();
  renderCalendar();
  renderUpcomingEvents();
}

// Modal management
function showCreateEventModal(date = null) {
  document.getElementById("eventModal").classList.remove("d-none");
  document.getElementById("eventForm").reset();
  document.getElementById("eventId").value = "";

  // Update modal title
  const modalTitle = document.querySelector("#eventModal .modal-header h3");
  if (modalTitle) modalTitle.textContent = "Add Event";

  // Hide delete button for new events
  const deleteBtn = document.getElementById("deleteEventBtn");
  if (deleteBtn) deleteBtn.style.display = "none";

  if (date) {
    // Format date without timezone conversion to avoid day shift
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const dateStr = `${year}-${month}-${day}`;
    document.getElementById("eventDate").value = dateStr;
  }
}

function showEditEventModal(event) {
  document.getElementById("eventModal").classList.remove("d-none");
  document.getElementById("eventForm").reset();

  // Populate form with event data
  document.getElementById("eventId").value = event.Id;
  document.getElementById("eventTitle").value = event.Title;
  document.getElementById("eventDescription").value = event.Description || "";

  // Parse date and time
  const startDate = new Date(event.StartDate);
  const dateStr = startDate.toISOString().split("T")[0];
  const timeStr = startDate.toTimeString().slice(0, 5);

  document.getElementById("eventDate").value = dateStr;
  document.getElementById("eventTime").value = timeStr;

  // Map EventType enum back to frontend type
  const eventTypeMap = {
    1: "meeting",
    2: "deadline",
    3: "task",
    4: "reminder",
  };
  document.getElementById("eventType").value =
    eventTypeMap[event.EventType] || "task";

  // Update modal title
  const modalTitle = document.querySelector("#eventModal .modal-header h3");
  if (modalTitle) modalTitle.textContent = "Edit Event";

  // Show delete button for existing events
  const deleteBtn = document.getElementById("deleteEventBtn");
  if (deleteBtn) deleteBtn.style.display = "inline-block";
}

function closeEventModal() {
  document.getElementById("eventModal").classList.add("d-none");
  document.getElementById("eventForm").reset();
  document.getElementById("eventId").value = "";
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

  const eventId = document.getElementById("eventId").value;
  const title = document.getElementById("eventTitle").value;
  const description = document.getElementById("eventDescription").value;
  const eventDate = document.getElementById("eventDate").value;
  const eventTime = document.getElementById("eventTime").value || "09:00";
  const eventType = document.getElementById("eventType").value;

  try {
    // Combine date and time
    const startDateTime = `${eventDate}T${eventTime}:00`;

    // Parse the datetime to create end time (1 hour later for meetings/tasks, end of day for deadlines)
    const startDate = new Date(startDateTime);
    const endDate = new Date(startDate);

    if (eventType === "deadline" || eventType === "reminder") {
      // For deadlines and reminders, set end time to end of the day
      endDate.setHours(23, 59, 59, 999);
    } else {
      // For meetings and tasks, add 1 hour
      endDate.setHours(endDate.getHours() + 1);
    }

    // Map frontend event type to backend CalendarEventType enum
    const eventTypeMap = {
      meeting: 1, // Meeting
      deadline: 2, // Deadline
      task: 3, // Task
      reminder: 4, // Reminder
    };

    const currentUser = auth.getCurrentUser();

    const eventData = {
      Title: title,
      Description: description || "",
      StartDate: startDate.toISOString(),
      EndDate: endDate.toISOString(),
      IsAllDay: false,
      Color: getEventTypeColor(eventType),
      EventType: eventTypeMap[eventType] || 3, // Default to Task
      AttendeeUserIds: currentUser?.UserId ? [currentUser.UserId] : [], // Add current user as attendee
      Reminders: [],
    };

    if (eventId) {
      await API.Calendar.updateEvent(eventId, eventData);
      showSuccess("Event updated successfully");
    } else {
      await API.Calendar.createEvent(eventData);
      showSuccess("Event created successfully");
    }

    closeEventModal();
    await loadCalendarData();
    renderCalendar();
    renderUpcomingEvents();
  } catch (error) {
    console.error("Error saving event:", error);
    showError(error.message || "Failed to save event");
  }
}

// Helper function to get color based on event type
function getEventTypeColor(type) {
  const colorMap = {
    meeting: "#2ecc71", // Emerald Green
    deadline: "#e74c3c", // Alizarin Red
    task: "#9b59b6", // Amethyst Purple
    reminder: "#f1c40f", // Sunflower Yellow
  };
  return colorMap[type] || "#9b59b6"; // Default Purple
}

// Delete event function
async function deleteEvent() {
  const eventId = document.getElementById("eventId").value;

  if (!eventId) {
    showError("No event selected");
    return;
  }

  if (!confirm("Are you sure you want to delete this event?")) {
    return;
  }

  try {
    await API.Calendar.deleteEvent(eventId);
    showSuccess("Event deleted successfully");
    closeEventModal();
    await loadCalendarData();
    renderCalendar();
    renderUpcomingEvents();
  } catch (error) {
    console.error("Error deleting event:", error);
    showError(error.message || "Failed to delete event");
  }
}

// Helper functions
function getStatusText(status) {
  const statuses = {
    1: "To Do",
    2: "In Progress",
    3: "In Review",
    4: "Completed",
    5: "Cancelled",
  };
  return statuses[status] || "To Do";
}

function getStatusBadgeClass(status) {
  const classes = {
    1: "secondary",
    2: "info",
    3: "warning",
    4: "success",
    5: "danger",
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
