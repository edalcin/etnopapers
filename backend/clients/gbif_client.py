"""
GBIF Species API Client

Integrates with GBIF to validate plant species names and get taxonomic information
https://www.gbif.org/developer/species
"""

import logging
import httpx
import asyncio
from typing import Optional, Dict, Any
from backend.config import settings

logger = logging.getLogger(__name__)


class GBIFClient:
    """Client for GBIF Species API"""

    BASE_URL = "https://api.gbif.org/v1/species"
    TIMEOUT = settings.TAXONOMY_API_TIMEOUT

    @staticmethod
    async def search_species(scientific_name: str) -> Optional[Dict[str, Any]]:
        """
        Search for a species in GBIF

        Args:
            scientific_name: Scientific name to search

        Returns:
            Species data if found, None otherwise
        """
        try:
            async with httpx.AsyncClient(timeout=GBIFClient.TIMEOUT) as client:
                response = await client.get(
                    f"{GBIFClient.BASE_URL}/search",
                    params={"q": scientific_name, "datasetKey": "d7dddbf4-2cf0-404f-9079-405b42bdf962"},
                )

                if response.status_code != 200:
                    logger.warning(
                        f"GBIF search failed for '{scientific_name}': {response.status_code}"
                    )
                    return None

                data = response.json()

                if not data.get("results") or len(data["results"]) == 0:
                    logger.debug(f"No GBIF results for '{scientific_name}'")
                    return None

                result = data["results"][0]
                return {
                    "nome_cientifico": result.get("scientificName", scientific_name),
                    "familia_botanica": result.get("family"),
                    "nome_aceito_atual": result.get("acceptedScientificName") or result.get("scientificName"),
                    "autores_nome_cientifico": result.get("authorship"),
                    "status_validacao": "validado",
                    "fonte_validacao": "GBIF",
                    "gbif_key": result.get("key"),
                    "rank": result.get("rank"),
                    "status": result.get("status"),
                }

        except asyncio.TimeoutError:
            logger.warning(f"GBIF timeout for '{scientific_name}'")
            return None
        except Exception as e:
            logger.error(f"GBIF API error: {e}")
            return None

    @staticmethod
    async def get_species_by_key(key: int) -> Optional[Dict[str, Any]]:
        """
        Get species details by GBIF key

        Args:
            key: GBIF species key

        Returns:
            Species data if found, None otherwise
        """
        try:
            async with httpx.AsyncClient(timeout=GBIFClient.TIMEOUT) as client:
                response = await client.get(f"{GBIFClient.BASE_URL}/{key}")

                if response.status_code != 200:
                    return None

                result = response.json()
                return {
                    "nome_cientifico": result.get("scientificName"),
                    "familia_botanica": result.get("family"),
                    "nome_aceito_atual": result.get("acceptedScientificName") or result.get("scientificName"),
                    "autores_nome_cientifico": result.get("authorship"),
                    "status_validacao": "validado",
                    "fonte_validacao": "GBIF",
                    "gbif_key": result.get("key"),
                    "rank": result.get("rank"),
                }

        except Exception as e:
            logger.error(f"GBIF API error: {e}")
            return None

    @staticmethod
    async def suggest_species(prefix: str) -> list:
        """
        Get species suggestions for autocomplete

        Args:
            prefix: Beginning of species name

        Returns:
            List of suggested species
        """
        try:
            async with httpx.AsyncClient(timeout=GBIFClient.TIMEOUT) as client:
                response = await client.get(
                    f"{GBIFClient.BASE_URL}/suggest",
                    params={"q": prefix},
                )

                if response.status_code != 200:
                    return []

                data = response.json()
                return data if isinstance(data, list) else []

        except Exception as e:
            logger.error(f"GBIF suggest error: {e}")
            return []
