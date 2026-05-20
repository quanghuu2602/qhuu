using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task UpdatePdfUrlAsync(int id, string pdfUrl);
    Task<IEnumerable<Payment>> GetAllAsync(DateTime? date = null);
}