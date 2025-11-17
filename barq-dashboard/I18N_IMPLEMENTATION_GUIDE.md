# Applying i18n to All Pages - Quick Guide

## Step-by-Step Instructions

### 1. Update Script Loading Order

In **every HTML page**, update the script section to include i18n scripts **before** utils.js:

```html
<!-- Scripts -->
<!-- SignalR Client -->
<script src="https://cdn.jsdelivr.net/npm/@microsoft/signalr@7.0.0/dist/browser/signalr.min.js"></script>

<!-- i18n System (MUST load first) -->
<script src="../../scripts/utils/i18n.js"></script>

<!-- Core Utils -->
<script src="../../scripts/utils/utils.js"></script>
<script src="../../scripts/utils/components.js"></script>
<script src="../../scripts/utils/api.js"></script>
<script src="../../scripts/utils/auth.js"></script>

<!-- Notification System -->
<script src="../../scripts/utils/notifications.js"></script>
<script src="../../scripts/components/notification-panel.js"></script>

<!-- Language Switcher -->
<script src="../../scripts/components/language-switcher.js"></script>

<!-- Page Script -->
<script src="../../scripts/pages/[role]/[page].js"></script>
```

### 2. Add Translation Attributes to Common Elements

#### Sidebar Navigation

```html
<span class="nav-item-text" data-i18n="nav.dashboard">Dashboard</span>
<span class="nav-item-text" data-i18n="nav.tasks">Tasks</span>
<span class="nav-item-text" data-i18n="nav.projects">Projects</span>
<span class="nav-item-text" data-i18n="nav.team">Team</span>
<span class="nav-item-text" data-i18n="nav.calendar">Calendar</span>
<span class="nav-item-text" data-i18n="nav.analytics">Analytics</span>
<span class="nav-item-text" data-i18n="nav.clients">Clients</span>
<span class="nav-item-text" data-i18n="nav.settings">Settings</span>
<span class="nav-item-text" data-i18n="auth.logout">Logout</span>
```

#### Common Buttons

```html
<button class="btn btn-primary" data-i18n="common.save">Save</button>
<button class="btn btn-secondary" data-i18n="common.cancel">Cancel</button>
<button class="btn btn-danger" data-i18n="common.delete">Delete</button>
<button class="btn" data-i18n="common.edit">Edit</button>
<button class="btn" data-i18n="common.create">Create</button>
<button class="btn" data-i18n="common.update">Update</button>
<button class="btn" data-i18n="common.view">View</button>
<button class="btn" data-i18n="common.export">Export</button>
```

#### Page Titles

```html
<h1 data-i18n="dashboard.title">Dashboard</h1>
<h1 data-i18n="tasks.title">Tasks Management</h1>
<h1 data-i18n="projects.title">Projects</h1>
```

#### Search/Filter Inputs

```html
<input
  type="text"
  class="form-control"
  data-i18n-placeholder="common.search"
  placeholder="Search"
/>
```

### 3. Priority Pages to Update

Start with these high-traffic pages:

1. **Manager Dashboard** - `frontend/pages/manager/dashboard.html`
2. **Employee Dashboard** - `frontend/pages/employee/dashboard.html`
3. **Manager Tasks** - `frontend/pages/manager/tasks.html`
4. **Employee My Tasks** - `frontend/pages/employee/my-tasks.html`

### 4. Testing

After updating pages:

1. Open page in browser
2. Check console for i18n initialization message
3. Look for language switcher in header (flag icon + language code)
4. Click language switcher and select Arabic
5. Verify:
   - Text changes to Arabic
   - Layout switches to RTL
   - All marked elements are translated

## Translation Key Reference

Common keys from `en.json` and `ar.json`:

### Navigation

- `nav.dashboard` - Dashboard / لوحة التحكم
- `nav.tasks` - Tasks / المهام
- `nav.projects` - Projects / المشاريع
- `nav.team` - Team / الفريق
- `nav.calendar` - Calendar / التقويم
- `nav.analytics` - Analytics / التحليلات
- `nav.clients` - Clients / العملاء
- `nav.settings` - Settings / الإعدادات

### Common Actions

- `common.save` - Save / حفظ
- `common.cancel` - Cancel / إلغاء
- `common.delete` - Delete / حذف
- `common.edit` - Edit / تعديل
- `common.create` - Create / إنشاء
- `common.search` - Search / بحث
- `common.filter` - Filter / تصفية
- `common.export` - Export / تصدير

### Status

- `status.todo` - To Do / للإنجاز
- `status.inProgress` - In Progress / قيد التنفيذ
- `status.done` - Done / منجز
- `status.active` - Active / نشط
- `status.completed` - Completed / مكتمل

### Priority

- `priority.critical` - Critical / حرج
- `priority.high` - High / عالي
- `priority.medium` - Medium / متوسط
- `priority.low` - Low / منخفض

## Automation Script

To apply i18n scripts to all pages at once, you can run this in PowerShell from project root:

```powershell
# This will be done programmatically by the agent
# Adding i18n and language-switcher scripts to all HTML pages
```

## Notes

- i18n.js **must** load before other utility scripts
- Translation files (`en.json`, `ar.json`) are in project root
- Language preference is saved in localStorage
- RTL styles are automatically applied for Arabic
- Numbers/dates are formatted according to locale
