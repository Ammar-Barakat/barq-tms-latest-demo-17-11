// API Configuration
const API_CONFIG = {
  BASE_URL: "https://localhost:44383/api",
  TOKEN_KEY: "auth_token",
  USER_KEY: "user_data",
};

// API Client Class
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

    console.log(`[API] ${options.method || "GET"} ${url}`);

    try {
      const response = await fetch(url, config);

      console.log(`[API] Response status: ${response.status}`);

      if (!response.ok) {
        if (response.status === 401) {
          // Handle unauthorized
          console.warn("[API] Unauthorized - clearing auth and redirecting");
          this.clearAuth();
          window.location.href = "../auth/login.html";
        }
        const errorText = await response.text().catch(() => "Unknown error");
        console.error(`[API] Error response:`, errorText);
        throw new Error(`HTTP ${response.status}: ${errorText}`);
      }

      const contentType = response.headers.get("content-type");
      if (contentType?.includes("application/json")) {
        const data = await response.json();
        console.log(`[API] Success:`, data);
        return data;
      }
      return await response.text();
    } catch (error) {
      console.error("[API] Request failed:", error);
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
    async login(userName, password) {
      const client = new APIClient();
      return client.post("/Auth/login", {
        userName: userName,
        password: password,
      });
    },
    async register(userData) {
      const client = new APIClient();
      return client.post("/Auth/register", userData);
    },
    async logout() {
      const client = new APIClient();
      return client.post("/Auth/logout", {});
    },
    async changePassword(passwordData) {
      const client = new APIClient();
      return client.post("/Auth/change-password", passwordData);
    },
    async me() {
      const client = new APIClient();
      return client.get("/Auth/me");
    },
  },

  // Dashboard Service
  Dashboard: {
    async getStats() {
      const client = new APIClient();
      return client.get("/Dashboard/stats");
    },
    async getActivities() {
      const client = new APIClient();
      return client.get("/Dashboard/activities");
    },
    async getRecentProjects() {
      const client = new APIClient();
      return client.get("/Dashboard/recent-projects");
    },
    async getTasksByStatus() {
      const client = new APIClient();
      return client.get("/Dashboard/tasks-by-status");
    },
    async getUserStats(userId) {
      const client = new APIClient();
      return client.get(`/Dashboard/user-stats/${userId}`);
    },
    async getTeamStats() {
      const client = new APIClient();
      return client.get("/Dashboard/team-stats");
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
    async getComments(taskId) {
      const client = new APIClient();
      return client.get(`/Tasks/${taskId}/comments`);
    },
    async addComment(taskId, comment) {
      const client = new APIClient();
      return client.post(`/Tasks/${taskId}/comments`, { comment: comment });
    },
    async getAttachments(taskId) {
      const client = new APIClient();
      return client.get(`/Tasks/${taskId}/attachments`);
    },
    async getHistory(taskId) {
      const client = new APIClient();
      return client.get(`/Tasks/${taskId}/history`);
    },
  },

  // Projects Service
  Projects: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Projects");
    },
    async getById(id) {
      const client = new APIClient();
      return client.get(`/Projects/${id}`);
    },
    async create(projectData) {
      const client = new APIClient();
      return client.post("/Projects", projectData);
    },
    async update(id, projectData) {
      const client = new APIClient();
      return client.put(`/Projects/${id}`, projectData);
    },
    async delete(id) {
      const client = new APIClient();
      return client.delete(`/Projects/${id}`);
    },
    async getTasks(projectId) {
      const client = new APIClient();
      return client.get(`/Projects/${projectId}/tasks`);
    },
    async getAuditLogs(projectId) {
      const client = new APIClient();
      return client.get(`/Projects/${projectId}/auditlogs`);
    },
  },

  // Users Service (replaces Employees)
  Users: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Users");
    },
    async getById(id) {
      const client = new APIClient();
      return client.get(`/Users/${id}`);
    },
    async create(userData) {
      const client = new APIClient();
      return client.post("/Users", userData);
    },
    async update(id, userData) {
      const client = new APIClient();
      return client.put(`/Users/${id}`, userData);
    },
    async delete(id) {
      const client = new APIClient();
      return client.delete(`/Users/${id}`);
    },
    async getDepartments(userId) {
      const client = new APIClient();
      return client.get(`/Users/${userId}/departments`);
    },
    async getTasks(userId) {
      const client = new APIClient();
      return client.get(`/Users/${userId}/tasks`);
    },
  },

  // Departments Service
  Departments: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Departments");
    },
    async getById(id) {
      const client = new APIClient();
      return client.get(`/Departments/${id}`);
    },
    async create(deptData) {
      const client = new APIClient();
      return client.post("/Departments", deptData);
    },
    async update(id, deptData) {
      const client = new APIClient();
      return client.put(`/Departments/${id}`, deptData);
    },
    async delete(id) {
      const client = new APIClient();
      return client.delete(`/Departments/${id}`);
    },
    async getTasks(deptId) {
      const client = new APIClient();
      return client.get(`/Departments/${deptId}/tasks`);
    },
    async getProjects(deptId) {
      const client = new APIClient();
      return client.get(`/Departments/${deptId}/projects`);
    },
    async getUsers(deptId) {
      const client = new APIClient();
      return client.get(`/Departments/${deptId}/users`);
    },
  },

  // Roles Service
  Roles: {
    async getAll() {
      const client = new APIClient();
      return client.get("/Roles");
    },
    async getById(id) {
      const client = new APIClient();
      return client.get(`/Roles/${id}`);
    },
    async create(roleData) {
      const client = new APIClient();
      return client.post("/Roles", roleData);
    },
    async update(id, roleData) {
      const client = new APIClient();
      return client.put(`/Roles/${id}`, roleData);
    },
    async delete(id) {
      const client = new APIClient();
      return client.delete(`/Roles/${id}`);
    },
  },

  // Notifications Service
  Notifications: {
    async getByUser(userId) {
      const client = new APIClient();
      return client.get(`/Notifications/user/${userId}`);
    },
    async getUnread(userId) {
      const client = new APIClient();
      return client.get(`/Notifications/user/${userId}/unread`);
    },
    async getUnreadCount(userId) {
      const client = new APIClient();
      return client.get(`/Notifications/user/${userId}/count/unread`);
    },
    async markAsRead(notifId) {
      const client = new APIClient();
      return client.put(`/Notifications/${notifId}/read`, {});
    },
    async markAllAsRead(userId) {
      const client = new APIClient();
      return client.put(`/Notifications/user/${userId}/read-all`, {});
    },
    async delete(notifId) {
      const client = new APIClient();
      return client.delete(`/Notifications/${notifId}`);
    },
    async create(notifData) {
      const client = new APIClient();
      return client.post("/Notifications", notifData);
    },
  },

  // Calendar Service
  Calendar: {
    async getEvents(startDate, endDate) {
      const client = new APIClient();
      let url = "/Calendar/events";
      if (startDate && endDate) {
        url += `?startDate=${startDate}&endDate=${endDate}`;
      }
      return client.get(url);
    },
    async getEventById(id) {
      const client = new APIClient();
      return client.get(`/Calendar/events/${id}`);
    },
    async createEvent(eventData) {
      const client = new APIClient();
      return client.post("/Calendar/events", eventData);
    },
    async updateEvent(id, eventData) {
      const client = new APIClient();
      return client.put(`/Calendar/events/${id}`, eventData);
    },
    async deleteEvent(id) {
      const client = new APIClient();
      return client.delete(`/Calendar/events/${id}`);
    },
    async getUpcomingEvents(days = 7) {
      const client = new APIClient();
      return client.get(`/Calendar/events/upcoming?days=${days}`);
    },
    async getStats() {
      const client = new APIClient();
      return client.get("/Calendar/stats");
    },
    async getTodayEvents() {
      const client = new APIClient();
      return client.get("/Calendar/events/today");
    },
    async getThisWeekEvents() {
      const client = new APIClient();
      return client.get("/Calendar/events/this-week");
    },
    async getThisMonthEvents() {
      const client = new APIClient();
      return client.get("/Calendar/events/this-month");
    },
  },

  // Statistics Service
  Statistics: {
    async getDashboard() {
      const client = new APIClient();
      return client.get("/Statistics/dashboard");
    },
    async getTasksByStatus() {
      const client = new APIClient();
      return client.get("/Statistics/tasks-by-status");
    },
    async getTasksByPriority() {
      const client = new APIClient();
      return client.get("/Statistics/tasks-by-priority");
    },
    async getProjectProgress() {
      const client = new APIClient();
      return client.get("/Statistics/project-progress");
    },
  },

  // Search Service
  Search: {
    async global(query, page = 1, pageSize = 10) {
      const client = new APIClient();
      return client.get(
        `/Search?query=${encodeURIComponent(
          query
        )}&page=${page}&pageSize=${pageSize}`
      );
    },
    async tasks(query, statusId, priorityId, projectId, assignedTo, page = 1) {
      const client = new APIClient();
      let url = `/Search/tasks?query=${encodeURIComponent(query)}&page=${page}`;
      if (statusId) url += `&statusId=${statusId}`;
      if (priorityId) url += `&priorityId=${priorityId}`;
      if (projectId) url += `&projectId=${projectId}`;
      if (assignedTo) url += `&assignedTo=${assignedTo}`;
      return client.get(url);
    },
    async projects(query, clientId = null) {
      const client = new APIClient();
      let url = `/Search/projects?query=${encodeURIComponent(query)}`;
      if (clientId) url += `&clientId=${clientId}`;
      return client.get(url);
    },
    async users(query, role = null, departmentId = null) {
      const client = new APIClient();
      let url = `/Search/users?query=${encodeURIComponent(query)}`;
      if (role) url += `&role=${role}`;
      if (departmentId) url += `&departmentId=${departmentId}`;
      return client.get(url);
    },
  },

  // Files Service
  Files: {
    async upload(taskId, formData) {
      const client = new APIClient();
      // Note: For file upload, we need to handle FormData differently
      const url = `${client.baseURL}/Files/upload/${taskId}`;
      const response = await fetch(url, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${client.getToken()}`,
        },
        body: formData, // FormData handles its own content-type
      });
      if (!response.ok) throw new Error(`Upload failed: ${response.status}`);
      return response.json();
    },
    async download(fileId) {
      const client = new APIClient();
      return client.get(`/Files/download/${fileId}`);
    },
    async delete(fileId) {
      const client = new APIClient();
      return client.delete(`/Files/${fileId}`);
    },
  },

  // AuditLogs Service
  AuditLogs: {
    async getAll(page = 1, pageSize = 50) {
      const client = new APIClient();
      return client.get(`/AuditLogs?page=${page}&pageSize=${pageSize}`);
    },
    async getByProject(projectId) {
      const client = new APIClient();
      return client.get(`/AuditLogs/project/${projectId}`);
    },
    async getByTask(taskId) {
      const client = new APIClient();
      return client.get(`/AuditLogs/task/${taskId}`);
    },
    async getByUser(userId, page = 1, pageSize = 50) {
      const client = new APIClient();
      return client.get(
        `/AuditLogs/user/${userId}?page=${page}&pageSize=${pageSize}`
      );
    },
    async getRecent(count = 10) {
      const client = new APIClient();
      return client.get(`/AuditLogs/recent?count=${count}`);
    },
  },

  // Keep legacy aliases for backward compatibility
  Employees: {
    getAll: () => API.Users.getAll(),
    getById: (id) => API.Users.getById(id),
    create: (data) => API.Users.create(data),
    update: (id, data) => API.Users.update(id, data),
    delete: (id) => API.Users.delete(id),
  },

  // Keep legacy Analytics alias
  Analytics: {
    getDashboardStats: () => API.Statistics.getDashboard(),
    getTaskStats: () => API.Statistics.getTasksByStatus(),
    getProjectStats: () => API.Statistics.getProjectProgress(),
  },
};

// Export for use in other files
window.API = API;

console.log("[API] API object loaded successfully:", Object.keys(API));
