// Team Leader Employees Page Script
auth.requireRole([USER_ROLES.TEAM_LEADER]);

let employees = [];
let filteredEmployees = [];
// Search filter for table rows
function handleSearch(e) {
  const searchTerm = e.target.value.toLowerCase();
  const rows = document.querySelectorAll("#employeesBody tr");
  rows.forEach((row) => {
    const text = row.textContent.toLowerCase();
    row.style.display = text.includes(searchTerm) ? "" : "none";
  });
}
let currentEditId = null;

document.addEventListener("DOMContentLoaded", async () => {
  await loadData();
  setupEventListeners();
});

async function loadData() {
  try {
    utils.showLoading();
    employees = await API.Users.getAll().catch(() => []);
    filteredEmployees = employees; // Initialize filtered list with all employees
    renderEmployees();
  } catch (error) {
    console.error("Error loading data:", error);
    utils.showError("Failed to load employees");
  } finally {
    utils.hideLoading();
  }
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

function setupEventListeners() {
  document
    .getElementById("employeeForm")
    .addEventListener("submit", handleSubmit);
  document
    .getElementById("searchInput")
    .addEventListener("input", handleSearch);

  // Role filter logic
  const roleFilter = document.getElementById("roleFilter");
  if (roleFilter) {
    roleFilter.addEventListener("change", (e) => {
      const val = e.target.value;
      if (!val) {
        filteredEmployees = employees;
      } else {
        filteredEmployees = employees.filter(
          (emp) => (emp.RoleId || emp.Role) == val
        );
      }
      renderEmployees();
    });
  }
}

function renderEmployees() {
  const tbody = document.getElementById("employeesBody");
  if (!tbody) return;
  if (filteredEmployees.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="4" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No employees found</h3>
            <p>Try changing the filter or add a new employee.</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }
  tbody.innerHTML = filteredEmployees
    .map(
      (emp) => `
    <tr>
      <td><strong>${emp.Name || "Unknown"}</strong></td>
      <td>${emp.Email || "N/A"}</td>
      <td><span class="badge badge-info">${getRoleName(
        emp.RoleId || emp.Role
      )}</span></td>
      <td>
        <div class="table-actions">
          <button class="btn btn-sm btn-primary" onclick="editEmployee(${
            emp.UserId
          })">
            <i class="fa-solid fa-pen"></i>
          </button>
          <button class="btn btn-sm btn-danger" onclick="deleteEmployee(${
            emp.UserId
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

async function handleSubmit(e) {
  e.preventDefault();

  const formData = {
    Name: document.getElementById("name").value,
    Email: document.getElementById("email").value,
    Role: parseInt(document.getElementById("role").value),
  };

  try {
    utils.showLoading();

    if (currentEditId) {
      await API.Users.update(currentEditId, formData);
      utils.showSuccess("Employee updated successfully");
    } else {
      await API.Users.create(formData);
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
  const employee = filteredEmployees.find((emp) => emp.UserId === id);
  if (!employee) return;

  currentEditId = id;
  document.getElementById("modalTitle").textContent = "Edit Employee";
  document.getElementById("employeeId").value = id;
  document.getElementById("name").value = employee.Name || "";
  document.getElementById("email").value = employee.Email || "";
  document.getElementById("role").value =
    employee.RoleId || employee.Role || "";

  document.getElementById("employeeModal").classList.remove("d-none");
}

async function deleteEmployee(id) {
  if (!utils.confirmAction("Are you sure you want to delete this employee?"))
    return;

  try {
    utils.showLoading();
    await API.Users.delete(id);
    utils.showSuccess("Employee deleted successfully");
    await loadData();
  } catch (error) {
    console.error("Error deleting employee:", error);
    utils.showError("Failed to delete employee");
  } finally {
    utils.hideLoading();
  }
}
