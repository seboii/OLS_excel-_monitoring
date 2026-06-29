namespace Ols.ControlCenter.Application.Abstractions.Security;

/// <summary>JWT yapılandırması (appsettings/env "Jwt" bölümünden bağlanır).</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ols-control-center";
    public string Audience { get; set; } = "ols-control-center";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}
