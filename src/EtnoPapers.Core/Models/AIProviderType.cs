namespace EtnoPapers.Core.Models;

/// <summary>
/// Enumeration of supported cloud AI providers for ethnobotanical metadata extraction.
/// </summary>
public enum AIProviderType
{
    /// <summary>
    /// Google Gemini API (generativelanguage.googleapis.com)
    /// </summary>
    Gemini = 0,

    /// <summary>
    /// OpenAI API (api.openai.com)
    /// </summary>
    OpenAI = 1,

    /// <summary>
    /// Anthropic Claude API (api.anthropic.com)
    /// </summary>
    Anthropic = 2
}
