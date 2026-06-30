using Microsoft.EntityFrameworkCore;
using Ols.ControlCenter.Application.Abstractions.Security;
using Ols.ControlCenter.Application.Features.Users;
using Ols.ControlCenter.Domain.Entities;
using Ols.ControlCenter.Infrastructure.Persistence;
using Ols.ControlCenter.Infrastructure.Security;
using Ols.ControlCenter.Shared.Authorization;

namespace Ols.ControlCenter.UnitTests;

/// <summary>
/// Kullanıcı/rol yönetiminin kritik iş kuralları: rol doğrulama, parola/e-posta kuralları,
/// "son aktif admin'i kilitleme" ve "kendini silme" korumaları.
/// </summary>
public class UserServiceTests
{
    private static (AppDbContext Db, UserService Svc, IPasswordHasher Hasher) Build()
    {
        var db = InMemoryDb.Create();
        var now = DateTimeOffset.UtcNow;
        db.Roles.AddRange(
            new Role { Code = AppRoles.Admin, Name = "Yönetici", CreatedAt = now },
            new Role { Code = AppRoles.Finance, Name = "Finans", CreatedAt = now },
            new Role { Code = AppRoles.ReadOnly, Name = "Salt Okuma", CreatedAt = now });
        db.SaveChanges();
        IPasswordHasher hasher = new BcryptPasswordHasher();
        return (db, new UserService(db, hasher), hasher);
    }

    private static async Task<long> SeedAdminAsync(AppDbContext db, IPasswordHasher hasher, string email)
    {
        var adminRole = await db.Roles.FirstAsync(r => r.Code == AppRoles.Admin);
        var user = new User
        {
            FullName = "Admin", Email = email, PasswordHash = hasher.Hash("Admin123!"),
            IsActive = true, CreatedAt = DateTimeOffset.UtcNow,
            UserRoles = new List<UserRole> { new() { RoleId = adminRole.Id } },
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task Create_Valid_Persists()
    {
        var (db, svc, _) = Build();
        var result = await svc.CreateAsync(new CreateUserRequest
        {
            FullName = "Ali Veli", Email = "ali@ols.local", Password = "Sifre1234", Roles = { AppRoles.Finance },
        }, default);

        Assert.True(result.IsSuccess);
        Assert.Equal("ali@ols.local", result.Value.Email);
        Assert.Contains(AppRoles.Finance, result.Value.Roles);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Create_DuplicateEmail_Fails()
    {
        var (db, svc, hasher) = Build();
        await SeedAdminAsync(db, hasher, "admin@ols.local");

        var result = await svc.CreateAsync(new CreateUserRequest
        {
            FullName = "Yeni", Email = "ADMIN@ols.local", Password = "Sifre1234", Roles = { AppRoles.ReadOnly },
        }, default);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Create_InvalidRole_Fails()
    {
        var (_, svc, _) = Build();
        var result = await svc.CreateAsync(new CreateUserRequest
        {
            FullName = "X", Email = "x@ols.local", Password = "Sifre1234", Roles = { "Patron" },
        }, default);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Create_ShortPassword_Fails()
    {
        var (_, svc, _) = Build();
        var result = await svc.CreateAsync(new CreateUserRequest
        {
            FullName = "X", Email = "x@ols.local", Password = "kisa", Roles = { AppRoles.ReadOnly },
        }, default);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Update_RemovingLastAdminRole_Fails()
    {
        var (db, svc, hasher) = Build();
        var adminId = await SeedAdminAsync(db, hasher, "admin@ols.local");

        var result = await svc.UpdateAsync(adminId, new UpdateUserRequest
        {
            FullName = "Admin", Roles = { AppRoles.Finance }, IsActive = true,
        }, default);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Delete_Self_Fails()
    {
        var (db, svc, hasher) = Build();
        var adminId = await SeedAdminAsync(db, hasher, "admin@ols.local");

        var result = await svc.DeleteAsync(adminId, currentUserId: adminId, default);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Delete_LastAdmin_Fails()
    {
        var (db, svc, hasher) = Build();
        var adminId = await SeedAdminAsync(db, hasher, "admin@ols.local");

        // Başka bir kullanıcı (silme isteğini yapan) farklı bir id olmalı.
        var result = await svc.DeleteAsync(adminId, currentUserId: 9999, default);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Delete_NonAdmin_SoftDeletes()
    {
        var (db, svc, hasher) = Build();
        var adminId = await SeedAdminAsync(db, hasher, "admin@ols.local");

        var created = await svc.CreateAsync(new CreateUserRequest
        {
            FullName = "Finansçı", Email = "fin@ols.local", Password = "Sifre1234", Roles = { AppRoles.Finance },
        }, default);
        Assert.True(created.IsSuccess);

        var result = await svc.DeleteAsync(created.Value.Id, currentUserId: adminId, default);
        Assert.True(result.IsSuccess);

        // Soft-delete: listeden düşer (global query filter).
        var users = await svc.GetUsersAsync(default);
        Assert.DoesNotContain(users, u => u.Email == "fin@ols.local");
    }
}
