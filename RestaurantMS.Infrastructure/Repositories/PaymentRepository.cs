using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;
using RestaurantMS.Infrastructure.Data;

namespace RestaurantMS.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;
    public PaymentRepository(ApplicationDbContext db) => _db = db;

    public async Task<Payment> CreateAsync(Payment p)
    {
        _db.Payments.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<Payment?> GetByOrderIdAsync(int orderId)
        => await _db.Payments
                    .Include(p => p.Order)
                        .ThenInclude(o => o.Details)
                            .ThenInclude(d => d.MenuItem)
                    .Include(p => p.Order)
                        .ThenInclude(o => o.Table)
                    .FirstOrDefaultAsync(p => p.OrderId == orderId);

    public async Task<IEnumerable<Payment>> GetAllAsync(
        DateTime? date = null)
    {
        var q = _db.Payments
                   .Include(p => p.Order)
                       .ThenInclude(o => o.Table)
                   .AsQueryable();

        if (date.HasValue)
            q = q.Where(p => p.PaidAt.Date == date.Value.Date);

        return await q.OrderByDescending(p => p.PaidAt).ToListAsync();
    }
    public async Task UpdatePdfUrlAsync(int id, string pdfUrl)
    {
        var p = await _db.Payments.FindAsync(id);
        if (p is null) return;
        p.InvoicePdfUrl = pdfUrl;
        await _db.SaveChangesAsync();
    }
}
