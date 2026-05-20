using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantMS.Application.DTOs;

public record CreatePaymentRequest(
    int OrderId,
    string PaymentMethod,   // Cash | BankTransfer | Momo | VnPay
    string? VoucherCode,
    decimal ServiceFeePercent  // VD: 5 = 5%
);

public record PaymentResultDto(
    int Id,
    int OrderId,
    string TableName,
    decimal Subtotal,
    decimal VatAmount,
    decimal ServiceFee,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? VoucherCode,
    string Method,
    string PdfUrl,
    DateTime PaidAt,
    List<PaymentItemDto> Items
);

public record PaymentItemDto(
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal
);

// Danh sách voucher mẫu — sau này có thể lưu DB
public static class VoucherStore
{
    public static readonly Dictionary<string, decimal> Vouchers = new()
    {
        { "WELCOME10", 10 },   // giảm 10%
        { "SALE20",    20 },   // giảm 20%
        { "VIP50K",    0  },   // giảm 50k (flat)
        { "FREESHIP",   5 },
    };

    // Trả về % giảm, -1 nếu không hợp lệ
    public static decimal GetDiscount(string? code, decimal subtotal)
    {
        if (string.IsNullOrEmpty(code)) return 0;
        if (!Vouchers.TryGetValue(code.ToUpper(), out var pct)) return -1;
        return subtotal * pct / 100;
    }
}
