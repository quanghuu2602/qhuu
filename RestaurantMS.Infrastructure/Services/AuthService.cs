using System;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RestaurantMS.Application.DTOs;
using RestaurantMS.Application.Interfaces;
using RestaurantMS.Domain.Entities;

namespace RestaurantMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwt;

    public AuthService(UserManager<ApplicationUser> userManager,
                       IOptions<JwtSettings> jwt)
    {
        _userManager = userManager;
        _jwt = jwt.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req,
                                                   string role = "Customer")
    {
        // Kiểm tra email đã tồn tại chưa
        var existing = await _userManager.FindByEmailAsync(req.Email);
        if (existing != null)
            throw new Exception("Email đã được sử dụng");

        var user = new ApplicationUser
        {
            FullName = req.FullName,
            Email = req.Email,
            UserName = req.Email,
            PhoneNumber = req.Phone
        };

        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ",
                result.Errors.Select(e => e.Description));
            throw new Exception(errors);
        }

        // Gán role
        await _userManager.AddToRoleAsync(user, role);

        return await BuildTokenAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email)
            ?? throw new Exception("Email hoặc mật khẩu không đúng");

        if (!user.IsActive)
            throw new Exception("Tài khoản đã bị khoá");

        var ok = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!ok)
            throw new Exception("Email hoặc mật khẩu không đúng");

        return await BuildTokenAsync(user);
    }

    // ── Tạo JWT token ────────────────────────────────────────────
    private async Task<AuthResponse> BuildTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),   // UserId — string
            new(ClaimTypes.Email,          user.Email!),
            new(ClaimTypes.Name,           user.FullName),
            new(ClaimTypes.Role,           role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
                          Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key,
                          SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResponse(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            FullName: user.FullName,
            Email: user.Email!,
            Role: role,
            ExpiresAt: expires
        );
    }
}