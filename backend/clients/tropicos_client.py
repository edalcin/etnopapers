"""
Tropicos API Client

Integrates with Tropicos (Missouri Botanical Garden) API as fallback for taxonomy validation
https://www.tropicos.org/
"""

import asyncio
import logging
from typing import Any
from typing import Dict
from typing import Optional

import httpx

from backend.config import settings

logger = logging.getLogger(__name__)


class TropicosClient:
    """Client for Tropicos API"""

    BASE_URL = "https://tropicos.org/webservices/v1"
    TIMEOUT = settings.TAXONOMY_API_TIMEOUT

    @staticmethod
    async def search_species(scientific_name: str, api_key: Optional[str] = None) -> Optional[Dict[str, Any]]:
        """
        Search for a species in Tropicos

        Args:
            scientific_name: Scientific name to search
            api_key: Tropicos API key (optional)

        Returns:
            Species data if found, None otherwise
        """
        try:
            # Note: Tropicos API key is optional for basic searches
            async with httpx.AsyncClient(timeout=TropicosClient.TIMEOUT) as client:
                params = {"name": scientific_name}
                if api_key:
                    params["apikey"] = api_key

                response = await client.get(
                    f"{TropicosClient.BASE_URL}/search",
                    params=params,
                )

                if response.status_code != 200:
                    logger.warning(
                        f"Tropicos search failed for '{scientific_name}': {response.status_code}"
                    )
                    return None

                data = response.json()

                if not data or len(data) == 0:
                    logger.debug(f"No Tropicos results for '{scientific_name}'")
                    return None

                result = data[0]
                return {
                    "nome_cientifico": result.get("ScientificName", scientific_name),
                    "familia_botanica": result.get("Family"),
                    "nome_aceito_atual": result.get("AcceptedScientificName") or result.get("ScientificName"),
                    "autores_nome_cientifico": result.get("Authors"),
                    "status_validacao": "validado",
                    "fonte_validacao": "Tropicos",
                    "tropicos_id": result.get("StampId"),
                    "unresolved": result.get("Unresolved"),
                }

        except asyncio.TimeoutError:
            logger.warning(f"Tropicos timeout for '{scientific_name}'")
            return None
        except Exception as e:
            logger.error(f"Tropicos API error: {e}")
            return None

    @staticmethod
    async def get_details(organism_id: int, api_key: Optional[str] = None) -> Optional[Dict[str, Any]]:
        """
        Get details about an organism in Tropicos

        Args:
            organism_id: Tropicos organism ID
            api_key: Tropicos API key (optional)

        Returns:
            Species data if found, None otherwise
        """
        try:
            params = {}
            if api_key:
                params["apikey"] = api_key

            async with httpx.AsyncClient(timeout=TropicosClient.TIMEOUT) as client:
                response = await client.get(
                    f"{TropicosClient.BASE_URL}/organisms/{organism_id}",
                    params=params,
                )

                if response.status_code != 200:
                    return None

                result = response.json()
                return {
                    "nome_cientifico": result.get("ScientificName"),
                    "familia_botanica": result.get("Family"),
                    "nome_aceito_atual": result.get("AcceptedScientificName") or result.get("ScientificName"),
                    "autores_nome_cientifico": result.get("Authors"),
                    "status_validacao": "validado",
                    "fonte_validacao": "Tropicos",
                    "tropicos_id": result.get("StampId"),
                }

        except Exception as e:
            logger.error(f"Tropicos details error: {e}")
            return None
