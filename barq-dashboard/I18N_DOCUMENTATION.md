# Internationalization (i18n) System

Complete localization system for Barq TMS Dashboard supporting multiple languages with RTL/LTR direction switching.

## Features

- ✅ **Multi-language Support**: English & Arabic (extensible to more languages)
- ✅ **RTL/LTR Direction**: Automatic layout direction switching
- ✅ **Dynamic Translation**: Real-time language switching without page reload
- ✅ **Nested Keys**: Support for nested translation keys using dot notation
- ✅ **String Interpolation**: Dynamic parameter replacement in translations
- ✅ **Number/Date Formatting**: Locale-aware formatting
- ✅ **Language Persistence**: Remembers user's language preference
- ✅ **Easy Integration**: Simple data attributes for translation

## Quick Start

### 1. Include i18n Script

Add i18n script **before** other utility scripts in your HTML:

```html
<!-- i18n must load first -->
<script src="../../scripts/utils/i18n.js"></script>
<script src="../../scripts/utils/utils.js"></script>
<script src="../../scripts/utils/api.js"></script>
<!-- ... other scripts -->
```

### 2. Add Language Switcher

Include language switcher component (optional but recommended):

```html
<script src="../../scripts/components/language-switcher.js"></script>
```

This automatically adds a language switcher to your header.

### 3. Mark Translatable Elements

Add `data-i18n` attribute with translation key:

```html
<!-- Sidebar Navigation -->
<span class="nav-item-text" data-i18n="nav.dashboard">Dashboard</span>
<span class="nav-item-text" data-i18n="nav.tasks">Tasks</span>
<span class="nav-item-text" data-i18n="nav.projects">Projects</span>

<!-- Buttons -->
<button class="btn btn-primary" data-i18n="common.save">Save</button>
<button class="btn btn-secondary" data-i18n="common.cancel">Cancel</button>

<!-- Headings -->
<h1 data-i18n="dashboard.title">Dashboard</h1>
<h2 data-i18n="dashboard.overview">Overview</h2>
```

### 4. Translate Placeholders

Use `data-i18n-placeholder` for input placeholders:

```html
<input
  type="text"
  class="form-control"
  data-i18n-placeholder="common.search"
  placeholder="Search"
/>
```

### 5. Translate Tooltips/Titles

Use `data-i18n-title` for title attributes:

```html
<button class="icon-btn" data-i18n-title="common.edit" title="Edit">
  <i class="fa-solid fa-edit"></i>
</button>
```

## Translation Files

Translation files are located at the project root:

- `en.json` - English translations
- `ar.json` - Arabic translations

### Structure

```json
{
  "nav": {
    "dashboard": "Dashboard",
    "tasks": "Tasks",
    "projects": "Projects"
  },
  "common": {
    "save": "Save",
    "cancel": "Cancel",
    "delete": "Delete"
  },
  "dashboard": {
    "title": "Dashboard",
    "welcome": "Welcome",
    "totalTasks": "Total Tasks"
  }
}
```

## JavaScript API

### Get Translation

```javascript
// Basic translation
const text = i18n.t("nav.dashboard"); // "Dashboard" or "لوحة التحكم"

// With parameters
const text = i18n.t("validation.minLength", { min: 5 });
// English: "Minimum length is 5 characters"
// Arabic: "الحد الأدنى للطول هو 5 حرفاً"
```

### Switch Language

```javascript
// Switch to Arabic
await i18n.switchLanguage("ar");

// Switch to English
await i18n.switchLanguage("en");
```

### Check Current Language

```javascript
const currentLang = i18n.getCurrentLanguage(); // 'en' or 'ar'
const isRTL = i18n.isRTL(); // true for Arabic
```

### Format Numbers/Dates

```javascript
// Format number
const formatted = i18n.formatNumber(1234.56);
// English: "1,234.56"
// Arabic: "١٬٢٣٤٫٥٦"

// Format date
const date = i18n.formatDate(new Date(), {
  year: "numeric",
  month: "long",
  day: "numeric",
});
// English: "November 17, 2025"
// Arabic: "١٧ نوفمبر ٢٠٢٥"

// Format currency
const price = i18n.formatCurrency(99.99, "USD");
// English: "$99.99"
// Arabic: "US$٩٩٫٩٩"
```

## Dynamic Content Translation

For dynamically generated content:

```javascript
// Method 1: Use i18n.t() when creating HTML
const html = `
  <button class="btn">${i18n.t("common.save")}</button>
  <h2>${i18n.t("tasks.title")}</h2>
`;

// Method 2: Add data-i18n and call translatePage()
container.innerHTML = `
  <button class="btn" data-i18n="common.save">Save</button>
`;
i18n.translatePage(); // Re-translate all elements
```

## Language Change Events

Listen to language changes:

```javascript
window.addEventListener("languageChanged", (event) => {
  const newLanguage = event.detail.language;
  console.log(`Language changed to: ${newLanguage}`);

  // Reload data with new language
  loadData();

  // Update dynamic content
  updateCharts();
});
```

## RTL Styling

RTL styles are automatically applied when Arabic is selected. The system:

1. Sets `dir="rtl"` on `<html>` element
2. Adds `.rtl` class to `<body>`
3. Applies RTL-specific CSS from `rtl.css`

### Custom RTL Styles

Add RTL-specific styles in your CSS:

```css
/* Normal (LTR) style */
.my-component {
  margin-left: 20px;
}

/* RTL override */
html[dir="rtl"] .my-component {
  margin-left: 0;
  margin-right: 20px;
}
```

## Best Practices

### 1. Always Use Translation Keys

❌ **Bad:**

```html
<button>Save</button>
```

✅ **Good:**

```html
<button data-i18n="common.save">Save</button>
```

### 2. Use Semantic Keys

❌ **Bad:**

```javascript
i18n.t("text1"); // What is text1?
```

✅ **Good:**

```javascript
i18n.t("tasks.createTask"); // Clear and descriptive
```

### 3. Group Related Translations

```json
{
  "tasks": {
    "title": "Tasks",
    "createTask": "Create Task",
    "editTask": "Edit Task",
    "deleteTask": "Delete Task"
  }
}
```

### 4. Keep Numbers LTR in Arabic

For numbers, IDs, dates in Arabic, add `.number` class:

```html
<span class="number">1234</span>
<!-- Stays LTR even in RTL -->
```

### 5. Test Both Languages

Always test your interface in both English and Arabic to ensure:

- All text is translated
- Layout works correctly in both directions
- No text overflow issues
- Icons and buttons are properly aligned

## Adding New Languages

### 1. Create Translation File

Create `[language-code].json` in project root:

```json
{
  "nav": { ... },
  "common": { ... },
  ...
}
```

### 2. Update i18n.js

Add language to `getAvailableLanguages()`:

```javascript
getAvailableLanguages() {
  return ['en', 'ar', 'fr']; // Add 'fr' for French
}
```

If RTL, add to `rtlLanguages`:

```javascript
this.rtlLanguages = ["ar", "he", "fa", "ur"];
```

### 3. Update Language Switcher

Add language to `language-switcher.js`:

```javascript
this.languages = {
  en: { name: "English", flag: "🇬🇧", dir: "ltr" },
  ar: { name: "العربية", flag: "🇸🇦", dir: "rtl" },
  fr: { name: "Français", flag: "🇫🇷", dir: "ltr" },
};
```

## Troubleshooting

### Translations Not Showing

1. Check console for errors
2. Verify JSON files are accessible at `/en.json` and `/ar.json`
3. Ensure i18n.js loads before other scripts
4. Check translation key exists in JSON file

### RTL Layout Issues

1. Check `rtl.css` is imported in `main.css`
2. Verify `dir="rtl"` is set on `<html>` element
3. Use browser inspector to check applied styles
4. Add custom RTL overrides if needed

### Language Not Persisting

Check localStorage is enabled and working:

```javascript
console.log(localStorage.getItem("language"));
```

## Example: Complete Page Setup

```html
<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title data-i18n="dashboard.title">Dashboard</title>
    <link rel="stylesheet" href="../../styles/main.css" />
  </head>
  <body>
    <div class="app">
      <aside class="sidebar">
        <nav class="sidebar-nav">
          <a href="#" class="nav-item">
            <span class="nav-item-icon"><i class="fa-solid fa-gauge"></i></span>
            <span class="nav-item-text" data-i18n="nav.dashboard"
              >Dashboard</span
            >
          </a>
          <a href="#" class="nav-item">
            <span class="nav-item-icon"
              ><i class="fa-solid fa-list-check"></i
            ></span>
            <span class="nav-item-text" data-i18n="nav.tasks">Tasks</span>
          </a>
        </nav>
      </aside>

      <main class="main-content">
        <h1 data-i18n="dashboard.welcome">Welcome</h1>
        <button class="btn btn-primary" data-i18n="common.save">Save</button>
      </main>
    </div>

    <!-- Load i18n FIRST -->
    <script src="../../scripts/utils/i18n.js"></script>
    <script src="../../scripts/utils/utils.js"></script>
    <script src="../../scripts/utils/api.js"></script>
    <script src="../../scripts/utils/auth.js"></script>

    <!-- Language Switcher Component -->
    <script src="../../scripts/components/language-switcher.js"></script>

    <!-- Page Script -->
    <script src="../../scripts/pages/dashboard.js"></script>
  </body>
</html>
```

## Support

For issues or questions about the i18n system:

- Check translation keys in `en.json` and `ar.json`
- Review RTL styles in `rtl.css`
- See implementation examples in page files
- Check browser console for errors
