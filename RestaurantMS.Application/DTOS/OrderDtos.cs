using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantMS.Application.DTOs;

public record OrderDto(
    int Id,
    int TableId,
    string TableName,
    string StaffId,
    string StaffName,
    string Status,
    string? Note,
    DateTime CreatedAt,
    List<OrderDetailDto> Details,
    decimal TotalAmount
);

public record OrderDetailDto(
    int Id,
    int MenuItemId,
    string MenuItemName,
    string MenuItemCategory,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal,
    string? SpecialNote
);

public record CreateOrderRequest(
    int TableId,
    string? Note
);

public record AddOrderItemRequest(
    int MenuItemId,
    int Quantity,
    string? SpecialNote
);

public record UpdateOrderItemRequest(
    int Quantity,
    string? SpecialNote
);
