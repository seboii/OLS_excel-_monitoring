using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.Auth;
using Ols.ControlCenter.Shared.Api;

namespace Ols.ControlCenter.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentUser _current;

    public AuthController(IAuthService auth, ICurrentUser current)
    {
        _auth = auth;
        _current = current;
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(req, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<LoginResponse>.Ok(result.Value, "Giriş başarılı."))
            : Unauthorized(ApiResponse<LoginResponse>.Fail(result.Error.Message));
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var result = await _auth.RefreshAsync(req.RefreshToken, ct);
        return result.IsSuccess
            ? Ok(ApiResponse<LoginResponse>.Ok(result.Value))
            : Unauthorized(ApiResponse<LoginResponse>.Fail(result.Error.Message));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken ct)
    {
        if (_current.UserId is { } id) await _auth.LogoutAsync(id, ct);
        return Ok(ApiResponse.Ok("Çıkış yapıldı."));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<CurrentUserDto>>> Me(CancellationToken ct)
    {
        if (_current.UserId is not { } id)
            return Unauthorized(ApiResponse<CurrentUserDto>.Fail("Oturum bulunamadı."));
        var dto = await _auth.GetCurrentUserAsync(id, ct);
        return dto is null
            ? Unauthorized(ApiResponse<CurrentUserDto>.Fail("Kullanıcı bulunamadı."))
            : Ok(ApiResponse<CurrentUserDto>.Ok(dto));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        if (_current.UserId is not { } id)
            return Unauthorized(ApiResponse.Fail("Oturum bulunamadı."));

        var result = await _auth.ChangePasswordAsync(id, req.CurrentPassword, req.NewPassword, ct);
        return result.IsSuccess
            ? Ok(ApiResponse.Ok("Parola güncellendi. Güvenlik için tekrar giriş yapmanız gerekiyor."))
            : BadRequest(ApiResponse.Fail(result.Error.Message));
    }
}
