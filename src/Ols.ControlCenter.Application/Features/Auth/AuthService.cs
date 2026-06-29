using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Shared.Results;

namespace Ols.ControlCenter.Application.Features.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct);
    Task<Result<LoginResponse>> RefreshAsync(string refreshToken, CancellationToken ct);
    Task LogoutAsync(long userId, CancellationToken ct);
    Task<CurrentUserDto?> GetCurrentUserAsync(long userId, CancellationToken ct);
    Task<Result> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken ct);
}

public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly JwtOptions _options;

    public AuthService(IApplicationDbContext db, IPasswordHasher hasher, IJwtTokenService jwt, JwtOptions options)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _options = options;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await LoadUserAsync(u => u.Email == req.Email, ct);
        if (user is null || !user.IsActive || !_hasher.Verify(req.Password, user.PasswordHash))
            return Error.Unauthorized("E-posta veya parola hatalı.");

        return await IssueAsync(user, ct);
    }

    public async Task<Result<LoginResponse>> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Error.Unauthorized("Refresh token gerekli.");

        var hash = Sha256(refreshToken);
        var user = await LoadUserAsync(u => u.RefreshTokenHash == hash, ct);
        if (user is null || !user.IsActive || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
            return Error.Unauthorized("Oturum süresi doldu, tekrar giriş yapın.");

        return await IssueAsync(user, ct);
    }

    public async Task LogoutAsync(long userId, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiresAt = null;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(long userId, CancellationToken ct)
    {
        var user = await LoadUserAsync(u => u.Id == userId, ct);
        return user is null ? null : ToDto(user);
    }

    public async Task<Result> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return Result.Failure(Error.Validation("Yeni parola en az 8 karakter olmalı."));

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return Result.Failure(Error.NotFound("Kullanıcı bulunamadı."));
        if (!_hasher.Verify(currentPassword, user.PasswordHash))
            return Result.Failure(Error.Unauthorized("Mevcut parola hatalı."));

        user.PasswordHash = _hasher.Hash(newPassword);
        // Parola değişince tüm oturumlar geçersiz olsun (güvenlik).
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiresAt = null;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<LoginResponse>> IssueAsync(User user, CancellationToken ct)
    {
        var roles = user.UserRoles.Select(r => r.Role.Code).ToList();
        var access = _jwt.CreateAccessToken(user, roles);
        var refresh = _jwt.CreateRefreshToken();

        user.RefreshTokenHash = Sha256(refresh);
        user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays);
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new LoginResponse(access.Token, access.ExpiresAt, refresh, ToDto(user));
    }

    private async Task<User?> LoadUserAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken ct)
        => await _db.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(predicate, ct);

    private static CurrentUserDto ToDto(User user) => new(
        user.Id, user.FullName, user.Email,
        user.UserRoles.Select(r => r.Role.Code).ToList(),
        user.DepartmentId, user.Department?.Name);

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
