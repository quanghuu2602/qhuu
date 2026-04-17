using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantMS.Application.DTOs;

public record CreateReservationRequest(
    string CustomerName,
    string CustomerPhone,
    int GuestCount,
    DateTime ReservedAt,
    string? Note
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
    int? TableId
);