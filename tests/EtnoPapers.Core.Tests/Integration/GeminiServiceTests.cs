using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EtnoPapers.Core.Services;
using Newtonsoft.Json.Linq;

namespace EtnoPapers.Core.Tests.Integration;

/// <summary>
/// Integration tests for GeminiService.
/// Tests API request/response handling with mocked HTTP client.
/// </summary>
public class GeminiServiceTests
{
    private const string TestApiKey = "test-gemini-api-key-12345";
    private const string TestPdfText = "Sample PDF text content for testing";

    private static readonly string ValidGeminiResponse = @"{
        ""candidates"": [{
            ""content"": {
                ""parts"": [{
                    ""text"": ""{\""titulo\"": \""Test Article\"", \""autores\"": [\""Smith, J.\""], \""ano\"": 2023, \""resumo\"": \""Este é um resumo de teste.\""}""
                }]
            }
        }]
    }";

    [Fact]
    public async Task ExtractMetadata_WithValidResponse_ReturnsJson()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, ValidGeminiResponse);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new GeminiService(httpClient);
        service.SetApiKey(TestApiKey);

        // Act
        var result = await service.ExtractMetadataAsync(TestPdfText);

        // Assert
        Assert.NotNull(result);
        var parsed = JObject.Parse(result);
        Assert.NotNull(parsed["titulo"]);
        Assert.Equal("Test Article", parsed["titulo"]?.ToString());
        Assert.NotNull(parsed["autores"]);
        Assert.NotNull(parsed["ano"]);
        Assert.NotNull(parsed["resumo"]);
    }

    [Fact]
    public async Task ExtractMetadata_With401Error_ThrowsWithMessage()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.Unauthorized, @"{""error"": ""Invalid API key""}");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new GeminiService(httpClient);
        service.SetApiKey(TestApiKey);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExtractMetadataAsync(TestPdfText));

        Assert.Contains("Chave de API inválida", ex.Message);
    }

    [Fact]
    public async Task ExtractMetadata_With429Error_ThrowsWithRateLimitMessage()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.TooManyRequests, @"{""error"": ""Rate limit exceeded""}");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new GeminiService(httpClient);
        service.SetApiKey(TestApiKey);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExtractMetadataAsync(TestPdfText));

        Assert.Contains("Limite de requisições excedido", ex.Message);
    }

    [Fact]
    public async Task ExtractMetadata_With500Error_ThrowsWithServerErrorMessage()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.InternalServerError, @"{""error"": ""Internal server error""}");
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new GeminiService(httpClient);
        service.SetApiKey(TestApiKey);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExtractMetadataAsync(TestPdfText));

        Assert.Contains("provedor de IA está indisponível", ex.Message);
    }

    [Fact]
    public async Task ExtractMetadata_WithMalformedResponse_ThrowsException()
    {
        // Arrange - Response missing required fields
        var malformedResponse = @"{""candidates"": []}";
        var mockHandler = CreateMockHttpHandler(HttpStatusCode.OK, malformedResponse);
        var httpClient = new HttpClient(mockHandler.Object);
        var service = new GeminiService(httpClient);
        service.SetApiKey(TestApiKey);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExtractMetadataAsync(TestPdfText));

        Assert.Contains("No candidates", ex.Message);
    }

    [Fact]
    public void ExtractMetadata_WithoutApiKey_ThrowsException()
    {
        // Arrange
        var service = new GeminiService();

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExtractMetadataAsync(TestPdfText));
    }

    [Fact]
    public async Task ExtractMetadata_WithEmptyPdfText_ThrowsArgumentException()
    {
        // Arrange
        var service = new GeminiService();
        service.SetApiKey(TestApiKey);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExtractMetadataAsync(""));
    }

    private static Mock<HttpMessageHandler> CreateMockHttpHandler(HttpStatusCode statusCode, string responseContent)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });

        return mockHandler;
    }
}
