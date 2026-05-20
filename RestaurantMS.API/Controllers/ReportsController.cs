using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantMS.Domain.Enums;
using RestaurantMS.Infrastructure.Data;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    // GET /api/reports/summary?date=2024-01-15
    // Tổng quan hôm nay
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? date = null)
    {
        var d = date?.Date ?? DateTime.Today;

        var payments = await _db.Payments
            .Where(p => p.PaidAt.Date == d)
            .ToListAsync();

        var orders = await _db.Orders
            .Where(o => o.CreatedAt.Date == d)
            .ToListAsync();

        var tables = await _db.Tables.ToListAsync();

        return Ok(new
        {
            date = d.ToString("dd/MM/yyyy"),
            totalRevenue = payments.Sum(p => p.TotalAmount),
            totalOrders = orders.Count,
            completedOrders = orders.Count(o =>
                o.Status == OrderStatus.Completed),
            totalTables = tables.Count,
            occupiedTables = tables.Count(t =>
                t.Status == TableStatus.Occupied),
            availableTables = tables.Count(t =>
                t.Status == TableStatus.Available),
        });
    }

    // GET /api/reports/revenue?days=7
    // Doanh thu N ngày gần nhất
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] int days = 7)
    {
        var from = DateTime.Today.AddDays(-(days - 1));

        var payments = await _db.Payments
            .Where(p => p.PaidAt.Date >= from)
            .ToListAsync();

        // Group theo ngày
        var result = Enumerable.Range(0, days)
            .Select(i => {
                var day = from.AddDays(i);
                var revenue = payments.Where(p => p.PaidAt.Date == day).Sum(p => p.TotalAmount);
                return new
                {
                    date = day.ToString("dd/MM"),
                    revenue = revenue,
                    orders = payments.Count(p => p.PaidAt.Date == day),
                };
            })
            .ToList();

        return Ok(result);
    }

    // GET /api/reports/top-items?limit=5
    // Top món bán chạy
    [HttpGet("top-items")]
    public async Task<IActionResult> GetTopItems([FromQuery] int limit = 5, [FromQuery] int days = 30)
    {
        var from = DateTime.Today.AddDays(-days);

        var result = await _db.OrderDetails
            .Include(d => d.MenuItem)
            .Include(d => d.Order)
            .Where(d => d.Order.CreatedAt.Date >= from && d.Order.Status == OrderStatus.Completed)
            .GroupBy(d => new { d.MenuItemId, d.MenuItem.Name })
            .Select(g => new {
                id = g.Key.MenuItemId,
                name = g.Key.Name,
                quantity = g.Sum(d => d.Quantity),
                revenue = g.Sum(d => d.Quantity * d.UnitPrice),
            })
            .OrderByDescending(x => x.quantity)
            .Take(limit)
            .ToListAsync();

        return Ok(result);
    }

    // GET /api/reports/hourly
    // Phân bố đơn theo giờ trong ngày
    [HttpGet("hourly")]
    public async Task<IActionResult> GetHourly([FromQuery] int days = 7)
    {
        var from = DateTime.Today.AddDays(-days);

        var result = await _db.Orders
            .Where(o => o.CreatedAt.Date >= from && o.Status == OrderStatus.Completed)
            .GroupBy(o => o.CreatedAt.Hour)
            .Select(g => new { hour = g.Key, orders = g.Count() })
            .OrderBy(x => x.hour)
            .ToListAsync();

        // Fill đủ 24 giờ từ 00:00 đến 23:00
        var full = Enumerable.Range(0, 24).Select(h => new {
            hour = h,
            label = $"{h:D2}:00",
            orders = result.FirstOrDefault(r => r.hour == h)?.orders ?? 0
        });

        return Ok(full);
    }

    // GET /api/reports/payment-methods
    // Tỉ lệ phương thức thanh toán
    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] int days = 30)
    {
        var from = DateTime.Today.AddDays(-days);

        var result = await _db.Payments
            .Where(p => p.PaidAt.Date >= from)
            .GroupBy(p => p.Method)
            .Select(g => new {
                method = g.Key.ToString(), // Chuyển Enum thành chuỗi (Ví dụ: "Cash", "CreditCard")
                count = g.Count(),
                revenue = g.Sum(p => p.TotalAmount),
            })
            .ToListAsync();

        return Ok(result);
    }
}