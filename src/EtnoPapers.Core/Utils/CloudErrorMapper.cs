using System.Net;

namespace EtnoPapers.Core.Utils;

/// <summary>
/// Maps cloud API HTTP errors to user-friendly Portuguese error messages.
/// </summary>
public static class CloudErrorMapper
{
    /// <summary>
    /// Maps HTTP status codes and exception types to Portuguese error messages.
    /// </summary>
    /// <param name="exception">The exception to map</param>
    /// <param name="httpStatusCode">Optional HTTP status code</param>
    /// <returns>User-friendly error message in Brazilian Portuguese</returns>
    public static string GetErrorMessage(Exception exception, HttpStatusCode? httpStatusCode = null)
    {
        if (exception == null)
            return "Erro desconhecido ao conectar com o provedor de IA";

        // Check for specific HTTP status codes
        if (httpStatusCode.HasValue)
        {
            return GetErrorMessageFromStatusCode(httpStatusCode.Value);
        }

        // Check exception type
        return exception switch
        {
            InvalidOperationException => "A chave de API não foi configurada. Verifique as configurações.",
            HttpRequestException httpEx => MapHttpRequestException(httpEx),
            TimeoutException => "Tempo limite excedido ao conectar com o provedor de IA",
            OperationCanceledException => "A operação foi cancelada",
            _ => "Erro ao extrair metadados. Tente novamente mais tarde."
        };
    }

    /// <summary>
    /// Maps HTTP status codes to Portuguese error messages.
    /// </summary>
    private static string GetErrorMessageFromStatusCode(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.Unauthorized => "Chave de API inválida ou expirada. Verifique suas configurações. Para Gemini, acesse https://ai.google.dev/ e copie sua chave.",
        HttpStatusCode.Forbidden => "Acesso negado ao provedor de IA. Verifique sua chave de API, permissões e limites de quota.",
        HttpStatusCode.NotFound => "Falha ao conectar com o modelo de IA. Verifique sua conexão de internet e se a chave de API está correta e ativa.",
        HttpStatusCode.TooManyRequests => "Limite de requisições excedido. Aguarde um momento e tente novamente.",
        HttpStatusCode.BadRequest => "Requisição inválida. Os dados do PDF podem estar corrompidos ou o formato não é suportado.",
        HttpStatusCode.InternalServerError => "O provedor de IA está indisponível no momento. Tente novamente em alguns instantes.",
        HttpStatusCode.BadGateway => "Erro de conexão com o provedor de IA. Verifique sua conexão de internet.",
        HttpStatusCode.ServiceUnavailable => "O provedor de IA está temporariamente indisponível.",
        HttpStatusCode.GatewayTimeout => "Tempo limite ao conectar com o provedor de IA.",
        _ => $"Erro da API: {statusCode:D} {statusCode:G}. Verifique sua chave de API e conexão de internet."
    };

    /// <summary>
    /// Maps HttpRequestException to a user-friendly message.
    /// </summary>
    private static string MapHttpRequestException(HttpRequestException exception)
    {
        if (exception.InnerException is TimeoutException)
            return "Tempo limite ao conectar com o provedor de IA";

        if (exception.Message.Contains("certificate", StringComparison.OrdinalIgnoreCase))
            return "Erro de certificado SSL ao conectar com o provedor de IA";

        if (exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) ||
            exception.Message.Contains("network", StringComparison.OrdinalIgnoreCase))
            return "Erro de conexão com o provedor de IA. Verifique sua conexão de internet.";

        return "Erro ao conectar com o provedor de IA. Verifique sua conexão de internet.";
    }

    /// <summary>
    /// Extracts HTTP status code from an exception if available.
    /// </summary>
    public static HttpStatusCode? TryExtractStatusCode(Exception exception)
    {
        return exception switch
        {
            HttpRequestException httpEx => httpEx.StatusCode,
            _ => null
        };
    }
}
