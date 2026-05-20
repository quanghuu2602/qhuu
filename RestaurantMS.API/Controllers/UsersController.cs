using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Domain.Entities;
using System.Security.Claims;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> um)
        => _userManager = um;

    // GET /api/users — Admin xem tất cả
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? role = null)
    {
        var users = await _userManager.Users
            .Where(u => u.IsActive || true)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            var userRole = roles.FirstOrDefault() ?? "Customer";

            if (role != null &&
                !userRole.Equals(role,
                    StringComparison.OrdinalIgnoreCase))
                continue;

            result.Add(new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.IsActive,
                role = userRole,
                createdAt = u.CreatedAt
            });
        }

        return Ok(result);
    }

    // PATCH /api/users/{id}/toggle-active — khoá/mở
    [HttpPatch("{id}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(new { message = "Không tìm thấy user" });

        // Không khoá được chính mình
        var myId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Id == myId)
            return BadRequest(new
            {
                message = "Không thể khoá tài khoản của chính mình"
            });

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            message = user.IsActive ? "Đã mở khoá" : "Đã khoá",
            isActive = user.IsActive
        });
    }

    // PATCH /api/users/{id}/change-role
    [HttpPatch("{id}/change-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeRole(
        string id, [FromBody] ChangeRoleRequest req)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(new { message = "Không tìm thấy user" });

        var allowed = new[] { "Admin", "Staff", "Kitchen", "Customer" };
        if (!allowed.Contains(req.Role))
            return BadRequest(new { message = "Role không hợp lệ" });

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, req.Role);

        return Ok(new { message = $"Đã đổi role → {req.Role}" });
    }

    // GET /api/users/me — lấy thông tin bản thân
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(id!);
        if (user is null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            role = roles.FirstOrDefault() ?? "Customer"
        });
    }

    // PUT /api/users/me — cập nhật thông tin
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(
        [FromBody] UpdateProfileRequest req)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(id!);
        if (user is null) return Unauthorized();

        user.FullName = req.FullName;
        user.PhoneNumber = req.Phone;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Đã cập nhật thông tin" });
    }

    // POST /api/users/me/change-password
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest req)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(id!);
        if (user is null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(
            user, req.CurrentPassword, req.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new
            {
                message = string.Join(", ",
                    result.Errors.Select(e => e.Description))
            });

        return Ok(new { message = "Đã đổi mật khẩu thành công" });
    }
}