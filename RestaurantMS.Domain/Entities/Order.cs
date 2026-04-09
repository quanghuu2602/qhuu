using RestaurantMS.Domain.Enums;

namespace RestaurantMS.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public Table Table { get; set; } = null!;
    public string StaffId { get; set; } = string.Empty;  // UserId kiểu string
    public ApplicationUser Staff { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<OrderDetail> Details { get; set; } = new List<OrderDetail>();
    public Payment? Payment { get; set; }
}