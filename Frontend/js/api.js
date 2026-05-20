const BASE_URL = 'https://localhost:7044/api';

async function callApi(endpoint, method = 'GET', body = null) {
    const token = sessionStorage.getItem('token');

    const options = {
        method,
        headers: { 'Content-Type': 'application/json' },
    };

    if (token) {
        options.headers['Authorization'] = `Bearer ${token}`;
    }

    if (body) {
        options.body = JSON.stringify(body);
    }

    let response;
    try {
        response = await fetch(`${BASE_URL}${endpoint}`, options);
    } catch (err) {
        // Lỗi này xảy ra khi API chưa chạy hoặc CORS bị chặn
        throw new Error('Không kết nối được tới server. Kiểm tra VS tím đang chạy chưa?');
    }

    // Đọc text trước, sau đó mới parse JSON
    const text = await response.text();

    // Nếu response rỗng
    if (!text) {
        if (!response.ok) {
            throw new Error(`Lỗi ${response.status}: Server không trả về dữ liệu`);
        }
        return null;
    }

    // Parse JSON
    let data;
    try {
        data = JSON.parse(text);
    } catch {
        throw new Error('Server trả về dữ liệu không hợp lệ: ' + text);
    }

    if (!response.ok) {
        throw new Error(data.message || `Lỗi ${response.status}`);
    }

    return data;
}

const authApi = {
    login: (email, password) =>
        callApi('/auth/login', 'POST', { email, password }),

    register: (fullName, email, password, phone) =>
        callApi('/auth/register', 'POST', { fullName, email, password, phone }),

    //làm menu
};
const menuApi = {
    // Lấy danh sách categories
    getCategories: () =>
        callApi('/menu/categories'),

    // Lấy danh sách món — có filter
    getAll: (categoryId = null, search = null) => {
        let url = '/menu?isAvailable=true';
        if (categoryId) url += `&categoryId=${categoryId}`;
        if (search)     url += `&search=${encodeURIComponent(search)}`;
        return callApi(url);
    },

    // Admin — lấy tất cả kể cả hết món
    getAllAdmin: (categoryId = null, search = null) => {
        let url = '/menu';
        const params = [];
        if (categoryId) params.push(`categoryId=${categoryId}`);
        if (search)     params.push(`search=${encodeURIComponent(search)}`);
        if (params.length) url += '?' + params.join('&');
        return callApi(url);
    },

    create: (data) =>
        callApi('/menu', 'POST', data),

    update: (id, data) =>
        callApi(`/menu/${id}`, 'PUT', data),

    delete: (id) =>
        callApi(`/menu/${id}`, 'DELETE'),

    toggle: (id) =>
        callApi(`/menu/${id}/toggle`, 'PATCH'),
};
const reservationApi = {
    // Customer tạo đặt bàn
    create: (data) =>
        callApi('/reservations', 'POST', data),

    // Customer xem của mình (truyền phone)
    getMine: (phone) =>
        callApi(`/reservations/mine${phone ? '?phone=' + phone : ''}`),

    // Admin/Staff xem tất cả
    getAll: (date = null, isConfirmed = null) => {
        const params = [];
        if (date)
            params.push(`date=${date}`);
        if (isConfirmed !== null)
            params.push(`isConfirmed=${isConfirmed}`);
        const qs = params.length ? '?' + params.join('&') : '';
        return callApi(`/reservations${qs}`);
    },

    // Tìm theo mã booking
    getByCode: (code) =>
        callApi(`/reservations/by-code/${code}`),

    // Xác nhận + gán bàn
    confirm: (id, tableId = null) =>
        callApi(`/reservations/${id}/confirm`, 'PATCH', { tableId }),

    // Huỷ
    cancel: (id) =>
        callApi(`/reservations/${id}/cancel`, 'PATCH'),
};
const tableApi = {
    // Lấy danh sách tất cả khu vực kèm theo bàn bên trong
    getAreas: () => 
        callApi('/tables/areas'),

    // Cập nhật trạng thái bàn (Available, Occupied, Reserved)
    updateStatus: (id, status) => 
        callApi(`/tables/${id}/status`, 'PATCH', { status }),

    // Lấy danh sách các bàn đang trống
    getAvailable: () => 
        callApi('/tables/available'),

    // Admin: Thêm bàn mới vào hệ thống
    addTable: (data) => 
        callApi('/tables', 'POST', data),

    // Admin: Xoá bàn khỏi hệ thống
    deleteTable: (id) => 
        callApi(`/tables/${id}`, 'DELETE'),
};
const orderApi = {
    // Lấy tất cả orders (Admin)
    getAll: (status = null) => {
        let url = '/orders';
        if (status) url += `?status=${status}`;
        return callApi(url);
    },

    // Lấy order của bàn
    getByTable: (tableId) =>
        callApi(`/orders/table/${tableId}`),

    // Lấy order theo id
    getById: (id) =>
        callApi(`/orders/${id}`),

    // Tạo order mới
    create: (tableId, note = null) =>
        callApi('/orders', 'POST', { tableId, note }),

    // Thêm món
    addItem: (orderId, menuItemId, quantity, specialNote = null) =>
        callApi(`/orders/${orderId}/items`, 'POST', {
            menuItemId, quantity, specialNote
        }),

    // Sửa số lượng
    updateItem: (orderId, detailId, quantity, specialNote) =>
        callApi(`/orders/${orderId}/items/${detailId}`, 'PUT', {
            quantity, specialNote
        }),

    // Xoá món
    removeItem: (orderId, detailId) =>
        callApi(`/orders/${orderId}/items/${detailId}`, 'DELETE'),

    // Gửi bếp
    sendToKitchen: (orderId) =>
        callApi(`/orders/${orderId}/send-to-kitchen`, 'PATCH'),

    // Bếp báo xong
    markReady: (orderId) =>
        callApi(`/orders/${orderId}/ready`, 'PATCH'),

    // Hoàn tất
    complete: (orderId) =>
        callApi(`/orders/${orderId}/complete`, 'PATCH'),

    // Huỷ
    cancel: (orderId) =>
        callApi(`/orders/${orderId}/cancel`, 'PATCH'),
};
const paymentApi = {
    // Thanh toán
    create: (data) =>
        callApi('/payments', 'POST', data),

    // Lấy payment của order
    getByOrder: (orderId) =>
        callApi(`/payments/order/${orderId}`),

    // Validate voucher
    validateVoucher: (code, subtotal) =>
        callApi(`/payments/validate-voucher?code=${code}&subtotal=${subtotal}`),

    // Tải PDF — thử fetch+blob trước, fallback window.open nếu lỗi
    downloadPdf: async (paymentId) => {
        const token = sessionStorage.getItem('token');
        const url = `${BASE_URL}/payments/download/${paymentId}`;

        // Cách 1: fetch + blob (kiểm soát lỗi tốt, không popup)
        try {
            const res = await fetch(url, {
                headers: token ? { 'Authorization': `Bearer ${token}` } : {},
            });

            if (!res.ok) {
                let msg = `Lỗi ${res.status}`;
                try { const e = await res.json(); msg = e.message || e.title || msg; } catch {}
                throw new Error(msg);
            }

            const blob = await res.blob();
            if (blob.size === 0) throw new Error('File rỗng');

            const blobUrl = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = blobUrl;
            a.download = `HD-${paymentId}.pdf`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            setTimeout(() => URL.revokeObjectURL(blobUrl), 5000);
            return true;
        } catch (e) {
            // Cách 2: Fallback window.open (kèm token query string)
            const fallbackUrl = `${url}?token=${encodeURIComponent(token || '')}`;
            const w = window.open(fallbackUrl, '_blank');
            if (!w || w.closed) {
                throw new Error(e.message);
            }
            return true;
        }
    },
};
const reportApi = {
    getSummary: (date = null) => {
        const qs = date ? `?date=${date}` : '';
        return callApi(`/reports/summary${qs}`);
    },
    getRevenue: (days = 7) =>
        callApi(`/reports/revenue?days=${days}`),
    getTopItems: (limit = 5, days = 30) =>
        callApi(`/reports/top-items?limit=${limit}&days=${days}`),
    getHourly: (days = 7) =>
        callApi(`/reports/hourly?days=${days}`),
    getPaymentMethods: (days = 30) =>
        callApi(`/reports/payment-methods?days=${days}`),
};
const userApi = {
    getAll:       (role = null) =>
        callApi(`/users${role ? '?role=' + role : ''}`),

    toggleActive: (id) =>
        callApi(`/users/${id}/toggle-active`, 'PATCH'),

    changeRole:   (id, role) =>
        callApi(`/users/${id}/change-role`, 'PATCH', { role }),

    getMe:        () =>
        callApi('/users/me'),

    updateMe:     (fullName, phone) =>
        callApi('/users/me', 'PUT', { fullName, phone }),

    changePassword: (currentPassword, newPassword) =>
        callApi('/users/me/change-password', 'POST',
            { currentPassword, newPassword }),
};