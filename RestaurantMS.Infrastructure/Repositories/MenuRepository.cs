using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Infrastructure.Data;

namespace RestaurantMS.Infrastructure.Repositories;

// Class này implement interface IMenuRepository
public class MenuRepository : IMenuRepository
{
    // DbContext dùng để làm việc với database
    private readonly ApplicationDbContext _db;

    // Constructor: nhận DbContext từ Dependency Injection
    public MenuRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    // ================= CATEGORY =================

    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        // Lấy danh sách category từ database
        var categories = await _db.Categories
            .Where(c => c.IsActive)           // chỉ lấy category đang hoạt động
            .OrderBy(c => c.SortOrder)        // sắp xếp theo thứ tự
            .ToListAsync();                   // thực thi query

        return categories;
    }

    // ================= MENU =================

    public async Task<IEnumerable<MenuItem>> GetAllAsync(
        int? categoryId = null,
        bool? isAvailable = null,
        string? search = null)
    {
        // Tạo query ban đầu từ bảng MenuItems
        var query = _db.MenuItems
            .Include(m => m.Category)   // JOIN bảng Category
            .AsQueryable();             // cho phép build query động

        // ===== LỌC THEO CATEGORY =====
        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        // ===== LỌC THEO TRẠNG THÁI =====
        if (isAvailable.HasValue)
        {
            query = query.Where(m => m.IsAvailable == isAvailable.Value);
        }

        // ===== TÌM KIẾM THEO TÊN =====
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Name.Contains(search));
        }

        // ===== SẮP XẾP + TRẢ VỀ =====
        var result = await query
            .OrderBy(m => m.Category.SortOrder) // sắp theo loại
            .ThenBy(m => m.Name)                // rồi theo tên
            .ToListAsync();                     // thực thi query

        return result;
    }

    // ================= GET BY ID =================

    public async Task<MenuItem?> GetByIdAsync(int id)
    {
        // Tìm 1 món theo ID
        var item = await _db.MenuItems
            .Include(m => m.Category)       // load luôn category
            .FirstOrDefaultAsync(m => m.Id == id); // lấy bản ghi đầu tiên

        return item; // nếu không có sẽ trả null
    }

    // ================= CREATE =================

    public async Task<MenuItem> CreateAsync(MenuItem item)
    {
        // Thêm item vào DbContext (chưa lưu DB ngay)
        _db.MenuItems.Add(item);

        // Lưu xuống database
        await _db.SaveChangesAsync();

        // Trả lại item vừa tạo
        return item;
    }

    // ================= UPDATE =================

    public async Task UpdateAsync(MenuItem item)
    {
        // Cập nhật thời gian sửa
        item.UpdatedAt = DateTime.UtcNow;

        // Update object
        _db.MenuItems.Update(item);

        // Lưu xuống database
        await _db.SaveChangesAsync();
    }

    // ================= DELETE =================

    public async Task DeleteAsync(int id)
    {
        // Tìm item theo ID
        var item = await _db.MenuItems.FindAsync(id);

        // Nếu không tìm thấy → báo lỗi
        if (item == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy món #{id}");
        }

        // Xóa khỏi DbContext
        _db.MenuItems.Remove(item);

        // Lưu thay đổi
        await _db.SaveChangesAsync();
    }

    // ================= TOGGLE =================

    public async Task ToggleAvailabilityAsync(int id)
    {
        // Tìm item
        var item = await _db.MenuItems.FindAsync(id);

        // Nếu không có → lỗi
        if (item == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy món #{id}");
        }

        // Đảo trạng thái (true → false, false → true)
        item.IsAvailable = !item.IsAvailable;

        // Cập nhật thời gian
        item.UpdatedAt = DateTime.UtcNow;

        // Lưu xuống DB
        await _db.SaveChangesAsync();
    }
}