using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;
using RestaurantMS.Infrastructure.Data;

namespace RestaurantMS.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _db;
    public OrderRepository(ApplicationDbContext db) => _db = db;
    // ── Include đầy đủ để map DTO ──────────────────────────────

    private IQueryable<Order>FullQuery() =>
        _db.Orders
           .Include(o =>o.Table)
           .Include(o => o.Staff)
           .Include(o => o.Details)
               .ThenInclude(d => d.MenuItem)
                   .ThenInclude(m => m.Category);
    public async Task<Order> CreateAsync( Order order)
    {
        _db.Orders .Add(order); 
        await _db.SaveChangesAsync();
        return order;
    }
    public async Task<Order?> GetByIdAsync(int id)
        => await FullQuery().FirstOrDefaultAsync(o => o.Id == id);
    // Lấy tất cả order đang active (chưa Completed/Cancelled)
    // Lấy danh sách order đang hoạt động (chưa hoàn thành hoặc bị hủy)
    public async Task<IEnumerable<Order>> GetActiveOrdersAsync()
        => await FullQuery() // query đã include đầy đủ quan hệ
            .Where(o => o.Status != OrderStatus.Completed
                     && o.Status != OrderStatus.Cancelled) // lọc order active
            .OrderByDescending(o => o.CreatedAt) // sắp xếp mới nhất trước
            .ToListAsync(); // thực thi query và trả về danh sách
    // Lấy order hiện tại của 1 bàn
    public async Task<Order?> GetByTableIdAsync(int tableId)
        => await FullQuery()
                .Where(o => o.TableId == tableId
                                      && o.Status != OrderStatus.Completed
                                      && o.Status != OrderStatus.Cancelled)
                 .OrderByDescending(o => o.CreatedAt)
                 .FirstOrDefaultAsync();
    // Cập nhật trạng thái order
    public async Task UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy order #{id}");

        order.Status = status; // cập nhật trạng thái
        order.UpdatedAt = DateTime.UtcNow; // cập nhật thời gian

        await _db.SaveChangesAsync(); // lưu xuống DB
    }

    // Xóa order
    public async Task DeleteAsync(int id)
    {
        var order = await _db.Orders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy order #{id}");

        _db.Orders.Remove(order); // đánh dấu xóa
        await _db.SaveChangesAsync(); // thực thi DELETE
    }
    public async Task<OrderDetail> AddItemAsync(OrderDetail detail)
    {
        // Nếu đã có món này trong order thì cộng thêm số lượng
        var existing = await _db.OrderDetails
            .FirstOrDefaultAsync(d => d.OrderId == detail.OrderId
                                   && d.MenuItemId == detail.MenuItemId
                                   && d.SpecialNote == detail.SpecialNote);
        if (existing != null)
        {
            existing.Quantity += detail.Quantity;
            await _db.SaveChangesAsync();
            return existing;
        }

        _db.OrderDetails.Add(detail);
        await _db.SaveChangesAsync();
        return detail;
    }
    public async Task UpdateItemAsync(int detailId,
                                       int quantity,
                                       string? note)
    {
        var detail = await _db.OrderDetails.FindAsync(detailId)
            ?? throw new KeyNotFoundException($"Không tìm thấy món #{detailId}");

        if (quantity <= 0)
        {
            _db.OrderDetails.Remove(detail);
        }
        else
        {
            detail.Quantity = quantity;
            detail.SpecialNote = note;
        }
        await _db.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(int detailId)
    {
        var detail = await _db.OrderDetails.FindAsync(detailId)
            ?? throw new KeyNotFoundException($"Không tìm thấy món #{detailId}");
        _db.OrderDetails.Remove(detail);
        await _db.SaveChangesAsync();
    }
}