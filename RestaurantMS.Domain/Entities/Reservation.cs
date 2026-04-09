namespace RestaurantMS.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public DateTime ReservedAt { get; set; }
    public string? Note { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; } = false;
    public int? TableId { get; set; }
    public Table? Table { get; set; }
}