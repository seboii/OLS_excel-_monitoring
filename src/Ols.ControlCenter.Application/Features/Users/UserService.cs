using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Persistence;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Shared.Authorization;
using Ols.ControlCenter.Shared.Results;

namespace Ols.ControlCenter.Application.Features.Users;

public sealed record UserListItemDto(
    long Id, string FullName, string Email, bool IsActive, IReadOnlyList<string> Roles,
    long? DepartmentId, string? DepartmentName, DateTimeOffset? LastLoginAt, DateTimeOffset CreatedAt);

public sealed record RoleDto(long Id, string Code, string Name, string? Description);
public sealed record DepartmentDto(long Id, string Name);

public sealed class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public long? DepartmentId { get; set; }
}

public sealed class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public long? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
}

public interface IUserService
{
    Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken ct);
    Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct);
    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct);
    Task<Result<UserListItemDto>> CreateAsync(CreateUserRequest req, CancellationToken ct);
    Task<Result<UserListItemDto>> UpdateAsync(long id, UpdateUserRequest req, CancellationToken ct);
    Task<Result> ResetPasswordAsync(long id, string newPassword, CancellationToken ct);
    Task<Result> DeleteAsync(long id, long currentUserId, CancellationToken ct);
}

/// <summary>
/// Kullanıcı/rol yönetimi (yalnızca Admin). Şimdiye dek tek seed admin vardı; bu servis ile
/// çok-kullanıcı ve rol ataması mümkün olur. Rol kodları <see cref="AppRoles"/> ile doğrulanır.
/// "Son admin'i kilitleme" ve "kendini silme/pasifleştirme" gibi tehlikeli durumlar engellenir.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UserService(IApplicationDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken ct)
    {
        var users = await _db.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);
        return users.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken ct)
        => await _db.Roles.AsNoTracking().OrderBy(r => r.Id)
            .Select(r => new RoleDto(r.Id, r.Code, r.Name, r.Description)).ToListAsync(ct);

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(CancellationToken ct)
        => await _db.Departments.AsNoTracking().OrderBy(d => d.Name)
            .Select(d => new DepartmentDto(d.Id, d.Name)).ToListAsync(ct);

    public async Task<Result<UserListItemDto>> CreateAsync(CreateUserRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(req.FullName)) return Error.Validation("Ad soyad zorunludur.");
        if (string.IsNullOrWhiteSpace(email)) return Error.Validation("E-posta zorunludur.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8)
            return Error.Validation("Parola en az 8 karakter olmalı.");

        var rolesResult = await ResolveRolesAsync(req.Roles, ct);
        if (rolesResult.IsFailure) return rolesResult.Error;

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            return Error.Validation("Bu e-posta zaten kayıtlı.");

        if (req.DepartmentId is { } dep && !await _db.Departments.AnyAsync(d => d.Id == dep, ct))
            return Error.Validation("Geçersiz departman.");

        var now = DateTimeOffset.UtcNow;
        var user = new User
        {
            FullName = req.FullName.Trim(),
            Email = email,
            PasswordHash = _hasher.Hash(req.Password),
            DepartmentId = req.DepartmentId,
            IsActive = true,
            CreatedAt = now,
            UserRoles = rolesResult.Value.Select(r => new UserRole { RoleId = r.Id }).ToList(),
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return ToDto(await ReloadAsync(user.Id, ct));
    }

    public async Task<Result<UserListItemDto>> UpdateAsync(long id, UpdateUserRequest req, CancellationToken ct)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return Error.NotFound("Kullanıcı bulunamadı.");
        if (string.IsNullOrWhiteSpace(req.FullName)) return Error.Validation("Ad soyad zorunludur.");

        var rolesResult = await ResolveRolesAsync(req.Roles, ct);
        if (rolesResult.IsFailure) return rolesResult.Error;

        // Son aktif admin'i admin olmaktan çıkarmayı/pasifleştirmeyi engelle.
        var willBeAdmin = rolesResult.Value.Any(r => r.Code == AppRoles.Admin);
        var wasAdmin = user.UserRoles.Any(ur => ur.Role.Code == AppRoles.Admin);
        if (wasAdmin && (!willBeAdmin || !req.IsActive) && await IsLastActiveAdminAsync(user.Id, ct))
            return Error.Validation("Sistemde en az bir aktif yönetici (Admin) kalmalı.");

        if (req.DepartmentId is { } dep && !await _db.Departments.AnyAsync(d => d.Id == dep, ct))
            return Error.Validation("Geçersiz departman.");

        user.FullName = req.FullName.Trim();
        user.DepartmentId = req.DepartmentId;
        user.IsActive = req.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        _db.UserRoles.RemoveRange(user.UserRoles);
        user.UserRoles = rolesResult.Value.Select(r => new UserRole { UserId = user.Id, RoleId = r.Id }).ToList();

        // Pasifleştirilen kullanıcının oturumu kapansın.
        if (!req.IsActive)
        {
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
        }

        await _db.SaveChangesAsync(ct);
        return ToDto(await ReloadAsync(user.Id, ct));
    }

    public async Task<Result> ResetPasswordAsync(long id, string newPassword, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return Result.Failure(Error.Validation("Parola en az 8 karakter olmalı."));

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return Result.Failure(Error.NotFound("Kullanıcı bulunamadı."));

        user.PasswordHash = _hasher.Hash(newPassword);
        user.RefreshTokenHash = null;       // mevcut oturumlar iptal
        user.RefreshTokenExpiresAt = null;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(long id, long currentUserId, CancellationToken ct)
    {
        if (id == currentUserId)
            return Result.Failure(Error.Validation("Kendi hesabınızı silemezsiniz."));

        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return Result.Failure(Error.NotFound("Kullanıcı bulunamadı."));

        if (user.UserRoles.Any(ur => ur.Role.Code == AppRoles.Admin) && await IsLastActiveAdminAsync(user.Id, ct))
            return Result.Failure(Error.Validation("Son aktif yöneticiyi silemezsiniz."));

        user.IsDeleted = true;
        user.DeletedAt = DateTimeOffset.UtcNow;
        user.IsActive = false;
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiresAt = null;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Result<List<Role>>> ResolveRolesAsync(List<string> codes, CancellationToken ct)
    {
        var distinct = codes.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().ToList();
        if (distinct.Count == 0) return Error.Validation("En az bir rol seçilmelidir.");

        var invalid = distinct.Where(c => !AppRoles.All.Contains(c)).ToList();
        if (invalid.Count > 0) return Error.Validation($"Geçersiz rol: {string.Join(", ", invalid)}");

        var roles = await _db.Roles.Where(r => distinct.Contains(r.Code)).ToListAsync(ct);
        if (roles.Count != distinct.Count) return Error.Validation("Bazı roller veritabanında bulunamadı.");
        return roles;
    }

    private async Task<bool> IsLastActiveAdminAsync(long excludingUserId, CancellationToken ct)
        => !await _db.Users.AnyAsync(
            u => u.Id != excludingUserId && u.IsActive && u.UserRoles.Any(ur => ur.Role.Code == AppRoles.Admin), ct);

    private async Task<User> ReloadAsync(long id, CancellationToken ct)
        => await _db.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == id, ct);

    private static UserListItemDto ToDto(User u) => new(
        u.Id, u.FullName, u.Email, u.IsActive,
        u.UserRoles.Select(ur => ur.Role.Code).ToList(),
        u.DepartmentId, u.Department?.Name, u.LastLoginAt, u.CreatedAt);
}
