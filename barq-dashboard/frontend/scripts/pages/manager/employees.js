// Manager Employees Page Script
auth.requireRole([USER_ROLES.MANAGER]);

let employees = [];
let departments = [];
let currentEditId = null;

document.addEventListener("DOMContentLoaded", async () => {
  await loadData();
  setupEventListeners();
});

async function loadData() {
  try {
    utils.showLoading();
    employees = await API.Employees.getAll().catch(() => []);
    departments = await API.Departments.getAll().catch(() => []);
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
    3: "Accountant",
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
        <td colspan="7" class="text-center" style="padding: 40px;">
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
    .map(
      (emp) => `
    <tr>
      <td><strong>${emp.Name || emp.Username || "Unknown"}</strong></td>
      <td>${emp.Username || "N/A"}</td>
      <td>${emp.Email || "N/A"}</td>
      <td><span class="badge badge-info">${getRoleName(
        emp.Role || emp.RoleId
      )}</span></td>
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
  `
    )
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
  document.getElementById("employeeModal").classList.remove("d-none");
}

function closeModal() {
  document.getElementById("employeeModal").classList.add("d-none");
  document.getElementById("employeeForm").reset();
  currentEditId = null;
}

async function editEmployee(id) {
  const employee = employees.find((e) => (e.UserId || e.Id) === id);
  if (!employee) return;

  currentEditId = id;
  document.getElementById("modalTitle").textContent = "Edit Employee";
  document.getElementById("employeeId").value = id;
  document.getElementById("name").value = employee.Name || "";
  document.getElementById("email").value = employee.Email || "";
  document.getElementById("phoneNumber").value = employee.PhoneNumber || "";
  document.getElementById("department").value = employee.Department || "";
  document.getElementById("position").value = employee.Position || "";
  document.getElementById("role").value = employee.Role || employee.RoleId || 5;

  document.getElementById("employeeModal").classList.remove("d-none");
}

async function handleSubmit(e) {
  e.preventDefault();

  const formData = {
    Name: document.getElementById("name").value,
    Email: document.getElementById("email").value,
    PhoneNumber: document.getElementById("phoneNumber").value,
    Department: document.getElementById("department").value,
    Position: document.getElementById("position").value,
    Role: parseInt(document.getElementById("role").value),
  };

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
    utils.showError("Failed to save employee");
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
