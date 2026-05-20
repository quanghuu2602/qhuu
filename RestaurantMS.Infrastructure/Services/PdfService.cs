using System.Reflection.Metadata;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RestaurantMS.Application.DTOs;
using Document = QuestPDF.Fluent.Document;
namespace RestaurantMS.Infrastructure.Services;

public class PdfService
{
    // Tạo PDF và trả về bytes
    public byte[] GenerateInvoice(PaymentResultDto payment)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Column(col =>
                {
                    // ── Header ────────────────────────────────
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("🍽 RestaurantMS")
                             .FontSize(18).Bold().FontColor("#4f46e5");
                            c.Item().Text("123 Đường ABC, Quận 1, TP.HCM")
                             .FontSize(9).FontColor("#888");
                            c.Item().Text("ĐT: 0909 999 888")
                             .FontSize(9).FontColor("#888");
                        });
                        row.ConstantItem(100).AlignRight().Column(c =>
                        {
                            c.Item().Text("HÓA ĐƠN")
                             .FontSize(14).Bold();
                            c.Item().Text($"#{payment.Id:D5}")
                             .FontSize(11).FontColor("#4f46e5");
                            c.Item().Text(payment.PaidAt
                             .ToString("dd/MM/yyyy HH:mm"))
                             .FontSize(9).FontColor("#888");
                        });
                    });

                    col.Item().PaddingVertical(8)
                       .LineHorizontal(1).LineColor("#e0e0e0");

                    // ── Thông tin bàn ─────────────────────────
                    col.Item().Background("#f8f8ff")
                       .Padding(10).Column(c =>
                       {
                           c.Item().Text($"Bàn: {payment.TableName}")
                            .FontSize(11).Bold();
                           c.Item().Text($"Thời gian: " +
                               $"{payment.PaidAt:HH:mm dd/MM/yyyy}")
                            .FontSize(9).FontColor("#555");
                       });

                    col.Item().PaddingTop(10);

                    // ── Bảng món ──────────────────────────────
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(4);  // Tên món
                            cols.RelativeColumn(1);  // SL
                            cols.RelativeColumn(2);  // Đơn giá
                            cols.RelativeColumn(2);  // Thành tiền
                        });

                        // Header
                        table.Header(h =>
                        {
                            foreach (var txt in new[]
                            { "Món", "SL", "Đơn giá", "Thành tiền" })
                            {
                                h.Cell().Background("#1a1a2e")
                                 .Padding(6)
                                 .Text(txt)
                                 .FontColor("#fff")
                                 .FontSize(9).Bold();
                            }
                        });

                        // Rows
                        bool alt = false;
                        foreach (var item in payment.Items)
                        {
                            var bg = alt ? "#f9f9ff" : "#ffffff";
                            alt = !alt;

                            table.Cell().Background(bg).Padding(5)
                                 .Text(item.Name).FontSize(9);
                            table.Cell().Background(bg).Padding(5)
                                 .AlignCenter()
                                 .Text(item.Quantity.ToString())
                                 .FontSize(9);
                            table.Cell().Background(bg).Padding(5)
                                 .AlignRight()
                                 .Text(item.UnitPrice
                                 .ToString("N0") + "đ")
                                 .FontSize(9);
                            table.Cell().Background(bg).Padding(5)
                                 .AlignRight()
                                 .Text(item.SubTotal
                                 .ToString("N0") + "đ")
                                 .FontSize(9).Bold();
                        }
                    });

                    col.Item().PaddingTop(10);

                    // ── Tổng cộng ─────────────────────────────
                    col.Item().AlignRight().Column(c =>
                    {
                        // Hàm con để vẽ từng dòng tổng kết
                        void Row(string label, decimal val, bool big = false, string color = "#333")
                        {
                            c.Item().Row(r =>
                            {
                                // Cột nhãn (Tên mục)
                                var labelText = r.RelativeItem().Text(label)
                                    .FontSize(big ? 11 : 9)
                                    .FontColor(color);

                                if (big) labelText.Bold();

                                // Cột giá trị (Số tiền)
                                var valueText = r.ConstantItem(90).AlignRight()
                                    .Text(val.ToString("N0") + "đ")
                                    .FontSize(big ? 12 : 9)
                                    .FontColor(color);

                                if (big) valueText.Bold();
                            });
                            c.Item().PaddingBottom(3);
                        }

                        // Gọi hàm để hiển thị dữ liệu
                        Row("Tạm tính:", payment.Subtotal);
                        Row("VAT (10%):", payment.VatAmount);
                        Row("Phí dịch vụ:", payment.ServiceFee);

                        if (payment.DiscountAmount > 0)
                            Row($"Giảm giá ({payment.VoucherCode}):", -payment.DiscountAmount, color: "#16a34a");

                        c.Item().LineHorizontal(1).LineColor("#e0e0e0");
                        c.Item().PaddingBottom(4);

                        Row("TỔNG THANH TOÁN:", payment.TotalAmount, big: true, color: "#4f46e5");
                    });

                    col.Item().PaddingVertical(10)
                       .LineHorizontal(1).LineColor("#e0e0e0");

                    // ── Footer ────────────────────────────────
                    col.Item().AlignCenter().Text(
                        "Cảm ơn quý khách! Hẹn gặp lại 🙏")
                       .FontSize(10).Italic().FontColor("#888");
                    col.Item().AlignCenter().Text(
                        $"Phương thức: {payment.Method}")
                       .FontSize(9).FontColor("#999");
                });
            });
        }).GeneratePdf();
    }
}