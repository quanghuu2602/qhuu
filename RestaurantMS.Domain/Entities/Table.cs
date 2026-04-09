using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RestaurantMS.Domain.Enums;

namespace RestaurantMS.Domain.Entities;

public class Table
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; } = 4;
    public TableStatus Status { get; set; } = TableStatus.Available;
    public int AreaId { get; set; }
    public Area Area { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}