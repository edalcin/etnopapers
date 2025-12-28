namespace EtnoPapers.Core.Models;

/// <summary>
/// Enumeration of supported Anthropic Claude API models for metadata extraction.
/// </summary>
public enum AnthropicModelType
{
    /// <summary>
    /// Claude 3.5 Sonnet - Balanced model with good performance (recommended)
    /// </summary>
    Claude35Sonnet = 0,

    /// <summary>
    /// Claude 3.5 Haiku - Fast and lightweight model
    /// </summary>
    Claude35Haiku = 1,

    /// <summary>
    /// Claude 3 Opus - Most capable model for complex tasks
    /// </summary>
    Claude3Opus = 2
}
