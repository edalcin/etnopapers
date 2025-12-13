using EtnoPapers.Core.Models;
using System.Net.Http;

namespace EtnoPapers.Core.Services;

/// <summary>
/// Factory for creating AI provider service instances based on configuration.
/// Supports Gemini, OpenAI, and Anthropic providers.
/// </summary>
public static class AIProviderFactory
{
    /// <summary>
    /// Creates an AI provider service instance based on the specified type.
    /// </summary>
    /// <param name="providerType">The type of AI provider to create</param>
    /// <param name="httpClient">Optional HttpClient instance for testing/injection</param>
    /// <returns>An IAIProvider implementation for the specified type</returns>
    /// <exception cref="InvalidOperationException">Thrown when provider type is not supported</exception>
    public static IAIProvider CreateProvider(AIProviderType providerType, HttpClient? httpClient = null)
    {
        return providerType switch
        {
            AIProviderType.Gemini => new GeminiService(httpClient),
            AIProviderType.OpenAI => new OpenAIService(httpClient),
            AIProviderType.Anthropic => new AnthropicService(httpClient),
            _ => throw new InvalidOperationException($"Unsupported AI provider type: {providerType}")
        };
    }
}
