// Account Manager Clients Page Script
auth.requireRole([USER_ROLES.ACCOUNTANT]);

let clients = [];
let currentEditId = null;

document.addEventListener("DOMContentLoaded", async () => {
  await loadData();
  setupEventListeners();
});

async function loadData() {
  try {
    utils.showLoading();

    // Load clients from the API so we have complete client records
    const allClients = await API.Clients.getAll().catch(() => []);
    clients = allClients.map((c) => ({
      ClientId: c.clientId || c.ClientId,
      ClientName: c.name || c.Name || c.clientName || c.ClientName,
      ProjectCount: c.projectCount || c.ProjectCount || 0,
      Email: c.email || c.Email || "",
      PhoneNumber: c.phoneNumber || c.PhoneNumber || "",
      Company: c.company || c.Company || "",
      Address: c.address || c.Address || "",
    }));
    renderClients();
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
        <td colspan="7" class="text-center" style="padding: 40px;">
          <div class="empty-state">
            <i class="fa-solid fa-inbox"></i>
            <h3>No clients found</h3>
            <p>Clients will appear here when projects are created</p>
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
      <td><strong>#${client.ClientId}</strong></td>
      <td><strong>${client.ClientName || "Unknown"}</strong></td>
      <td>${client.Email || "-"}</td>
      <td>${client.PhoneNumber || "-"}</td>
      <td>${client.Company || "-"}</td>
      <td><span class="badge badge-info">${
        client.ProjectCount || 0
      } projects</span></td>
      <td>
        <div class="table-actions">
          <button class="btn btn-sm btn-primary" onclick="viewClientDetails(${
            client.ClientId
          })" title="View Details">
            <i class="fa-solid fa-eye"></i> View Details
          </button>
        </div>
      </td>
    </tr>
  `
    )
    .join("");
}

function viewClientDetails(clientId) {
  window.location.href = `client-details.html?id=${clientId}`;
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
