# Barq TMS - Troubleshooting Guide

## Quick Start

### Option 1: Using index.html (Recommended for first-time setup)

1. Open `index.html` in your browser from the project root
2. This will run a system check and show you if everything is configured correctly
3. Click "Go to Login Page" if all checks pass

### Option 2: Direct Login Access

1. Navigate to `frontend/pages/auth/login.html`
2. Open in your browser or serve via local server

## Common Issues and Solutions

### Issue 1: "Cannot read property 'isAuthenticated' of undefined"

**Cause:** Scripts not loaded in correct order or not loaded at all

**Solution:**

1. Open browser Developer Tools (F12)
2. Check the Console tab for script loading errors
3. Verify all script files exist:
   - `frontend/scripts/utils/utils.js`
   - `frontend/scripts/utils/api.js`
   - `frontend/scripts/utils/auth.js`
4. Make sure you're opening from correct path

### Issue 2: "Failed to fetch" or CORS errors

**Cause:** Backend API not running or CORS not configured

**Solution:**

1. Ensure your backend API is running at `https://localhost:44383/api`
2. If using different URL, update `API_CONFIG.BASE_URL` in `frontend/scripts/utils/api.js`
3. Check backend CORS settings allow requests from your frontend origin

### Issue 3: Login form doesn't submit

**Cause:** JavaScript errors or DOM not ready

**Solution:**

1. Open browser console and check for errors
2. Verify the DOMContentLoaded event is firing
3. Check that form element exists with ID `loginForm`

### Issue 4: CSS not loading properly

**Cause:** Incorrect relative paths or missing CSS files

**Solution:**

1. Verify CSS files exist in `frontend/styles/`
2. Check browser Network tab for 404 errors
3. Ensure correct relative paths in HTML files

## Testing the Setup

### Test 1: System Check

1. Open `index.html` in browser
2. Wait for checks to complete
3. All items should show "OK" or "INFO" status

### Test 2: Console Logs

Open browser console and you should see:

```
[API] Configuration loaded
[Auth] AuthManager initialized
```

When you try to login, you should see:

```
[Auth] Attempting login for user: username
[API] POST https://localhost:44383/api/Auth/login
[API] Response status: 200
[Auth] Login response received: {...}
[Auth] Login successful, setting auth data
[Auth] Redirecting to: ../manager/dashboard.html
```

### Test 3: Local Server (Recommended)

For best results, serve the application via a local server:

**Using Python:**

```bash
cd barq-dashboard
python -m http.server 8000
```

Then open: `http://localhost:8000/index.html`

**Using Node.js:**

```bash
cd barq-dashboard
npx http-server -p 8000
```

Then open: `http://localhost:8000/index.html`

**Using VS Code:**

- Install "Live Server" extension
- Right-click on `index.html`
- Select "Open with Live Server"

## Backend API Requirements

Your backend must return the following format for login:

```json
{
  "Token": "eyJhbGciOiJIUzI1NiIs...",
  "User": {
    "UserId": 1,
    "Name": "John Doe",
    "Email": "john@example.com",
    "Role": 1
  }
}
```

Role IDs:

- 1: Manager
- 2: Assistant Manager
- 3: Accountant
- 4: Team Leader
- 5: Employee
- 6: Client

## File Structure Check

Ensure you have this structure:

```
barq-dashboard/
├── index.html                          ← System check page
├── README.md
├── TROUBLESHOOTING.md                  ← This file
└── frontend/
    ├── pages/
    │   └── auth/
    │       └── login.html              ← Login page
    ├── scripts/
    │   ├── utils/
    │   │   ├── api.js                  ← API client
    │   │   ├── auth.js                 ← Authentication
    │   │   ├── utils.js                ← Helper functions
    │   │   └── components.js           ← UI components
    │   └── pages/
    │       └── auth/
    │           └── login.js            ← Login page logic
    └── styles/
        ├── main.css                     ← Main CSS entry
        ├── base.css
        ├── layout.css
        ├── components.css
        └── utils/
            ├── variables.css
            └── utilities.css
```

## Configuration

### Change API URL

Edit `frontend/scripts/utils/api.js`:

```javascript
const API_CONFIG = {
  BASE_URL: "https://your-api-url.com/api", // Change this
  TOKEN_KEY: "auth_token",
  USER_KEY: "user_data",
};
```

### Enable/Disable Debug Logs

The system now logs all API calls and auth operations to console.
To disable, remove or comment out `console.log()` statements in:

- `frontend/scripts/utils/api.js`
- `frontend/scripts/utils/auth.js`

## Still Having Issues?

1. **Clear browser cache and localStorage:**

   - Open Developer Tools (F12)
   - Go to Application tab (Chrome) or Storage tab (Firefox)
   - Click "Clear storage" or manually delete items

2. **Check browser compatibility:**

   - Use modern browsers (Chrome 90+, Firefox 88+, Edge 90+, Safari 14+)
   - Enable JavaScript
   - Disable browser extensions that might interfere

3. **Verify backend is working:**

   ```bash
   # Test with curl
   curl -X POST https://localhost:44383/api/Auth/login \
     -H "Content-Type: application/json" \
     -d '{"Username":"admin","Password":"password"}'
   ```

4. **Check network connectivity:**
   - Open Developer Tools → Network tab
   - Try to login and watch for failed requests
   - Look for CORS errors, 404s, or 500s

## Debug Mode

For detailed debugging, add this to your login page:

```javascript
// Add to login.js temporarily
window.DEBUG_MODE = true;

// Then wrap console.logs with:
if (window.DEBUG_MODE) {
  console.log("[DEBUG]", "your message", data);
}
```

## Contact Support

If none of these solutions work:

1. Check the browser console for specific error messages
2. Copy the error details
3. Check the Network tab for failed requests
4. Document steps to reproduce
5. Contact development team with this information
