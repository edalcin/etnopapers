using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Utils;
using Serilog;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Manages application configuration (Cloud AI, MongoDB) persistence.
    /// Stores configuration in AppData/Local/EtnoPapers/config.json
    /// API keys are encrypted using Windows DPAPI.
    /// </summary>
    public class ConfigurationService
    {
        private readonly string _configPath;
        private Configuration _cachedConfiguration;
        private static readonly ILogger Logger = Log.ForContext<ConfigurationService>();

        public ConfigurationService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EtnoPapers"
            );

            Directory.CreateDirectory(appDataPath);
            _configPath = Path.Combine(appDataPath, "config.json");
        }

        /// <summary>
        /// Loads configuration from file, decrypting API key if present.
        /// Uses cache if available.
        /// </summary>
        public Configuration LoadConfiguration()
        {
            if (_cachedConfiguration != null)
                return _cachedConfiguration;

            try
            {
                if (!File.Exists(_configPath))
                {
                    Logger.Debug("Configuration file not found, using defaults");
                    _cachedConfiguration = new Configuration();
                    return _cachedConfiguration;
                }

                var json = File.ReadAllText(_configPath);
                _cachedConfiguration = JsonConvert.DeserializeObject<Configuration>(json) ?? new();

                // Decrypt API key if present
                if (!string.IsNullOrEmpty(_cachedConfiguration.ApiKey))
                {
                    try
                    {
                        _cachedConfiguration.ApiKey = EncryptionHelper.Decrypt(_cachedConfiguration.ApiKey);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to decrypt API key");
                        _cachedConfiguration.ApiKey = null;
                    }
                }

                return _cachedConfiguration;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading configuration");
                _cachedConfiguration = new Configuration();
                return _cachedConfiguration;
            }
        }

        /// <summary>
        /// Saves configuration to file, encrypting API key if present.
        /// Updates cache after successful save.
        /// </summary>
        public void SaveConfiguration(Configuration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                // Validate before saving
                if (!ValidateConfiguration(config))
                    throw new InvalidOperationException("Configuration validation failed");

                // Create a copy for serialization (to avoid encrypting the cached copy)
                var configToSave = JsonConvert.DeserializeObject<Configuration>(
                    JsonConvert.SerializeObject(config)) ?? config;

                // Encrypt API key if present
                if (!string.IsNullOrEmpty(configToSave.ApiKey))
                {
                    configToSave.ApiKey = EncryptionHelper.Encrypt(configToSave.ApiKey);
                }

                var json = JsonConvert.SerializeObject(configToSave, Formatting.Indented);
                File.WriteAllText(_configPath, json, Encoding.UTF8);
                _cachedConfiguration = config;

                Logger.Information("Configuration saved successfully for provider: {Provider}",
                    config.AIProvider?.ToString() ?? "None");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to save configuration");
                throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Resets configuration to defaults and saves to file.
        /// </summary>
        public void ResetToDefaults()
        {
            _cachedConfiguration = new Configuration();
            SaveConfiguration(_cachedConfiguration);
            Logger.Information("Configuration reset to defaults");
        }

        /// <summary>
        /// Validates configuration values for cloud AI provider setup.
        /// </summary>
        public bool ValidateConfiguration(Configuration config)
        {
            if (config == null)
                return false;

            // Validate cloud AI provider if configured
            if (config.AIProvider.HasValue)
            {
                // API key is required for cloud AI
                if (string.IsNullOrWhiteSpace(config.ApiKey))
                {
                    Logger.Warning("API key is empty for configured provider");
                    return false;
                }

                // API key must be at least 10 characters (rough validation)
                if (config.ApiKey.Length < 10)
                {
                    Logger.Warning("API key appears too short");
                    return false;
                }
            }

            // Validate OLLAMA URL if provided (legacy)
            if (!string.IsNullOrEmpty(config.OllamaUrl))
            {
                if (!Uri.TryCreate(config.OllamaUrl, UriKind.Absolute, out _))
                {
                    Logger.Warning("Invalid OLLAMA URL format");
                    return false;
                }
            }

            // Validate MongoDB URI if provided
            if (!string.IsNullOrEmpty(config.MongodbUri))
            {
                // Basic validation - must start with mongodb+srv:// or mongodb://
                if (!config.MongodbUri.StartsWith("mongodb://") &&
                    !config.MongodbUri.StartsWith("mongodb+srv://"))
                {
                    Logger.Warning("Invalid MongoDB URI format");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        public string GetConfigurationFilePath()
        {
            return _configPath;
        }

        /// <summary>
        /// Clears the cache, forcing reload on next access.
        /// </summary>
        public void ClearCache()
        {
            _cachedConfiguration = null;
        }
    }
}
