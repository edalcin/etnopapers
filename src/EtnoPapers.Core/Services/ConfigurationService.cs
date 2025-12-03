using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Manages application configuration (OLLAMA, MongoDB) persistence.
    /// Stores configuration in Documents/EtnoPapers/config.json
    /// </summary>
    public class ConfigurationService
    {
        private readonly string _configPath;
        private Configuration _cachedConfiguration;

        public ConfigurationService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EtnoPapers"
            );

            Directory.CreateDirectory(appDataPath);
            _configPath = Path.Combine(appDataPath, "config.json");
        }

        /// <summary>
        /// Loads configuration from file, using cache if available.
        /// </summary>
        public Configuration LoadConfiguration()
        {
            if (_cachedConfiguration != null)
                return _cachedConfiguration;

            try
            {
                if (!File.Exists(_configPath))
                {
                    // Return default configuration
                    _cachedConfiguration = new Configuration();
                    return _cachedConfiguration;
                }

                var json = File.ReadAllText(_configPath);
                _cachedConfiguration = JsonConvert.DeserializeObject<Configuration>(json) ?? new();
                return _cachedConfiguration;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                _cachedConfiguration = new Configuration();
                return _cachedConfiguration;
            }
        }

        /// <summary>
        /// Saves configuration to file and updates cache.
        /// </summary>
        public void SaveConfiguration(Configuration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            try
            {
                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configPath, json, Encoding.UTF8);
                _cachedConfiguration = config;
            }
            catch (Exception ex)
            {
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
        }

        /// <summary>
        /// Validates configuration values.
        /// </summary>
        public bool ValidateConfiguration(Configuration config)
        {
            if (config == null)
                return false;

            // Validate OLLAMA URL if provided
            if (!string.IsNullOrEmpty(config.OllamaUrl))
            {
                if (!Uri.TryCreate(config.OllamaUrl, UriKind.Absolute, out _))
                    return false;
            }

            // Validate MongoDB URI if provided
            if (!string.IsNullOrEmpty(config.MongodbUri))
            {
                // Basic validation - must start with mongodb+srv:// or mongodb://
                if (!config.MongodbUri.StartsWith("mongodb://") && !config.MongodbUri.StartsWith("mongodb+srv://"))
                    return false;
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
