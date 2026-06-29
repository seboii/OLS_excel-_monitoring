namespace Ols.ControlCenter.Application.Features.Auth;

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed record CurrentUserDto(
    long Id,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles,
    long? DepartmentId,
    string? DepartmentName);

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    CurrentUserDto User);
