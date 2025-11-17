# Barq TMS - Task Management System

A modern, fully-featured Task Management System built with vanilla HTML, CSS, and JavaScript.

## 🚀 Features

- **Role-Based Access Control** - 6 different user roles (Manager, Assistant Manager, Accountant, Team Leader, Employee, Client)
- **Task Management** - Create, edit, delete, and assign tasks
- **Project Management** - Manage projects with budget tracking
- **Employee Management** - Manage team members and roles
- **Client Management** - Maintain client relationships
- **Analytics Dashboard** - View performance metrics and insights
- **Responsive Design** - Works on desktop, tablet, and mobile devices
- **Modern UI** - Clean, professional interface with smooth animations

## 📁 Project Structure

```
barq-dashboard/
├── frontend/
│   ├── pages/
│   │   ├── auth/
│   │   │   └── login.html
│   │   └── manager/
│   │       ├── dashboard.html
│   │       ├── tasks.html
│   │       ├── projects.html
│   │       ├── employees.html
│   │       ├── clients.html
│   │       ├── analytics.html
│   │       └── settings.html
│   ├── styles/
│   │   ├── main.css
│   │   ├── base.css
│   │   ├── layout.css
│   │   ├── components.css
│   │   └── utils/
│   │       ├── variables.css
│   │       └── utilities.css
│   ├── scripts/
│   │   ├── utils/
│   │   │   ├── api.js
│   │   │   ├── auth.js
│   │   │   ├── utils.js
│   │   │   └── components.js
│   │   └── pages/
│   │       ├── auth/
│   │       │   └── login.js
│   │       └── manager/
│   │           ├── dashboard.js
│   │           ├── tasks.js
│   │           ├── projects.js
│   │           ├── employees.js
│   │           ├── clients.js
│   │           ├── analytics.js
│   │           └── settings.js
│   └── media/
│       └── logo.png (optional)
└── PROJECT_STRUCTURE_GUIDE.md
```

## 🎨 Design System

### Color Palette

- **Primary**: `#7e2d96` (Purple)
- **Accent**: `#88d3ce` (Teal)
- **Dark**: `#121212`
- **Success**: `#10b981`
- **Warning**: `#f59e0b`
- **Error**: `#ef4444`

### Typography

- Font: System font stack (Segoe UI, Roboto, etc.)
- Sizes: 0.75rem to 1.5rem
- Weights: 400 (normal), 600 (semi-bold), 700 (bold)

## 🛠️ Setup & Installation

### Prerequisites

- A modern web browser (Chrome, Firefox, Edge, Safari)
- A backend API server running at `https://localhost:44383/api` (or update the API_CONFIG.BASE_URL in `api.js`)

### Installation Steps

1. **No Build Required!** This project uses vanilla HTML/CSS/JavaScript
2. Simply open any HTML file in your browser or serve via a local server
3. For development, you can use any simple HTTP server:

   ```bash
   # Using Python
   python -m http.server 8000

   # Using Node.js http-server
   npx http-server

   # Using PHP
   php -S localhost:8000
   ```

4. Navigate to `http://localhost:8000/frontend/pages/auth/login.html`

## 🔧 Configuration

### API Configuration

Update the API base URL in `frontend/scripts/utils/api.js`:

```javascript
const API_CONFIG = {
  BASE_URL: "https://localhost:44383/api", // Change this to your API URL
  TOKEN_KEY: "auth_token",
  USER_KEY: "user_data",
};
```

### User Roles

Defined in `frontend/scripts/utils/auth.js`:

```javascript
const USER_ROLES = {
  MANAGER: 1,
  ASSISTANT_MANAGER: 2,
  ACCOUNTANT: 3,
  TEAM_LEADER: 4,
  EMPLOYEE: 5,
  CLIENT: 6,
};
```

## 📱 Pages Overview

### Authentication

- **Login** (`/pages/auth/login.html`) - User authentication with role-based redirection

### Manager Dashboard

- **Dashboard** - Overview with statistics and recent activity
- **Tasks** - Full CRUD operations for tasks
- **Projects** - Project management with budget tracking
- **Employees** - Team member management
- **Clients** - Client relationship management
- **Analytics** - Performance metrics and insights
- **Settings** - User profile and system settings

## 🎯 Key Features Explained

### Authentication & Authorization

- Token-based authentication stored in localStorage
- Role-based access control (RBAC)
- Automatic redirection to role-specific dashboards
- Session persistence across page reloads

### Data Management

- RESTful API integration
- CRUD operations for all entities
- Real-time data updates
- Search and filter functionality

### User Experience

- Responsive mobile-first design
- Loading states and error handling
- Toast notifications for user feedback
- Modal dialogs for forms
- Smooth animations and transitions

## 🔌 API Integration

### Expected Backend Endpoints

```
POST   /api/Auth/login              - User login
POST   /api/Auth/logout             - User logout

GET    /api/Tasks                   - Get all tasks
GET    /api/Tasks/{id}              - Get task by ID
POST   /api/Tasks                   - Create task
PUT    /api/Tasks/{id}              - Update task
DELETE /api/Tasks/{id}              - Delete task

GET    /api/Projects                - Get all projects
POST   /api/Projects                - Create project
PUT    /api/Projects/{id}           - Update project
DELETE /api/Projects/{id}           - Delete project

GET    /api/Employees               - Get all employees
POST   /api/Employees               - Create employee
PUT    /api/Employees/{id}          - Update employee
DELETE /api/Employees/{id}          - Delete employee

GET    /api/Clients                 - Get all clients
POST   /api/Clients                 - Create client
PUT    /api/Clients/{id}            - Update client
DELETE /api/Clients/{id}            - Delete client
```

### Expected Response Format

```javascript
// Success Response
{
  "Token": "eyJhbGciOiJIUzI1NiIs...",
  "User": {
    "UserId": 1,
    "Name": "John Doe",
    "Email": "john@example.com",
    "Role": 1
  }
}

// Data Response
{
  "Data": [...],  // PascalCase properties
  "Success": true,
  "Message": "Success"
}
```

## 🎨 Customization

### Changing Colors

Edit `frontend/styles/utils/variables.css`:

```css
:root {
  --primary-color: #7e2d96; /* Change to your brand color */
  --accent-color: #88d3ce; /* Change accent color */
}
```

### Adding New Pages

1. Copy an existing page HTML as a template
2. Update the navigation links
3. Create a corresponding JavaScript file
4. Update the `auth.requireRole()` call with appropriate roles

### Adding New API Services

Add to `frontend/scripts/utils/api.js`:

```javascript
const API = {
  // ... existing services

  NewService: {
    async getAll() {
      const client = new APIClient();
      return client.get("/NewEndpoint");
    },
  },
};
```

## 🐛 Troubleshooting

### Common Issues

1. **API Connection Errors**

   - Verify the backend API is running
   - Check the API_CONFIG.BASE_URL in `api.js`
   - Ensure CORS is properly configured on the backend

2. **Authentication Issues**

   - Clear browser localStorage
   - Check token expiration
   - Verify API authentication endpoints

3. **Styling Issues**
   - Clear browser cache
   - Check CSS import order in `main.css`
   - Verify Font Awesome CDN is accessible

## 📝 Development Notes

- Pure vanilla JavaScript - no frameworks or build tools
- CSS uses modern features (Grid, Flexbox, CSS Variables)
- Mobile-first responsive design
- Accessibility considerations included
- No external dependencies except Font Awesome icons

## 🚀 Future Enhancements

- [ ] Real-time notifications with WebSockets
- [ ] Advanced filtering and sorting
- [ ] Data export functionality (PDF, Excel)
- [ ] Calendar view for tasks
- [ ] Drag-and-drop task management
- [ ] Dark mode toggle
- [ ] Multi-language support
- [ ] File attachments for tasks
- [ ] Activity timeline
- [ ] Advanced analytics with charts (Chart.js integration)

## 📄 License

This project is created for the Barq TMS system. All rights reserved.

## 👥 Support

For issues or questions:

- Check the PROJECT_STRUCTURE_GUIDE.md for detailed architecture
- Review the inline code comments
- Contact the development team

---

**Built with ❤️ using vanilla HTML, CSS, and JavaScript**
