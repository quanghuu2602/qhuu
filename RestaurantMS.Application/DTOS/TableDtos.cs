using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantMS.Application.DTOs;

public record AreaDto(
    int Id,
    string Name,
    List<TableDto> Tables
);

public record TableDto(
    int Id,
    string Name,
    int Capacity,
    string Status,      // "Available" | "Occupied" | "Reserved"
    string StatusColor, // "green" | "red" | "yellow"
    int AreaId,
    string AreaName
);

public record CreateTableRequest(
    string Name,
    int Capacity,
    int AreaId
);

public record UpdateTableStatusRequest(
    string Status  // "Available" | "Occupied" | "Reserved"
);