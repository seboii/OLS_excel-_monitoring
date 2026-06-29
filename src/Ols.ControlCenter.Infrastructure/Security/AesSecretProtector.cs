using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Ols.ControlCenter.Application.Abstractions.Security;

namespace Ols.ControlCenter.Infrastructure.Security;

/// <summary>AES-GCM ile bağlantı bilgisi şifreleme. Anahtar `Encryption:Key` (env) üzerinden gelir.</summary>
public sealed class AesSecretProtector : ISecretProtector
{
    private readonly byte[] _key;

    public AesSecretProtector(IConfiguration config)
        => _key = DeriveKey(config["Encryption:Key"]);

    private static byte[] DeriveKey(string? configured)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            try
            {
                var bytes = Convert.FromBase64String(configured);
                if (bytes.Length == 32) return bytes;
            }
            catch (FormatException) { /* base64 değil; SHA256 türet */ }

            return SHA256.HashData(Encoding.UTF8.GetBytes(configured));
        }
        // Geliştirme yedeği (prod'da Encryption:Key zorunlu)
        return SHA256.HashData(Encoding.UTF8.GetBytes("ols-dev-fallback-key"));
    }

    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return string.Empty;

        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var plain = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plain.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var gcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        gcm.Encrypt(nonce, plain, cipher, tag);

        var blob = new byte[nonce.Length + tag.Length + cipher.Length];
        nonce.CopyTo(blob, 0);
        tag.CopyTo(blob, nonce.Length);
        cipher.CopyTo(blob, nonce.Length + tag.Length);
        return Convert.ToBase64String(blob);
    }

    public string Unprotect(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext)) return string.Empty;

        var blob = Convert.FromBase64String(ciphertext);
        int nonceLen = AesGcm.NonceByteSizes.MaxSize;
        int tagLen = AesGcm.TagByteSizes.MaxSize;

        var nonce = blob[..nonceLen];
        var tag = blob[nonceLen..(nonceLen + tagLen)];
        var cipher = blob[(nonceLen + tagLen)..];
        var plain = new byte[cipher.Length];

        using var gcm = new AesGcm(_key, tagLen);
        gcm.Decrypt(nonce, tag, cipher, plain);
        return Encoding.UTF8.GetString(plain);
    }
}
