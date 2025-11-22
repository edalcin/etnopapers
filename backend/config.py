"""
Configuration management for Etnopapers backend
"""

import logging
import os
from typing import Optional

logger = logging.getLogger(__name__)


class Settings:
    """Application settings from environment variables"""

    # Server
    PORT: int = int(os.getenv("PORT", 8000))
    HOST: str = os.getenv("HOST", "0.0.0.0")
    ENVIRONMENT: str = os.getenv("ENVIRONMENT", "development")

    # Database (MongoDB)
    # Connection URI MUST be provided via MONGO_URI environment variable
    # No local connections allowed. Examples:
    #   - mongodb+srv://user:pass@cluster.mongodb.net/etnopapers (MongoDB Atlas)
    #   - mongodb+srv://user:pass@mongodb-server/etnopapers (Self-hosted with SRV)
    # IMPORTANT: MONGO_URI environment variable is required
    MONGO_URI: str = os.getenv("MONGO_URI", "")

    # Logging
    LOG_LEVEL: str = os.getenv("LOG_LEVEL", "info").upper()

    # Taxonomy API
    TAXONOMY_API_TIMEOUT: int = int(os.getenv("TAXONOMY_API_TIMEOUT", 5))
    CACHE_TTL_DAYS: int = int(os.getenv("CACHE_TTL_DAYS", 30))

    # CORS
    CORS_ORIGINS: list = (
        os.getenv("CORS_ORIGINS", "http://localhost:3000,http://localhost:8000")
        .split(",")
    )

    # Security
    SECRET_KEY: Optional[str] = os.getenv("SECRET_KEY")

    # Rate limiting
    RATE_LIMIT_PER_MINUTE: int = 60
    RATE_LIMIT_DOWNLOAD_PER_MINUTE: int = 1

    # File upload
    MAX_UPLOAD_SIZE_MB: int = 50
    MAX_UPLOAD_SIZE_BYTES: int = MAX_UPLOAD_SIZE_MB * 1024 * 1024

    # Pagination
    DEFAULT_PAGE_SIZE: int = 50
    MAX_PAGE_SIZE: int = 500

    @classmethod
    def get_log_level(cls):
        """Get logging level"""
        levels = {
            "DEBUG": logging.DEBUG,
            "INFO": logging.INFO,
            "WARNING": logging.WARNING,
            "ERROR": logging.ERROR,
            "CRITICAL": logging.CRITICAL,
        }
        return levels.get(cls.LOG_LEVEL, logging.INFO)

    @classmethod
    def is_production(cls) -> bool:
        """Check if running in production"""
        return cls.ENVIRONMENT == "production"

# Initialize settings
settings = Settings()


# Configure logging
def setup_logging():
    """Setup logging configuration"""
    logging.basicConfig(
        level=settings.get_log_level(),
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    )
    logger.info(f"Logging configured at {settings.LOG_LEVEL} level")
    logger.info(f"Environment: {settings.ENVIRONMENT}")
    logger.info(f"Database: MongoDB (via MONGO_URI)")


# Setup on module load
setup_logging()
