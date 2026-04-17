namespace RestaurantMS.Application.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    int SortOrder,
    int TotalItems
);

public record MenuItemDto(
    int Id,
    string Name,
    decimal Price,
    string? ImageUrl,
    string? Description,
    bool IsAvailable,
    int CategoryId,
    string CategoryName,
    List<string> Tags
);

public record CreateMenuItemRequest(
    string Name,
    decimal Price,
    string? ImageUrl,
    string? Description,
    int CategoryId,
    List<string>? Tags
);

public record UpdateMenuItemRequest(
    string Name,
    decimal Price,
    string? ImageUrl,
    string? Description,
    int CategoryId,
    bool IsAvailable,
    List<string>? Tags
);