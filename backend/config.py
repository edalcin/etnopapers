"""
Configuration management for Etnopapers backend
"""

import os
import logging
from typing import Optional
from pathlib import Path

logger = logging.getLogger(__name__)


class Settings:
    """Application settings from environment variables"""

    # Server
    PORT: int = int(os.getenv("PORT", 8000))
    HOST: str = os.getenv("HOST", "0.0.0.0")
    ENVIRONMENT: str = os.getenv("ENVIRONMENT", "development")

    # Database (Mongita)
    DATABASE_PATH: str = os.getenv("DATABASE_PATH", "data/etnopapers")
    DATABASE_BACKEND: str = os.getenv("DATABASE_BACKEND", "disk")  # disk or memory

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

    @classmethod
    def ensure_database_dir(cls):
        """Ensure database directory exists"""
        # For Mongita, DATABASE_PATH is a directory, not a file
        db_path = Path(cls.DATABASE_PATH)
        db_path.mkdir(parents=True, exist_ok=True)


# Initialize settings
settings = Settings()
settings.ensure_database_dir()


# Configure logging
def setup_logging():
    """Setup logging configuration"""
    logging.basicConfig(
        level=settings.get_log_level(),
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    )
    logger.info(f"Logging configured at {settings.LOG_LEVEL} level")
    logger.info(f"Environment: {settings.ENVIRONMENT}")
    logger.info(f"Database: {settings.DATABASE_PATH}")


# Setup on module load
setup_logging()
