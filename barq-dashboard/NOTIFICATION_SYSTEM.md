# Notification System Implementation

## Overview

A real-time notification system using SignalR for push notifications from the backend API.

## Features

- ✅ Real-time push notifications via SignalR
- ✅ Toast notifications for new alerts
- ✅ Notification panel with full history
- ✅ Mark as read/unread functionality
- ✅ Delete notifications
- ✅ Notification badge counter
- ✅ Auto-reconnection on connection loss
- ✅ Sound alerts (optional)
- ✅ Responsive design

## Files Added

### 1. `/frontend/scripts/utils/notifications.js`

SignalR service for managing WebSocket connections and receiving real-time notifications.

**Key Features:**

- Establishes SignalR connection with authentication
- Handles reconnection automatically
- Shows toast notifications
- Updates badge counts
- Plays notification sounds

### 2. `/frontend/scripts/components/notification-panel.js`

UI component for displaying notification history in a dropdown panel.

**Key Features:**

- Displays all user notifications
- Mark individual notifications as read
- Mark all notifications as read
- Delete notifications
- Navigate to related pages via notification links

### 3. `/frontend/styles/components.css` (updated)

Added comprehensive styles for:

- Toast notifications (sliding in from right)
- Notification panel dropdown
- Notification items
- Animations and transitions

## Setup Instructions

### 1. Add SignalR Client Library

Add the SignalR client library to your HTML pages **before** other scripts:

```html
<!-- SignalR Client -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.0/dist/browser/signalr.min.js"></script>

<!-- Your existing scripts -->
<script src="../../scripts/utils/utils.js"></script>
<script src="../../scripts/utils/components.js"></script>
<script src="../../scripts/utils/api.js"></script>
<script src="../../scripts/utils/auth.js"></script>

<!-- Notification System -->
<script src="../../scripts/utils/notifications.js"></script>
<script src="../../scripts/components/notification-panel.js"></script>

<!-- Page-specific script -->
<script src="../../scripts/pages/[role]/[page].js"></script>
```

### 2. Update Notification Button

Ensure your notification button has the correct class for the panel to attach:

```html
<button class="header-btn notification-btn">
  <i class="fa-solid fa-bell"></i>
  <span class="badge">0</span>
</button>
```

### 3. Backend Configuration

Ensure your backend SignalR hub is configured at:

```
https://localhost:44383/notificationHub
```

If using a different URL, update the connection URL in `notifications.js`:

```javascript
.withUrl('YOUR_API_URL/notificationHub', {
  accessTokenFactory: () => token,
  // ...
})
```

## Usage

### Automatic Initialization

The notification system initializes automatically when:

1. DOM is loaded
2. User is authenticated
3. SignalR library is available

### Manual Control

#### Access Notification Service

```javascript
// Check connection status
if (notificationService.isConnected) {
  console.log("Connected to notifications");
}

// Manually refresh badge count
await notificationService.updateNotificationBadge();

// Mark all as read
await notificationService.markAllAsRead();
```

#### Listen to Events

```javascript
// Listen for new notifications
notificationService.on("onReceive", (notification) => {
  console.log("New notification:", notification);
  // Your custom logic here
});

// Listen for connection events
notificationService.on("onConnect", () => {
  console.log("Connected to notification service");
});

notificationService.on("onDisconnect", (error) => {
  console.log("Disconnected:", error);
});
```

#### Open/Close Panel Programmatically

```javascript
// Open notification panel
notificationPanel.open();

// Close notification panel
notificationPanel.close();

// Toggle panel
notificationPanel.toggle();
```

## API Integration

### Required API Endpoints

The system uses these API endpoints (already implemented in `api.js`):

```javascript
// Get all notifications for user
API.Notifications.getByUser(userId);

// Get unread notifications
API.Notifications.getUnread(userId);

// Get unread count
API.Notifications.getUnreadCount(userId);

// Mark as read
API.Notifications.markAsRead(notificationId);

// Mark all as read
API.Notifications.markAllAsRead(userId);

// Delete notification
API.Notifications.delete(notificationId);

// Create notification (admin only)
API.Notifications.create(notificationData);
```

### SignalR Hub Events

The backend should emit these events:

#### Server → Client Events:

- `ReceiveNotification` - New notification received
- `NotificationRead` - Notification marked as read
- `NotificationDeleted` - Notification deleted

Example notification object:

```javascript
{
  NotificationId: 123,
  UserId: 456,
  Title: "New Task Assigned",
  Message: "You have been assigned to Project Alpha",
  Type: "task", // info|success|warning|error|task|project|message|reminder
  Link: "/pages/employee/my-tasks.html",
  IsRead: false,
  CreatedAt: "2025-11-17T10:30:00Z"
}
```

## Customization

### Change Toast Duration

In `notifications.js`, line ~195:

```javascript
setTimeout(() => {
  toast.style.animation = "slideOut 0.3s ease";
  setTimeout(() => toast.remove(), 300);
}, 5000); // Change 5000 to desired milliseconds
```

### Disable Sound

In `notifications.js`, line ~147, comment out:

```javascript
// this.playNotificationSound();
```

### Change Notification Types

Add custom notification types in both files:

**notifications.js** (line ~207):

```javascript
getNotificationIcon(type) {
  const icons = {
    // ...existing icons
    'custom': '<i class="fa-solid fa-your-icon"></i>'
  };
  return icons[type] || icons.info;
}
```

**notification-panel.js** (line ~208):

```javascript
getNotificationIcon(type) {
  // Add same custom type
}
```

### Modify Styles

Edit `/frontend/styles/components.css` starting at line 559:

- `.notification-toast` - Toast notification styles
- `.notification-panel` - Panel dropdown styles
- `.notification-item` - Individual notification item styles

## Troubleshooting

### Notifications Not Appearing

1. Check browser console for errors
2. Verify SignalR library is loaded: `typeof signalR !== 'undefined'`
3. Check authentication: `auth.isAuthenticated()`
4. Verify API URL is correct
5. Check network tab for WebSocket connection

### Connection Issues

```javascript
// Check connection status
console.log("Connected:", notificationService.isConnected);

// Check connection state
console.log("Connection:", notificationService.connection?.state);
```

### Badge Not Updating

Manually trigger update:

```javascript
await notificationService.updateNotificationBadge();
```

### CORS Issues

Ensure backend SignalR hub allows your frontend origin:

```csharp
// Backend CORS configuration
services.AddCors(options => {
  options.AddPolicy("AllowFrontend", builder => {
    builder.WithOrigins("http://localhost:5500")
           .AllowCredentials()
           .AllowAnyHeader()
           .AllowAnyMethod();
  });
});
```

## Testing

### Test Toast Notification

```javascript
// Manually trigger a test notification
notificationService.handleNotification({
  NotificationId: 999,
  Title: "Test Notification",
  Message: "This is a test message",
  Type: "info",
  CreatedAt: new Date(),
});
```

### Test Panel

```javascript
// Open panel and load notifications
await notificationPanel.open();
```

## Performance

### Optimization Tips

1. Toast notifications auto-remove after 5 seconds
2. Panel lazy-loads on first open
3. Badge updates are debounced
4. Connection uses automatic reconnection with exponential backoff
5. Only active when user is authenticated

### Resource Cleanup

Connection automatically stops on:

- User logout
- Page unload
- Extended inactivity (backend timeout)

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Security

- Authentication token sent with every SignalR request
- All API calls use JWT bearer token
- XSS protection via HTML escaping
- HTTPS required for production

## Future Enhancements

- [ ] Browser notification API integration
- [ ] Notification categories/filters
- [ ] Bulk actions (delete multiple)
- [ ] Notification preferences
- [ ] Rich media notifications (images, buttons)
- [ ] Notification search
- [ ] Notification archiving
