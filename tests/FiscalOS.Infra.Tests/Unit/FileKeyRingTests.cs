namespace FiscalOS.Infra.Tests.Unit;

public class FileKeyRingTests
{
  private readonly MockOptionsMonitor<FileKeyRingOptions> _mockOptionsMonitor = new();
  private readonly Mock<IFileSystem> _mockFileSystem = new();

  [Fact]
  public void From_WhenCalledWithOptionsMonitorAndFileSystem_ItShouldCreateFileKeyRing()
  {
    var mockPath = new Mock<IPath>();
    var mockDirectory = new Mock<IDirectory>();

    _mockFileSystem
      .Setup(static fs => fs.Path)
      .Returns(mockPath.Object);

    _mockFileSystem
      .Setup(static fs => fs.Directory)
      .Returns(mockDirectory.Object);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    sut.Should().NotBeNull();
    sut.Should().BeAssignableTo<IKeyRing>();
    sut.Should().BeAssignableTo<IDisposable>();
  }

  [Fact]
  public void From_WhenCalledWithServiceProvider_ItShouldCreateFileKeyRing()
  {
    var mockPath = new Mock<IPath>();
    var mockDirectory = new Mock<IDirectory>();
    var mockFileSystem = new Mock<IFileSystem>();
    var mockServiceProvider = new Mock<IServiceProvider>();
    var mockOptionsMonitor = new MockOptionsMonitor<FileKeyRingOptions>();

    mockFileSystem
      .Setup(static fs => fs.Path)
      .Returns(mockPath.Object);

    mockFileSystem
      .Setup(static fs => fs.Directory)
      .Returns(mockDirectory.Object);

    mockServiceProvider
      .Setup(static sp => sp.GetService(typeof(IOptionsMonitor<FileKeyRingOptions>)))
      .Returns(mockOptionsMonitor);

    mockServiceProvider
      .Setup(static sp => sp.GetService(typeof(IFileSystem)))
      .Returns(mockFileSystem.Object);

    var sut = FileKeyRing.From(mockServiceProvider.Object);

    sut.Should().NotBeNull();
    sut.Should().BeAssignableTo<IKeyRing>();
  }

  [Fact]
  public void GetKey_WhenCalledWithValidKeyId_ItShouldReturnKey()
  {
    var keyId = "test-key";
    var keyContent = "test-key-content";
    var mockPath = new Mock<IPath>();
    var mockDirectory = new Mock<IDirectoryInfoFactory>();
    var mockFile = new Mock<IFileInfoFactory>();
    var mockDirectoryInfo = new Mock<IDirectoryInfo>();
    var mockFileInfo = new Mock<IFileInfo>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = keyId
    };

    mockPath
      .Setup(p => p.GetFullPath("/keys", It.IsAny<string>()))
      .Returns("/full/keys");

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{keyId}.key"))
      .Returns(keyId);

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(fs => fs.Directory.Exists("/full/keys"))
      .Returns(true);

    _mockFileSystem
      .Setup(fs => fs.Directory.GetFiles("/full/keys", "*.key"))
      .Returns([$"/full/keys/{keyId}.key"]);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{keyId}.key"))
      .Returns(keyContent);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var result = sut.GetKey(keyId);

    result.Should().NotBeNull();
    result.KeyId.Should().Be(keyId);
  }

  [Fact]
  public void GetKey_WhenCalledWithInvalidKeyId_ItShouldThrowKeyNotFoundException()
  {
    var mockPath = new Mock<IPath>();
    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = "valid-key"
    };

    mockPath
      .Setup(p => p.GetFullPath(It.IsAny<string>(), It.IsAny<string>()))
      .Returns("/full/keys");

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(fs => fs.Directory.Exists(It.IsAny<string>()))
      .Returns(false);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var action = () => sut.GetKey("non-existent-key");

    action.Should().Throw<KeyNotFoundException>();
  }

  [Fact]
  public void GetPrimaryKey_WhenPrimaryKeyExists_ItShouldReturnPrimaryKey()
  {
    var primaryKeyId = "primary-key";
    var keyContent = "primary-key-content";
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = primaryKeyId
    };

    mockPath
      .Setup(p => p.GetFullPath("/keys", It.IsAny<string>()))
      .Returns("/full/keys");

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{primaryKeyId}.key"))
      .Returns(primaryKeyId);

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(fs => fs.Directory.Exists("/full/keys"))
      .Returns(true);

    _mockFileSystem
      .Setup(fs => fs.Directory.GetFiles("/full/keys", "*.key"))
      .Returns([$"/full/keys/{primaryKeyId}.key"]);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{primaryKeyId}.key"))
      .Returns(keyContent);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var result = sut.GetPrimaryKey();

    result.Should().NotBeNull();
    result.KeyId.Should().Be(primaryKeyId);
  }

  [Fact]
  public void GetPrimaryKey_WhenPrimaryKeyDoesNotExist_ItShouldThrowKeyNotFoundException()
  {
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = "non-existent-primary-key"
    };

    mockPath
      .Setup(static p => p.GetFullPath(It.IsAny<string>(), It.IsAny<string>()))
      .Returns("/full/keys");

    _mockFileSystem.Setup(static fs => fs.Path).Returns(mockPath.Object);
    _mockFileSystem
      .Setup(static fs => fs.Directory.Exists(It.IsAny<string>()))
      .Returns(false);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var action = sut.GetPrimaryKey;

    action.Should().Throw<KeyNotFoundException>();
  }

  [Fact]
  public void FileKeyRing_WhenOptionsChangeHandlerInvoked_ItShouldReloadKeys()
  {
    var initialKeyId = "initial-key";
    var initialKeyContent = "initial-key-content";
    var updatedKeyId = "updated-key";
    var updatedKeyContent = "updated-key-content";
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = initialKeyId
    };

    mockPath
      .Setup(p => p.GetFullPath("/keys", It.IsAny<string>()))
      .Returns("/full/keys");

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{initialKeyId}.key"))
      .Returns(initialKeyId);

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{updatedKeyId}.key"))
      .Returns(updatedKeyId);

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(fs => fs.Directory.Exists("/full/keys"))
      .Returns(true);

    _mockFileSystem
      .SetupSequence(fs => fs.Directory.GetFiles("/full/keys", "*.key"))
      .Returns([$"/full/keys/{initialKeyId}.key"]);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{initialKeyId}.key"))
      .Returns(initialKeyContent);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var initialKey = sut.GetKey(initialKeyId);
    initialKey.Should().NotBeNull();

    _mockFileSystem
      .Setup(fs => fs.Directory.GetFiles("/full/keys", "*.key"))
      .Returns([$"/full/keys/{updatedKeyId}.key"]);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{updatedKeyId}.key"))
      .Returns(updatedKeyContent);

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = updatedKeyId
    };

    var updatedKey = sut.GetKey(updatedKeyId);
    updatedKey.Should().NotBeNull();
    updatedKey.KeyId.Should().Be(updatedKeyId);
  }

  [Fact]
  public void FileKeyRing_WhenDirectoryDoesNotExist_ItShouldNotThrowAndLoadKeysReturnsEmpty()
  {
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/non-existent-keys",
      PrimaryKeyId = "any-key"
    };

    mockPath
      .Setup(p => p.GetFullPath("/non-existent-keys", It.IsAny<string>()))
      .Returns("/full/non-existent-keys");

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(fs => fs.Directory.Exists("/full/non-existent-keys"))
      .Returns(false);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var action = () => sut.GetKey("any-key");
    action.Should().Throw<KeyNotFoundException>();
  }

  [Fact]
  public void FileKeyRing_WhenKeyFileContainsOnlyWhitespace_ItShouldSkipKeyAndNotLoad()
  {
    var validKeyId = "valid-key";
    var validKeyContent = "valid-key-content";
    var whitespaceKeyId = "whitespace-key";
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = validKeyId
    };

    mockPath
      .Setup(p => p.GetFullPath("/keys", It.IsAny<string>()))
      .Returns("/full/keys");

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{validKeyId}.key"))
      .Returns(validKeyId);

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{whitespaceKeyId}.key"))
      .Returns(whitespaceKeyId);

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(fs => fs.Directory.Exists("/full/keys"))
      .Returns(true);

    _mockFileSystem
      .Setup(fs => fs.Directory.GetFiles("/full/keys", "*.key"))
      .Returns(
      [
        $"/full/keys/{validKeyId}.key",
        $"/full/keys/{whitespaceKeyId}.key"
      ]);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{validKeyId}.key"))
      .Returns(validKeyContent);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{whitespaceKeyId}.key"))
      .Returns("   \n\t   ");

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var validKey = sut.GetKey(validKeyId);
    validKey.Should().NotBeNull();

    var action = () => sut.GetKey(whitespaceKeyId);
    action.Should().Throw<KeyNotFoundException>();
  }

  [Fact]
  public void FileKeyRing_WhenMultipleKeysExist_ItShouldLoadAllKeys()
  {
    var key1Id = "key-1";
    var key1Content = "key-1-content";
    var key2Id = "key-2";
    var key2Content = "key-2-content";
    var key3Id = "key-3";
    var key3Content = "key-3-content";
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = key1Id
    };

    mockPath
      .Setup(p => p.GetFullPath("/keys", It.IsAny<string>()))
      .Returns("/full/keys");

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{key1Id}.key"))
      .Returns(key1Id);

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{key2Id}.key"))
      .Returns(key2Id);

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{key3Id}.key"))
      .Returns(key3Id);

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);
    _mockFileSystem
      .Setup(fs => fs.Directory.Exists("/full/keys"))
      .Returns(true);

    _mockFileSystem
      .Setup(fs => fs.Directory.GetFiles("/full/keys", "*.key"))
      .Returns(
      [
        $"/full/keys/{key1Id}.key",
        $"/full/keys/{key2Id}.key",
        $"/full/keys/{key3Id}.key"
      ]);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{key1Id}.key"))
      .Returns(key1Content);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{key2Id}.key"))
      .Returns(key2Content);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{key3Id}.key"))
      .Returns(key3Content);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var retrievedKey1 = sut.GetKey(key1Id);
    var retrievedKey2 = sut.GetKey(key2Id);
    var retrievedKey3 = sut.GetKey(key3Id);

    retrievedKey1.Should().NotBeNull();
    retrievedKey2.Should().NotBeNull();
    retrievedKey3.Should().NotBeNull();
    retrievedKey1.KeyId.Should().Be(key1Id);
    retrievedKey2.KeyId.Should().Be(key2Id);
    retrievedKey3.KeyId.Should().Be(key3Id);
  }

  [Fact]
  public void Dispose_WhenCalled_ItShouldDisposeOptionsChangeHandler()
  {
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = "key"
    };

    mockPath
      .Setup(static p => p.GetFullPath(It.IsAny<string>(), It.IsAny<string>()))
      .Returns("/full/keys");

    _mockFileSystem.Setup(static fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(static fs => fs.Directory.Exists(It.IsAny<string>()))
      .Returns(false);

    using var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var action = sut.Dispose;
    action.Should().NotThrow();
  }

  [Fact]
  public void FileKeyRing_WhenKeyTrimsWhitespace_ItShouldLoadKeyWithTrimmedContent()
  {
    var keyId = "test-key";
    var keyContentWithWhitespace = "  \n  test-key-content  \t  ";
    var mockPath = new Mock<IPath>();

    _mockOptionsMonitor.CurrentValue = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = keyId
    };

    mockPath
      .Setup(p => p.GetFullPath("/keys", It.IsAny<string>()))
      .Returns("/full/keys");

    mockPath
      .Setup(p => p.GetFileNameWithoutExtension($"/full/keys/{keyId}.key"))
      .Returns(keyId);

    _mockFileSystem.Setup(fs => fs.Path).Returns(mockPath.Object);

    _mockFileSystem
      .Setup(fs => fs.Directory.Exists("/full/keys"))
      .Returns(true);

    _mockFileSystem
      .Setup(fs => fs.Directory.GetFiles("/full/keys", "*.key"))
      .Returns([$"/full/keys/{keyId}.key"]);

    _mockFileSystem
      .Setup(fs => fs.File.ReadAllText($"/full/keys/{keyId}.key"))
      .Returns(keyContentWithWhitespace);

    var sut = FileKeyRing.From(_mockOptionsMonitor, _mockFileSystem.Object);

    var result = sut.GetKey(keyId);

    result.Should().NotBeNull();
    result.Key.Should().Be("test-key-content");
  }
}