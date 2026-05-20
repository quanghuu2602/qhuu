namespace RestaurantMS.Application.DTOs;

public record CreateReservationRequest(
    string CustomerName,
    string CustomerPhone,
    int GuestCount,
    DateTime ReservedAt,
    string? Note
);

public record ConfirmReservationRequest(
    int? TableId   // Gán bàn cụ thể khi xác nhận
);

public record ReservationDto(
    int Id,
    string CustomerName,
    string CustomerPhone,
    int GuestCount,
    DateTime ReservedAt,
    string? Note,
    string BookingCode,
    bool IsConfirmed,
    int? TableId,
    string? TableName   // Thêm tên bàn để FE hiện
);