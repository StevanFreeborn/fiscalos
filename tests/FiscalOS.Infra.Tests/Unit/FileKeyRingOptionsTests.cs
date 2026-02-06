namespace FiscalOS.Infra.Tests.Unit;

public class FileKeyRingOptionsTests
{
  [Fact]
  public void FileKeyRingOptions_WhenCreatedWithDefaults_ItShouldHaveCorrectDefaultValues()
  {
    var options = new FileKeyRingOptions();

    options.KeysDirectoryPath.Should().Be(string.Empty);
    options.PrimaryKeyId.Should().Be(string.Empty);
  }

  [Fact]
  public void FileKeyRingOptions_WhenInitializedWithValues_ItShouldHaveCorrectValues()
  {
    var keysDirectoryPath = "/secure/keys";
    var primaryKeyId = "primary-key-2024";

    var options = new FileKeyRingOptions
    {
      KeysDirectoryPath = keysDirectoryPath,
      PrimaryKeyId = primaryKeyId
    };

    options.KeysDirectoryPath.Should().Be(keysDirectoryPath);
    options.PrimaryKeyId.Should().Be(primaryKeyId);
  }

  [Fact]
  public void FileKeyRingOptions_WhenUsedAsRecord_ItShouldSupportEquality()
  {
    var options1 = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = "key-1"
    };

    var options2 = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/keys",
      PrimaryKeyId = "key-1"
    };

    options1.Should().Be(options2);
  }

  [Fact]
  public void FileKeyRingOptions_WhenCreatedWithDifferentValues_ItShouldNotBeEqual()
  {
    var options1 = new FileKeyRingOptions { KeysDirectoryPath = "/keys1" };
    var options2 = new FileKeyRingOptions { KeysDirectoryPath = "/keys2" };

    options1.Should().NotBe(options2);
  }

  [Fact]
  public void FileKeyRingOptions_WhenInitializedWithLongPath_ItShouldStoreFullPath()
  {
    var longPath = "/secure/keys/directory/with/multiple/levels";
    var options = new FileKeyRingOptions { KeysDirectoryPath = longPath };

    options.KeysDirectoryPath.Should().Be(longPath);
  }

  [Fact]
  public void FileKeyRingOptions_WhenInitializedWithRelativePath_ItShouldStoreRelativePath()
  {
    var relativePath = "./keys";
    var options = new FileKeyRingOptions { KeysDirectoryPath = relativePath };

    options.KeysDirectoryPath.Should().Be(relativePath);
  }

  [Fact]
  public void FileKeyRingOptions_WhenUsedWithModification_ItShouldSupportWith()
  {
    var originalOptions = new FileKeyRingOptions
    {
      KeysDirectoryPath = "/original",
      PrimaryKeyId = "original-key"
    };

    var modifiedOptions = originalOptions with { PrimaryKeyId = "modified-key" };

    originalOptions.PrimaryKeyId.Should().Be("original-key");
    modifiedOptions.PrimaryKeyId.Should().Be("modified-key");
    modifiedOptions.KeysDirectoryPath.Should().Be("/original");
  }

  [Fact]
  public void FileKeyRingOptionsSetup_WhenConstructed_ItShouldStoreConfiguration()
  {
    var configuration = new ConfigurationBuilder().Build();

    var setup = new FileKeyRingOptionsSetup(configuration);

    setup.Should().NotBeNull();
  }

  [Fact]
  public void Configure_WhenCalledWithValidConfiguration_ItShouldBindFileKeyRingOptionsCorrectly()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", "/secure/keys" },
      { "FileKeyRingOptions:PrimaryKeyId", "primary-key-id" }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be("/secure/keys");
    options.PrimaryKeyId.Should().Be("primary-key-id");
  }

  [Fact]
  public void Configure_WhenCalledWithPartialConfiguration_ItShouldBindAvailableValuesAndKeepDefaults()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", "/keys" }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be("/keys");
    options.PrimaryKeyId.Should().Be(string.Empty);
  }

  [Fact]
  public void Configure_WhenCalledWithMissingSection_ItShouldLeaveOptionsUnchanged()
  {
    var configuration = new ConfigurationBuilder().Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be(string.Empty);
    options.PrimaryKeyId.Should().Be(string.Empty);
  }

  [Fact]
  public void Configure_WhenCalledWithEmptyStringValues_ItShouldBindEmptyStrings()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", "" },
      { "FileKeyRingOptions:PrimaryKeyId", "" }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be(string.Empty);
    options.PrimaryKeyId.Should().Be(string.Empty);
  }

  [Fact]
  public void Configure_WhenCalledWithComplexDirectoryPath_ItShouldBindFullPath()
  {
    var complexPath = "/var/lib/app/secure/encryption/keys/storage";
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", complexPath }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be(complexPath);
  }

  [Fact]
  public void Configure_WhenCalledWithRelativeDirectoryPath_ItShouldBindRelativePath()
  {
    var relativePath = "./keys";
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", relativePath }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be(relativePath);
  }

  [Fact]
  public void Configure_WhenCalledWithGuidKeyId_ItShouldBindKeyId()
  {
    var guidKeyId = "550e8400-e29b-41d4-a716-446655440000";
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:PrimaryKeyId", guidKeyId }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.PrimaryKeyId.Should().Be(guidKeyId);
  }

  [Fact]
  public void Configure_WhenCalledMultipleTimes_ItShouldUpdateOptionsEachTime()
  {
    var configBuilder1 = new ConfigurationBuilder();
    configBuilder1.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", "/keys1" }
    });
    var configuration1 = configBuilder1.Build();
    var setup1 = new FileKeyRingOptionsSetup(configuration1);
    var options = new FileKeyRingOptions();

    setup1.Configure(options);
    options.KeysDirectoryPath.Should().Be("/keys1");

    var configBuilder2 = new ConfigurationBuilder();
    configBuilder2.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", "/keys2" }
    });
    var configuration2 = configBuilder2.Build();
    var setup2 = new FileKeyRingOptionsSetup(configuration2);

    setup2.Configure(options);
    options.KeysDirectoryPath.Should().Be("/keys2");
  }

  [Fact]
  public void Configure_WhenCalledWithWindowsPath_ItShouldBindWindowsPath()
  {
    var windowsPath = "C:\\secure\\keys";
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", windowsPath }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be(windowsPath);
  }

  [Fact]
  public void Configure_WhenCalledWithOnlyPrimaryKeyId_ItShouldBindKeyIdAndLeavePathDefault()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:PrimaryKeyId", "my-key-id" }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.PrimaryKeyId.Should().Be("my-key-id");
    options.KeysDirectoryPath.Should().Be(string.Empty);
  }

  [Fact]
  public void Configure_WhenCalledWithWhitespaceValues_ItShouldBindWhitespace()
  {
    var whitespaceValue = "   ";
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "FileKeyRingOptions:KeysDirectoryPath", whitespaceValue }
    });
    var configuration = configBuilder.Build();
    var setup = new FileKeyRingOptionsSetup(configuration);
    var options = new FileKeyRingOptions();

    setup.Configure(options);

    options.KeysDirectoryPath.Should().Be(whitespaceValue);
  }
}