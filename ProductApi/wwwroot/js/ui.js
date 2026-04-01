// Initialize Icons
lucide.createIcons();

/* =======================
   STATE & CONSTANTS
======================== */
const API_BASE = '/api';
let State = {
    token: localStorage.getItem('token'),
    refreshToken: localStorage.getItem('refreshToken'),
    role: null,
    email: null,
    currentPage: 1,
    pageSize: 5,
    search: '',
    isEditing: false
};

// Elements
const authView = document.getElementById('auth-view');
const dashboardView = document.getElementById('dashboard-view');
const authForm = document.getElementById('auth-form');
const authError = document.getElementById('auth-error');
const modalError = document.getElementById('modal-error');
const loadingOverlay = document.getElementById('loading-overlay');
const productTableBody = document.getElementById('product-tbody');

/* =======================
   JWT & AUTH LOGIC
======================== */
function parseJwt(token) {
    if (!token) return null;
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch {
        return null;
    }
}

function updateAuthState() {
    if (State.token) {
        const payload = parseJwt(State.token);
        if (payload) {
            State.email = payload.email || payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"];
            State.role = payload.role || payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
            
            document.getElementById('user-info-badge').innerText = `${State.email} (${State.role})`;
            
            // Toggle view visibility
            authView.classList.remove('active');
            authView.classList.add('hidden');
            dashboardView.classList.remove('hidden');
            dashboardView.classList.add('active');

            // Apply RBAC
            const adminElements = document.querySelectorAll('.admin-only');
            if (State.role === 'Admin') {
                adminElements.forEach(el => el.classList.remove('hidden'));
            } else {
                adminElements.forEach(el => el.classList.add('hidden'));
            }

            loadProducts();
            return;
        }
    }
    // Logged out state
    logout();
}

function logout() {
    State.token = null;
    State.refreshToken = null;
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    authView.classList.remove('hidden');
    authView.classList.add('active');
    dashboardView.classList.remove('active');
    dashboardView.classList.add('hidden');
}

document.getElementById('logout-btn').addEventListener('click', logout);

/* =======================
   API WRAPPER (Auto-Refresh)
======================== */
async function activeRefresh() {
    try {
        const res = await fetch(`${API_BASE}/auth/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                accessToken: State.token,
                refreshToken: State.refreshToken
            })
        });
        if (res.ok) {
            const data = await res.json();
            State.token = data.token;
            State.refreshToken = data.refreshToken;
            localStorage.setItem('token', data.token);
            localStorage.setItem('refreshToken', data.refreshToken);
            return true;
        }
    } catch {}
    return false;
}

async function apiCall(endpoint, options = {}) {
    if (!options.headers) options.headers = {};
    if (State.token) options.headers['Authorization'] = `Bearer ${State.token}`;
    if (!options.headers['Content-Type'] && options.body) {
         options.headers['Content-Type'] = 'application/json';
    }

    let response = await fetch(`${API_BASE}${endpoint}`, options);

    // If Access Token is expired
    if (response.status === 401 && State.refreshToken) {
        const refreshSuccess = await activeRefresh();
        if (refreshSuccess) {
            // Retry original request with new token
            options.headers['Authorization'] = `Bearer ${State.token}`;
            response = await fetch(`${API_BASE}${endpoint}`, options);
        } else {
            logout();
        }
    }
    return response;
}

/* =======================
   LOGIN / REGISTER
======================== */
let isRegistering = false;
document.getElementById('auth-toggle-link').addEventListener('click', (e) => {
    e.preventDefault();
    isRegistering = !isRegistering;
    document.querySelector('.register-only').classList.toggle('hidden', !isRegistering);
    document.getElementById('auth-submit-btn').innerText = isRegistering ? 'Create Account' : 'Sign In';
    document.getElementById('auth-toggle-text').innerText = isRegistering ? 'Already have an account? ' : "Don't have an account? ";
    authError.classList.add('hidden');
});

authForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    authError.classList.add('hidden');

    const email = document.getElementById('auth-email').value;
    const password = document.getElementById('auth-password').value;
    const url = isRegistering ? '/auth/register' : '/auth/login';
    
    let body = { email, password };
    if (isRegistering) {
        const username = document.getElementById('reg-username').value;
        if (!username) {
            showError(authError, "Username is required for registration.");
            return;
        }
        body.username = username;
    }

    try {
        const res = await fetch(`${API_BASE}${url}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(body)
        });

        const data = await res.json();
        if (res.ok) {
            State.token = data.token;
            State.refreshToken = data.refreshToken;
            localStorage.setItem('token', data.token);
            localStorage.setItem('refreshToken', data.refreshToken);
            updateAuthState();
        } else {
            showError(authError, data.message || extractDotNetErrors(data));
        }
    } catch {
        showError(authError, "Network error. Please ensure API is running.");
    }
});

function showError(element, msg) {
    element.innerText = msg;
    element.classList.remove('hidden');
}

function extractDotNetErrors(data) {
    if (data.errors) {
        return Object.values(data.errors).flat().join('\n');
    }
    return 'An unexpected error occurred.';
}

/* =======================
   PRODUCTS CRUD
======================== */
document.getElementById('search-input').addEventListener('input', debounce((e) => {
    State.search = e.target.value;
    State.currentPage = 1;
    loadProducts();
}, 300));

async function loadProducts() {
    loadingOverlay.classList.remove('hidden');
    try {
        const qs = `?page=${State.currentPage}&pageSize=${State.pageSize}&search=${encodeURIComponent(State.search)}`;
        const res = await apiCall(`/product${qs}`);
        if (res.ok) {
            const data = await res.json();
            renderProducts(data.items);
            updatePagination(data);
        }
    } finally {
        loadingOverlay.classList.add('hidden');
    }
}

function renderProducts(items) {
    productTableBody.innerHTML = '';
    
    if (items.length === 0) {
        productTableBody.innerHTML = `<tr><td colspan="5" style="text-align:center;color:var(--text-secondary)">No products found.</td></tr>`;
        return;
    }

    items.forEach(p => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>#${p.id}</td>
            <td><strong>${p.name}</strong></td>
            <td>${p.description || '-'}</td>
            <td>$${p.price.toFixed(2)}</td>
            <td class="text-right admin-only ${State.role !== 'Admin' ? 'hidden' : ''}">
                <div class="td-actions">
                    <button class="btn btn-icon btn-ghost btn-edit" data-id="${p.id}" title="Edit">
                        <i data-lucide="edit-3"></i>
                    </button>
                    <button class="btn btn-icon btn-danger-ghost btn-delete" data-id="${p.id}" title="Delete">
                        <i data-lucide="trash-2"></i>
                    </button>
                </div>
            </td>
        `;
        productTableBody.appendChild(tr);
    });

    // Re-initialize dynamic icons
    lucide.createIcons();

    // Attach Action Listeners
    if (State.role === 'Admin') {
        document.querySelectorAll('.btn-edit').forEach(btn => 
            btn.addEventListener('click', (e) => editProduct(e.currentTarget.dataset.id))
        );
        document.querySelectorAll('.btn-delete').forEach(btn => 
            btn.addEventListener('click', (e) => deleteProduct(e.currentTarget.dataset.id))
        );
    }
}

// Pagination setup
document.getElementById('btn-prev-page').addEventListener('click', () => {
    if (State.currentPage > 1) { State.currentPage--; loadProducts(); }
});
document.getElementById('btn-next-page').addEventListener('click', () => {
    State.currentPage++; loadProducts();
});

function updatePagination(data) {
    document.getElementById('page-info').innerText = `Page ${data.page} of ${data.totalPages}`;
    document.getElementById('btn-prev-page').disabled = !data.hasPreviousPage;
    document.getElementById('btn-next-page').disabled = !data.hasNextPage;
}

/* =======================
   MODAL LOGIC (Admin Only)
======================== */
const productModal = document.getElementById('product-modal');
const productForm = document.getElementById('product-form');
const btnAddProduct = document.getElementById('btn-add-product');

btnAddProduct.addEventListener('click', () => {
    State.isEditing = false;
    document.getElementById('modal-title').innerText = 'Add Product';
    productForm.reset();
    document.getElementById('prod-id').value = '';
    modalError.classList.add('hidden');
    productModal.classList.remove('hidden');
});

document.getElementById('btn-close-modal').addEventListener('click', () => productModal.classList.add('hidden'));
document.getElementById('btn-cancel-modal').addEventListener('click', () => productModal.classList.add('hidden'));

productForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    modalError.classList.add('hidden');

    const id = document.getElementById('prod-id').value;
    const body = JSON.stringify({
        name: document.getElementById('prod-name').value,
        price: parseFloat(document.getElementById('prod-price').value),
        description: document.getElementById('prod-desc').value
    });

    const method = State.isEditing ? 'PUT' : 'POST';
    const url = State.isEditing ? `/product/${id}` : '/product';

    try {
        const res = await apiCall(url, { method, body });
        if (res.ok) {
            productModal.classList.add('hidden');
            loadProducts();
        } else {
            const data = await res.json();
            showError(modalError, extractDotNetErrors(data));
        }
    } catch {
        showError(modalError, "Request failed.");
    }
});

async function editProduct(id) {
    try {
        const res = await apiCall(`/product/${id}`);
        if (res.ok) {
            const prod = await res.json();
            State.isEditing = true;
            document.getElementById('modal-title').innerText = 'Edit Product';
            document.getElementById('prod-id').value = prod.id;
            document.getElementById('prod-name').value = prod.name;
            document.getElementById('prod-price').value = prod.price;
            document.getElementById('prod-desc').value = prod.description;
            
            modalError.classList.add('hidden');
            productModal.classList.remove('hidden');
        }
    } catch {}
}

async function deleteProduct(id) {
    if (!confirm('Are you sure you want to delete this product?')) return;
    try {
        const res = await apiCall(`/product/${id}`, { method: 'DELETE' });
        if (res.ok) loadProducts();
    } catch {}
}

/* Utilities */
function debounce(func, timeout = 300) {
    let timer;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => { func.apply(this, args); }, timeout);
    };
}

// Initial Bootup
updateAuthState();
