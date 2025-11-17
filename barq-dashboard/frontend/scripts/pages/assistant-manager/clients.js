// Assistant Manager Clients Page Script
auth.requireRole([USER_ROLES.ASSISTANT_MANAGER]);

let clients = [];
let accountants = [];
let currentEditId = null;

document.addEventListener("DOMContentLoaded", async () => {
  await loadData();
  setupEventListeners();
});

async function loadData() {
  try {
    utils.showLoading();

    // Get projects and extract unique clients
    const projects = await API.Projects.getAll().catch(() => []);
    const clientsMap = new Map();
    projects.forEach((project) => {
      if (project.ClientId && project.ClientName) {
        if (!clientsMap.has(project.ClientId)) {
          clientsMap.set(project.ClientId, {
            ClientId: project.ClientId,
            ClientName: project.ClientName,
            ProjectCount: 0,
          });
        }
        clientsMap.get(project.ClientId).ProjectCount++;
      }
    });
    clients = Array.from(clientsMap.values());

    // Fetch accountants (role 3)
    const allEmployees = await API.Employees.getAll().catch(() => []);
    accountants = allEmployees.filter((emp) => (emp.Role || emp.RoleId) === 3);

    renderClients();
    populateAccountantDropdown();
  } catch (error) {
    console.error("Error loading data:", error);
    utils.showError("Failed to load clients");
  } finally {
    utils.hideLoading();
  }
}

function renderClients() {
  const tbody = document.getElementById("clientsBody");

  if (clients.length === 0) {
    tbody.innerHTML = `
      <tr>
        <td colspan="6" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No clients found</h3>
            <p>Add your first client to get started</p>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  tbody.innerHTML = clients
    .map(
      (client) => `
    <tr>
      <td><strong>${client.ClientId}</strong></td>
      <td>${client.ClientName || "Unknown"}</td>
      <td><span class="badge badge-info">${
        client.ProjectCount || 0
      } projects</span></td>
      <td><span class="badge badge-secondary"><i class="fa-solid fa-folder-open"></i> From Projects</span></td>
      <td>
        <div class="table-actions">
          <button class="btn btn-sm btn-primary" disabled title="View only - extracted from projects">
            <i class="fa-solid fa-eye"></i>
          </button>
          <button class="btn btn-sm btn-danger" disabled title="Cannot delete - extracted from projects">
            <i class="fa-solid fa-trash"></i>
          </button>
        </div>
      </td>
    </tr>
  `
    )
    .join("");
}

function populateAccountantDropdown() {
  const accountantSelect = document.getElementById("accountant");

  // Clear existing options except the first one
  accountantSelect.innerHTML = '<option value="">Select Accountant</option>';

  // Add accountant options
  accountants.forEach((acc) => {
    const option = document.createElement("option");
    option.value = acc.UserId || acc.Id;
    option.textContent = acc.Name || acc.Username || "Unknown";
    accountantSelect.appendChild(option);
  });
}

function setupEventListeners() {
  document
    .getElementById("searchInput")
    .addEventListener("input", handleSearch);
}

function handleSearch(e) {
  const searchTerm = e.target.value.toLowerCase();
  const rows = document.querySelectorAll("#clientsBody tr");

  rows.forEach((row) => {
    const text = row.textContent.toLowerCase();
    row.style.display = text.includes(searchTerm) ? "" : "none";
  });
}
