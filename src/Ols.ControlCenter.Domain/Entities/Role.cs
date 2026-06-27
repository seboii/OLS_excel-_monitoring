using Ols.ControlCenter.Domain.Common;

namespace Ols.ControlCenter.Domain.Entities;

/// <summary>Yetki rolü (Admin, Departman Müdürü, Operasyon Uzmanı, Finans, Salt Okuma).</summary>
public class Role : AuditableEntity
{
    /// <summary>Kullanıcıya gösterilen Türkçe ad.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Kod sabiti (Admin, DepartmentManager, OperationSpecialist, Finance, ReadOnly).</summary>
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
