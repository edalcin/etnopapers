/**
 * Settings Page Component
 * Manages configuration for OLLAMA and MongoDB connections
 */

import React, { useState, useEffect } from 'react';
import { AppConfiguration, OLLAMAConfig, MongoDBConfig } from '@shared/types/config';

/**
 * Settings page component
 */
export const SettingsPage: React.FC = () => {
  const [config, setConfig] = useState<AppConfiguration | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<{
    type: 'success' | 'error';
    text: string;
  } | null>(null);

  // OLLAMA form state
  const [ollamaUrl, setOllamaUrl] = useState('');
  const [ollamaModel, setOllamaModel] = useState('');
  const [ollamaPrompt, setOllamaPrompt] = useState('');
  const [testingOllama, setTestingOllama] = useState(false);

  // MongoDB form state
  const [mongoUri, setMongoUri] = useState('');
  const [testingMongo, setTestingMongo] = useState(false);
  const [mongoConfigured, setMongoConfigured] = useState(false);

  // Language state
  const [language, setLanguage] = useState<'pt-BR' | 'en-US'>('pt-BR');

  /**
   * Load configuration on mount
   */
  useEffect(() => {
    loadConfiguration();
  }, []);

  /**
   * Load configuration from main process
   */
  const loadConfiguration = async () => {
    try {
      setLoading(true);
      const result = await window.etnopapers.config.load();
      setConfig(result);

      // Set form values from loaded config
      setOllamaUrl(result.ollama.baseUrl);
      setOllamaModel(result.ollama.model);
      setOllamaPrompt(result.ollama.prompt || '');
      setLanguage(result.language);

      if (result.mongodb) {
        setMongoUri(result.mongodb.uri);
        setMongoConfigured(true);
      }

      setMessage({ type: 'success', text: 'Configuration loaded' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Failed to load configuration: ${error instanceof Error ? error.message : String(error)}`,
      });
    } finally {
      setLoading(false);
    }
  };

  /**
   * Save OLLAMA configuration
   */
  const saveOllamaConfig = async () => {
    try {
      setSaving(true);
      const ollamaConfig: OLLAMAConfig = {
        baseUrl: ollamaUrl,
        model: ollamaModel,
        prompt: ollamaPrompt || undefined,
      };

      await window.etnopapers.config.updateOllama(ollamaConfig);
      setMessage({ type: 'success', text: 'OLLAMA configuration saved' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Failed to save OLLAMA config: ${error instanceof Error ? error.message : String(error)}`,
      });
    } finally {
      setSaving(false);
    }
  };

  /**
   * Test OLLAMA connection
   */
  const testOllamaConnection = async () => {
    try {
      setTestingOllama(true);
      await window.etnopapers.config.testOllamaConnection(ollamaUrl);
      setMessage({ type: 'success', text: 'OLLAMA connection successful' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `OLLAMA connection failed: ${error instanceof Error ? error.message : String(error)}`,
      });
    } finally {
      setTestingOllama(false);
    }
  };

  /**
   * Save MongoDB configuration
   */
  const saveMongoDBConfig = async () => {
    try {
      setSaving(true);
      const mongoConfig: MongoDBConfig = {
        uri: mongoUri,
        database: 'etnopapers',
        collection: 'articles',
      };

      await window.etnopapers.config.updateMongoDB(mongoConfig);
      setMongoConfigured(true);
      setMessage({ type: 'success', text: 'MongoDB configuration saved' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Failed to save MongoDB config: ${error instanceof Error ? error.message : String(error)}`,
      });
    } finally {
      setSaving(false);
    }
  };

  /**
   * Test MongoDB connection
   */
  const testMongoDBConnection = async () => {
    try {
      setTestingMongo(true);
      await window.etnopapers.config.testMongoDBConnection(mongoUri);
      setMessage({ type: 'success', text: 'MongoDB connection successful' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `MongoDB connection failed: ${error instanceof Error ? error.message : String(error)}`,
      });
    } finally {
      setTestingMongo(false);
    }
  };

  /**
   * Save language preference
   */
  const saveLanguage = async () => {
    try {
      setSaving(true);
      await window.etnopapers.config.update('language', language);
      setMessage({ type: 'success', text: 'Language preference saved' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Failed to save language: ${error instanceof Error ? error.message : String(error)}`,
      });
    } finally {
      setSaving(false);
    }
  };

  /**
   * Reset to defaults
   */
  const resetToDefaults = async () => {
    if (!window.confirm('Reset all settings to defaults?')) return;

    try {
      setSaving(true);
      await window.etnopapers.config.reset();
      await loadConfiguration();
      setMessage({ type: 'success', text: 'Settings reset to defaults' });
      setTimeout(() => setMessage(null), 3000);
    } catch (error) {
      setMessage({
        type: 'error',
        text: `Failed to reset settings: ${error instanceof Error ? error.message : String(error)}`,
      });
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-center">
          <div className="text-gray-400 mb-4">Loading settings...</div>
        </div>
      </div>
    );
  }

  return (
    <div className="h-full bg-gray-50 overflow-y-auto">
      <div className="max-w-2xl mx-auto p-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Settings</h1>

        {/* Message display */}
        {message && (
          <div
            className={`mb-6 p-4 rounded-lg ${
              message.type === 'success'
                ? 'bg-green-50 text-green-800 border border-green-200'
                : 'bg-red-50 text-red-800 border border-red-200'
            }`}
          >
            {message.text}
          </div>
        )}

        {/* Language Settings */}
        <div className="bg-white rounded-lg shadow mb-6 p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Language</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Application Language
              </label>
              <select
                value={language}
                onChange={(e) => setLanguage(e.target.value as 'pt-BR' | 'en-US')}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="pt-BR">Portuguese (Brazil)</option>
                <option value="en-US">English (US)</option>
              </select>
            </div>
            <button
              onClick={saveLanguage}
              disabled={saving}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
            >
              Save Language
            </button>
          </div>
        </div>

        {/* OLLAMA Configuration */}
        <div className="bg-white rounded-lg shadow mb-6 p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">OLLAMA Configuration</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                OLLAMA Server URL
              </label>
              <input
                type="text"
                value={ollamaUrl}
                onChange={(e) => setOllamaUrl(e.target.value)}
                placeholder="http://localhost:11434"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Model Name
              </label>
              <input
                type="text"
                value={ollamaModel}
                onChange={(e) => setOllamaModel(e.target.value)}
                placeholder="llama2"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Custom Extraction Prompt (Optional)
              </label>
              <textarea
                value={ollamaPrompt}
                onChange={(e) => setOllamaPrompt(e.target.value)}
                placeholder="Leave empty to use default prompt"
                rows={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div className="flex gap-2">
              <button
                onClick={testOllamaConnection}
                disabled={saving || testingOllama}
                className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:bg-gray-400"
              >
                {testingOllama ? 'Testing...' : 'Test Connection'}
              </button>
              <button
                onClick={saveOllamaConfig}
                disabled={saving}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
              >
                Save OLLAMA Config
              </button>
            </div>
          </div>
        </div>

        {/* MongoDB Configuration */}
        <div className="bg-white rounded-lg shadow mb-6 p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">MongoDB Configuration</h2>
          <p className="text-sm text-gray-600 mb-4">
            MongoDB is optional. Configure it to enable cloud synchronization of your records.
          </p>
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                MongoDB Connection URI
              </label>
              <input
                type="text"
                value={mongoUri}
                onChange={(e) => setMongoUri(e.target.value)}
                placeholder="mongodb+srv://user:password@cluster.mongodb.net/?retryWrites=true&w=majority"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              <p className="text-xs text-gray-500 mt-2">
                Example: mongodb+srv://username:password@cluster.mongodb.net/
              </p>
            </div>

            <div className="flex gap-2">
              <button
                onClick={testMongoDBConnection}
                disabled={saving || testingMongo || !mongoUri}
                className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:bg-gray-400"
              >
                {testingMongo ? 'Testing...' : 'Test Connection'}
              </button>
              <button
                onClick={saveMongoDBConfig}
                disabled={saving || !mongoUri}
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400"
              >
                Save MongoDB Config
              </button>
            </div>

            {mongoConfigured && (
              <div className="p-3 bg-blue-50 border border-blue-200 text-blue-800 rounded-md text-sm">
                ✓ MongoDB is configured and ready for synchronization
              </div>
            )}
          </div>
        </div>

        {/* Danger Zone */}
        <div className="bg-white rounded-lg shadow p-6 border-l-4 border-red-500">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">Danger Zone</h2>
          <button
            onClick={resetToDefaults}
            disabled={saving}
            className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 disabled:bg-gray-400"
          >
            Reset All Settings to Defaults
          </button>
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
