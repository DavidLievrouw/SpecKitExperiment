using ModalCalendarNotification.Core.Shared.Utilities;

namespace ModalCalendarNotification.Core.Features.Authentication;

public sealed class CredentialEncryption
{
    public string Encrypt(string plaintext)
    {
        return CryptoHelper.ProtectString(plaintext);
    }

    public string Decrypt(string ciphertext)
    {
        return CryptoHelper.UnprotectString(ciphertext);
    }
}
