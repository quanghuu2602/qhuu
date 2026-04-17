// ── Lưu thông tin sau khi login ───────────────────────────────
function saveAuth(data) {
    localStorage.setItem('token',    data.token);
    localStorage.setItem('role',     data.role);
    localStorage.setItem('fullName', data.fullName);
    localStorage.setItem('email',    data.email);
}

// ── Đọc role từ localStorage ──────────────────────────────────
function getRole()     { return localStorage.getItem('role'); }
function getFullName() { return localStorage.getItem('fullName'); }
function getToken()    { return localStorage.getItem('token'); }

// ── Đăng xuất ─────────────────────────────────────────────────
function logout() {
    localStorage.clear();
    window.location.href = '../pages/login.html';
}

// ── Kiểm tra đã đăng nhập chưa ───────────────────────────────
// Gọi hàm này ở đầu mỗi trang cần bảo vệ
function requireAuth() {
    const token = getToken();
    if (!token) {
        window.location.href = '../pages/login.html';
        return false;
    }
    return true;
}

// ── Sau login redirect đúng trang theo role ───────────────────
function redirectByRole(role) {
    const map = {
        'Admin':    '../pages/dashboard-admin.html',
        'Staff':    '../pages/dashboard-staff.html',
        'Kitchen':  '../pages/dashboard-kitchen.html',
        'Customer': '../pages/dashboard-customer.html',
    };
    window.location.href = map[role] || '../pages/login.html';
}

// ── Kiểm tra role có được phép vào trang không ────────────────
function requireRole(...allowedRoles) {
    if (!requireAuth()) return false;
    const role = getRole();
    if (!allowedRoles.includes(role)) {
        alert('Bạn không có quyền truy cập trang này');
        redirectByRole(role);
        return false;
    }
    return true;
}