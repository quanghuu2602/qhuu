using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;
using RestaurantMS.Infrastructure.Services;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _repo;
    private readonly IOrderRepository _order;
    private readonly ITableRepository _table;
    private readonly PdfService _pdf;
    private readonly IWebHostEnvironment _env;

    public PaymentsController(
        IPaymentRepository repo,
        IOrderRepository order,
        ITableRepository table,
        PdfService pdf,
        IWebHostEnvironment env)
    {
        _repo = repo;
        _order = order;
        _table = table;
        _pdf = pdf;
        _env = env;
    }

    // GET /api/payments?date=2024-01-15
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? date = null)
    {
        var list = await _repo.GetAllAsync(date);
        return Ok(list.Select(p => new
        {
            p.Id,
            p.OrderId,
            TableName = p.Order?.Table?.Name,
            p.TotalAmount,
            p.Method,
            p.VoucherCode,
            p.PaidAt,
            p.InvoicePdfUrl
        }));
    }

    // POST /api/payments — tạo thanh toán
    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] CreatePaymentRequest req)
    {
        var order = await _order.GetByIdAsync(req.OrderId);
        if (order is null)
            return NotFound(new { message = "Không tìm thấy order" });

        if (order.Status == OrderStatus.Completed)
            return BadRequest(new { message = "Order này đã thanh toán rồi" });

        // ── Tính tiền ──────────────────────────────────────────────
        var subtotal = order.Details.Sum(d => d.UnitPrice * d.Quantity);
        var vat = Math.Round(subtotal * 0.10m, 0);
        var svcFee = Math.Round(subtotal * req.ServiceFeePercent / 100, 0);

        var discount = VoucherStore.GetDiscount(req.VoucherCode, subtotal);
        if (discount < 0)
            return BadRequest(new
            {
                message = $"Mã voucher '{req.VoucherCode}' không hợp lệ"
            });

        var total = subtotal + vat + svcFee - discount;
        var cashierId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // ── BƯỚC 1: Lưu DB trước để có Id thực ────────────────────
        var payment = new Payment
        {
            OrderId = req.OrderId,
            Subtotal = subtotal,
            VatAmount = vat,
            ServiceFee = svcFee,
            DiscountAmount = discount,
            TotalAmount = total,
            VoucherCode = req.VoucherCode?.ToUpper(),
            Method = Enum.Parse<PaymentMethod>(req.PaymentMethod),
            CashierId = cashierId,
            PaidAt = DateTime.Now,
            InvoicePdfUrl = null   // chưa có PDF
        };

        var created = await _repo.CreateAsync(payment);
        // Lúc này created.Id đã có giá trị thực (VD: 1, 2, 3...)

        // ── BƯỚC 2: Cập nhật Order + giải phóng bàn ───────────────
        await _order.UpdateStatusAsync(req.OrderId, OrderStatus.Completed);
        await _table.UpdateStatusAsync(
            order.TableId, TableStatus.Available);

        // ── BƯỚC 3: Build DTO với Id thực để tạo PDF ──────────────
        var pdfDto = new PaymentResultDto(
            Id: created.Id,        // ← Id thực, không phải 0
            OrderId: created.OrderId,
            TableName: order.Table?.Name ?? "",
            Subtotal: created.Subtotal,
            VatAmount: created.VatAmount,
            ServiceFee: created.ServiceFee,
            DiscountAmount: created.DiscountAmount,
            TotalAmount: created.TotalAmount,
            VoucherCode: created.VoucherCode,
            Method: created.Method.ToString(),
            PdfUrl: "",
            PaidAt: created.PaidAt,
            Items: order.Details?.Select(d => new PaymentItemDto(
                d.MenuItem?.Name ?? "",
                d.Quantity,
                d.UnitPrice,
                d.UnitPrice * d.Quantity
            )).ToList() ?? new()
        );

        // ── BƯỚC 4: Tạo PDF với Id đúng ───────────────────────────
        try
        {
            var pdfBytes = _pdf.GenerateInvoice(pdfDto);
            var folder = Path.Combine(_env.WebRootPath, "invoices");
            Directory.CreateDirectory(folder);

            // Tên file có Id để dễ tìm
            var fileName = $"invoice_{created.Id:D5}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(folder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            // Cập nhật đường dẫn PDF vào DB
            created.InvoicePdfUrl = $"/invoices/{fileName}";
            await _repo.UpdatePdfUrlAsync(created.Id, created.InvoicePdfUrl);
        }
        catch (Exception ex)
        {
            // Lỗi PDF không ảnh hưởng thanh toán
            Console.WriteLine($"Lỗi tạo PDF: {ex.Message}");
        }

        return Ok(pdfDto with { PdfUrl = created.InvoicePdfUrl ?? "" });
    }
    // GET /api/payments/order/5 — lấy payment của 1 order
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        var p = await _repo.GetByOrderIdAsync(orderId);
        if (p is null)
            return NotFound(new
            {
                message = "Order chưa thanh toán"
            });
        return Ok(BuildPdfDto(p, p.Order!));
    }

    // GET /api/payments/download/5 — tải PDF
    [HttpGet("download/{paymentId}")]
    public async Task<IActionResult> Download(int paymentId)
    {
        var p = await _repo.GetByOrderIdAsync(paymentId);
        if (p?.InvoicePdfUrl is null)
            return NotFound();

        var path = Path.Combine(
            _env.WebRootPath,
            p.InvoicePdfUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (!System.IO.File.Exists(path))
            return NotFound(new { message = "File PDF không tồn tại" });

        var bytes = await System.IO.File.ReadAllBytesAsync(path);
        return File(bytes, "application/pdf",
            $"HoaDon_{paymentId}.pdf");
    }

    // ── Validate voucher ──────────────────────────────────────
    [HttpGet("validate-voucher")]
    public IActionResult ValidateVoucher(
        [FromQuery] string code,
        [FromQuery] decimal subtotal)
    {
        var discount = VoucherStore.GetDiscount(code, subtotal);
        if (discount < 0)
            return Ok(new
            {
                valid = false,
                message = "Mã không hợp lệ"
            });

        return Ok(new
        {
            valid = true,
            discountAmount = discount,
            message = $"Giảm {discount:N0}đ"
        });
    }

    // ── Helper build DTO ──────────────────────────────────────
    private static PaymentResultDto BuildPdfDto(
        Payment payment, Order order) => new(
        payment.Id,
        payment.OrderId,
        order.Table?.Name ?? "",
        payment.Subtotal,
        payment.VatAmount,
        payment.ServiceFee,
        payment.DiscountAmount,
        payment.TotalAmount,
        payment.VoucherCode,
        payment.Method.ToString(),
        payment.InvoicePdfUrl ?? "",
        payment.PaidAt,
        order.Details?.Select(d => new PaymentItemDto(
            d.MenuItem?.Name ?? "",
            d.Quantity,
            d.UnitPrice,
            d.UnitPrice * d.Quantity
        )).ToList() ?? new()
    );
}