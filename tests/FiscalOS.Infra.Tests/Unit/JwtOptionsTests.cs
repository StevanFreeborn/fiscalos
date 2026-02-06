namespace FiscalOS.Infra.Tests.Unit;

public class JwtOptionsTests
{
  [Fact]
  public void JwtOptions_WhenCreatedWithDefaults_ItShouldHaveCorrectDefaultValues()
  {
    var options = new JwtOptions();

    options.Issuer.Should().Be(string.Empty);
    options.Audience.Should().Be(string.Empty);
    options.Secret.Should().Be(string.Empty);
    options.ExpiryInMinutes.Should().Be(5);
  }

  [Fact]
  public void JwtOptions_WhenInitializedWithValues_ItShouldHaveCorrectValues()
  {
    var issuer = "test-issuer";
    var audience = "test-audience";
    var secret = "test-secret-key";
    var expiryInMinutes = 30;

    var options = new JwtOptions
    {
      Issuer = issuer,
      Audience = audience,
      Secret = secret,
      ExpiryInMinutes = expiryInMinutes
    };

    options.Issuer.Should().Be(issuer);
    options.Audience.Should().Be(audience);
    options.Secret.Should().Be(secret);
    options.ExpiryInMinutes.Should().Be(expiryInMinutes);
  }

  [Fact]
  public void JwtOptions_WhenSecretIsSet_ItShouldReturnSymmetricSecurityKey()
  {
    var secret = "my-super-secret-key-12345";
    var options = new JwtOptions { Secret = secret };

    var result = options.Key;

    result.Should().NotBeNull();
    result.Should().BeOfType<SymmetricSecurityKey>();
  }

  [Fact]
  public void JwtOptions_WhenSecretIsSet_ItShouldEncodeSecretAsUtf8Bytes()
  {
    var secret = "test-secret";
    var options = new JwtOptions { Secret = secret };

    var result = options.Key;
    var expectedKey = Encoding.UTF8.GetBytes(secret);

    result.Key.Should().BeEquivalentTo(expectedKey);
  }

  [Fact]
  public void JwtOptions_WhenSecretChanges_ItShouldReturnUpdatedSymmetricSecurityKey()
  {
    var initialSecret = "initial-secret";
    var newSecret = "new-secret";
    var options = new JwtOptions { Secret = initialSecret };

    var initialKey = options.Key;
    var updatedOptions = options with { Secret = newSecret };
    var updatedKey = updatedOptions.Key;

    initialKey.Key.Should().NotBeEquivalentTo(updatedKey.Key);
  }

  [Fact]
  public void JwtOptions_WhenUsedAsRecord_ItShouldSupportEquality()
  {
    var options1 = new JwtOptions
    {
      Issuer = "issuer",
      Audience = "audience",
      Secret = "secret",
      ExpiryInMinutes = 30
    };

    var options2 = new JwtOptions
    {
      Issuer = "issuer",
      Audience = "audience",
      Secret = "secret",
      ExpiryInMinutes = 30
    };

    options1.Should().Be(options2);
  }

  [Fact]
  public void JwtOptions_WhenCreatedWithDifferentValues_ItShouldNotBeEqual()
  {
    var options1 = new JwtOptions { Secret = "secret1" };
    var options2 = new JwtOptions { Secret = "secret2" };

    options1.Should().NotBe(options2);
  }

  [Fact]
  public void JwtOptionsSetup_WhenConstructed_ItShouldStoreConfiguration()
  {
    var configuration = new ConfigurationBuilder().Build();

    var setup = new JwtOptionsSetup(configuration);

    setup.Should().NotBeNull();
  }

  [Fact]
  public void Configure_WhenCalledWithValidConfiguration_ItShouldBindJwtOptionsCorrectly()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "JwtOptions:Issuer", "test-issuer" },
      { "JwtOptions:Audience", "test-audience" },
      { "JwtOptions:Secret", "test-secret" },
      { "JwtOptions:ExpiryInMinutes", "60" }
    });
    var configuration = configBuilder.Build();
    var setup = new JwtOptionsSetup(configuration);
    var options = new JwtOptions();

    setup.Configure(options);

    options.Issuer.Should().Be("test-issuer");
    options.Audience.Should().Be("test-audience");
    options.Secret.Should().Be("test-secret");
    options.ExpiryInMinutes.Should().Be(60);
  }

  [Fact]
  public void Configure_WhenCalledWithPartialConfiguration_ItShouldBindAvailableValuesAndKeepDefaults()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "JwtOptions:Issuer", "test-issuer" }
    });
    var configuration = configBuilder.Build();
    var setup = new JwtOptionsSetup(configuration);
    var options = new JwtOptions();

    setup.Configure(options);

    options.Issuer.Should().Be("test-issuer");
    options.Audience.Should().Be(string.Empty);
    options.Secret.Should().Be(string.Empty);
    options.ExpiryInMinutes.Should().Be(5);
  }

  [Fact]
  public void Configure_WhenCalledWithMissingSection_ItShouldLeaveOptionsUnchanged()
  {
    var configuration = new ConfigurationBuilder().Build();
    var setup = new JwtOptionsSetup(configuration);
    var options = new JwtOptions();

    setup.Configure(options);

    options.Issuer.Should().Be(string.Empty);
    options.Audience.Should().Be(string.Empty);
    options.Secret.Should().Be(string.Empty);
    options.ExpiryInMinutes.Should().Be(5);
  }

  [Fact]
  public void Configure_WhenCalledWithEmptyStringValues_ItShouldBindEmptyStrings()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "JwtOptions:Issuer", "" },
      { "JwtOptions:Audience", "" },
      { "JwtOptions:Secret", "" }
    });
    var configuration = configBuilder.Build();
    var setup = new JwtOptionsSetup(configuration);
    var options = new JwtOptions();

    setup.Configure(options);

    options.Issuer.Should().Be(string.Empty);
    options.Audience.Should().Be(string.Empty);
    options.Secret.Should().Be(string.Empty);
  }

  [Fact]
  public void Configure_WhenCalledWithNumericString_ItShouldBindExpiryInMinutesAsInteger()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "JwtOptions:ExpiryInMinutes", "120" }
    });
    var configuration = configBuilder.Build();
    var setup = new JwtOptionsSetup(configuration);
    var options = new JwtOptions();

    setup.Configure(options);

    options.ExpiryInMinutes.Should().Be(120);
  }

  [Fact]
  public void Configure_WhenCalledMultipleTimes_ItShouldUpdateOptionsEachTime()
  {
    var configBuilder = new ConfigurationBuilder();
    configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "JwtOptions:Issuer", "issuer-1" }
    });
    var configuration = configBuilder.Build();
    var setup = new JwtOptionsSetup(configuration);
    var options = new JwtOptions();

    setup.Configure(options);
    options.Issuer.Should().Be("issuer-1");

    var configBuilder2 = new ConfigurationBuilder();
    configBuilder2.AddInMemoryCollection(new Dictionary<string, string?>
    {
      { "JwtOptions:Issuer", "issuer-2" }
    });
    var configuration2 = configBuilder2.Build();
    var setup2 = new JwtOptionsSetup(configuration2);

    setup2.Configure(options);
    options.Issuer.Should().Be("issuer-2");
  }

  [Fact]
  public void Configure_WhenCalledWithDefaultScheme_ItShouldSetupJwtBearerOptionsWithValidation()
  {
    var jwtOptions = new JwtOptions
    {
      Issuer = "test-issuer",
      Audience = "test-audience",
      Secret = "test-secret-key-for-jwt-validation"
    };
    var options = Options.Create(jwtOptions);
    var setup = new JwtBearerOptionsSetup(options);
    var bearerOptions = new JwtBearerOptions();

    setup.Configure(JwtBearerDefaults.AuthenticationScheme, bearerOptions);

    bearerOptions.TokenValidationParameters.Should().NotBeNull();
    bearerOptions.TokenValidationParameters!.ValidIssuer.Should().Be("test-issuer");
    bearerOptions.TokenValidationParameters.ValidAudience.Should().Be("test-audience");
    bearerOptions.TokenValidationParameters.ValidateIssuer.Should().BeTrue();
    bearerOptions.TokenValidationParameters.ValidateAudience.Should().BeTrue();
    bearerOptions.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
    bearerOptions.TokenValidationParameters.ValidateLifetime.Should().BeTrue();
  }

  [Fact]
  public void Configure_WhenCalledWithAllowExpiredTokensScheme_ItShouldDisableLifetimeValidation()
  {
    var jwtOptions = new JwtOptions
    {
      Issuer = "test-issuer",
      Audience = "test-audience",
      Secret = "test-secret-key-for-jwt-validation"
    };
    var options = Options.Create(jwtOptions);
    var setup = new JwtBearerOptionsSetup(options);
    var bearerOptions = new JwtBearerOptions();

    setup.Configure(Schemes.AllowExpiredTokens, bearerOptions);

    bearerOptions.TokenValidationParameters.Should().NotBeNull();
    bearerOptions.TokenValidationParameters!.ValidateLifetime.Should().BeFalse();
  }

  [Fact]
  public void Configure_WhenCalledWithNullSchemeName_ItShouldSetupWithValidation()
  {
    var jwtOptions = new JwtOptions
    {
      Issuer = "test-issuer",
      Audience = "test-audience",
      Secret = "test-secret-key-for-jwt-validation"
    };
    var options = Options.Create(jwtOptions);
    var setup = new JwtBearerOptionsSetup(options);
    var bearerOptions = new JwtBearerOptions();

    setup.Configure(null, bearerOptions);

    bearerOptions.TokenValidationParameters.Should().NotBeNull();
    bearerOptions.TokenValidationParameters!.ValidateLifetime.Should().BeTrue();
  }

  [Fact]
  public void Configure_WhenCalledWithoutNameParameter_ItShouldSetupWithValidation()
  {
    var jwtOptions = new JwtOptions
    {
      Issuer = "test-issuer",
      Audience = "test-audience",
      Secret = "test-secret-key-for-jwt-validation"
    };
    var options = Options.Create(jwtOptions);
    var setup = new JwtBearerOptionsSetup(options);
    var bearerOptions = new JwtBearerOptions();

    setup.Configure(bearerOptions);

    bearerOptions.TokenValidationParameters.Should().NotBeNull();
    bearerOptions.TokenValidationParameters!.ValidateLifetime.Should().BeTrue();
  }

  [Fact]
  public void Configure_WhenCalledWithDifferentSchemeNames_ItShouldOnlyDisableLifetimeForAllowExpiredTokens()
  {
    var jwtOptions = new JwtOptions
    {
      Issuer = "test-issuer",
      Audience = "test-audience",
      Secret = "test-secret-key-for-jwt-validation"
    };
    var options = Options.Create(jwtOptions);
    var setup = new JwtBearerOptionsSetup(options);

    var defaultBearerOptions = new JwtBearerOptions();
    setup.Configure(Schemes.Default, defaultBearerOptions);

    var allowExpiredBearerOptions = new JwtBearerOptions();
    setup.Configure(Schemes.AllowExpiredTokens, allowExpiredBearerOptions);

    defaultBearerOptions.TokenValidationParameters!.ValidateLifetime.Should().BeTrue();
    allowExpiredBearerOptions.TokenValidationParameters!.ValidateLifetime.Should().BeFalse();
  }

  [Fact]
  public void Configure_WhenCalledWithCustomSchemeName_ItShouldUseDefaultValidationBehavior()
  {
    var jwtOptions = new JwtOptions
    {
      Issuer = "test-issuer",
      Audience = "test-audience",
      Secret = "test-secret-key-for-jwt-validation"
    };
    var options = Options.Create(jwtOptions);
    var setup = new JwtBearerOptionsSetup(options);
    var bearerOptions = new JwtBearerOptions();

    setup.Configure("CustomScheme", bearerOptions);

    bearerOptions.TokenValidationParameters!.ValidateLifetime.Should().BeTrue();
  }

  [Fact]
  public void Configure_WhenCalled_ItShouldSetIssuerSigningKeyFromJwtOptions()
  {
    var secret = "test-secret-key-for-jwt-validation";
    var jwtOptions = new JwtOptions
    {
      Issuer = "test-issuer",
      Audience = "test-audience",
      Secret = secret
    };
    var options = Options.Create(jwtOptions);
    var setup = new JwtBearerOptionsSetup(options);
    var bearerOptions = new JwtBearerOptions();

    setup.Configure(bearerOptions);

    var expectedKey = jwtOptions.Key;
    bearerOptions.TokenValidationParameters!.IssuerSigningKey.Should().BeEquivalentTo(expectedKey);
  }
}