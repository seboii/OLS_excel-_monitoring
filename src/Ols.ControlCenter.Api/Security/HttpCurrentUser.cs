using System.Security.Claims;
using Ols.ControlCenter.Application.Abstractions.Security;

namespace Ols.ControlCenter.Api.Security;

/// <summary>JWT claim'lerinden oturum açmış kullanıcıyı çözen ICurrentUser implementasyonu.</summary>
public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public long? UserId
    {
        get
        {
            var value = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? Principal?.FindFirst("sub")?.Value;
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email
        => Principal?.FindFirst(ClaimTypes.Email)?.Value ?? Principal?.FindFirst("email")?.Value;

    public IReadOnlyList<string> Roles
        => Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
}
