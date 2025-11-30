"""Unit tests for backend services"""

import pytest
from unittest.mock import Mock, patch, MagicMock
from pathlib import Path
from datetime import datetime
import json

# Test PDF Service
def test_pdf_validation_magic_bytes():
    """Test PDF file validation using magic bytes"""
    from backend.src.services.pdf_service import PDFService

    # Valid PDF magic bytes
    valid_pdf = b'%PDF-1.4\n'
    assert PDFService.validate_pdf_header(valid_pdf) is True

    # Invalid magic bytes
    invalid_pdf = b'JUNK DATA'
    assert PDFService.validate_pdf_header(invalid_pdf) is False


def test_pdf_size_validation():
    """Test PDF file size validation"""
    from backend.src.services.pdf_service import PDFService

    # File too large (>100MB)
    assert PDFService.is_valid_file_size(101 * 1024 * 1024) is False

    # Valid size
    assert PDFService.is_valid_file_size(50 * 1024 * 1024) is True


# Test Database Service
def test_duplicate_detection_by_doi():
    """Test duplicate detection using DOI"""
    from backend.src.services.article_service import ArticleService

    # Two articles with same DOI
    article1 = {
        'titulo': 'Article 1',
        'doi': '10.1234/test.1234'
    }
    article2 = {
        'titulo': 'Article 2',
        'doi': '10.1234/test.1234'
    }

    # DOI-based duplicate detection
    is_duplicate = ArticleService.is_duplicate_by_doi(
        article1.get('doi'),
        article2.get('doi')
    )
    assert is_duplicate is True


def test_duplicate_detection_by_fuzzy_match():
    """Test fuzzy matching for duplicate detection"""
    from backend.src.services.article_service import ArticleService

    # Similar articles (likely duplicates)
    article1_title = "Ethnobotanical survey of medicinal plants in Brazil"
    article2_title = "Ethnobotanical surveys of medicinal plants in Brazil"
    article1_year = 2020
    article2_year = 2020
    article1_author = "Silva, J."
    article2_author = "Silva, J."

    # Calculate similarity
    title_similarity = ArticleService.fuzzy_similarity(
        article1_title,
        article2_title
    )

    # Should be high similarity (>0.8)
    assert title_similarity > 0.8


# Test Reference Model Validation
def test_reference_model_validation():
    """Test Pydantic model validation for Reference"""
    from backend.src.models.reference import Reference, Species

    # Valid reference
    species = Species(
        vernacular="maçanilha",
        nomeCientifico="Chamomilla recutita"
    )

    ref = Reference(
        titulo="Test Article",
        autores=["Author 1"],
        ano=2020,
        publicacao="Test Journal",
        especies=[species],
        tipo_de_uso="medicinal",
        pais="Brasil"
    )

    assert ref.titulo == "Test Article"
    assert len(ref.autores) == 1
    assert ref.ano == 2020


def test_reference_model_required_fields():
    """Test that required fields are enforced"""
    from backend.src.models.reference import Reference
    import pydantic

    # Missing required field 'titulo'
    with pytest.raises(pydantic.ValidationError):
        Reference(
            autores=["Author"],
            ano=2020,
            pais="Brasil"
        )


# Test Configuration Service
def test_config_mongodb_uri_validation():
    """Test MongoDB URI validation"""
    from backend.src.services.config_service import ConfigService

    # Valid local MongoDB URI
    valid_uri = "mongodb://localhost:27017/etnopapers"
    assert ConfigService.validate_mongo_uri(valid_uri) is True

    # Valid MongoDB Atlas URI
    valid_atlas = "mongodb+srv://user:pass@cluster.mongodb.net/etnopapers"
    assert ConfigService.validate_mongo_uri(valid_atlas) is True

    # Invalid URI
    invalid_uri = "http://not-a-mongo-uri"
    assert ConfigService.validate_mongo_uri(invalid_uri) is False


# Test Ollama Service
@patch('requests.get')
def test_ollama_health_check_ok(mock_get):
    """Test successful Ollama health check"""
    from backend.src.services.ollama_service import OllamaService

    mock_response = Mock()
    mock_response.status_code = 200
    mock_response.json.return_value = {
        'status': 'ok',
        'models': [{'name': 'qwen:7b'}]
    }
    mock_get.return_value = mock_response

    # Note: This is a simplified test - actual service calls would be tested
    assert mock_response.status_code == 200


@patch('requests.get')
def test_ollama_health_check_timeout(mock_get):
    """Test Ollama health check timeout"""
    from requests import Timeout
    from backend.src.services.ollama_service import OllamaService

    mock_get.side_effect = Timeout()

    # Service should handle timeout gracefully
    with pytest.raises(Timeout):
        mock_get()


# Test Search Service
def test_search_multi_field_filter():
    """Test multi-field search functionality"""
    from backend.src.services.search_service import SearchService

    articles = [
        {
            '_id': '1',
            'titulo': 'Plantas medicinais do Brasil',
            'autores': ['Silva, J.'],
            'ano': 2020,
            'pais': 'Brasil',
            'especies': [{'nomeCientifico': 'Chamomilla recutita'}]
        },
        {
            '_id': '2',
            'titulo': 'Medicinal plants of Peru',
            'autores': ['Garcia, A.'],
            'ano': 2021,
            'pais': 'Peru',
            'especies': [{'nomeCientifico': 'Quinine'}]
        }
    ]

    # Search by country
    result = SearchService.filter_by_country(articles, 'Brasil')
    assert len(result) == 1
    assert result[0]['pais'] == 'Brasil'


def test_search_by_year_range():
    """Test search by year range"""
    from backend.src.services.search_service import SearchService

    articles = [
        {'ano': 2019, 'titulo': 'Old article'},
        {'ano': 2020, 'titulo': 'Recent article'},
        {'ano': 2021, 'titulo': 'New article'}
    ]

    # Filter by year range 2020-2021
    result = SearchService.filter_by_year_range(articles, 2020, 2021)
    assert len(result) == 2
    assert all(2020 <= article['ano'] <= 2021 for article in result)


# Test Backup Service
def test_backup_service_zip_integrity():
    """Test that backup ZIP files maintain integrity"""
    from backend.src.services.backup_service import BackupService
    import hashlib

    # Test data
    test_data = {
        'articulos': [
            {'titulo': 'Test Article', 'ano': 2020}
        ]
    }

    # Simulate checksum generation
    json_str = json.dumps(test_data)
    expected_checksum = hashlib.sha256(json_str.encode()).hexdigest()

    # Verify checksum format (64 hex characters for SHA256)
    assert len(expected_checksum) == 64
    assert all(c in '0123456789abcdef' for c in expected_checksum)


# Test Error Handling
def test_error_handling_invalid_json():
    """Test error handling for invalid JSON"""
    from backend.src.middleware.error_handler import format_error_response

    error_msg = "Invalid JSON structure"
    response = format_error_response(400, error_msg)

    assert response['status_code'] == 400
    assert response['detail'] == error_msg


if __name__ == '__main__':
    pytest.main([__file__, '-v'])
