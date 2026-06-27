namespace Ols.ControlCenter.Application.Abstractions.Security;

/// <summary>Parola hash'leme soyutlaması (uygulama Infrastructure implementasyonuna bağımlı kalmaz).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
