namespace Ols.ControlCenter.Shared.Authorization;

/// <summary>Rol kod sabitleri. [Authorize(Roles = ...)] ve seed/policy tanımlarında kullanılır.</summary>
public static class AppRoles
{
    public const string Admin = "Admin";                           // Yönetici / CEO / COO
    public const string DepartmentManager = "DepartmentManager";   // Departman Müdürü
    public const string OperationSpecialist = "OperationSpecialist"; // Operasyon Uzmanı
    public const string Finance = "Finance";                       // Finans Kullanıcısı
    public const string ReadOnly = "ReadOnly";                     // Salt Okuma

    public static readonly string[] All =
        { Admin, DepartmentManager, OperationSpecialist, Finance, ReadOnly };
}
