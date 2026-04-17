using Microsoft.AspNetCore.Mvc;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Application.Interfaces;

namespace RestaurantMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    // POST api/auth/register
    // Khách hàng tự đăng ký — mặc định role Customer
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        try
        {
            var result = await _auth.RegisterAsync(req, role: "Customer");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var result = await _auth.LoginAsync(req);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
    // POST api/auth/create-admin
    // Chỉ dùng 1 lần để tạo tài khoản Admin đầu tiên
    // Sau khi tạo xong nên xoá hoặc khoá endpoint này
    [HttpPost("create-admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] RegisterRequest req,
                                                  [FromQuery] string secretKey)
    {
        // Khoá bí mật để tránh ai cũng tạo được Admin
        if (secretKey != "gitSETUP_ADMIN_2024")
            return Forbid();

        try
        {
            var result = await _auth.RegisterAsync(req, role: "Admin");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST api/auth/create-staff  (Admin gọi để tạo Staff/Kitchen)
    [HttpPost("create-Staff")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStaff([FromBody] RegisterRequest req,
                                                  [FromQuery] string role)
    {
        if (role is not ("Staff" or "Kitchen"))
            return BadRequest(new { message = "Role không hợp lệ" });

        try
        {
            var result = await _auth.RegisterAsync(req, role: role);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}