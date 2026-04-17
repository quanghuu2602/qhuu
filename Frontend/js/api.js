const BASE_URL = 'https://localhost:7044/api';

async function callApi(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem('token');

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
    // Customer tạo đặt bàn mới
    create: (data) =>
        callApi('/reservations', 'POST', data),

    // Customer xem đặt bàn của mình
    getMine: () =>
        callApi('/reservations/mine'),

    // Customer huỷ
    cancel: (id) =>
        callApi(`/reservations/${id}/cancel`, 'PATCH'),

    // Admin xem tất cả
    getAll: () =>
        callApi('/reservations'),

    // Admin xác nhận
    confirm: (id) =>
        callApi(`/reservations/${id}/confirm`, 'PATCH'),
};
const tableApi = {
    // Lấy tất cả bàn theo khu vực
    getAreas: () =>
        callApi('/tables/areas'),

    // Cập nhật trạng thái bàn
    updateStatus: (id, status) =>
        callApi(`/tables/${id}/status`, 'PATCH', { status }),

    // Lấy bàn trống (Customer dùng)
    getAvailable: () =>
        callApi('/tables/available'),
};