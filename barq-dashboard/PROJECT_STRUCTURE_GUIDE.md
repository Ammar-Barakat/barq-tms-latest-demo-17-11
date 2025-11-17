# Barq TMS - Project Structure Extraction Guide

## 🎯 Purpose

This guide extracts the core HTML structure, CSS styling patterns, and JavaScript architecture from the current Barq TMS project to help you rebuild it from scratch efficiently.

---

## 📁 1. PROJECT ARCHITECTURE

### Folder Structure

```
frontend/
├── pages/                          # HTML pages organized by role
│   ├── auth/
│   │   ├── login.html
│   │   └── debug.html
│   │
│   ├── manager/                    # Manager Role (Role ID: 1)
│   │   ├── dashboard.html
│   │   ├── tasks.html
│   │   ├── projects.html
│   │   ├── employees.html
│   │   ├── clients.html
│   │   ├── calendar.html
│   │   ├── analytics.html
│   │   └── settings.html
│   │
│   ├── assistant-manager/          # Assistant Manager Role (Role ID: 2)
│   │   ├── dashboard.html
│   │   ├── tasks.html
│   │   ├── projects.html
│   │   ├── employees.html
│   │   ├── clients.html
│   │   ├── calendar.html
│   │   ├── analytics.html
│   │   └── settings.html
│   │
│   ├── accountant/                 # Accountant Role (Role ID: 3)
│   │   ├── dashboard.html
│   │   ├── projects.html
│   │   ├── clients.html
│   │   ├── invoices.html
│   │   ├── expenses.html
│   │   ├── financial-reports.html
│   │   └── settings.html
│   │
│   ├── team-leader/                # Team Leader Role (Role ID: 4)
│   │   ├── dashboard.html
│   │   ├── tasks.html
│   │   ├── team-tasks.html
│   │   ├── projects.html
│   │   ├── my-team.html
│   │   ├── team-members.html
│   │   ├── calendar.html
│   │   ├── analytics.html
│   │   └── settings.html
│   │
│   ├── employee/                   # Employee Role (Role ID: 5)
│   │   ├── dashboard.html
│   │   ├── tasks.html
│   │   ├── my-tasks.html
│   │   ├── projects.html
│   │   ├── calendar.html
│   │   ├── profile.html
│   │   └── settings.html
│   │
│   └── client/                     # Client Role (Role ID: 6)
│       ├── dashboard.html
│       ├── projects.html
│       ├── project-details.html
│       └── settings.html
│
├── styles/
│   ├── main.css                    # Main import file
│   ├── utils/
│   │   ├── variables.css           # CSS Variables/Design tokens
│   │   └── utilities.css           # Utility classes
│   ├── base.css                    # Reset & base styles
│   ├── layout.css                  # Sidebar, header, main layout
│   ├── components.css              # Buttons, cards, modals, etc.
│   ├── animations.css              # Animations & transitions
│   ├── loading-states.css          # Loading indicators
│   ├── notifications.css           # Toast notifications
│   └── pages/                      # Role-specific page styles
│       ├── manager/
│       ├── assistant-manager/
│       ├── accountant/
│       ├── team-leader/
│       ├── employee/
│       ├── client/
│       └── auth/
│           └── login.css
│
├── scripts/
│   ├── utils/
│   │   ├── api.js                  # API client & backend integration
│   │   ├── auth.js                 # Authentication & authorization
│   │   ├── components.js           # Reusable UI components
│   │   ├── modals.js               # Modal management
│   │   ├── utils.js                # Helper functions
│   │   ├── icons.js                # Icon mappings
│   │   ├── notifications.js        # Toast notifications
│   │   ├── charts-manager.js       # Chart utilities (if using charts)
│   │   ├── export-manager.js       # Export to CSV/PDF
│   │   └── file-upload.js          # File upload handling
│   │
│   ├── pages/                      # Page-specific scripts
│   │   ├── manager/
│   │   │   ├── dashboard.js
│   │   │   ├── tasks.js
│   │   │   ├── projects.js
│   │   │   ├── employees.js
│   │   │   ├── clients.js
│   │   │   ├── analytics.js
│   │   │   └── settings.js
│   │   │
│   │   ├── assistant-manager/
│   │   │   ├── dashboard.js
│   │   │   ├── tasks.js
│   │   │   ├── projects.js
│   │   │   ├── employees.js
│   │   │   ├── clients.js
│   │   │   ├── analytics.js
│   │   │   └── settings.js
│   │   │
│   │   ├── accountant/
│   │   │   ├── dashboard.js
│   │   │   ├── projects.js
│   │   │   ├── clients.js
│   │   │   ├── invoices.js
│   │   │   ├── expenses.js
│   │   │   ├── financial-reports.js
│   │   │   └── settings.js
│   │   │
│   │   ├── team-leader/
│   │   │   ├── dashboard.js
│   │   │   ├── tasks.js
│   │   │   ├── team-tasks.js
│   │   │   ├── projects.js
│   │   │   ├── my-team.js
│   │   │   ├── analytics.js
│   │   │   └── settings.js
│   │   │
│   │   ├── employee/
│   │   │   ├── dashboard.js
│   │   │   ├── tasks.js
│   │   │   ├── my-tasks.js
│   │   │   ├── projects.js
│   │   │   ├── profile.js
│   │   │   └── settings.js
│   │   │
│   │   ├── client/
│   │   │   ├── dashboard.js
│   │   │   ├── projects.js
│   │   │   ├── project-details.js
│   │   │   └── settings.js
│   │   │
│   │   └── auth/
│   │       └── login.js
│   │
│   └── data/                       # i18n translations (if applicable)
│       ├── en.json
│       └── ar.json
│
└── media/
    ├── logo.png
    └── Icon.png
```

---

## 🎨 2. HTML STRUCTURE TEMPLATE

### Standard Page Template

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Page Title - Barq TMS</title>

    <!-- Stylesheets -->
    <link rel="stylesheet" href="../../styles/main.css" />
    <link
      rel="stylesheet"
      href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css"
    />
    <link rel="stylesheet" href="../../styles/pages/[role]/[page].css" />
  </head>
  <body>
    <div class="app">
      <!-- SIDEBAR -->
      <aside class="sidebar">
        <!-- Sidebar Header -->
        <div class="sidebar-header">
          <div class="sidebar-logo">
            <img src="../../media/logo.png" alt="Barq" />
          </div>
          <div class="sidebar-brand">
            <h1>Barq TMS</h1>
            <p>Task Management</p>
          </div>
        </div>

        <!-- Sidebar Navigation -->
        <nav class="sidebar-nav">
          <div class="nav-section">
            <a href="dashboard.html" class="nav-item active">
              <span class="nav-item-icon"
                ><i class="fa-solid fa-gauge"></i
              ></span>
              <span class="nav-item-text">Dashboard</span>
            </a>
            <a href="tasks.html" class="nav-item">
              <span class="nav-item-icon"
                ><i class="fa-solid fa-list-check"></i
              ></span>
              <span class="nav-item-text">Tasks</span>
              <span class="nav-item-badge">12</span>
            </a>
            <!-- More nav items -->
          </div>

          <div class="nav-section">
            <div class="nav-section-title">Settings</div>
            <a href="settings.html" class="nav-item">
              <span class="nav-item-icon"
                ><i class="fa-solid fa-gear"></i
              ></span>
              <span class="nav-item-text">Settings</span>
            </a>
          </div>
        </nav>

        <!-- Sidebar Footer -->
        <div class="sidebar-footer">
          <div class="sidebar-user">
            <div class="sidebar-user-avatar"></div>
            <div>
              <div class="sidebar-user-name">Loading...</div>
              <div class="sidebar-user-role">Role</div>
            </div>
          </div>
        </div>
      </aside>

      <!-- MAIN CONTENT -->
      <main class="main">
        <!-- Header -->
        <header class="header">
          <div class="header-left">
            <button class="menu-toggle">
              <i class="fa-solid fa-bars"></i>
            </button>
            <div class="search-bar">
              <span class="search-icon"
                ><i class="fa-solid fa-magnifying-glass"></i
              ></span>
              <input type="text" class="search-input" placeholder="Search..." />
            </div>
          </div>

          <div class="header-right">
            <button class="header-btn notification-btn">
              <i class="fa-solid fa-bell"></i>
              <span class="badge">3</span>
            </button>

            <div class="user-menu">
              <div class="user-avatar">U</div>
              <span class="user-name">User Name</span>
            </div>
          </div>
        </header>

        <!-- Content Area -->
        <div class="content">
          <!-- Stats Cards Row -->
          <div class="stats-row">
            <div class="stat-card">
              <div class="stat-icon">
                <i class="fa-solid fa-chart-line"></i>
              </div>
              <div class="stat-info">
                <span class="stat-label">Total Items</span>
                <span class="stat-value">42</span>
              </div>
            </div>
            <!-- More stat cards -->
          </div>

          <!-- Data Table Card -->
          <div class="card">
            <div class="card-header">
              <h3>Data Table</h3>
              <button class="btn btn-primary">
                <i class="fa-solid fa-plus"></i> Add New
              </button>
            </div>
            <div class="card-body">
              <table class="table">
                <thead>
                  <tr>
                    <th>Column 1</th>
                    <th>Column 2</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody id="tableBody">
                  <!-- Dynamic content -->
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </main>
    </div>

    <!-- Modal Template -->
    <div id="myModal" class="modal-backdrop d-none">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h3>Modal Title</h3>
            <button class="btn-icon" onclick="closeModal()">
              <i class="fa-solid fa-xmark"></i>
            </button>
          </div>
          <div class="modal-body">
            <form id="myForm">
              <div class="form-group">
                <label for="inputField">Field Label *</label>
                <input
                  type="text"
                  class="form-control"
                  id="inputField"
                  required
                />
              </div>
              <div class="form-actions">
                <button
                  type="button"
                  class="btn btn-secondary"
                  onclick="closeModal()"
                >
                  Cancel
                </button>
                <button type="submit" class="btn btn-primary">Save</button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>

    <!-- Scripts -->
    <script src="../../scripts/utils/utils.js"></script>
    <script src="../../scripts/utils/components.js"></script>
    <script src="../../scripts/utils/api.js"></script>
    <script src="../../scripts/utils/auth.js"></script>
    <script src="../../scripts/pages/[role]/[page].js"></script>
  </body>
</html>
```

---

## 🎨 3. CSS STYLING SYSTEM

### Design Tokens (variables.css)

```css
:root {
  /* Brand Colors */
  --primary-color: #7e2d96;
  --primary-dark: #5c216e;
  --primary-light: #9755aa;
  --accent-color: #88d3ce;
  --dark-color: #121212;
  --darker-color: #0a0a0a;
  --text-main: #f2f0d9;
  --text-secondary: #aaaaaa;

  /* Status Colors */
  --success: #10b981;
  --warning: #f59e0b;
  --error: #ef4444;
  --info: #3b82f6;

  /* Spacing */
  --space-1: 0.25rem;
  --space-2: 0.5rem;
  --space-3: 0.75rem;
  --space-4: 1rem;
  --space-6: 1.5rem;
  --space-8: 2rem;

  /* Typography */
  --text-xs: 0.75rem;
  --text-sm: 0.875rem;
  --text-base: 1rem;
  --text-lg: 1.125rem;
  --text-xl: 1.25rem;
  --text-2xl: 1.5rem;

  /* Layout */
  --sidebar-width: 280px;
  --header-height: 70px;

  /* Borders */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;

  /* Transitions */
  --transition-fast: all 0.2s ease;
  --transition-normal: all 0.3s ease;

  /* Z-index */
  --z-dropdown: 1000;
  --z-fixed: 1030;
  --z-modal: 1050;
}
```

### Layout Structure (layout.css)

```css
/* App Container */
.app {
  display: flex;
  min-height: 100vh;
  background: var(--darker-color);
}

/* Sidebar */
.sidebar {
  width: var(--sidebar-width);
  background: var(--dark-color);
  position: fixed;
  left: 0;
  top: 0;
  height: 100vh;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  z-index: var(--z-fixed);
  transition: transform 0.3s ease;
}

/* Main Content Area */
.main {
  flex: 1;
  margin-left: var(--sidebar-width);
  display: flex;
  flex-direction: column;
}

.header {
  height: var(--header-height);
  background: var(--dark-color);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-6);
  border-bottom: 1px solid rgba(242, 240, 217, 0.1);
  position: sticky;
  top: 0;
  z-index: 100;
}

.content {
  flex: 1;
  padding: var(--space-6);
}

/* Responsive */
@media (max-width: 768px) {
  .sidebar {
    transform: translateX(-100%);
  }
  .sidebar.show {
    transform: translateX(0);
  }
  .main {
    margin-left: 0;
  }
}
```

### Component Styles (components.css)

```css
/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-3) var(--space-6);
  border-radius: var(--radius-lg);
  font-weight: 600;
  cursor: pointer;
  transition: var(--transition-fast);
  border: none;
}

.btn-primary {
  background: linear-gradient(
    135deg,
    var(--primary-color),
    var(--primary-dark)
  );
  color: white;
  box-shadow: 0 4px 12px rgba(126, 45, 150, 0.3);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(126, 45, 150, 0.4);
}

/* Cards */
.card {
  background: var(--dark-color);
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.card-header {
  padding: var(--space-6);
  border-bottom: 1px solid rgba(242, 240, 217, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-body {
  padding: var(--space-6);
}

/* Forms */
.form-group {
  margin-bottom: var(--space-4);
}

.form-control {
  width: 100%;
  padding: var(--space-3);
  background: rgba(10, 10, 10, 0.5);
  border: 1px solid rgba(126, 45, 150, 0.3);
  border-radius: var(--radius-md);
  color: var(--text-main);
}

/* Modals */
.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: var(--z-modal);
}

.modal-dialog {
  background: var(--dark-color);
  border-radius: var(--radius-xl);
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow: auto;
}

.d-none {
  display: none !important;
}
```

---

## 💻 4. JAVASCRIPT ARCHITECTURE

### API Client Pattern (api.js)

```javascript
const API_CONFIG = {
  BASE_URL: "https://localhost:44383/api",
  TOKEN_KEY: "auth_token",
  USER_KEY: "user_data",
};

class APIClient {
  constructor() {
    this.baseURL = API_CONFIG.BASE_URL;
  }

  getToken() {
    return localStorage.getItem(API_CONFIG.TOKEN_KEY);
  }

  getHeaders(includeAuth = true) {
    const headers = {
      "Content-Type": "application/json",
      Accept: "application/json",
    };
    if (includeAuth) {
      const token = this.getToken();
      if (token) {
        headers["Authorization"] = `Bearer ${token}`;
      }
    }
    return headers;
  }

  async request(endpoint, options = {}) {
    const url = `${this.baseURL}${endpoint}`;
    const config = {
      ...options,
      headers: this.getHeaders(options.includeAuth !== false),
    };

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        if (response.status === 401) {
          // Handle unauthorized
          this.clearAuth();
          window.location.href = "../auth/login.html";
        }
        throw new Error(`HTTP ${response.status}`);
      }

      const contentType = response.headers.get("content-type");
      if (contentType?.includes("application/json")) {
        return await response.json();
      }
      return await response.text();
    } catch (error) {
      console.error("API Error:", error);
      throw error;
    }
  }

  // Convenience methods
  async get(endpoint) {
    return this.request(endpoint, { method: "GET" });
  }

  async post(endpoint, data) {
    return this.request(endpoint, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async put(endpoint, data) {
    return this.request(endpoint, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  }

  async delete(endpoint) {
    return this.request(endpoint, { method: "DELETE" });
  }

  clearAuth() {
    localStorage.removeItem(API_CONFIG.TOKEN_KEY);
    localStorage.removeItem(API_CONFIG.USER_KEY);
  }
}

// Service Layer Pattern
const API = {
  // Auth Service
  Auth: {
    async login(username, password) {
      const client = new APIClient();
      return client.post("/Auth/login", {
        Username: username,
        Password: password,
      });
    },
    async logout() {
      const client = new APIClient();
      return client.post("/Auth/logout", {});
    },
  },

  // Tasks Service
  Tasks: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Tasks");
    },
    async getById(id) {
      const client = new APIClient();
      return client.get(`/Tasks/${id}`);
    },
    async create(taskData) {
      const client = new APIClient();
      return client.post("/Tasks", taskData);
    },
    async update(id, taskData) {
      const client = new APIClient();
      return client.put(`/Tasks/${id}`, taskData);
    },
    async delete(id) {
      const client = new APIClient();
      return client.delete(`/Tasks/${id}`);
    },
  },

  // Projects Service
  Projects: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Projects");
    },
    async create(projectData) {
      const client = new APIClient();
      return client.post("/Projects", projectData);
    },
    // ... more methods
  },

  // Employees Service
  Employees: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Employees");
    },
    // ... more methods
  },

  // Clients Service
  Clients: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Clients");
    },
    // ... more methods
  },
};

// Export for use in other files
window.API = API;
```

### Authentication Pattern (auth.js)

```javascript
const AUTH_STORAGE_KEYS = {
  TOKEN: "auth_token",
  USER: "user_data",
};

const USER_ROLES = {
  MANAGER: 1,
  ASSISTANT_MANAGER: 2,
  ACCOUNTANT: 3,
  TEAM_LEADER: 4,
  EMPLOYEE: 5,
  CLIENT: 6,
};

const ROLE_DASHBOARDS = {
  1: "../manager/dashboard.html",
  2: "../assistant-manager/dashboard.html",
  3: "../accountant/dashboard.html",
  4: "../team-leader/dashboard.html",
  5: "../employee/dashboard.html",
  6: "../client/dashboard.html",
};

class AuthManager {
  constructor() {
    this.currentUser = null;
    this.loadUserFromStorage();
  }

  async login(username, password) {
    try {
      const response = await API.Auth.login(username, password);
      if (response && response.Token && response.User) {
        this.setAuthData(response.Token, response.User);
        return {
          success: true,
          user: response.User,
          redirectUrl: this.getDashboardUrl(response.User.Role),
        };
      }
      throw new Error("Invalid response");
    } catch (error) {
      return { success: false, error: error.message };
    }
  }

  logout() {
    API.Auth.logout().catch(() => {});
    this.clearAuthData();
    window.location.href = "../auth/login.html";
  }

  setAuthData(token, user) {
    localStorage.setItem(AUTH_STORAGE_KEYS.TOKEN, token);
    localStorage.setItem(AUTH_STORAGE_KEYS.USER, JSON.stringify(user));
    this.currentUser = user;
  }

  clearAuthData() {
    localStorage.removeItem(AUTH_STORAGE_KEYS.TOKEN);
    localStorage.removeItem(AUTH_STORAGE_KEYS.USER);
    this.currentUser = null;
  }

  loadUserFromStorage() {
    const userData = localStorage.getItem(AUTH_STORAGE_KEYS.USER);
    if (userData) {
      try {
        this.currentUser = JSON.parse(userData);
      } catch (error) {
        this.clearAuthData();
      }
    }
  }

  isAuthenticated() {
    return !!localStorage.getItem(AUTH_STORAGE_KEYS.TOKEN);
  }

  getCurrentUser() {
    return this.currentUser;
  }

  getUserRole() {
    return this.currentUser?.Role || this.currentUser?.RoleId;
  }

  getDashboardUrl(role = null) {
    const userRole = role || this.getUserRole();
    return ROLE_DASHBOARDS[userRole] || "../auth/login.html";
  }

  hasRole(role) {
    return this.getUserRole() === role;
  }

  requireAuth() {
    if (!this.isAuthenticated()) {
      window.location.href = "../auth/login.html";
      return false;
    }
    return true;
  }

  requireRole(allowedRoles) {
    if (!this.requireAuth()) return false;
    const userRole = this.getUserRole();
    if (!allowedRoles.includes(userRole)) {
      alert("Access denied");
      window.location.href = this.getDashboardUrl();
      return false;
    }
    return true;
  }
}

// Initialize auth globally
const auth = new AuthManager();
window.auth = auth;

// Auto-initialize UI on page load
document.addEventListener("DOMContentLoaded", () => {
  initAuthUI();
});

function initAuthUI() {
  const user = auth.getCurrentUser();
  if (!user) return;

  // Update user info in sidebar
  const sidebarName = document.querySelector(".sidebar-user-name");
  const sidebarRole = document.querySelector(".sidebar-user-role");
  if (sidebarName) sidebarName.textContent = user.Name || user.name;
  if (sidebarRole) sidebarRole.textContent = getRoleName(auth.getUserRole());

  // Update header user info
  const userName = document.querySelector(".user-name");
  if (userName) userName.textContent = user.Name || user.name;
}

function getRoleName(roleId) {
  const names = {
    1: "Manager",
    2: "Assistant Manager",
    3: "Accountant",
    4: "Team Leader",
    5: "Employee",
    6: "Client",
  };
  return names[roleId] || "User";
}
```

### Page-Specific Pattern (pages/[role]/dashboard.js)

```javascript
// Protect page with role check
auth.requireRole([USER_ROLES.MANAGER]);

// Page state
let currentData = [];

// Initialize page
document.addEventListener("DOMContentLoaded", async () => {
  await loadDashboardData();
  setupEventListeners();
});

// Load data from API
async function loadDashboardData() {
  try {
    showLoading();

    const [tasks, projects, employees] = await Promise.all([
      API.Tasks.getAll(),
      API.Projects.getAll(),
      API.Employees.getAll(),
    ]);

    updateStats({ tasks, projects, employees });
    renderTasks(tasks);
  } catch (error) {
    console.error("Error loading data:", error);
    showError("Failed to load dashboard data");
  } finally {
    hideLoading();
  }
}

// Update stats cards
function updateStats(data) {
  document.getElementById("totalTasks").textContent = data.tasks.length;
  document.getElementById("totalProjects").textContent = data.projects.length;
  document.getElementById("totalEmployees").textContent = data.employees.length;
}

// Render data to table/cards
function renderTasks(tasks) {
  const container = document.getElementById("tasksContainer");
  container.innerHTML = tasks
    .map(
      (task) => `
    <div class="task-card">
      <h4>${task.Title}</h4>
      <p>${task.Description}</p>
      <div class="task-actions">
        <button class="btn btn-sm btn-primary" onclick="editTask(${task.Id})">
          Edit
        </button>
        <button class="btn btn-sm btn-danger" onclick="deleteTask(${task.Id})">
          Delete
        </button>
      </div>
    </div>
  `
    )
    .join("");
}

// Event handlers
function setupEventListeners() {
  document
    .getElementById("createTaskBtn")
    ?.addEventListener("click", showCreateModal);
  document
    .getElementById("taskForm")
    ?.addEventListener("submit", handleTaskSubmit);
}

// Modal management
function showCreateModal() {
  document.getElementById("taskModal").classList.remove("d-none");
}

function closeModal() {
  document.getElementById("taskModal").classList.add("d-none");
  document.getElementById("taskForm").reset();
}

// CRUD operations
async function handleTaskSubmit(e) {
  e.preventDefault();
  const formData = new FormData(e.target);
  const taskData = {
    Title: formData.get("title"),
    Description: formData.get("description"),
    // ... more fields
  };

  try {
    showLoading();
    await API.Tasks.create(taskData);
    await loadDashboardData();
    closeModal();
    showSuccess("Task created successfully");
  } catch (error) {
    showError("Failed to create task");
  } finally {
    hideLoading();
  }
}

async function deleteTask(id) {
  if (!confirm("Are you sure?")) return;

  try {
    await API.Tasks.delete(id);
    await loadDashboardData();
    showSuccess("Task deleted");
  } catch (error) {
    showError("Failed to delete task");
  }
}

// Utility functions
function showLoading() {
  document.body.classList.add("loading");
}

function hideLoading() {
  document.body.classList.remove("loading");
}

function showSuccess(message) {
  // Show toast notification
  console.log("✓", message);
}

function showError(message) {
  // Show error notification
  console.error("✗", message);
  alert(message);
}
```

---

## 🔑 5. KEY PATTERNS & BEST PRACTICES

### Data Flow Pattern

```
1. User Action (click, submit, etc.)
   ↓
2. Event Handler in page.js
   ↓
3. Call API Service Method (API.Tasks.create())
   ↓
4. APIClient handles HTTP request
   ↓
5. Backend processes request
   ↓
6. Response returns through chain
   ↓
7. Update UI with new data
```

### File Naming Conventions

- HTML: `kebab-case.html` (e.g., `task-details.html`)
- CSS: `kebab-case.css`
- JS: `camelCase.js` (e.g., `taskManager.js`)
- Classes: `PascalCase` for classes, `camelCase` for instances
- CSS Classes: `kebab-case` (e.g., `.nav-item`)

### Role-Based Access Control

```javascript
// In each protected page
auth.requireRole([USER_ROLES.MANAGER, USER_ROLES.ASSISTANT_MANAGER]);

// In navigation (auto-handled by auth.js)
<a href="employees.html" class="nav-item" data-roles="1,2">
  Employees
</a>;
```

### API Response Format (from backend)

```javascript
// Success response
{
  "Data": {...},      // PascalCase properties
  "Success": true,
  "Message": "Success"
}

// Error response
{
  "Success": false,
  "Message": "Error message",
  "Errors": []
}
```

### Modal Pattern

```javascript
// Open modal
function showModal() {
  document.getElementById("myModal").classList.remove("d-none");
}

// Close modal
function closeModal() {
  document.getElementById("myModal").classList.add("d-none");
  document.getElementById("myForm").reset();
}

// Handle form submit
document.getElementById("myForm").addEventListener("submit", async (e) => {
  e.preventDefault();
  // Process form
  await API.Something.create(data);
  closeModal();
});
```

---

## 📦 6. REUSABLE COMPONENTS

### Stat Card Component

```html
<div class="stat-card">
  <div class="stat-icon">
    <i class="fa-solid fa-icon"></i>
  </div>
  <div class="stat-info">
    <span class="stat-label">Label</span>
    <span class="stat-value">42</span>
  </div>
</div>
```

### Data Table Component

```html
<div class="card">
  <div class="card-header">
    <h3>Table Title</h3>
    <button class="btn btn-primary">Add New</button>
  </div>
  <div class="card-body">
    <table class="table">
      <thead>
        <tr>
          <th>Column 1</th>
          <th>Column 2</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody id="tableBody">
        <!-- Dynamic rows -->
      </tbody>
    </table>
  </div>
</div>
```

### Badge Component

```html
<span class="badge badge-success">Active</span>
<span class="badge badge-warning">Pending</span>
<span class="badge badge-danger">Overdue</span>
```

---

## 🚀 7. QUICK START CHECKLIST

When starting from scratch:

### Phase 1: Setup (1 hour)

- [ ] Create folder structure
- [ ] Set up `variables.css` with design tokens
- [ ] Create `base.css` with resets
- [ ] Set up `main.css` with imports
- [ ] Create empty `layout.css` and `components.css`

### Phase 2: Core Layout (2 hours)

- [ ] Build sidebar structure (HTML + CSS)
- [ ] Build header structure
- [ ] Build main content area
- [ ] Add responsive mobile menu toggle
- [ ] Test layout on mobile/desktop

### Phase 3: API Integration (2 hours)

- [ ] Create `api.js` with APIClient class
- [ ] Set up service methods for each entity
- [ ] Test with backend endpoints
- [ ] Add error handling

### Phase 4: Authentication (2 hours)

- [ ] Create `auth.js` with AuthManager
- [ ] Build login page
- [ ] Implement role-based routing
- [ ] Add auto-navigation enhancements

### Phase 5: Components (3 hours)

- [ ] Build button styles
- [ ] Build card component
- [ ] Build modal component
- [ ] Build form controls
- [ ] Build table component

### Phase 6: Pages (per page: 1-2 hours)

- [ ] Create HTML structure from template
- [ ] Load data from API
- [ ] Implement CRUD operations
- [ ] Add loading states
- [ ] Add error handling

---

## 🔧 8. DEPENDENCIES

### Required External Libraries

```html
<!-- Font Awesome 6 (Icons) -->
<link
  rel="stylesheet"
  href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css"
/>

<!-- Optional: SignalR for real-time (only if needed) -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.14/dist/browser/signalr.min.js"></script>
```

### No Build Tools Required

- Pure HTML/CSS/JavaScript
- No npm, webpack, or bundlers needed
- Works directly in browser
- Link files with `<link>` and `<script>` tags

---

## 💡 9. TIPS FOR REBUILDING

1. **Start with one role** (e.g., Manager) and build all pages for that role first
2. **Use the template** - Copy the standard page template for each new page
3. **API first** - Get API integration working before building complex UI
4. **Component reuse** - Build reusable components in `components.css` and `components.js`
5. **Test incrementally** - Test each page as you build it
6. **Mobile responsive** - Test on mobile throughout development
7. **Role-based access** - Add `auth.requireRole()` to every protected page
8. **Consistent naming** - Follow the naming conventions strictly
9. **Error handling** - Always wrap API calls in try-catch
10. **Loading states** - Show loading indicators during API calls
11. **Build shared pages first** - Dashboard and Settings are similar across roles
12. **Test cross-role access** - Ensure employees can't access manager pages
13. **Use data-roles attribute** - For conditional navigation visibility
14. **Implement search/filter** - Most list pages need search functionality
15. **Add pagination** - For large data sets (tasks, projects, etc.)

---

## 📊 10. ROLE-SPECIFIC NAVIGATION & FEATURES

### Manager (Role ID: 1)

**Navigation Structure:**

```
Main Section:
  - Dashboard (overview, stats, recent activity)
  - Tasks (all tasks management)
  - Projects (all projects oversight)
  - Employees (employee management)
  - Clients (client management)
  - Calendar (schedule view)

Reports Section:
  - Analytics (reports, charts, KPIs)

Settings Section:
  - Settings (system configuration)
```

**Key Features:**

- Full CRUD on all entities
- Assign tasks to employees
- Manage projects and budgets
- Employee performance tracking
- Client relationship management
- System-wide analytics
- Access to all data

---

### Assistant Manager (Role ID: 2)

**Navigation Structure:**

```
Main Section:
  - Dashboard
  - Tasks
  - Projects
  - Employees
  - Clients
  - Calendar

Reports Section:
  - Analytics

Settings Section:
  - Settings
```

**Key Features:**

- Similar to Manager but with limited permissions
- Can manage tasks and projects
- Can view employee data
- Limited analytics access
- Cannot modify system settings
- Assists manager with operations

---

### Accountant (Role ID: 3)

**Navigation Structure:**

```
Main Section:
  - Dashboard (financial overview)
  - Projects (budget tracking)
  - Clients (billing information)
  - Invoices (invoice management)
  - Expenses (expense tracking)

Reports Section:
  - Financial Reports (P&L, balance sheet)

Settings Section:
  - Settings (personal settings)
```

**Key Features:**

- Financial data focus
- Invoice creation and management
- Expense approval and tracking
- Budget monitoring per project
- Financial reports generation
- Client billing management
- Export to Excel/PDF

---

### Team Leader (Role ID: 4)

**Navigation Structure:**

```
Main Section:
  - Dashboard (team overview)
  - Tasks (team task management)
  - Team Tasks (assigned to team)
  - Projects (team projects)
  - My Team (team roster)
  - Team Members (member details)
  - Calendar (team schedule)

Reports Section:
  - Analytics (team performance)

Settings Section:
  - Settings
```

**Key Features:**

- Team-focused management
- Assign tasks to team members
- Monitor team performance
- Track team project progress
- View team member details
- Team analytics and reports
- Limited to own team only

---

### Employee (Role ID: 5)

**Navigation Structure:**

```
Main Section:
  - Dashboard (personal overview)
  - Tasks (all assigned tasks)
  - My Tasks (personal tasks)
  - Projects (assigned projects)
  - Calendar (personal schedule)
  - Profile (personal information)

Settings Section:
  - Settings
```

**Key Features:**

- View assigned tasks
- Update task status
- View project details (read-only)
- Personal calendar
- Update own profile
- Track own performance
- Cannot manage others
- Limited data visibility

---

### Client (Role ID: 6)

**Navigation Structure:**

```
Main Section:
  - Dashboard (project overview)
  - Projects (my projects)
  - Project Details (detailed view)

Settings Section:
  - Settings (account settings)
```

**Key Features:**

- View own projects only
- Track project progress
- View project deliverables
- Communication with team
- Request updates
- Minimal system access
- Read-only for most data
- Cannot create/edit projects

---

## 📊 11. ENTITY STRUCTURE EXAMPLES

### Task DTO (PascalCase for backend)

```javascript
{
  Id: 1,
  Title: "Task title",
  Description: "Task description",
  Status: 1,
  Priority: 2,
  DueDate: "2025-12-31",
  AssignedToId: 5,
  ProjectId: 3,
  CreatedAt: "2025-01-01T00:00:00",
  UpdatedAt: "2025-01-15T00:00:00"
}
```

### Project DTO

```javascript
{
  Id: 1,
  Name: "Project name",
  Description: "Description",
  StartDate: "2025-01-01",
  EndDate: "2025-12-31",
  Budget: 50000,
  ClientId: 2,
  ManagerId: 1,
  Status: 1
}
```

### Employee/User DTO

```javascript
{
  UserId: 1,
  Name: "John Doe",
  Email: "john@example.com",
  Role: 1,  // or RoleId
  Department: "IT",
  Position: "Developer",
  PhoneNumber: "+1234567890"
}
```

### Client DTO

```javascript
{
  Id: 1,
  Name: "Acme Corporation",
  ContactName: "Jane Smith",
  Email: "jane@acme.com",
  Phone: "+1234567890",
  Address: "123 Main St",
  Industry: "Technology",
  CreatedAt: "2025-01-01T00:00:00"
}
```

### Invoice DTO (Accountant)

```javascript
{
  Id: 1,
  InvoiceNumber: "INV-2025-001",
  ClientId: 2,
  ProjectId: 3,
  Amount: 15000,
  TaxAmount: 1500,
  TotalAmount: 16500,
  DueDate: "2025-02-28",
  Status: 1, // 1=Pending, 2=Paid, 3=Overdue
  IssueDate: "2025-01-28",
  CreatedAt: "2025-01-28T00:00:00"
}
```

### Expense DTO (Accountant)

```javascript
{
  Id: 1,
  Description: "Office supplies",
  Amount: 500,
  Category: "Operations",
  ProjectId: 3,
  EmployeeId: 5,
  Date: "2025-01-15",
  Status: 1, // 1=Pending, 2=Approved, 3=Rejected
  ReceiptUrl: "/uploads/receipt-123.pdf"
}
```

---

## 📋 12. API ENDPOINT MAPPING

### Authentication Endpoints

```javascript
POST / api / Auth / login; // Login
POST / api / Auth / logout; // Logout
POST / api / Auth / refresh; // Refresh token
GET / api / Auth / me; // Get current user
```

### Tasks Endpoints

```javascript
GET / api / Tasks; // Get all tasks (role-filtered)
GET / api / Tasks / { id }; // Get task by ID
POST / api / Tasks; // Create task
PUT / api / Tasks / { id }; // Update task
DELETE / api / Tasks / { id }; // Delete task
GET / api / Tasks / my - tasks; // Get current user's tasks
GET / api / Tasks / team - tasks; // Get team tasks (Team Leader)
PUT / api / Tasks / { id } / status; // Update task status
```

### Projects Endpoints

```javascript
GET / api / Projects; // Get all projects (role-filtered)
GET / api / Projects / { id }; // Get project by ID
POST / api / Projects; // Create project
PUT / api / Projects / { id }; // Update project
DELETE / api / Projects / { id }; // Delete project
GET / api / Projects / { id } / tasks; // Get project tasks
GET / api / Projects / { id } / budget; // Get project budget (Accountant)
```

### Employees Endpoints

```javascript
GET / api / Employees; // Get all employees
GET / api / Employees / { id }; // Get employee by ID
POST / api / Employees; // Create employee (Manager only)
PUT / api / Employees / { id }; // Update employee
DELETE / api / Employees / { id }; // Delete employee (Manager only)
GET / api / Employees / team; // Get team members (Team Leader)
```

### Clients Endpoints

```javascript
GET / api / Clients; // Get all clients
GET / api / Clients / { id }; // Get client by ID
POST / api / Clients; // Create client (Manager/Accountant)
PUT / api / Clients / { id }; // Update client
DELETE / api / Clients / { id }; // Delete client (Manager only)
GET / api / Clients / { id } / projects; // Get client projects
```

### Invoices Endpoints (Accountant)

```javascript
GET    /api/Invoices                // Get all invoices
GET    /api/Invoices/{id}           // Get invoice by ID
POST   /api/Invoices                // Create invoice
PUT    /api/Invoices/{id}           // Update invoice
DELETE /api/Invoices/{id}           // Delete invoice
POST   /api/Invoices/{id}/send      // Send invoice to client
GET    /api/Invoices/export         // Export invoices (PDF/Excel)
```

### Expenses Endpoints (Accountant)

```javascript
GET / api / Expenses; // Get all expenses
GET / api / Expenses / { id }; // Get expense by ID
POST / api / Expenses; // Create expense
PUT / api / Expenses / { id }; // Update expense
DELETE / api / Expenses / { id }; // Delete expense
PUT / api / Expenses / { id } / approve; // Approve expense
PUT / api / Expenses / { id } / reject; // Reject expense
```

### Reports Endpoints

```javascript
GET / api / Reports / dashboard; // Dashboard stats (role-specific)
GET / api / Reports / analytics; // Analytics data
GET / api / Reports / financial; // Financial reports (Accountant)
GET / api / Reports / team - performance; // Team performance (Team Leader)
```

---

## 🎯 ESTIMATED TIMELINE

### Complete Project (All 6 Roles)

- **Full rebuild from scratch**: 120-180 hours
- **Core infrastructure** (Layout, Auth, API): 20 hours
- **Shared components**: 10 hours

### Per Role Breakdown

- **Manager** (8 pages): 12-16 hours
- **Assistant Manager** (8 pages): 12-16 hours
- **Accountant** (7 pages): 10-14 hours
- **Team Leader** (9 pages): 14-18 hours
- **Employee** (7 pages): 10-14 hours
- **Client** (4 pages): 6-8 hours

### Per Component

- **Per page**: 1-2 hours
- **API service layer**: 1 hour per entity
- **Testing per role**: 2-3 hours
- **Final integration & refinement**: 10-15 hours

---

## 📝 NOTES

- All CSS uses CSS variables for easy theming
- Layout is fully responsive (desktop + mobile)
- API client handles authentication automatically
- Role-based access is enforced at both navigation and page level
- All backend DTOs use PascalCase properties
- Frontend uses consistent camelCase for JS, kebab-case for CSS
- No external dependencies except Font Awesome
- Works on all modern browsers (Chrome, Firefox, Edge, Safari)

---

## 🔄 13. COMMON PAGE PATTERNS BY TYPE

### Dashboard Pages (All Roles)

**Components:**

- 3-4 stat cards with key metrics
- Recent activity list
- Quick actions section
- Role-specific charts/graphs
- Welcome message with user name

**API Calls:**

```javascript
// Load dashboard data
const stats = await API.Reports.dashboard();
const recentTasks = await API.Tasks.getAll(); // filtered
const recentProjects = await API.Projects.getAll(); // filtered
```

---

### List Pages (Tasks, Projects, Employees, etc.)

**Components:**

- Search bar
- Filter dropdowns (status, date, etc.)
- Sort options
- Data table or card grid
- Pagination controls
- "Create New" button (if has permission)

**API Calls:**

```javascript
// Load list with filters
const items = await API.Entity.getAll();
// Filter/search client-side or pass query params
```

---

### Detail/View Pages

**Components:**

- Breadcrumb navigation
- Header with title and actions
- Tabbed sections (Overview, Details, History)
- Related data lists
- Edit/Delete buttons (if has permission)

**API Calls:**

```javascript
// Load single item
const item = await API.Entity.getById(id);
const relatedData = await API.RelatedEntity.getAll();
```

---

### Form Pages (Create/Edit)

**Components:**

- Form with validation
- Required field indicators
- Date pickers
- Dropdowns for foreign keys
- File upload (if needed)
- Cancel and Save buttons

**API Calls:**

```javascript
// For edit, load existing data
if (editMode) {
  const item = await API.Entity.getById(id);
  populateForm(item);
}

// On submit
const result = editMode
  ? await API.Entity.update(id, formData)
  : await API.Entity.create(formData);
```

---

### Settings Pages (All Roles)

**Components:**

- Tabbed sections (Profile, Security, Preferences)
- Form fields for user data
- Password change section
- Language/theme selector
- Notification preferences

**API Calls:**

```javascript
// Load user settings
const user = await API.Auth.me();
// Update settings
await API.Users.updateProfile(userId, profileData);
```

---

## 🎨 14. RECOMMENDED BUILD ORDER

### Phase 1: Foundation (Week 1)

1. Set up folder structure
2. Create `variables.css` with all design tokens
3. Build `base.css` with resets
4. Create `layout.css` for app structure
5. Build `components.css` for reusable UI
6. Create `api.js` with APIClient and all services
7. Create `auth.js` with authentication logic
8. Build login page and test authentication

### Phase 2: Manager Role (Week 2)

1. Manager dashboard (template for all dashboards)
2. Manager tasks page (template for task management)
3. Manager projects page (template for project management)
4. Manager employees page (template for list pages)
5. Manager clients page
6. Manager calendar page
7. Manager analytics page
8. Manager settings page

### Phase 3: Assistant Manager Role (Week 3)

1. Copy Manager pages
2. Adjust permissions/API calls
3. Test role-based access
4. Implement different dashboard metrics

### Phase 4: Accountant Role (Week 4)

1. Accountant dashboard (financial focus)
2. Invoices page (new component type)
3. Expenses page
4. Financial reports page
5. Projects page (budget view)
6. Clients page (billing view)
7. Settings page

### Phase 5: Team Leader Role (Week 5)

1. Team Leader dashboard (team metrics)
2. Team tasks page
3. My team page (team roster)
4. Team members page
5. Projects page (team projects)
6. Calendar page (team schedule)
7. Analytics page (team performance)
8. Settings page

### Phase 6: Employee Role (Week 6)

1. Employee dashboard (personal view)
2. Tasks page (assigned only)
3. My tasks page
4. Projects page (read-only)
5. Calendar page (personal)
6. Profile page
7. Settings page

### Phase 7: Client Role (Week 7)

1. Client dashboard (minimal)
2. Projects page (own projects only)
3. Project details page
4. Settings page

### Phase 8: Polish & Testing (Week 8-9)

1. Cross-browser testing
2. Mobile responsive testing
3. Role-based access testing
4. Performance optimization
5. Error handling improvements
6. Loading state refinements
7. Final UI polish
8. Documentation

---

## 🛠️ 15. QUICK COPY-PASTE SNIPPETS

### Role Check at Top of Every Page

```javascript
// For Manager pages
auth.requireRole([USER_ROLES.MANAGER]);

// For pages accessible by multiple roles
auth.requireRole([USER_ROLES.MANAGER, USER_ROLES.ASSISTANT_MANAGER]);

// For Accountant-only pages
auth.requireRole([USER_ROLES.ACCOUNTANT]);

// For Team Leader pages
auth.requireRole([USER_ROLES.TEAM_LEADER]);

// For Employee pages
auth.requireRole([USER_ROLES.EMPLOYEE]);

// For Client pages
auth.requireRole([USER_ROLES.CLIENT]);
```

### Standard Page Initialization

```javascript
document.addEventListener("DOMContentLoaded", async () => {
  // Protect page
  auth.requireRole([USER_ROLES.MANAGER]);

  // Load data
  await loadPageData();

  // Setup event listeners
  setupEventListeners();
});

async function loadPageData() {
  try {
    showLoading();
    const data = await API.Entity.getAll();
    renderData(data);
  } catch (error) {
    console.error("Error loading data:", error);
    showError("Failed to load data");
  } finally {
    hideLoading();
  }
}

function setupEventListeners() {
  // Setup your event listeners here
}
```

### Standard CRUD Operations

```javascript
// CREATE
async function handleCreate(formData) {
  try {
    showLoading();
    await API.Entity.create(formData);
    await loadPageData(); // Refresh
    closeModal();
    showSuccess("Created successfully");
  } catch (error) {
    showError("Failed to create");
  } finally {
    hideLoading();
  }
}

// UPDATE
async function handleUpdate(id, formData) {
  try {
    showLoading();
    await API.Entity.update(id, formData);
    await loadPageData(); // Refresh
    closeModal();
    showSuccess("Updated successfully");
  } catch (error) {
    showError("Failed to update");
  } finally {
    hideLoading();
  }
}

// DELETE
async function handleDelete(id) {
  if (!confirm("Are you sure you want to delete this?")) return;

  try {
    showLoading();
    await API.Entity.delete(id);
    await loadPageData(); // Refresh
    showSuccess("Deleted successfully");
  } catch (error) {
    showError("Failed to delete");
  } finally {
    hideLoading();
  }
}
```

### Standard Modal Management

```javascript
function showModal(modalId = "modal") {
  document.getElementById(modalId).classList.remove("d-none");
}

function closeModal(modalId = "modal") {
  document.getElementById(modalId).classList.add("d-none");
  const form = document.querySelector(`#${modalId} form`);
  if (form) form.reset();
}

// Close modal on backdrop click
document.addEventListener("click", (e) => {
  if (e.target.classList.contains("modal-backdrop")) {
    closeModal(e.target.id);
  }
});
```

---

## 📝 FINAL NOTES

- All CSS uses CSS variables for easy theming
- Layout is fully responsive (desktop + mobile)
- API client handles authentication automatically
- Role-based access is enforced at both navigation and page level
- All backend DTOs use PascalCase properties
- Frontend uses consistent camelCase for JS, kebab-case for CSS
- No external dependencies except Font Awesome
- Works on all modern browsers (Chrome, Firefox, Edge, Safari)
- Total project size: **45 HTML pages, 43 JS files, 15+ CSS files**
- Estimated lines of code: **15,000-20,000 lines**
- Database entities: **Tasks, Projects, Employees, Clients, Invoices, Expenses, Users**

---

**Good luck with your rebuild! This complete structure covers all 6 roles and should save you significant time.** 🚀
