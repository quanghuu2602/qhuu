using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantMS.Domain.Enums;

public enum TableStatus
{
    Available = 0,   // Trống — xanh
    Occupied = 1,   // Đang phục vụ — đỏ
    Reserved = 2    // Đặt trước — vàng
}