namespace EtnoPapers.Core.Services;

/// <summary>
/// Interface for cloud AI provider metadata extraction services.
/// Implemented by Gemini, OpenAI, and Anthropic providers.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Sets the API key for authentication.
    /// </summary>
    /// <param name="apiKey">API key for the cloud provider</param>
    void SetApiKey(string apiKey);

    /// <summary>
    /// Tests connection to the provider API without extracting metadata.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if connection test succeeds, false otherwise</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts ethnobotanical metadata from PDF text content.
    /// </summary>
    /// <param name="pdfText">Extracted text from the PDF file</param>
    /// <param name="customPrompt">Optional custom extraction prompt. If null/empty, uses default prompt.</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>JSON string with extracted metadata matching estrutura.json schema</returns>
    /// <throws>InvalidOperationException if API key is not configured</throws>
    /// <throws>HttpRequestException for network errors</throws>
    /// <throws>OperationCanceledException if cancelled</throws>
    Task<string> ExtractMetadataAsync(string pdfText, string customPrompt = null, CancellationToken cancellationToken = default);
}
