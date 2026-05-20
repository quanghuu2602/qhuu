using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RestaurantMS.API.Hubs;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Kitchen")]   // ✅ THÊM Kitchen
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repo;
    private readonly IMenuRepository _menu;
    private readonly ITableRepository _table;
    private readonly IHubContext<KitchenHub> _hub;

    public OrdersController(
        IOrderRepository repo,
        IMenuRepository menu,
        ITableRepository table,
        IHubContext<KitchenHub> hub)
    {
        _repo = repo;
        _menu = menu;
        _table = table;
        _hub = hub;
    }

    // GET /api/orders — danh sách order đang active
    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var orders = await _repo.GetActiveOrdersAsync();
        return Ok(orders.Select(ToDto));
    }

    // GET /api/orders/table/5 — order của 1 bàn
    [HttpGet("table/{tableId}")]
    public async Task<IActionResult> GetByTable(int tableId)
    {
        var order = await _repo.GetByTableIdAsync(tableId);
        if (order is null)
            return NotFound(new { message = "Bàn chưa có order" });
        return Ok(ToDto(order));
    }

    // GET /api/orders/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order is null)
            return NotFound(new { message = "Không tìm thấy order" });
        return Ok(ToDto(order));
    }

    // POST /api/orders — tạo order mới
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderRequest req)
    {
        // Kiểm tra bàn có order chưa
        var existing = await _repo.GetByTableIdAsync(req.TableId);
        if (existing != null)
            return BadRequest(new
            {
                message = "Bàn đang có order, vui lòng dùng order đó",
                orderId = existing.Id
            });

        var staffId = User.FindFirstValue(
            ClaimTypes.NameIdentifier) ?? "";

        var order = new Order
        {
            TableId = req.TableId,
            StaffId = staffId,
            Note = req.Note,
            Status = OrderStatus.Pending
        };

        var created = await _repo.CreateAsync(order);

        // Cập nhật bàn → Occupied
        await _table.UpdateStatusAsync(
            req.TableId, Domain.Enums.TableStatus.Occupied);

        return CreatedAtAction(nameof(GetById),
            new { id = created.Id },
            new { message = "Đã tạo order", orderId = created.Id });
    }

    // POST /api/orders/5/items — thêm món vào order
    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItem(int id,
        [FromBody] AddOrderItemRequest req)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order is null)
            return NotFound(new { message = "Không tìm thấy order" });

        if (order.Status == OrderStatus.Completed ||
            order.Status == OrderStatus.Cancelled)
            return BadRequest(new
            {
                message = "Order đã hoàn thành, không thể thêm món"
            });

        // Lấy giá hiện tại của món
        var menuItem = await _menu.GetByIdAsync(req.MenuItemId);
        if (menuItem is null)
            return NotFound(new { message = "Không tìm thấy món" });

        if (!menuItem.IsAvailable)
            return BadRequest(new { message = "Món này đã hết" });

        var detail = new OrderDetail
        {
            OrderId = id,
            MenuItemId = req.MenuItemId,
            Quantity = req.Quantity,
            UnitPrice = menuItem.Price,  // chốt giá tại thời điểm gọi
            SpecialNote = req.SpecialNote
        };

        await _repo.AddItemAsync(detail);
        var updated = await _repo.GetByIdAsync(id);
        return Ok(ToDto(updated!));
    }

    // PUT /api/orders/5/items/3 — sửa số lượng
    [HttpPut("{id}/items/{detailId}")]
    public async Task<IActionResult> UpdateItem(int id,
        int detailId,
        [FromBody] UpdateOrderItemRequest req)
    {
        await _repo.UpdateItemAsync(
            detailId, req.Quantity, req.SpecialNote);
        var updated = await _repo.GetByIdAsync(id);
        return Ok(ToDto(updated!));
    }

    // DELETE /api/orders/5/items/3 — xoá món khỏi order
    [HttpDelete("{id}/items/{detailId}")]
    public async Task<IActionResult> RemoveItem(int id, int detailId)
    {
        await _repo.RemoveItemAsync(detailId);
        var updated = await _repo.GetByIdAsync(id);
        return Ok(ToDto(updated!));
    }

    // PATCH /api/orders/5/send-to-kitchen — gửi bếp + SignalR
    [HttpPatch("{id}/send-to-kitchen")]
    public async Task<IActionResult> SendToKitchen(int id)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order is null)
            return NotFound(new { message = "Không tìm thấy order" });

        if (!order.Details.Any())
            return BadRequest(new
            {
                message = "Chưa có món nào trong order"
            });

        await _repo.UpdateStatusAsync(id, OrderStatus.Cooking);
        order = await _repo.GetByIdAsync(id);

        // ── Gửi SignalR tới group Kitchen ─────────────────────
        var kotPayload = new
        {
            orderId = order!.Id,
            tableName = order.Table?.Name,
            items = order.Details.Select(d => new
            {
                name = d.MenuItem?.Name,
                quantity = d.Quantity,
                note = d.SpecialNote
            }),
            sentAt = DateTime.Now.ToString("HH:mm")
        };

        await _hub.Clients.Group("Kitchen").SendAsync("NewKOT", kotPayload);

        return Ok(new { message = "Đã gửi bếp thành công" });
    }

    // PATCH /api/orders/5/ready — bếp xong
    [HttpPatch("{id}/ready")]
    [Authorize(Roles = "Admin,Staff,Kitchen")]
    public async Task<IActionResult> MarkReady(int id)
    {
        await _repo.UpdateStatusAsync(id, OrderStatus.Ready);

        // Báo Staff món đã xong
        await _hub.Clients
                  .Group("Staff")
                  .SendAsync("OrderReady", new { orderId = id });

        return Ok(new { message = "Đã đánh dấu xong món" });
    }

    // PATCH /api/orders/5/complete — thanh toán xong
    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order is null)
            return NotFound(new { message = "Không tìm thấy order" });

        await _repo.UpdateStatusAsync(id, OrderStatus.Completed);

        // Giải phóng bàn → Available
        await _table.UpdateStatusAsync(
            order.TableId, Domain.Enums.TableStatus.Available);

        return Ok(new { message = "Hoàn tất order" });
    }

    // PATCH /api/orders/5/cancel
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order is null)
            return NotFound(new { message = "Không tìm thấy order" });

        await _repo.UpdateStatusAsync(id, OrderStatus.Cancelled);
        await _table.UpdateStatusAsync(
            order.TableId, Domain.Enums.TableStatus.Available);

        return Ok(new { message = "Đã huỷ order" });
    }

    // ── Helper map Entity → DTO ───────────────────────────────
    private static OrderDto ToDto(Order o)
    {
        var details = o.Details?.Select(d => new OrderDetailDto(
            d.Id,
            d.MenuItemId,
            d.MenuItem?.Name ?? "",
            d.MenuItem?.Category?.Name ?? "",
            d.Quantity,
            d.UnitPrice,
            d.UnitPrice * d.Quantity,
            d.SpecialNote
        )).ToList() ?? new();

        var total = details.Sum(d => d.SubTotal);

        return new OrderDto(
            o.Id,
            o.TableId,
            o.Table?.Name ?? "",
            o.StaffId,
            o.Staff?.FullName ?? "",
            o.Status.ToString(),
            o.Note,
            o.CreatedAt,
            details,
            total
        );
    }
}