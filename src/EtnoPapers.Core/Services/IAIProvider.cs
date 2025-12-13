namespace EtnoPapers.Core.Services;

/// <summary>
/// Interface for cloud AI provider metadata extraction services.
/// Implemented by Gemini, OpenAI, and Anthropic providers.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Extracts ethnobotanical metadata from PDF text content.
    /// </summary>
    /// <param name="pdfText">Extracted text from the PDF file</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>JSON string with extracted metadata matching estrutura.json schema</returns>
    /// <throws>InvalidOperationException if API key is not configured</throws>
    /// <throws>HttpRequestException for network errors</throws>
    /// <throws>OperationCanceledException if cancelled</throws>
    Task<string> ExtractMetadataAsync(string pdfText, CancellationToken cancellationToken = default);
}
