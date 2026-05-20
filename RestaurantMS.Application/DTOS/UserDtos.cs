namespace RestaurantMS.Application.DTOs;

public record ChangeRoleRequest(string Role);

public record UpdateProfileRequest(
    string FullName,
    string? Phone
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);