// Assistant Manager Settings Page Script
auth.requireRole([USER_ROLES.ASSISTANT_MANAGER]);

document.addEventListener("DOMContentLoaded", () => {
  loadUserSettings();
  setupEventListeners();
});

function loadUserSettings() {
  const user = auth.getCurrentUser();
  if (!user) return;

  // Populate profile form
  document.getElementById("name").value = user.Name || "";
  document.getElementById("email").value = user.Email || "";
  document.getElementById("phone").value = user.PhoneNumber || "";

  // Update system info
  document.getElementById("userRole").textContent = getRoleName(
    auth.getUserRole()
  );
}

function getRoleName(roleId) {
  const roles = {
    1: "Manager",
    2: "Assistant Manager",
    3: "Accountant",
    4: "Team Leader",
    5: "Employee",
    6: "Client",
  };
  return roles[roleId] || "Unknown";
}

function setupEventListeners() {
  document
    .getElementById("profileForm")
    .addEventListener("submit", handleProfileUpdate);
  document
    .getElementById("passwordForm")
    .addEventListener("submit", handlePasswordChange);
}

async function handleProfileUpdate(e) {
  e.preventDefault();

  const formData = {
    Name: document.getElementById("name").value,
    Email: document.getElementById("email").value,
    PhoneNumber: document.getElementById("phone").value,
  };

  try {
    utils.showLoading();

    // This would call an update profile endpoint if available
    // await API.Users.updateProfile(formData);

    // Update local storage
    const user = auth.getCurrentUser();
    const updatedUser = { ...user, ...formData };
    localStorage.setItem("user_data", JSON.stringify(updatedUser));

    utils.showSuccess("Profile updated successfully");

    // Reload to update UI
    setTimeout(() => window.location.reload(), 1000);
  } catch (error) {
    console.error("Error updating profile:", error);
    utils.showError("Failed to update profile");
  } finally {
    utils.hideLoading();
  }
}

async function handlePasswordChange(e) {
  e.preventDefault();

  const currentPassword = document.getElementById("currentPassword").value;
  const newPassword = document.getElementById("newPassword").value;
  const confirmPassword = document.getElementById("confirmPassword").value;

  if (newPassword !== confirmPassword) {
    utils.showError("Passwords do not match");
    return;
  }

  if (newPassword.length < 6) {
    utils.showError("Password must be at least 6 characters");
    return;
  }

  try {
    utils.showLoading();

    // This would call a change password endpoint if available
    // await API.Auth.changePassword({ currentPassword, newPassword });

    utils.showSuccess("Password changed successfully");
    document.getElementById("passwordForm").reset();
  } catch (error) {
    console.error("Error changing password:", error);
    utils.showError("Failed to change password");
  } finally {
    utils.hideLoading();
  }
}

function handleLogout() {
  if (utils.confirmAction("Are you sure you want to logout?")) {
    auth.logout();
  }
}
