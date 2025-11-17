// SignalR Notification Service
class NotificationService {
  constructor() {
    this.connection = null;
    this.isConnected = false;
    this.callbacks = {
      onReceive: [],
      onConnect: [],
      onDisconnect: [],
      onError: [],
    };
    this.reconnectAttempts = 0;
    this.maxReconnectAttempts = 5;
    this.reconnectDelay = 3000;
  }

  // Initialize SignalR connection
  async initialize() {
    try {
      const token = localStorage.getItem("auth_token");
      if (!token) {
        console.warn(
          "[Notifications] No auth token found, skipping initialization"
        );
        return;
      }

      // Create SignalR connection
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl("https://localhost:44383/notificationHub", {
          accessTokenFactory: () => token,
          skipNegotiation: false,
          transport:
            signalR.HttpTransportType.WebSockets |
            signalR.HttpTransportType.ServerSentEvents |
            signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.elapsedMilliseconds < 60000) {
              return Math.random() * 10000;
            } else {
              return null; // Stop reconnecting after 1 minute
            }
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up event handlers
      this.setupEventHandlers();

      // Start connection
      await this.start();

      console.log("[Notifications] Service initialized successfully");
    } catch (error) {
      console.error("[Notifications] Initialization failed:", error);
      this.triggerCallbacks("onError", error);
    }
  }

  // Setup SignalR event handlers
  setupEventHandlers() {
    if (!this.connection) return;

    // Handle incoming notifications
    this.connection.on("ReceiveNotification", (notification) => {
      console.log("[Notifications] Received:", notification);
      this.handleNotification(notification);
    });

    // Handle notification updates
    this.connection.on("NotificationRead", (notificationId) => {
      console.log("[Notifications] Marked as read:", notificationId);
      this.triggerCallbacks("onReceive", { type: "read", id: notificationId });
    });

    // Handle notification deletions
    this.connection.on("NotificationDeleted", (notificationId) => {
      console.log("[Notifications] Deleted:", notificationId);
      this.triggerCallbacks("onReceive", {
        type: "deleted",
        id: notificationId,
      });
    });

    // Handle connection state changes
    this.connection.onclose((error) => {
      this.isConnected = false;
      console.warn("[Notifications] Connection closed:", error);
      this.triggerCallbacks("onDisconnect", error);
      this.attemptReconnect();
    });

    this.connection.onreconnecting((error) => {
      console.log("[Notifications] Reconnecting...", error);
    });

    this.connection.onreconnected((connectionId) => {
      this.isConnected = true;
      this.reconnectAttempts = 0;
      console.log("[Notifications] Reconnected:", connectionId);
      this.triggerCallbacks("onConnect");
    });
  }

  // Start SignalR connection
  async start() {
    if (!this.connection) {
      throw new Error("Connection not initialized");
    }

    try {
      await this.connection.start();
      this.isConnected = true;
      this.reconnectAttempts = 0;
      console.log("[Notifications] Connected successfully");
      this.triggerCallbacks("onConnect");

      // Request initial notification count
      this.refreshNotificationCount();
    } catch (error) {
      console.error("[Notifications] Connection failed:", error);
      this.isConnected = false;
      this.triggerCallbacks("onError", error);
      throw error;
    }
  }

  // Stop SignalR connection
  async stop() {
    if (this.connection) {
      try {
        await this.connection.stop();
        this.isConnected = false;
        console.log("[Notifications] Disconnected");
        this.triggerCallbacks("onDisconnect");
      } catch (error) {
        console.error("[Notifications] Error stopping connection:", error);
      }
    }
  }

  // Handle received notification
  handleNotification(notification) {
    // Show toast notification
    this.showToast(notification);

    // Update notification badge
    this.updateNotificationBadge();

    // Trigger callbacks
    this.triggerCallbacks("onReceive", notification);

    // Play sound (optional)
    this.playNotificationSound();
  }

  // Show toast notification
  showToast(notification) {
    const toast = document.createElement("div");
    toast.className = "notification-toast";

    const icon = this.getNotificationIcon(notification.Type || "info");

    toast.innerHTML = `
      <div class="notification-toast-content">
        <div class="notification-toast-icon">${icon}</div>
        <div class="notification-toast-body">
          <div class="notification-toast-title">${this.escapeHtml(
            notification.Title || "Notification"
          )}</div>
          <div class="notification-toast-message">${this.escapeHtml(
            notification.Message || ""
          )}</div>
          <div class="notification-toast-time">${this.getRelativeTime(
            notification.CreatedAt || new Date()
          )}</div>
        </div>
        <button class="notification-toast-close" onclick="this.parentElement.parentElement.remove()">
          <i class="fa-solid fa-xmark"></i>
        </button>
      </div>
    `;

    // Add click handler to mark as read and navigate
    toast.addEventListener("click", (e) => {
      if (!e.target.closest(".notification-toast-close")) {
        if (notification.NotificationId) {
          this.markAsRead(notification.NotificationId);
        }
        if (notification.Link) {
          window.location.href = notification.Link;
        }
      }
    });

    document.body.appendChild(toast);

    // Auto remove after 5 seconds
    setTimeout(() => {
      toast.style.animation = "slideOut 0.3s ease";
      setTimeout(() => toast.remove(), 300);
    }, 5000);
  }

  // Get notification icon based on type
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

  // Update notification badge count
  async updateNotificationBadge() {
    try {
      const currentUser = auth.getCurrentUser();
      if (!currentUser) return;

      const response = await API.Notifications.getUnreadCount(
        currentUser.UserId
      );
      const count = response.count || response.Count || 0;

      // Update all notification badges
      const badges = document.querySelectorAll(
        ".notification-btn .badge, .header-btn.notification-btn .badge"
      );
      badges.forEach((badge) => {
        badge.textContent = count;
        badge.style.display = count > 0 ? "flex" : "none";
      });
    } catch (error) {
      console.error("[Notifications] Failed to update badge:", error);
    }
  }

  // Refresh notification count
  refreshNotificationCount() {
    this.updateNotificationBadge();
  }

  // Mark notification as read
  async markAsRead(notificationId) {
    try {
      await API.Notifications.markAsRead(notificationId);
      await this.updateNotificationBadge();
    } catch (error) {
      console.error("[Notifications] Failed to mark as read:", error);
    }
  }

  // Mark all notifications as read
  async markAllAsRead() {
    try {
      const currentUser = auth.getCurrentUser();
      if (!currentUser) return;

      await API.Notifications.markAllAsRead(currentUser.UserId);
      await this.updateNotificationBadge();
      utils.showSuccess("All notifications marked as read");
    } catch (error) {
      console.error("[Notifications] Failed to mark all as read:", error);
      utils.showError("Failed to mark notifications as read");
    }
  }

  // Play notification sound
  playNotificationSound() {
    try {
      const audio = new Audio(
        "data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBTGH0fPTgjMGHm7A7+OZSA0PVqzn77BdGAg+ltrzxnMnBSl+zPLaizsIGGS57OihUBALTKXh8bllHAU2jtTzzn0vBSd6yu/glEcMEVOn4/G2aR0GOZPc8st3KwUme8rx3I0+CRVht+vpoVkTCkql4/K6aB0FOo/V88V4LgUgdcjw2YpAChNdsOnrtV4YBzyU2/HEdigFKnvK8dyOPwkVYrfs6aBaDgtJo+PxuGgdBjqQ1fPEeC0FI3XJ8NiJPwoUXK/q7LVeGAc8lNvxxHYoBSt7y/HcjT8JFmG37OmgWg0LRqLi8bhqHgU7kdXzxHgtBSN0yPDZiz8KFF2v6+y1XhgHO5Tb8sR2KAUsesrx3I0+CRZiuO3poFkNDEWh4vG3ax4FO5DV8sV5LgUkdcnv2Is/CxRcr+vstl0YCDuV2/HFdisFKnrK8d2MPwkWYrju6aFaDgtFouLxt2seBS+T1fPEeS4FJHfK79iLQAoUXK/q7LZdGAg6lNvxxXYrBCp7yvHdjD4KFmG47umgWg4LQ6Lh8LhqHgU6kdT0xXktBCR4ye/YikALFVyu6uy2XRgIPJPb8cV3KwQqe8rx3Iw+ChVhue3poVoOCkSi4fG3ah4FOZPVfPFeC0FI3bJ8NmJPgoUXrfr7bVeGAc7ltvyxHYpBCt8yfDZi0ALFFys6uy1XhgHPJXa8sR2KQQresjw2YtACxRcr+rstV4YBzyV2vLDdikELH7G8dmKQAsRW7Do7LReGQY6k9vxw3opBCx9x/DZiz8KFFuv6+y0XhkGOpTa8sR2KQQsecjw2Ys/ChRbr+vstF4YCDqT2/HEdikELHnJ8NmLPwoTWrDp67ReGAY7k9rxw3YpBCx6yPDZiz4LEFq0LAALG0ujA="
      );
      audio.volume = 0.3;
      audio
        .play()
        .catch((e) => console.log("[Notifications] Could not play sound:", e));
    } catch (error) {
      // Silent fail for sound
    }
  }

  // Attempt to reconnect
  attemptReconnect() {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error("[Notifications] Max reconnection attempts reached");
      return;
    }

    this.reconnectAttempts++;
    console.log(
      `[Notifications] Reconnecting... Attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts}`
    );

    setTimeout(() => {
      this.start().catch((error) => {
        console.error("[Notifications] Reconnection failed:", error);
      });
    }, this.reconnectDelay * this.reconnectAttempts);
  }

  // Register callbacks
  on(event, callback) {
    if (this.callbacks[event]) {
      this.callbacks[event].push(callback);
    }
  }

  // Unregister callbacks
  off(event, callback) {
    if (this.callbacks[event]) {
      this.callbacks[event] = this.callbacks[event].filter(
        (cb) => cb !== callback
      );
    }
  }

  // Trigger callbacks
  triggerCallbacks(event, data) {
    if (this.callbacks[event]) {
      this.callbacks[event].forEach((callback) => {
        try {
          callback(data);
        } catch (error) {
          console.error(`[Notifications] Error in ${event} callback:`, error);
        }
      });
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

// Create global notification service instance
const notificationService = new NotificationService();

// Auto-initialize when DOM is ready
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", () => {
    if (auth.isAuthenticated()) {
      notificationService.initialize();
    }
  });
} else {
  if (auth.isAuthenticated()) {
    notificationService.initialize();
  }
}
