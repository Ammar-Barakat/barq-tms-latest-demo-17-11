# Notification System Implementation

## Overview

The project now uses a REST-based notification system implemented in `frontend/scripts/utils/notifications.js` that polls the backend `API.Notifications` endpoints for unread notifications and counts. This replaces the previous SignalR-based push implementation.

## Features

- ✅ Polling for new unread notifications
- ✅ Toast notifications for new alerts
- ✅ Notification badge counter
- ✅ Mark as read / mark all as read
- ✅ Delete notifications
- ✅ Create notifications via API
- ✅ Sound alerts (optional)

## Files

### `/frontend/scripts/utils/notifications.js`

Polling-based notification service which:

- Periodically queries `API.Notifications.getUnread` and `API.Notifications.getUnreadCount`
- Detects new notifications and shows toasts
- Updates badge counts
- Exposes `on(event, callback)` hooks: `onReceive`, `onUpdate`, `onError`
- Methods: `markAsRead`, `markAllAsRead`, `delete`, `create`

## Setup / Usage

The notification service initializes automatically when:

1. DOM is loaded
2. User is authenticated (`auth.isAuthenticated()`)

The file `frontend/scripts/utils/notifications.js` depends on the existing utilities: `auth`, `API` and `utils`.

You can also interact with it programmatically:

```javascript
// Listen for new notifications
notificationService.on("onReceive", (notif) => console.log("new", notif));

// Refresh badge
await notificationService.updateNotificationBadge();

// Mark all as read
await notificationService.markAllAsRead();
```

## API Integration

The service uses these endpoints (already provided in `api.js`):

```javascript
API.Notifications.getByUser(userId);
API.Notifications.getUnread(userId);
API.Notifications.getUnreadCount(userId);
API.Notifications.markAsRead(notificationId);
API.Notifications.markAllAsRead(userId);
API.Notifications.delete(notificationId);
API.Notifications.create(notificationData);
```

The Notification DTO returned by the API typically contains fields like `notifId`, `userId`, `message`, `createdAt`, `isRead`, and optional `taskId` / `projectId` for navigation.

## Customization

- Change polling interval by editing `pollInterval` in the `NotificationService` constructor (default `30000` ms).
- Change toast duration by modifying the timeout in `notifications.js`.
- Disable sound by removing the `playNotificationSound()` call in `showToastFromDto`.

## Notes and Migration

- The SignalR-based implementation and documentation was removed in favor of a simpler REST/polling approach that avoids requiring the SignalR client library.
- If you later want to reintroduce push notifications, you can add a SignalR hub on the backend and rework the service to subscribe to hub events.

## Troubleshooting

- If badges don't update, call `notificationService.updateNotificationBadge()` and inspect network calls to `/api/Notifications/user/{userId}/count/unread`.
- If toasts don't appear, confirm `API.Notifications.getUnread` returns an array of unread notifications.

## Future Enhancements

- Add server-sent events (SSE) or WebSocket fallback for true push behavior
- Add a notification panel UI component to show history and actions
- Support browser native notifications via the Notifications API
