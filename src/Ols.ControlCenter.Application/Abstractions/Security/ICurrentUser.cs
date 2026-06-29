namespace Ols.ControlCenter.Application.Abstractions.Security;

/// <summary>İstek bağlamındaki oturum açmış kullanıcı bilgisi (JWT claim'lerinden).</summary>
public interface ICurrentUser
{
    long? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IReadOnlyList<string> Roles { get; }
}
