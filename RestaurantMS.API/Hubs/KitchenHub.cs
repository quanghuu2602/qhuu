using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace RestaurantMS.API.Hubs;

[Authorize]
public class KitchenHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // 1. Lấy thông tin Role và Tên người dùng để Log cho dễ nhìn
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value
                ?? Context.User?.FindFirst("role")?.Value;
        var userName = Context.User?.Identity?.Name ?? "Unknown User";

        Console.WriteLine($"[SignalR Connect] ID: {Context.ConnectionId} | User: {userName} | Role: {role}");

        // 2. Phân loại vào Group Kitchen
        if (role == "Kitchen" || role == "Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Kitchen");
            Console.WriteLine($"   ==> Added {userName} to group: Kitchen");
        }

        // 3. Phân loại vào Group Staff
        if (role == "Staff" || role == "Admin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Staff");
            Console.WriteLine($"   ==> Added {userName} to group: Staff");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[SignalR Disconnect] ID: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // --- Bổ sung thêm các phương thức tương tác ngược lại nếu cần ---

    /// <summary>
    /// Khi bếp nấu xong, gọi hàm này để thông báo cho tất cả Staff/Admin
    /// </summary>
    public async Task OrderReady(int orderId, string tableName)
    {
        await Clients.Group("Staff").SendAsync("ReceiveOrderReady", new
        {
            orderId,
            tableName,
            message = $"Món ăn của {tableName} đã sẵn sàng!"
        });
    }
}