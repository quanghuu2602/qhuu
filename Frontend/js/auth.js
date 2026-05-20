// ── Lưu thông tin sau khi login ───────────────────────────────
function saveAuth(data) {
    sessionStorage.setItem('token', data.token);

    // ✅ FIX: chuẩn hóa role về lowercase
    const role = (data.role || '').toLowerCase();
    sessionStorage.setItem('role', role);

    sessionStorage.setItem('fullName', data.fullName);
    sessionStorage.setItem('email', data.email);
    sessionStorage.setItem('phone', data.phone || '');
}

// ── Đọc dữ liệu ───────────────────────────────────────────────
function getRole()     { return sessionStorage.getItem('role'); }
function getFullName() { return sessionStorage.getItem('fullName'); }
function getToken()    { return sessionStorage.getItem('token'); }
function getPhone()    { return sessionStorage.getItem('phone'); }

// ── Đăng xuất ─────────────────────────────────────────────────
function logout() {
    sessionStorage.clear();
    window.location.href = '../pages/login.html';
}

// ── Check đăng nhập ───────────────────────────────────────────
function requireAuth() {
    const token = getToken();
    if (!token) {
        window.location.href = '../pages/login.html';
        return false;
    }
    return true;
}

// ── Redirect theo role ────────────────────────────────────────
function redirectByRole(role) {
    role = (role || '').toLowerCase(); // ✅ FIX

    const map = {
        'admin':    '../pages/dashboard-admin.html',
        'staff':    '../pages/dashboard-staff.html',
        'kitchen':  '../pages/dashboard-kitchen.html',
        'customer': '../pages/dashboard-customer.html',
    };

    window.location.href = map[role] || '../pages/login.html';
}

// ── Check quyền truy cập ──────────────────────────────────────
function requireRole(...allowedRoles) {
    if (!requireAuth()) return false;

    const role = (getRole() || '').toLowerCase(); // ✅ FIX
    const allowed = allowedRoles.map(r => r.toLowerCase()); // ✅ FIX

    if (!allowed.includes(role)) {
        alert('Bạn không có quyền truy cập trang này');
        redirectByRole(role);
        return false;
    }
    return true;
}