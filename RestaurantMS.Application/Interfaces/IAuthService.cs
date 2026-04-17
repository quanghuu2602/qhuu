using RestaurantMS.Application.DTOs;

namespace RestaurantMS.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string role = "Customer");
    Task<AuthResponse> LoginAsync(LoginRequest request);
}