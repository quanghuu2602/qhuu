using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
namespace RestaurantMS.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    // IdentityUser đã có sẵn Id kiểu string — đúng yêu cầu
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}