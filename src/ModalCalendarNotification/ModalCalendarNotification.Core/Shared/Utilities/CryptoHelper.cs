using System.Security.Cryptography;
using System.Text;

namespace ModalCalendarNotification.Core.Shared.Utilities;

public static class CryptoHelper
{
    public static byte[] Protect(byte[] plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        return ProtectedData.Protect(plaintext, null, DataProtectionScope.CurrentUser);
    }

    public static byte[] Unprotect(byte[] ciphertext)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        return ProtectedData.Unprotect(ciphertext, null, DataProtectionScope.CurrentUser);
    }

    public static string ProtectString(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        var bytes = Encoding.UTF8.GetBytes(plaintext);
        return Convert.ToBase64String(Protect(bytes));
    }

    public static string UnprotectString(string ciphertext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext);
        var bytes = Convert.FromBase64String(ciphertext);
        return Encoding.UTF8.GetString(Unprotect(bytes));
    }
}
