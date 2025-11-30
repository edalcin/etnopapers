import json
import logging
from typing import Optional, Dict, Any
import httpx
import asyncio
from ..models.reference import Reference, ReferenceCreate
from ..models.species import SpeciesData

logger = logging.getLogger(__name__)

class ExtractionService:
    """Service for metadata extraction using Ollama"""
    
    @staticmethod
    async def extract_metadata(
        pdf_text: str,
        researcher_profile: Optional[Dict[str, Any]] = None,
        ollama_url: str = "http://localhost:11434",
        model: str = "qwen2.5:7b-instruct-q4_K_M"
    ) -> ReferenceCreate:
        """Extract ethnobotanical metadata from PDF text using Ollama"""
        
        # Build extraction prompt
        prompt = ExtractionService._build_extraction_prompt(pdf_text, researcher_profile)
        
        try:
            # Call Ollama API
            async with httpx.AsyncClient(timeout=300.0) as client:
                response = await client.post(
                    f"{ollama_url}/api/generate",
                    json={
                        "model": model,
                        "prompt": prompt,
                        "stream": False,
                        "temperature": 0.3,
                        "num_predict": 2000
                    }
                )
                
                if response.status_code != 200:
                    raise Exception(f"Ollama error: {response.text}")
                
                result = response.json()
                response_text = result.get('response', '')
                
                # Parse JSON response
                metadata = ExtractionService._parse_extraction_response(response_text)
                return metadata
                
        except asyncio.TimeoutError:
            raise Exception("Timeout ao processar no Ollama - PDF muito grande")
        except Exception as e:
            logger.error(f"Erro ao extrair metadados: {e}")
            raise
    
    @staticmethod
    def _build_extraction_prompt(pdf_text: str, researcher_profile: Optional[Dict] = None) -> str:
        """Build the extraction prompt for Ollama"""
        
        context = ""
        if researcher_profile:
            context = f"""
Contexto do Pesquisador:
- Nome: {researcher_profile.get('name', 'N/A')}
- Especializacao: {researcher_profile.get('specialization', 'N/A')}
- Regiao: {researcher_profile.get('region', 'N/A')}

"""
        
        prompt = f"""Voce é um especialista em etnobotânica. Analise o seguinte documento e extraia os metadados de pesquisa etnobotânica em formato JSON.

{context}

Documento:
{pdf_text[:5000]}

Extraia e retorne um JSON válido com a seguinte estrutura, preenchendo com null se nao encontrado:
{{
  "titulo": "titulo do artigo",
  "autores": ["autor1", "autor2"],
  "ano": 2020,
  "publicacao": "nome da revista/publicacao",
  "doi": "DOI se disponivel",
  "resumo": "resumo/abstract",
  "especies": [
    {{
      "vernacular": "nome comum",
      "nomeCientifico": "nome cientifico"
    }}
  ],
  "tipo_de_uso": "medicinal, alimentar, ritualistico, etc",
  "metodologia": "entrevistas, observacao, experimental, etc",
  "pais": "pais da pesquisa",
  "estado": "estado/provincia",
  "municipio": "municipio",
  "local": "comunidade ou local especifico",
  "bioma": "bioma ou ecosistema"
}}

Retorne APENAS o JSON válido, sem texto adicional.
"""
        
        return prompt
    
    @staticmethod
    def _parse_extraction_response(response_text: str) -> ReferenceCreate:
        """Parse JSON response from Ollama"""
        try:
            # Try to extract JSON from response
            json_start = response_text.find('{')
            json_end = response_text.rfind('}') + 1
            
            if json_start >= 0 and json_end > json_start:
                json_str = response_text[json_start:json_end]
                data = json.loads(json_str)
            else:
                raise ValueError("No JSON found in response")
            
            # Convert species data
            especies = []
            if data.get('especies'):
                for species in data.get('especies', []):
                    if species.get('nomeCientifico'):
                        especies.append(SpeciesData(
                            vernacular=species.get('vernacular', ''),
                            nomeCientifico=species.get('nomeCientifico', '')
                        ))
            
            # Create ReferenceCreate object
            reference = ReferenceCreate(
                titulo=data.get('titulo', 'Sem titulo'),
                autores=data.get('autores', []),
                ano=data.get('ano'),
                publicacao=data.get('publicacao'),
                doi=data.get('doi'),
                resumo=data.get('resumo'),
                especies=especies,
                tipo_de_uso=data.get('tipo_de_uso'),
                metodologia=data.get('metodologia'),
                pais=data.get('pais'),
                estado=data.get('estado'),
                municipio=data.get('municipio'),
                local=data.get('local'),
                bioma=data.get('bioma'),
                status='rascunho'
            )
            
            return reference
            
        except json.JSONDecodeError as e:
            logger.error(f"Erro ao fazer parsing do JSON: {e}")
            raise ValueError(f"Resposta invalida do Ollama: {str(e)}")
