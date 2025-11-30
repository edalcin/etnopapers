"""Prompts for ethnobotanical metadata extraction"""

EXTRACTION_SYSTEM_PROMPT = """Você é um especialista em etnobotânica com experiência em análise de 
artigos científicos sobre plantas medicinais e tradicionais. Sua tarefa é extrair metadados 
estruturados de documentos etnobotânicos.

Siga estas regras:
1. Extraia informações com precisão do texto fornecido
2. Retorne APENAS JSON válido, sem texto adicional
3. Use null para campos não encontrados
4. Mantenha nomes científicos com capitalização correta
5. Padronize nomes de países para forma completa
6. Identifique corretamente tipos de uso (medicinal, alimentar, ritualistico, etc)
"""

EXTRACTION_USER_TEMPLATE = """Analise este documento etnobotânico:

{pdf_text}

Contexto do Pesquisador:
{researcher_context}

Extraia os metadados em JSON com esta estrutura:
{{
  "titulo": "string",
  "autores": ["string"],
  "ano": "int",
  "publicacao": "string",
  "doi": "string",
  "resumo": "string",
  "especies": [
    {{
      "vernacular": "string",
      "nomeCientifico": "string"
    }}
  ],
  "tipo_de_uso": "string",
  "metodologia": "string",
  "pais": "string",
  "estado": "string",
  "municipio": "string",
  "local": "string",
  "bioma": "string"
}}

Retorne APENAS o JSON válido.
"""

RESEARCHER_CONTEXT_TEMPLATE = """Nome: {name}
Especialização: {specialization}
Região de Interesse: {region}"""
