using RestaurantMS.Domain.Entities;
using RestaurantMS.Domain.Enums;

namespace RestaurantMS.Application.Interfaces;

public interface IOrderRepository
{
    // Order
    Task<Order> CreateAsync(Order order);
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetActiveOrdersAsync();
    Task<Order?> GetByTableIdAsync(int tableId);
    Task UpdateStatusAsync(int id, OrderStatus status);
    Task DeleteAsync(int id);

    // OrderDetail
    Task<OrderDetail> AddItemAsync(OrderDetail detail);
    Task UpdateItemAsync(int detailId,
                        int quantity,
                        string? note);
    Task RemoveItemAsync(int detailId);
}