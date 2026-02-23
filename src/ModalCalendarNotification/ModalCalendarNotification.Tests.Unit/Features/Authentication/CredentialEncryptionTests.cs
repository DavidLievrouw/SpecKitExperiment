using ModalCalendarNotification.Core.Features.Authentication;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.Authentication;

public sealed class CredentialEncryptionTests
{
    [Fact]
    public void EncryptThenDecrypt_RoundTripsValue()
    {
        var sut = new CredentialEncryption();

        var encrypted = sut.Encrypt("oauth-secret");
        var decrypted = sut.Decrypt(encrypted);

        decrypted.ShouldBe("oauth-secret");
    }
}
