// Team Leader - Team Members (View Only)
auth.requireRole([USER_ROLES.TEAM_LEADER]);

let employees = [];

document.addEventListener("DOMContentLoaded", async () => {
  await loadEmployees();
  setupSearch();
});

async function loadEmployees() {
  try {
    utils.showLoading();
    const currentUser = auth.getCurrentUser();
    const users = await API.Users.getAll();

    // Filter to only employees (RoleId = 5) under this team leader
    employees = users.filter((u) => {
      const roleId = u.Role || u.RoleId;
      const teamLeaderId = u.TeamLeaderId || u.teamLeaderId;
      return roleId === 5 && teamLeaderId === currentUser.UserId;
    });

    renderEmployees();
  } catch (error) {
    console.error("Error loading employees:", error);
    utils.showError("Failed to load team members");
  } finally {
    utils.hideLoading();
  }
}

function renderEmployees() {
  const tbody = document.getElementById("employeesBody");
  if (!tbody) return;

  if (employees.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="5" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No team members found</h3>
            <p>You don't have any employees under your supervision</p>
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
      <td><strong>${emp.Name || "N/A"}</strong></td>
      <td>${emp.Email || "N/A"}</td>
      <td>${emp.Position || "N/A"}</td>
      <td>${getRoleName(emp.Role)}</td>
      <td>
        <button class="btn btn-sm btn-secondary" onclick="viewEmployee(${
          emp.UserId
        })">
          <i class="fa-solid fa-eye"></i> View
        </button>
      </td>
    </tr>
  `
    )
    .join("");
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

function viewEmployee(userId) {
  const employee = employees.find((e) => e.UserId === userId);
  if (!employee) return;

  const message = `
Employee Details:

Name: ${employee.Name || "N/A"}
Email: ${employee.Email || "N/A"}
Position: ${employee.Position || "N/A"}
Role: ${getRoleName(employee.Role)}
Team Leader: ${employee.TeamLeaderName || "N/A"}
  `;

  alert(message);
}

function setupSearch() {
  const searchInput = document.getElementById("searchInput");
  if (searchInput) {
    searchInput.addEventListener("input", (e) => {
      const searchTerm = e.target.value.toLowerCase();
      const rows = document.querySelectorAll("#employeesBody tr");
      rows.forEach((row) => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(searchTerm) ? "" : "none";
      });
    });
  }
}
