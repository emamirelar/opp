using UNOPS.PAO.ExternalDataService.Models.Configuration;
using UNOPS.PAO.ExternalDataService.Models.External;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace UNOPS.PAO.ExternalDataService.Services.Configuration;

public class ConfigurationService : IConfigurationService, IDisposable
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConfigurationValidator _validator;
    private readonly Dictionary<string, SyncConfiguration> _configurations = new();
    private FileSystemWatcher? _fileWatcher;
    private readonly string _configurationPath;

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationService(
        ILogger<ConfigurationService> logger,
        IConfiguration configuration,
        ConfigurationValidator validator)
    {
        _logger = logger;
        _configuration = configuration;
        _validator = validator;
        _configurationPath = _configuration["ExternalDataService:ConfigurationPath"] 
            ?? Path.Combine(AppContext.BaseDirectory, "config");

        try
        {
            InitializeFileWatcher();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not initialize file watcher for configuration path: {Path}", _configurationPath);
        }
    }

    public async Task<IEnumerable<SyncConfiguration>> LoadAllConfigurationsAsync()
    {
        var configurations = new List<SyncConfiguration>();
        
        if (!Directory.Exists(_configurationPath))
        {
            _logger.LogWarning("Configuration directory does not exist: {Path}", _configurationPath);
            return configurations;
        }

        var configFiles = Directory.GetFiles(_configurationPath, "*.yaml", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(_configurationPath, "*.yml", SearchOption.TopDirectoryOnly));

        foreach (var configFile in configFiles)
        {
            try
            {
                var config = await LoadConfigurationFromFileAsync(configFile);
                if (config != null && await _validator.ValidateAsync(config))
                {
                    configurations.Add(config);
                    _configurations[config.Metadata.Name] = config;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load configuration from {ConfigFile}", configFile);
            }
        }

        _logger.LogInformation("Loaded {Count} valid configurations", configurations.Count);
        return configurations;
    }

    public async Task<SyncConfiguration?> LoadConfigurationAsync(string name)
    {
        if (_configurations.TryGetValue(name, out var cachedConfig))
        {
            return cachedConfig;
        }

        // Try to find and load the configuration file
        if (!Directory.Exists(_configurationPath))
        {
            _logger.LogWarning("Configuration directory does not exist: {Path}", _configurationPath);
            return null;
        }

        var configFiles = Directory.GetFiles(_configurationPath, "*.yaml", SearchOption.TopDirectoryOnly)
                                 .Concat(Directory.GetFiles(_configurationPath, "*.yml", SearchOption.TopDirectoryOnly));

        foreach (var configFile in configFiles)
        {
            var config = await LoadConfigurationFromFileAsync(configFile);
            if (config?.Metadata.Name == name)
            {
                if (await _validator.ValidateAsync(config))
                {
                    _configurations[name] = config;
                    return config;
                }
                break;
            }
        }

        return null;
    }

    public async Task<bool> ValidateConfigurationAsync(SyncConfiguration configuration)
    {
        return await _validator.ValidateAsync(configuration);
    }

    public async Task RefreshConfigurationsAsync()
    {
        _configurations.Clear();
        await LoadAllConfigurationsAsync();
    }

    private async Task<SyncConfiguration?> LoadConfigurationFromFileAsync(string filePath)
    {
        try
        {
            var yaml = await File.ReadAllTextAsync(filePath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var config = deserializer.Deserialize<SyncConfiguration>(yaml);
            
            _logger.LogDebug("Successfully loaded configuration from {FilePath}", filePath);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse configuration file {FilePath}", filePath);
            return null;
        }
    }

    private void InitializeFileWatcher()
    {
        if (!Directory.Exists(_configurationPath))
        {
            Directory.CreateDirectory(_configurationPath);
        }

        _fileWatcher = new FileSystemWatcher(_configurationPath)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
            Filter = "*.*"  // Watch all files, we'll filter for YAML in the handler
        };

        _fileWatcher.Changed += OnConfigurationFileChanged;
        _fileWatcher.Created += OnConfigurationFileChanged;
        _fileWatcher.Deleted += OnConfigurationFileChanged;
        _fileWatcher.Renamed += OnConfigurationFileRenamed;
        _fileWatcher.EnableRaisingEvents = true;
    }

    private async void OnConfigurationFileChanged(object sender, FileSystemEventArgs e)
    {
        // Only process YAML files
        if (!e.FullPath.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) && 
            !e.FullPath.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Debounce multiple rapid changes
        await Task.Delay(1000);
        
        try
        {
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                // Find and remove the configuration
                var configToRemove = _configurations.FirstOrDefault(c => 
                    Path.GetFileNameWithoutExtension(e.FullPath).Equals(c.Key, StringComparison.OrdinalIgnoreCase));
                
                if (!configToRemove.Equals(default(KeyValuePair<string, SyncConfiguration>)))
                {
                    _configurations.Remove(configToRemove.Key);
                    _logger.LogInformation("Configuration removed: {ConfigName}", configToRemove.Key);
                }
                return;
            }

            var config = await LoadConfigurationFromFileAsync(e.FullPath);
            if (config != null && await _validator.ValidateAsync(config))
            {
                _configurations[config.Metadata.Name] = config;
                ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(config, ChangeType.Updated));
                _logger.LogInformation("Configuration updated: {ConfigName}", config.Metadata.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing configuration file change: {FilePath}", e.FullPath);
        }
    }

    private void OnConfigurationFileRenamed(object sender, RenamedEventArgs e)
    {
        // Treat rename as delete old + create new
        OnConfigurationFileChanged(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(e.OldFullPath) ?? string.Empty, e.OldName));
        OnConfigurationFileChanged(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath) ?? string.Empty, e.Name));
    }

    public void Dispose()
    {
        _fileWatcher?.Dispose();
    }
}
