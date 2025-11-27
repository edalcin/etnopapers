"""
Custom exceptions for Etnopapers backend

Provides structured error handling with Portuguese messages and resolution suggestions.
"""


class EtnopaperException(Exception):
    """Base exception for all Etnopapers errors"""

    def __init__(self, message: str, error_code: str = None, suggestion: str = None):
        """
        Initialize exception with message, code, and suggestion

        Args:
            message: Error message in Portuguese
            error_code: Structured error code (e.g., 'PDF_001')
            suggestion: User-friendly resolution suggestion
        """
        self.message = message
        self.error_code = error_code or "UNKNOWN"
        self.suggestion = suggestion or "Verifique a documentação ou contact suporte."
        super().__init__(self.message)

    def to_dict(self) -> dict:
        """Convert to JSON-serializable dict"""
        return {
            "error": self.__class__.__name__,
            "message": self.message,
            "code": self.error_code,
            "suggestion": self.suggestion,
        }


# PDF-related exceptions


class InvalidPDFError(EtnopaperException):
    """Raised when file is not a valid PDF"""

    def __init__(self, detail: str = ""):
        super().__init__(
            message=f"❌ Arquivo inválido: não é um PDF válido. {detail}",
            error_code="PDF_001",
            suggestion="Verifique se o arquivo é realmente um PDF e não está corrompido.",
        )


class PDFTooLargeError(EtnopaperException):
    """Raised when PDF exceeds size limits"""

    def __init__(self, size_mb: float = None):
        detail = f"({size_mb:.1f} MB)" if size_mb else ""
        super().__init__(
            message=f"❌ PDF muito grande {detail}. Máximo 50 MB.",
            error_code="PDF_002",
            suggestion="Reduza o tamanho do PDF (comprima ou divida em múltiplos arquivos).",
        )


class PDFCorruptedError(EtnopaperException):
    """Raised when PDF is corrupted or cannot be read"""

    def __init__(self, detail: str = ""):
        super().__init__(
            message=f"❌ PDF corrompido ou ilegível. {detail}",
            error_code="PDF_003",
            suggestion="Tente abrir o PDF em um leitor de PDF local. Se não abrir, está corrompido.",
        )


class ScannedPDFWarning(EtnopaperException):
    """Warning when PDF appears to be scanned (not an error, but quality alert)"""

    def __init__(self):
        super().__init__(
            message="⚠️  Este PDF parece ser escaneado. A qualidade da extração pode estar reduzida.",
            error_code="PDF_WARN_001",
            suggestion="Revise os dados extraídos com atenção especial. Considere usar OCR de melhor qualidade.",
        )


# Extraction exceptions


class OllamaTimeoutError(EtnopaperException):
    """Raised when Ollama inference times out"""

    def __init__(self, timeout_sec: int = 60):
        super().__init__(
            message=f"❌ Timeout na inferência de AI local (>{timeout_sec}s). O PDF pode ser muito longo.",
            error_code="OLLAMA_001",
            suggestion="Tente com um PDF menor ou aguarde mais tempo. Se persistir, reinicie o container.",
        )


class OllamaConnectionError(EtnopaperException):
    """Raised when cannot connect to Ollama service"""

    def __init__(self, url: str = ""):
        super().__init__(
            message=f"❌ Não consegue conectar ao Ollama {url}. Serviço pode estar desligado.",
            error_code="OLLAMA_002",
            suggestion="Verifique se o container Ollama está rodando. Reinicie se necessário.",
        )


class OllamaModelNotFoundError(EtnopaperException):
    """Raised when requested model is not available"""

    def __init__(self, model_name: str = ""):
        super().__init__(
            message=f"❌ Modelo '{model_name}' não está carregado no Ollama.",
            error_code="OLLAMA_003",
            suggestion=f"Execute: ollama pull {model_name}. Aguarde o download (~5-20 min).",
        )


# Extraction validation exceptions


class ExtractionValidationError(EtnopaperException):
    """Raised when extracted data fails validation"""

    def __init__(self, detail: str = ""):
        super().__init__(
            message=f"❌ Dados extraídos inválidos: {detail}",
            error_code="EXTRACT_001",
            suggestion="A resposta do AI não está no formato esperado. Tente novamente ou revise manualmente.",
        )


class MissingRequiredFieldError(EtnopaperException):
    """Raised when required field is missing from extraction"""

    def __init__(self, field_name: str = ""):
        super().__init__(
            message=f"❌ Campo obrigatório não extraído: '{field_name}'",
            error_code="EXTRACT_002",
            suggestion="Preencha manualmente ou tente extrair novamente.",
        )


class InvalidMetadataError(EtnopaperException):
    """Raised when metadata doesn't match schema"""

    def __init__(self, detail: str = ""):
        super().__init__(
            message=f"❌ Metadados inválidos: {detail}",
            error_code="EXTRACT_003",
            suggestion="Verifique o formato dos dados. Pode precisar de correção manual.",
        )


# Taxonomy exceptions


class TaxonomyValidationError(EtnopaperException):
    """Raised when taxonomic validation fails"""

    def __init__(self, species_name: str = ""):
        super().__init__(
            message=f"❌ Não consegue validar '{species_name}' no GBIF/Tropicos.",
            error_code="TAXONOMY_001",
            suggestion="Verifique o nome científico ou marque como 'não validado'. APIs podem estar lentas.",
        )


class TaxonomyTimeoutError(EtnopaperException):
    """Raised when taxonomy API times out"""

    def __init__(self):
        super().__init__(
            message="⚠️  Timeout validando nomes científicos. GBIF pode estar lento.",
            error_code="TAXONOMY_002",
            suggestion="Tente novamente em alguns minutos ou valide manualmente.",
        )


# Database exceptions


class DuplicateDetectionError(EtnopaperException):
    """Raised when duplicate detection fails"""

    def __init__(self, detail: str = ""):
        super().__init__(
            message=f"❌ Erro na detecção de duplicata: {detail}",
            error_code="DB_001",
            suggestion="Verifique a conexão com MongoDB. Tente novamente.",
        )


class DatabaseConnectionError(EtnopaperException):
    """Raised when cannot connect to MongoDB"""

    def __init__(self):
        super().__init__(
            message="❌ Não consegue conectar ao MongoDB. Banco de dados pode estar desligado.",
            error_code="DB_002",
            suggestion="Verifique MONGO_URI e se MongoDB está rodando. Reinicie se necessário.",
        )


# Rate limiting exceptions


class RateLimitExceededError(EtnopaperException):
    """Raised when rate limit is exceeded"""

    def __init__(self, retry_after_sec: int = 60):
        super().__init__(
            message=f"❌ Taxa de requisições excedida. Aguarde {retry_after_sec}s.",
            error_code="RATE_001",
            suggestion="Espere alguns segundos antes de tentar novamente.",
        )


# Generic server exceptions


class InternalServerError(EtnopaperException):
    """Raised for unexpected server errors"""

    def __init__(self, detail: str = ""):
        super().__init__(
            message=f"❌ Erro interno do servidor. {detail}",
            error_code="SERVER_001",
            suggestion="Verifique os logs do servidor. Contate suporte se persistir.",
        )
