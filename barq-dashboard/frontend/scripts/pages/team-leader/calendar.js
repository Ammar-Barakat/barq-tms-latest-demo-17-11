// Team Leader Calendar Page Script
auth.requireRole([USER_ROLES.TEAM_LEADER]);

// State
let currentDate = new Date();
let events = [];
let tasks = [];
let teamMembers = [];
let currentUser = null;
let currentEditId = null;

// Constants
const MONTH_NAMES = [
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

// Event Types mapping
const EVENT_TYPES = {
  meeting: { id: 1, label: "Meeting", class: "meeting" },
  deadline: { id: 2, label: "Deadline", class: "deadline" },
  task: { id: 3, label: "Task", class: "task" },
  reminder: { id: 4, label: "Reminder", class: "reminder" },
  teamTask: { id: 5, label: "Team Task", class: "team-task" }, // Custom type for display
};

// Initialization
document.addEventListener("DOMContentLoaded", async () => {
  currentUser = auth.getCurrentUser();
  setupEventListeners();
  await loadCalendarData();
});

function setupEventListeners() {
  // Modal form submit
  document
    .getElementById("eventForm")
    .addEventListener("submit", handleEventSubmit);
}

// Load Data
async function loadCalendarData() {
  try {
    utils.showLoading();

    // Calculate date range for the current view (month)
    const year = currentDate.getFullYear();
    const month = currentDate.getMonth() + 1; // 1-based

    // Fetch events, tasks, and users (to identify team members)
    const [eventsResponse, tasksResponse, usersResponse] = await Promise.all([
      API.Calendar.getEvents({
        StartDate: new Date(year, month - 2, 1).toISOString(), // Previous month
        EndDate: new Date(year, month + 1, 0).toISOString(), // Next month
      }).catch((err) => {
        console.error("Error fetching events:", err);
        return { Events: [] };
      }),
      API.Tasks.getAll().catch((err) => {
        console.error("Error fetching tasks:", err);
        return [];
      }),
      API.Users.getAll().catch((err) => {
        console.error("Error fetching users:", err);
        return [];
      }),
    ]);

    // Identify team members
    // Team Leader ID matches current user ID
    teamMembers = usersResponse.filter((u) => {
      const roleId = u.Role || u.RoleId;
      const teamLeaderId = u.TeamLeaderId || u.teamLeaderId;
      // Role 5 is Employee (usually), check if they report to this TL
      return teamLeaderId === currentUser.UserId;
    });
    const teamMemberIds = teamMembers.map((u) => u.UserId);

    events = eventsResponse.Events || eventsResponse.events || [];

    // Filter tasks
    const myTasks = tasksResponse.filter(
      (t) => t.AssignedTo === currentUser.UserId
    );
    const teamTasks = tasksResponse.filter((t) =>
      teamMemberIds.includes(t.AssignedTo)
    );

    // Combine for display
    // We'll store them separately in state if needed, but for rendering we merge
    // Mark team tasks with a special property to style them differently
    const formattedTeamTasks = teamTasks.map((t) => ({
      ...t,
      isTeamTask: true,
    }));

    // We want to show My Tasks AND Team Tasks
    // Avoid duplicates if I assign a task to myself (it would be in myTasks and potentially teamTasks if I am my own leader? Unlikely but possible)
    // Set logic:
    const allRelevantTasks = [...myTasks];
    formattedTeamTasks.forEach((t) => {
      if (!allRelevantTasks.find((at) => at.TaskId === t.TaskId)) {
        allRelevantTasks.push(t);
      }
    });

    tasks = allRelevantTasks;

    renderCalendar();
    renderUpcomingEvents();
  } catch (error) {
    console.error("Error loading calendar data:", error);
    utils.showError("Failed to load calendar data");
  } finally {
    utils.hideLoading();
  }
}

// Render Calendar
function renderCalendar() {
  const year = currentDate.getFullYear();
  const month = currentDate.getMonth();

  // Update Header
  document.getElementById("currentMonthYear").textContent = `${
    MONTH_NAMES[month]
  } ${year}`;

  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const startingDay = firstDay.getDay(); // 0 = Sunday
  const totalDays = lastDay.getDate();

  const grid = document.getElementById("calendarGrid");
  grid.innerHTML = "";

  // Add Day Headers
  const days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
  days.forEach((day) => {
    const header = document.createElement("div");
    header.className = "calendar-day-header";
    header.textContent = day;
    grid.appendChild(header);
  });

  // Previous Month Days
  const prevMonthLastDay = new Date(year, month, 0).getDate();
  for (let i = startingDay - 1; i >= 0; i--) {
    const dayDiv = createDayElement(prevMonthLastDay - i, true);
    grid.appendChild(dayDiv);
  }

  // Current Month Days
  for (let i = 1; i <= totalDays; i++) {
    const dayDiv = createDayElement(i, false);
    grid.appendChild(dayDiv);
  }

  // Next Month Days
  const remainingCells = 42 - (startingDay + totalDays);
  for (let i = 1; i <= remainingCells; i++) {
    const dayDiv = createDayElement(i, true);
    grid.appendChild(dayDiv);
  }
}

function createDayElement(day, isOtherMonth) {
  const div = document.createElement("div");
  div.className = `calendar-day ${isOtherMonth ? "other-month" : ""}`;

  // Check if today
  const today = new Date();
  if (
    !isOtherMonth &&
    day === today.getDate() &&
    currentDate.getMonth() === today.getMonth() &&
    currentDate.getFullYear() === today.getFullYear()
  ) {
    div.classList.add("today");
  }

  // Day Number
  const number = document.createElement("div");
  number.className = "calendar-day-number";
  number.textContent = day;
  div.appendChild(number);

  // Events Container
  const eventsContainer = document.createElement("div");
  eventsContainer.className = "calendar-day-events";

  // Filter events for this day
  const currentMonth = isOtherMonth
    ? day > 15
      ? currentDate.getMonth() - 1
      : currentDate.getMonth() + 1
    : currentDate.getMonth();
  
  // Handle year change for other months
  let currentYear = currentDate.getFullYear();
  if (isOtherMonth) {
      if (day > 15 && currentDate.getMonth() === 0) currentYear--;
      if (day <= 15 && currentDate.getMonth() === 11) currentYear++;
  }

  const dateStr = new Date(currentYear, currentMonth, day).toDateString();

  // Add Calendar Events
  const dayEvents = events.filter((e) => {
    const eDate = new Date(e.StartDate || e.startDate);
    return eDate.toDateString() === dateStr;
  });

  // Add Tasks (Deadlines)
  const dayTasks = tasks.filter((t) => {
    if (!t.DueDate) return false;
    const tDate = new Date(t.DueDate);
    return tDate.toDateString() === dateStr;
  });

  // Render Events
  dayEvents.forEach((e) => {
    const el = document.createElement("div");
    // Map TypeId to class
    let typeClass = "meeting";
    if (e.EventType === 2) typeClass = "deadline";
    if (e.EventType === 3) typeClass = "task";
    if (e.EventType === 4) typeClass = "reminder";

    el.className = `calendar-event ${typeClass}`;
    el.textContent = e.Title;
    el.onclick = (evt) => {
      evt.stopPropagation();
      openEventModal(e);
    };
    eventsContainer.appendChild(el);
  });

  // Render Tasks
  dayTasks.forEach((t) => {
    const el = document.createElement("div");
    // Use 'team-task' class if it's a team task, otherwise 'task'
    const typeClass = t.isTeamTask ? "team-task" : "task";
    
    el.className = `calendar-event ${typeClass}`;
    el.textContent = t.Title;
    // Tasks are read-only in calendar view for now, or could open task details
    // For now, let's just show them. If we want to edit, we'd need a task modal.
    // Let's make them clickable to view basic info or just prevent click if it's not an event
    el.title = `Task: ${t.Title} (Due Today)`;
    eventsContainer.appendChild(el);
  });

  div.appendChild(eventsContainer);

  // Click to add event (only on current month days)
  if (!isOtherMonth) {
    div.onclick = () => {
      const selectedDate = new Date(
        currentDate.getFullYear(),
        currentDate.getMonth(),
        day
      );
      // Adjust for timezone offset to ensure the date input gets the correct YYYY-MM-DD
      const offset = selectedDate.getTimezoneOffset();
      const adjustedDate = new Date(selectedDate.getTime() - (offset*60*1000));
      
      openEventModal(null, adjustedDate.toISOString().split("T")[0]);
    };
  }

  return div;
}

// Render Upcoming Events (Sidebar)
function renderUpcomingEvents() {
  const container = document.getElementById("upcomingEvents");
  container.innerHTML = "";

  // Combine and sort all items by date
  const allItems = [
    ...events.map((e) => ({
      ...e,
      date: new Date(e.StartDate || e.startDate),
      type: "event",
    })),
    ...tasks.map((t) => ({
      ...t,
      date: new Date(t.DueDate),
      type: "task",
    })),
  ];

  // Filter for future items only
  const now = new Date();
  now.setHours(0, 0, 0, 0);
  
  const futureItems = allItems
    .filter((item) => item.date >= now)
    .sort((a, b) => a.date - b.date)
    .slice(0, 5); // Show top 5

  if (futureItems.length === 0) {
    container.innerHTML =
      '<p class="text-muted text-center">No upcoming events</p>';
    return;
  }

  futureItems.forEach((item) => {
    const div = document.createElement("div");
    
    let typeClass = "meeting";
    if (item.type === "event") {
        if (item.EventType === 2) typeClass = "deadline";
        if (item.EventType === 3) typeClass = "task";
        if (item.EventType === 4) typeClass = "reminder";
    } else {
        typeClass = item.isTeamTask ? "team-task" : "task";
    }

    div.className = `upcoming-event-item ${typeClass}`;
    
    // Format date: "Nov 24"
    const dateStr = item.date.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric"
    });

    div.innerHTML = `
      <div class="upcoming-event-date">${dateStr}</div>
      <div class="upcoming-event-details">
        <div class="upcoming-event-title">${item.Title}</div>
        <div class="upcoming-event-time">${
          item.type === "event" ? "Event" : "Task Due"
        }</div>
      </div>
    `;
    
    if (item.type === "event") {
        div.onclick = () => openEventModal(item);
    }
    
    container.appendChild(div);
  });
}

// Navigation
function previousMonth() {
  currentDate.setMonth(currentDate.getMonth() - 1);
  loadCalendarData(); // Reload to get new month's data
}

function nextMonth() {
  currentDate.setMonth(currentDate.getMonth() + 1);
  loadCalendarData();
}

// Modal Functions
function showCreateEventModal() {
  openEventModal();
}

function openEventModal(event = null, dateStr = null) {
  const modal = document.getElementById("eventModal");
  const form = document.getElementById("eventForm");
  const deleteBtn = document.getElementById("deleteEventBtn");

  if (event) {
    // Edit Mode
    currentEditId = event.Id || event.id;
    document.getElementById("eventId").value = currentEditId;
    document.getElementById("eventTitle").value = event.Title;
    document.getElementById("eventDescription").value = event.Description || "";
    
    // Format date for input
    const d = new Date(event.StartDate || event.startDate);
    document.getElementById("eventDate").value = d.toISOString().split("T")[0];
    document.getElementById("eventTime").value = d.toTimeString().slice(0, 5);
    
    // Set Type
    const typeSelect = document.getElementById("eventType");
    // Map ID back to value
    if (event.EventType === 1) typeSelect.value = "meeting";
    if (event.EventType === 2) typeSelect.value = "deadline";
    if (event.EventType === 3) typeSelect.value = "task";
    if (event.EventType === 4) typeSelect.value = "reminder";

    deleteBtn.style.display = "block";
  } else {
    // Create Mode
    currentEditId = null;
    form.reset();
    if (dateStr) {
      document.getElementById("eventDate").value = dateStr;
    } else {
      document.getElementById("eventDate").value = new Date().toISOString().split("T")[0];
    }
    document.getElementById("eventTime").value = "09:00";
    deleteBtn.style.display = "none";
  }

  modal.classList.remove("d-none");
}

function closeEventModal() {
  document.getElementById("eventModal").classList.add("d-none");
  currentEditId = null;
}

async function handleEventSubmit(e) {
  e.preventDefault();

  const title = document.getElementById("eventTitle").value;
  const description = document.getElementById("eventDescription").value;
  const dateVal = document.getElementById("eventDate").value;
  const timeVal = document.getElementById("eventTime").value || "09:00";
  const typeVal = document.getElementById("eventType").value;

  // Combine date and time
  const startDateTime = `${dateVal}T${timeVal}:00`;
  const startDate = new Date(startDateTime);
  const endDate = new Date(startDate);
  
  // Set duration based on type
  if (typeVal === "deadline" || typeVal === "reminder") {
      endDate.setHours(23, 59, 59, 999);
  } else {
      endDate.setHours(endDate.getHours() + 1);
  }

  // Map type value to ID
  let typeId = 1;
  if (typeVal === "deadline") typeId = 2;
  if (typeVal === "task") typeId = 3;
  if (typeVal === "reminder") typeId = 4;

  // Color mapping
  const colorMap = {
    1: "#3b82f6", // Meeting - Blue
    2: "#ef4444", // Deadline - Red
    3: "#f59e0b", // Task - Orange
    4: "#8b5cf6", // Reminder - Purple
  };

  const eventData = {
    Title: title,
    Description: description,
    StartDate: startDate.toISOString(),
    EndDate: endDate.toISOString(),
    IsAllDay: false,
    Color: colorMap[typeId] || "#3b82f6",
    EventType: typeId,
    AttendeeUserIds: currentUser?.UserId ? [currentUser.UserId] : []
  };

  try {
    utils.showLoading();
    if (currentEditId) {
      await API.Calendar.updateEvent(currentEditId, eventData);
      utils.showSuccess("Event updated successfully");
    } else {
      await API.Calendar.createEvent(eventData);
      utils.showSuccess("Event created successfully");
    }
    closeEventModal();
    await loadCalendarData();
  } catch (error) {
    console.error("Error saving event:", error);
    utils.showError("Failed to save event: " + (error.message || "Unknown error"));
  } finally {
    utils.hideLoading();
  }
}

async function deleteEvent() {
  if (!currentEditId) return;

  if (!confirm("Are you sure you want to delete this event?")) return;

  try {
    utils.showLoading();
    await API.Calendar.deleteEvent(currentEditId);
    utils.showSuccess("Event deleted successfully");
    closeEventModal();
    await loadCalendarData();
  } catch (error) {
    console.error("Error deleting event:", error);
    utils.showError("Failed to delete event");
  } finally {
    utils.hideLoading();
  }
}