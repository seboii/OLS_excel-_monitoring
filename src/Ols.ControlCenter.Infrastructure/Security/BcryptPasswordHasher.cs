using Ols.ControlCenter.Application.Abstractions.Security;
using BC = BCrypt.Net.BCrypt;

namespace Ols.ControlCenter.Infrastructure.Security;

/// <summary>BCrypt tabanlı parola hash'leyici (work factor 12).</summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password) => BC.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BC.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
