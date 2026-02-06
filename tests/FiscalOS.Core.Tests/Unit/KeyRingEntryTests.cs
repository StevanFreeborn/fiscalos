namespace FiscalOS.Core.Tests.Unit;

public class KeyRingEntryTests
{
  [Fact]
  public void From_WhenCalled_ItShouldReturnAnInstance()
  {
    var keyId = "test-key-id";
    var key = "key";

    var keyRingEntry = KeyRingEntry.From(keyId, key);

    keyRingEntry.KeyId.Should().Be(keyId);
    keyRingEntry.Key.Should().Be(key);
  }

  [Fact]
  public void From_WhenCalledWithNullKeyId_ItShouldThrowArgumentNullException()
  {
    var key = "key";

    var createEntryWithNullKeyId = () => KeyRingEntry.From(null!, key);

    createEntryWithNullKeyId.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void From_WhenCalledWithNullKey_ItShouldThrowArgumentNullException()
  {
    var keyId = "test-key-id";

    var createEntryWithNullKey = () => KeyRingEntry.From(keyId, null!);

    createEntryWithNullKey.Should().Throw<ArgumentNullException>();
  }
}