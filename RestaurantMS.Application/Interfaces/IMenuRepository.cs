using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Application.Interfaces;

public interface IMenuRepository
{
    // Category
    Task<IEnumerable<Category>> GetCategoriesAsync();

    // MenuItem
    Task<IEnumerable<MenuItem>> GetAllAsync(int? categoryId = null,
                                             bool? isAvailable = null,
                                             string? search = null);
    Task<MenuItem?> GetByIdAsync(int id);
    Task<MenuItem> CreateAsync(MenuItem item);
    Task UpdateAsync(MenuItem item);
    Task DeleteAsync(int id);
    Task ToggleAvailabilityAsync(int id);
}