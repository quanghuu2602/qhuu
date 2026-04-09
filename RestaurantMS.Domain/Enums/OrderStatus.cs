using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantMS.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,   // Vừa tạo
    Cooking = 1,   // Bếp đang làm
    Ready = 2,   // Xong, chờ mang ra
    Served = 3,   // Đã mang ra bàn
    Completed = 4,   // Đã thanh toán
    Cancelled = 5
}