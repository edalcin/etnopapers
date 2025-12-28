namespace EtnoPapers.Core.Models;

/// <summary>
/// Enumeration of supported OpenAI API models for metadata extraction.
/// </summary>
public enum OpenAIModelType
{
    /// <summary>
    /// GPT-4o Mini - Fast and cost-effective model (recommended for most cases)
    /// </summary>
    Gpt4oMini = 0,

    /// <summary>
    /// GPT-4o - Most capable model with best reasoning
    /// </summary>
    Gpt4o = 1,

    /// <summary>
    /// GPT-4 Turbo - Previous generation powerful model
    /// </summary>
    Gpt4Turbo = 2
}
