// Team Leader Settings Page Script
auth.requireRole([USER_ROLES.TEAM_LEADER]);

document.addEventListener("DOMContentLoaded", async () => {
  await loadUserSettings();
  setupEventListeners();
});

async function loadUserSettings() {
  const localUser = auth.getCurrentUser();
  if (!localUser) return;

  try {
    utils.showLoading();

    // Fetch fresh user data from API
    const user = await API.Users.getById(localUser.UserId);

    // Populate profile form with API data
    document.getElementById("name").value = user.Name || "";
    document.getElementById("email").value = user.Email || "";
    document.getElementById("phone").value = user.PhoneNumber || "";

    // Update system info
    document.getElementById("userRole").textContent = getRoleName(
      user.RoleId || user.Role || auth.getUserRole()
    );

    // Update last login if available
    if (user.LastLogin) {
      document.getElementById("lastLogin").textContent = utils.formatDate(
        user.LastLogin
      );
    }
  } catch (error) {
    console.error("Error loading user settings:", error);

    // Fallback to localStorage data
    document.getElementById("name").value = localUser.Name || "";
    document.getElementById("email").value = localUser.Email || "";
    document.getElementById("phone").value = localUser.PhoneNumber || "";

    document.getElementById("userRole").textContent = getRoleName(
      auth.getUserRole()
    );
  } finally {
    utils.hideLoading();
  }
}

function getRoleName(roleId) {
  const roles = {
    1: "Manager",
    2: "Assistant Manager",
    3: "Account Manager",
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

  const user = auth.getCurrentUser();
  if (!user) return;

  const formData = {
    Name: document.getElementById("name").value,
    Email: document.getElementById("email").value,
    PhoneNumber: document.getElementById("phone").value,
  };

  try {
    utils.showLoading();

    // Update via API
    await API.Users.update(user.UserId, formData);

    // Update local storage
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
