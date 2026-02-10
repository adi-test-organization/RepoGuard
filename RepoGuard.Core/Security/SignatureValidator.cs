using RepoGuard.Core.Interfaces;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace RepoGuard.Core.Security;

public class SignatureValidator : ISignatureValidator
{
    public bool IsValid(string payload, string? signatureWithPrefix, string secret)
    {
        if (string.IsNullOrEmpty(signatureWithPrefix))
        {
            return false;
        }

        const string prefix = "sha256=";
        if (!signatureWithPrefix.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var signatureHex = signatureWithPrefix.Substring(prefix.Length);

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(secretBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        var hashHex = Convert.ToHexString(hashBytes);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(hashHex),
            Encoding.UTF8.GetBytes(signatureHex.ToUpperInvariant()));
    }
}