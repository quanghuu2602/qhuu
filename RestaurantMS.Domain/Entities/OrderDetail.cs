namespace RestaurantMS.Domain.Entities;

public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }      // Chốt giá lúc gọi món
    public string? SpecialNote { get; set; }    // "ít đá", "không hành"
}