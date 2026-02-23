using ModalCalendarNotification.Core.Shared.Utilities;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Shared;

public sealed class CryptoHelperTests
{
    [Fact]
    public void ProtectThenUnprotectString_RoundTripsOriginalValue()
    {
        const string plaintext = "super-secret-token";

        var encrypted = CryptoHelper.ProtectString(plaintext);
        var decrypted = CryptoHelper.UnprotectString(encrypted);

        decrypted.ShouldBe(plaintext);
        encrypted.ShouldNotBe(plaintext);
    }

    [Fact]
    public void Protect_ThrowsForNullInput()
    {
        Should.Throw<ArgumentNullException>(() => CryptoHelper.Protect(null!));
    }
}
