// Notification Panel Component
class NotificationPanel {
  constructor() {
    this.isOpen = false;
    this.notifications = [];
    this.panel = null;
    this.initialized = false;
  }

  // Initialize the panel
  async initialize() {
    if (this.initialized) return;

    this.createPanel();
    this.setupEventListeners();
    await this.loadNotifications();
    this.initialized = true;

    // Listen to notification service events
    notificationService.on("onReceive", (notification) => {
      this.handleNewNotification(notification);
    });

    console.log("[NotificationPanel] Initialized");
  }

  // Create panel HTML
  createPanel() {
    const panel = document.createElement("div");
    panel.className = "notification-panel";
    panel.id = "notificationPanel";
    panel.innerHTML = `
      <div class="notification-panel-header">
        <h3 class="notification-panel-title">Notifications</h3>
        <div class="notification-panel-actions">
          <button class="notification-panel-btn" id="markAllReadBtn">
            Mark all read
          </button>
        </div>
      </div>
      <div class="notification-panel-list" id="notificationList">
        <div class="notification-panel-empty">
          <i class="fa-solid fa-bell-slash"></i>
          <p>No notifications</p>
        </div>
      </div>
    `;

    document.body.appendChild(panel);
    this.panel = panel;

    // Close panel when clicking outside
    document.addEventListener("click", (e) => {
      if (
        this.isOpen &&
        !panel.contains(e.target) &&
        !e.target.closest(".notification-btn")
      ) {
        this.close();
      }
    });
  }

  // Setup event listeners
  setupEventListeners() {
    const markAllReadBtn = document.getElementById("markAllReadBtn");
    if (markAllReadBtn) {
      markAllReadBtn.addEventListener("click", async () => {
        await this.markAllAsRead();
      });
    }

    // Setup notification button click handler
    const notificationBtns = document.querySelectorAll(
      ".notification-btn, .header-btn.notification-btn"
    );
    notificationBtns.forEach((btn) => {
      btn.addEventListener("click", (e) => {
        e.stopPropagation();
        this.toggle();
      });
    });
  }

  // Toggle panel
  toggle() {
    if (this.isOpen) {
      this.close();
    } else {
      this.open();
    }
  }

  // Open panel
  async open() {
    if (!this.initialized) {
      await this.initialize();
    }

    this.panel.classList.add("active");
    this.isOpen = true;
    await this.loadNotifications();
  }

  // Close panel
  close() {
    this.panel.classList.remove("active");
    this.isOpen = false;
  }

  // Load notifications from API
  async loadNotifications() {
    try {
      const currentUser = auth.getCurrentUser();
      if (!currentUser) return;

      const notifications = await API.Notifications.getByUser(
        currentUser.UserId
      );
      this.notifications = Array.isArray(notifications) ? notifications : [];
      this.render();
    } catch (error) {
      console.error("[NotificationPanel] Failed to load notifications:", error);
    }
  }

  // Handle new notification from SignalR
  handleNewNotification(notification) {
    if (notification.type === "read") {
      // Mark notification as read
      const index = this.notifications.findIndex(
        (n) => n.NotificationId === notification.id
      );
      if (index !== -1) {
        this.notifications[index].IsRead = true;
      }
    } else if (notification.type === "deleted") {
      // Remove notification
      this.notifications = this.notifications.filter(
        (n) => n.NotificationId !== notification.id
      );
    } else {
      // Add new notification at the beginning
      this.notifications.unshift(notification);
    }

    this.render();
  }

  // Render notifications
  render() {
    const listContainer = document.getElementById("notificationList");
    if (!listContainer) return;

    if (this.notifications.length === 0) {
      listContainer.innerHTML = `
        <div class="notification-panel-empty">
          <i class="fa-solid fa-bell-slash"></i>
          <p>No notifications</p>
        </div>
      `;
      return;
    }

    listContainer.innerHTML = this.notifications
      .map((notification) => this.renderNotificationItem(notification))
      .join("");

    // Add event listeners to notification items
    listContainer
      .querySelectorAll(".notification-item")
      .forEach((item, index) => {
        const notification = this.notifications[index];

        item.addEventListener("click", async (e) => {
          if (e.target.closest(".notification-item-action-btn")) return;

          await this.markAsRead(notification.NotificationId);

          if (notification.Link) {
            window.location.href = notification.Link;
          }
        });

        const deleteBtn = item.querySelector(".notification-delete-btn");
        if (deleteBtn) {
          deleteBtn.addEventListener("click", async (e) => {
            e.stopPropagation();
            await this.deleteNotification(notification.NotificationId);
          });
        }

        const readBtn = item.querySelector(".notification-read-btn");
        if (readBtn) {
          readBtn.addEventListener("click", async (e) => {
            e.stopPropagation();
            await this.markAsRead(notification.NotificationId);
          });
        }
      });
  }

  // Render single notification item
  renderNotificationItem(notification) {
    const isRead = notification.IsRead;
    const icon = this.getNotificationIcon(notification.Type || "info");
    const time = this.getRelativeTime(notification.CreatedAt);

    return `
      <div class="notification-item ${isRead ? "read" : "unread"}">
        <div class="notification-item-icon">${icon}</div>
        <div class="notification-item-content">
          <div class="notification-item-title">${this.escapeHtml(
            notification.Title || "Notification"
          )}</div>
          <div class="notification-item-message">${this.escapeHtml(
            notification.Message || ""
          )}</div>
          <div class="notification-item-time">${time}</div>
        </div>
        <div class="notification-item-actions">
          ${
            !isRead
              ? `
            <button class="notification-item-action-btn notification-read-btn" title="Mark as read">
              <i class="fa-solid fa-check"></i>
            </button>
          `
              : ""
          }
          <button class="notification-item-action-btn notification-delete-btn" title="Delete">
            <i class="fa-solid fa-trash"></i>
          </button>
        </div>
      </div>
    `;
  }

  // Get notification icon
  getNotificationIcon(type) {
    const icons = {
      info: '<i class="fa-solid fa-circle-info"></i>',
      success: '<i class="fa-solid fa-circle-check"></i>',
      warning: '<i class="fa-solid fa-triangle-exclamation"></i>',
      error: '<i class="fa-solid fa-circle-exclamation"></i>',
      task: '<i class="fa-solid fa-list-check"></i>',
      project: '<i class="fa-solid fa-folder-open"></i>',
      message: '<i class="fa-solid fa-message"></i>',
      reminder: '<i class="fa-solid fa-bell"></i>',
    };
    return icons[type] || icons.info;
  }

  // Mark notification as read
  async markAsRead(notificationId) {
    try {
      await API.Notifications.markAsRead(notificationId);

      const index = this.notifications.findIndex(
        (n) => n.NotificationId === notificationId
      );
      if (index !== -1) {
        this.notifications[index].IsRead = true;
      }

      this.render();
      notificationService.updateNotificationBadge();
    } catch (error) {
      console.error("[NotificationPanel] Failed to mark as read:", error);
      utils.showError("Failed to mark notification as read");
    }
  }

  // Mark all as read
  async markAllAsRead() {
    try {
      await notificationService.markAllAsRead();

      this.notifications.forEach((n) => (n.IsRead = true));
      this.render();
    } catch (error) {
      console.error("[NotificationPanel] Failed to mark all as read:", error);
    }
  }

  // Delete notification
  async deleteNotification(notificationId) {
    try {
      await API.Notifications.delete(notificationId);

      this.notifications = this.notifications.filter(
        (n) => n.NotificationId !== notificationId
      );
      this.render();
      notificationService.updateNotificationBadge();
      utils.showSuccess("Notification deleted");
    } catch (error) {
      console.error(
        "[NotificationPanel] Failed to delete notification:",
        error
      );
      utils.showError("Failed to delete notification");
    }
  }

  // Utility: Escape HTML
  escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
  }

  // Utility: Get relative time
  getRelativeTime(date) {
    const now = new Date();
    const then = new Date(date);
    const seconds = Math.floor((now - then) / 1000);

    if (seconds < 60) return "Just now";
    if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
    if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
    if (seconds < 604800) return `${Math.floor(seconds / 86400)}d ago`;
    return then.toLocaleDateString();
  }
}

// Create global notification panel instance
const notificationPanel = new NotificationPanel();

// Auto-initialize when DOM is ready
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", () => {
    if (auth.isAuthenticated()) {
      notificationPanel.initialize();
    }
  });
} else {
  if (auth.isAuthenticated()) {
    notificationPanel.initialize();
  }
}
