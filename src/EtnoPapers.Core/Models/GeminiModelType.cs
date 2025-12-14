namespace EtnoPapers.Core.Models;

/// <summary>
/// Enumeration of supported Google Gemini API models for metadata extraction.
/// </summary>
public enum GeminiModelType
{
    /// <summary>
    /// Gemini 2.5 Flash - Fast and efficient model (recommended for most cases)
    /// </summary>
    Flash = 0,

    /// <summary>
    /// Gemini 2.5 Pro - More powerful model with better reasoning (slower)
    /// </summary>
    Pro = 1
}
