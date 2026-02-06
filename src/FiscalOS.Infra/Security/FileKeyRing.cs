
namespace FiscalOS.Infra.Security;

public sealed class FileKeyRing : IKeyRing, IDisposable
{
  private const string KeyFileExtension = ".key";
  private FileKeyRingOptions _options;
  private readonly IFileSystem _fileSystem;
  private readonly IDisposable? _optionsChangeHandler;
  private readonly Dictionary<string, KeyRingEntry> _keys = [];
  private string KeyRingPath => _fileSystem.Path.GetFullPath(_options.KeysDirectoryPath, AppContext.BaseDirectory);

  private FileKeyRing(IOptionsMonitor<FileKeyRingOptions> options, IFileSystem fileSystem)
  {
    _fileSystem = fileSystem;
    _options = options.CurrentValue;

    LoadKeys();

    _optionsChangeHandler = options.OnChange(options =>
    {
      _options = options;
      LoadKeys();
    });
  }

  public static FileKeyRing From(IServiceProvider serviceProvider)
  {
    var options = serviceProvider.GetRequiredService<IOptionsMonitor<FileKeyRingOptions>>();
    var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    return new(options, fileSystem);
  }

  public static FileKeyRing From(IOptionsMonitor<FileKeyRingOptions> options, IFileSystem fileSystem)
  {
    return new(options, fileSystem);
  }

  public KeyRingEntry GetKey(string keyId)
  {
    if (_keys.TryGetValue(keyId, out var key))
    {
      return key;
    }

    throw new KeyNotFoundException($"Key with ID '{keyId}' not found in the key ring.");
  }

  public KeyRingEntry GetPrimaryKey()
  {
    return GetKey(_options.PrimaryKeyId);
  }

  private void LoadKeys()
  {
    _keys.Clear();

    if (_fileSystem.Directory.Exists(KeyRingPath))
    {
      var keyFiles = _fileSystem.Directory.GetFiles(KeyRingPath, $"*{KeyFileExtension}");

      foreach (var keyFile in keyFiles)
      {
        var keyId = _fileSystem.Path.GetFileNameWithoutExtension(keyFile);
        var key = _fileSystem.File.ReadAllText(keyFile).Trim();

        if (string.IsNullOrEmpty(key) is false)
        {
          _keys[keyId] = KeyRingEntry.From(keyId, key);
        }
      }
    }
  }

  public void Dispose()
  {
    _optionsChangeHandler?.Dispose();
  }

  public async Task<KeyRingEntry> SaveKeyAsync(string key)
  {
    var id = Guid.NewGuid().ToString();
    var entry = KeyRingEntry.From(id, key);
    var filename = id + KeyFileExtension;
    var entryPath = _fileSystem.Path.Combine(KeyRingPath, filename);
    await _fileSystem.File.WriteAllTextAsync(entryPath, key).ConfigureAwait(false);
    return entry;
  }
}