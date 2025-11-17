// Login Page Script

// Wait for DOM to be ready
document.addEventListener("DOMContentLoaded", () => {
  // Redirect if already logged in
  if (auth && auth.isAuthenticated()) {
    window.location.href = auth.getDashboardUrl();
    return;
  }

  // Handle form submission
  const loginForm = document.getElementById("loginForm");
  if (loginForm) {
    loginForm.addEventListener("submit", handleLogin);
  }

  // Allow Enter key to submit
  const passwordInput = document.getElementById("password");
  if (passwordInput) {
    passwordInput.addEventListener("keypress", (e) => {
      if (e.key === "Enter") {
        e.preventDefault();
        loginForm.dispatchEvent(new Event("submit"));
      }
    });
  }
});

async function handleLogin(e) {
  e.preventDefault();

  const username = document.getElementById("username").value;
  const password = document.getElementById("password").value;
  const errorMessage = document.getElementById("errorMessage");

  // Hide previous errors
  errorMessage.classList.remove("show");

  // Show loading
  const submitBtn = e.target.querySelector('button[type="submit"]');
  const originalText = submitBtn.innerHTML;
  submitBtn.disabled = true;
  submitBtn.innerHTML =
    '<i class="fa-solid fa-spinner fa-spin"></i> Signing in...';

  try {
    const result = await auth.login(username, password);

    if (result.success) {
      // Show success and redirect
      if (window.utils && window.utils.showSuccess) {
        utils.showSuccess("Login successful! Redirecting...");
      }
      setTimeout(() => {
        window.location.href = result.redirectUrl;
      }, 500);
    } else {
      // Show error
      errorMessage.textContent = result.error || "Invalid username or password";
      errorMessage.classList.add("show");
      submitBtn.disabled = false;
      submitBtn.innerHTML = originalText;
    }
  } catch (error) {
    console.error("Login error:", error);
    errorMessage.textContent = "An error occurred. Please try again.";
    errorMessage.classList.add("show");
    submitBtn.disabled = false;
    submitBtn.innerHTML = originalText;
  }
}
