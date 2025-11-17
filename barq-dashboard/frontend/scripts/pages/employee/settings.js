// Employee Settings Script

// Protect page - require Employee role
auth.requireRole([USER_ROLES.EMPLOYEE]);

// Initialize page
document.addEventListener("DOMContentLoaded", () => {
  loadSettings();
});

// Load saved settings
function loadSettings() {
  // Load notification settings
  const emailNotifications =
    localStorage.getItem("emailNotifications") !== "false";
  const taskReminders = localStorage.getItem("taskReminders") !== "false";
  const assignmentNotifications =
    localStorage.getItem("assignmentNotifications") !== "false";

  document.getElementById("emailNotifications").checked = emailNotifications;
  document.getElementById("taskReminders").checked = taskReminders;
  document.getElementById("assignmentNotifications").checked =
    assignmentNotifications;

  // Load display settings
  const theme = localStorage.getItem("theme") || "light";
  const language = localStorage.getItem("language") || "en";

  document.getElementById("theme").value = theme;
  document.getElementById("language").value = language;
}

// Save notification settings
function saveNotificationSettings() {
  const emailNotifications =
    document.getElementById("emailNotifications").checked;
  const taskReminders = document.getElementById("taskReminders").checked;
  const assignmentNotifications = document.getElementById(
    "assignmentNotifications"
  ).checked;

  localStorage.setItem("emailNotifications", emailNotifications);
  localStorage.setItem("taskReminders", taskReminders);
  localStorage.setItem("assignmentNotifications", assignmentNotifications);

  utils.showSuccess("Notification preferences saved successfully");
}

// Save display settings
function saveDisplaySettings() {
  const theme = document.getElementById("theme").value;
  const language = document.getElementById("language").value;

  localStorage.setItem("theme", theme);
  localStorage.setItem("language", language);

  utils.showSuccess("Display settings saved successfully");

  // Apply theme if needed
  if (theme === "dark") {
    document.body.classList.add("dark-theme");
  } else {
    document.body.classList.remove("dark-theme");
  }
}
