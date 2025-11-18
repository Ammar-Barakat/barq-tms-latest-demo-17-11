// Accountant Clients Page Script
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
        <td colspan="5" class="text-center" style="padding: 40px;">
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

function showCreateModal() {
  currentEditId = null;
  document.getElementById("modalTitle").textContent = "Add Client";
  document.getElementById("clientForm").reset();
  document.getElementById("clientId").value = "";
  document.getElementById("clientModal").classList.remove("d-none");
}

function closeModal() {
  document.getElementById("clientModal").classList.add("d-none");
  document.getElementById("clientForm").reset();
  currentEditId = null;
}

async function editClient(id) {
  const client = clients.find((c) => c.Id === id);
  if (!client) return;

  currentEditId = id;
  document.getElementById("modalTitle").textContent = "Edit Client";
  document.getElementById("clientId").value = id;
  document.getElementById("name").value = client.Name || "";
  document.getElementById("email").value = client.Email || "";
  document.getElementById("phoneNumber").value = client.PhoneNumber || "";
  document.getElementById("company").value = client.Company || "";
  document.getElementById("address").value = client.Address || "";

  document.getElementById("clientModal").classList.remove("d-none");
}

async function handleSubmit(e) {
  e.preventDefault();

  const formData = {
    Name: document.getElementById("name").value,
    Email: document.getElementById("email").value,
    PhoneNumber: document.getElementById("phoneNumber").value,
    Company: document.getElementById("company").value,
    Address: document.getElementById("address").value,
  };

  try {
    utils.showLoading();

    if (currentEditId) {
      await API.Clients.update(currentEditId, formData);
      utils.showSuccess("Client updated successfully");
    } else {
      await API.Clients.create(formData);
      utils.showSuccess("Client added successfully");
    }

    closeModal();
    await loadData();
  } catch (error) {
    console.error("Error saving client:", error);
    utils.showError("Failed to save client");
  } finally {
    utils.hideLoading();
  }
}

async function deleteClient(id) {
  if (!utils.confirmAction("Are you sure you want to delete this client?"))
    return;

  try {
    utils.showLoading();
    await API.Clients.delete(id);
    utils.showSuccess("Client deleted successfully");
    await loadData();
  } catch (error) {
    console.error("Error deleting client:", error);
    utils.showError("Failed to delete client");
  } finally {
    utils.hideLoading();
  }
}
