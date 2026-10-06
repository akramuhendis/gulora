using System.Security.Cryptography;

namespace Gulora.Application.Common.Security;

internal static class SecureToken
{
    public static string CreateHexToken(int byteLength = 32)
    {
        var buf = new byte[byteLength];
        RandomNumberGenerator.Fill(buf);
        return Convert.ToHexString(buf).ToLowerInvariant();
    }
}
