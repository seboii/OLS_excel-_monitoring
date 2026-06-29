namespace Ols.ControlCenter.Application.Abstractions.Security;

/// <summary>Hassas bağlantı bilgilerini (link, token) şifreleyip çözer. Düz metin saklanmaz.</summary>
public interface ISecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}
