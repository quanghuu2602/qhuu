using RestaurantMS.Domain.Enums;

namespace RestaurantMS.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public decimal Subtotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? VoucherCode { get; set; }
    public PaymentMethod Method { get; set; }
    public string? InvoicePdfUrl { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string CashierId { get; set; } = string.Empty;  // UserId kiểu string
}