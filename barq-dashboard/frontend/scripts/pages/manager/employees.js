// Manager Employees Page Script
auth.requireRole([USER_ROLES.MANAGER]);

let employees = [];
let departments = [];
let teamLeaders = [];
let clients = [];
let currentEditId = null;

document.addEventListener("DOMContentLoaded", async () => {
  await loadData();
  setupEventListeners();
});

async function loadData() {
  try {
    utils.showLoading();
    employees = await API.Users.getAll().catch(() => []);
    departments = await API.Departments.getAll().catch(() => []);
    clients = await API.Clients.getAll().catch(() => []);

    // Filter team leaders from employees (RoleId = 4)
    teamLeaders = employees.filter((emp) => {
      const roleId = emp.RoleId || emp.Role;
      return roleId === 4;
    });

    renderEmployees();
    populateDepartmentDropdown();
  } catch (error) {
    console.error("Error loading data:", error);
    utils.showError("Failed to load employees");
  } finally {
    utils.hideLoading();
  }
}

function populateDepartmentDropdown() {
  const departmentSelect = document.getElementById("department");

  // Clear existing options except the first one
  departmentSelect.innerHTML = '<option value="">Select Department</option>';

  // Add department options
  departments.forEach((dept) => {
    const option = document.createElement("option");
    option.value = dept.DeptId;
    option.textContent = dept.DeptName;
    departmentSelect.appendChild(option);
  });
}

function getRoleName(roleId) {
  const roles = {
    1: "Manager",
    2: "Assistant Manager",
    3: "Account Manager",
    4: "Team Leader",
    5: "Employee",
    6: "Client",
  };
  return roles[roleId] || "Unknown";
}

function renderEmployees() {
  const tbody = document.getElementById("employeesBody");

  if (employees.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="6" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No employees found</h3>
            <p>Add your first employee to get started</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = employees
    .map((emp) => {
      const roleName = getRoleName(emp.Role || emp.RoleId);
      const teamLeaderName = emp.TeamLeaderName || "Not assigned";

      return `
    <tr>
      <td><strong>${emp.Name || emp.Username || "Unknown"}</strong></td>
      <td>${emp.Username || "N/A"}</td>
      <td>${emp.Email || "N/A"}</td>
      <td><span class="badge badge-info">${roleName}</span></td>
      <td>${teamLeaderName}</td>
      <td>
        <div class="table-actions">
          <button class="btn btn-sm btn-primary" onclick="editEmployee(${
            emp.UserId || emp.Id
          })">
            <i class="fa-solid fa-pen"></i>
          </button>
          <button class="btn btn-sm btn-danger" onclick="deleteEmployee(${
            emp.UserId || emp.Id
          })">
            <i class="fa-solid fa-trash"></i>
          </button>
        </div>
      </td>
    </tr>
  `;
    })
    .join("");
}

function setupEventListeners() {
  document
    .getElementById("employeeForm")
    .addEventListener("submit", handleSubmit);
  document
    .getElementById("searchInput")
    .addEventListener("input", handleSearch);
}

function handleSearch(e) {
  const searchTerm = e.target.value.toLowerCase();
  const rows = document.querySelectorAll("#employeesBody tr");

  rows.forEach((row) => {
    const text = row.textContent.toLowerCase();
    row.style.display = text.includes(searchTerm) ? "" : "none";
  });
}

function showCreateModal() {
  currentEditId = null;
  document.getElementById("modalTitle").textContent = "Add Employee";
  document.getElementById("employeeForm").reset();
  document.getElementById("employeeId").value = "";
  document.getElementById("role").value = "5"; // Default to Employee

  // Make password required for new employee
  const passwordInput = document.getElementById("password");
  passwordInput.required = true;
  passwordInput.placeholder = "Enter password (min 6 characters)";
  document
    .getElementById("passwordGroup")
    .querySelector(".form-text").style.display = "none";

  handleRoleChange(); // Update conditional fields
  document.getElementById("employeeModal").classList.remove("d-none");
}

function closeModal() {
  document.getElementById("employeeForm").reset();
  currentEditId = null;
  document.getElementById("modalTitle").textContent = "Add Employee";
  document.getElementById("employeeId").value = "";
  document.getElementById("username").value = "";

  // Hide all conditional groups
  document.getElementById("teamLeaderGroup").style.display = "none";
  document.getElementById("managedEmployeesGroup").style.display = "none";
  document.getElementById("managedClientsGroup").style.display = "none";

  document.getElementById("employeeModal").classList.add("d-none");
}

// Handle role change to show/hide conditional fields
function handleRoleChange() {
  const role = parseInt(document.getElementById("role").value);
  const teamLeaderGroup = document.getElementById("teamLeaderGroup");
  const managedEmployeesGroup = document.getElementById(
    "managedEmployeesGroup"
  );
  const managedClientsGroup = document.getElementById("managedClientsGroup");

  // Hide all conditional fields first
  teamLeaderGroup.style.display = "none";
  managedEmployeesGroup.style.display = "none";
  managedClientsGroup.style.display = "none";

  // Show relevant fields based on role
  if (role === 5) {
    // Employee - show team leader selection
    teamLeaderGroup.style.display = "block";
    populateTeamLeaderDropdown();
  } else if (role === 4) {
    // Team Leader - show employee assignment
    managedEmployeesGroup.style.display = "block";
    populateManagedEmployeesDropdown();
  } else if (role === 3) {
    // Account Manager - show client assignment
    managedClientsGroup.style.display = "block";
    populateManagedClientsDropdown();
  }
}

// Populate team leader dropdown
function populateTeamLeaderDropdown() {
  const teamLeaderSelect = document.getElementById("teamLeader");
  teamLeaderSelect.innerHTML = '<option value="">No Team Leader</option>';

  teamLeaders.forEach((tl) => {
    const option = document.createElement("option");
    option.value = tl.UserId || tl.Id;
    option.textContent = tl.Name || tl.Username;
    teamLeaderSelect.appendChild(option);
  });
}

// Populate managed employees dropdown
function populateManagedEmployeesDropdown() {
  const managedEmployeesSelect = document.getElementById("managedEmployees");
  managedEmployeesSelect.innerHTML = "";

  // Get all employees (role 5) that don't have this team leader yet or have no team leader
  const availableEmployees = employees.filter((emp) => {
    const empRole = emp.Role || emp.RoleId;
    const empId = emp.UserId || emp.Id;
    return empRole === 5 && empId !== currentEditId; // Don't include self
  });

  availableEmployees.forEach((emp) => {
    const option = document.createElement("option");
    option.value = emp.UserId || emp.Id;
    option.textContent = emp.Name || emp.Username;
    managedEmployeesSelect.appendChild(option);
  });
}

// Populate managed clients dropdown
function populateManagedClientsDropdown() {
  const managedClientsSelect = document.getElementById("managedClients");
  managedClientsSelect.innerHTML = "";

  clients.forEach((client) => {
    const option = document.createElement("option");
    option.value = client.ClientId || client.Id;
    option.textContent = client.Name || client.Company;
    managedClientsSelect.appendChild(option);
  });
}

async function editEmployee(id) {
  const employee = employees.find((e) => (e.UserId || e.Id) === id);
  if (!employee) return;

  currentEditId = id;
  document.getElementById("modalTitle").textContent = "Edit Employee";
  document.getElementById("employeeId").value = id;
  document.getElementById("name").value = employee.Name || "";
  document.getElementById("username").value = employee.Username || "";
  document.getElementById("email").value = employee.Email || "";
  document.getElementById("position").value = employee.Position || "";
  document.getElementById("role").value = employee.Role || employee.RoleId || 5;

  // Make password optional for editing
  const passwordInput = document.getElementById("password");
  passwordInput.value = "";
  passwordInput.required = false;
  passwordInput.placeholder = "Leave blank to keep current password";
  document
    .getElementById("passwordGroup")
    .querySelector(".form-text").style.display = "block";

  // Update conditional fields based on role
  handleRoleChange();

  // Set team leader if employee
  if ((employee.Role || employee.RoleId) === 5 && employee.TeamLeaderId) {
    document.getElementById("teamLeader").value = employee.TeamLeaderId;
  }

  // Set managed employees if team leader
  if ((employee.Role || employee.RoleId) === 4 && employee.ManagedEmployeeIds) {
    const managedEmployeesSelect = document.getElementById("managedEmployees");
    Array.from(managedEmployeesSelect.options).forEach((option) => {
      option.selected = employee.ManagedEmployeeIds.includes(
        parseInt(option.value)
      );
    });
  }

  // Set managed clients if account manager
  if ((employee.Role || employee.RoleId) === 3 && employee.ManagedClientIds) {
    const managedClientsSelect = document.getElementById("managedClients");
    Array.from(managedClientsSelect.options).forEach((option) => {
      option.selected = employee.ManagedClientIds.includes(
        parseInt(option.value)
      );
    });
  }

  document.getElementById("employeeModal").classList.remove("d-none");
}

async function handleSubmit(e) {
  e.preventDefault();

  const role = parseInt(document.getElementById("role").value);
  const password = document.getElementById("password").value;

  const formData = {
    Name: document.getElementById("name").value,
    Username: document.getElementById("username").value,
    Email: document.getElementById("email").value || null,
    Position: document.getElementById("position").value || null,
    Role: role,
    DepartmentIds: [], // Empty for now, can be enhanced later
  };

  // Add password if provided (required for new, optional for edit)
  if (password) {
    formData.Password = password;
  }

  // Add TeamLeaderId for employees (optional)
  if (role === 5) {
    const teamLeaderId = document.getElementById("teamLeader").value;
    if (teamLeaderId) {
      formData.TeamLeaderId = parseInt(teamLeaderId);
    }
  }

  // Add ManagedEmployeeIds for team leaders (optional)
  if (role === 4) {
    const managedEmployeesSelect = document.getElementById("managedEmployees");
    const selectedEmployees = Array.from(
      managedEmployeesSelect.selectedOptions
    ).map((option) => parseInt(option.value));
    if (selectedEmployees.length > 0) {
      formData.ManagedEmployeeIds = selectedEmployees;
    }
  }

  // Add ManagedClientIds for account managers (optional)
  if (role === 3) {
    const managedClientsSelect = document.getElementById("managedClients");
    const selectedClients = Array.from(
      managedClientsSelect.selectedOptions
    ).map((option) => parseInt(option.value));
    if (selectedClients.length > 0) {
      formData.ManagedClientIds = selectedClients;
    }
  }

  try {
    utils.showLoading();

    if (currentEditId) {
      await API.Employees.update(currentEditId, formData);
      utils.showSuccess("Employee updated successfully");
    } else {
      await API.Employees.create(formData);
      utils.showSuccess("Employee added successfully");
    }

    closeModal();
    await loadData();
  } catch (error) {
    console.error("Error saving employee:", error);
    utils.showError(error.message || "Failed to save employee");
  } finally {
    utils.hideLoading();
  }
}

async function deleteEmployee(id) {
  if (!utils.confirmAction("Are you sure you want to delete this employee?"))
    return;

  try {
    utils.showLoading();
    await API.Employees.delete(id);
    utils.showSuccess("Employee deleted successfully");
    await loadData();
  } catch (error) {
    console.error("Error deleting employee:", error);
    utils.showError("Failed to delete employee");
  } finally {
    utils.hideLoading();
  }
}
