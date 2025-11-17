// Client Settings Page Script
auth.requireRole([USER_ROLES.CLIENT]);

document.addEventListener("DOMContentLoaded", () => {
  loadUserSettings();
  setupEventListeners();
});

async function loadUserSettings() {
  try {
    const currentUser = auth.getCurrentUser();
    if (!currentUser) return;

    // Fetch full user data from API
    const userData = await API.Users.getById(currentUser.UserId);

    // Populate profile form
    document.getElementById("name").value =
      userData.FirstName && userData.LastName
        ? `${userData.FirstName} ${userData.LastName}`
        : userData.UserName || "";
    document.getElementById("email").value = userData.Email || "";
    document.getElementById("phone").value = userData.Phone || "";

    // Update system info
    document.getElementById("userRole").textContent = getRoleName(
      auth.getUserRole()
    );
  } catch (error) {
    console.error("Error loading settings:", error);
  }
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

  const currentUser = auth.getCurrentUser();
  const formData = {
    Email: document.getElementById("email").value,
    Phone: document.getElementById("phone").value,
  };

  try {
    utils.showLoading();

    await API.Users.update(currentUser.UserId, formData);

    // Update stored user data
    auth.setAuthData(auth.getToken(), { ...currentUser, ...formData });

    utils.showSuccess("Profile updated successfully");
    await loadUserSettings();
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

    const currentUser = auth.getCurrentUser();
    await API.Auth.changePassword({
      userId: currentUser.UserId,
      currentPassword: currentPassword,
      newPassword: newPassword,
    });

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
