namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Kullanıcı ↔ Rol çok-çoka ilişki tablosu (bileşik anahtar).</summary>
public class UserRole
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
