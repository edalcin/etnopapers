// AI Provider types
export type AIProvider = 'gemini' | 'openai' | 'claude'

export interface APIKeyConfig {
  provider: AIProvider
  key: string
  isValid: boolean
}

// Article types
export interface Author {
  nome: string
  sobrenome: string
  email?: string
}

export interface Article {
  _id: string
  id?: string
  titulo: string
  ano: number
  ano_publicacao?: number // For backward compatibility
  autores: string[] | Author[]
  publicacao?: string
  doi?: string
  resumo?: string
  especies?: Species[]
  tipoUso?: string
  metodologia?: string
  pais?: string
  estado?: string
  municipio?: string
  local?: string
  bioma?: string
  status?: 'rascunho' | 'finalizado'
}

// Clarifications 2025-11-27: Enhanced species structure with usosPorComunidade
export interface ComunidadeUso {
  nome: string // Nome da comunidade
  tipo: string // Tipo: indígena, quilombola, ribeirinha, caiçara, outro
  país: string
  estado: string
  município: string
}

export interface UsoEspeciePorComunidade {
  comunidade: ComunidadeUso // Inline denormalized object
  formaDeUso?: string // Forma: chá, pó, óleo, infusão, decocção, cataplasma, tinctura, banho
  tipoDeUso?: string // Tipo: medicinal, alimentar, ritual, cosmético, construção
  propositoEspecifico?: string // Propósito: febre, tosse, digestão, analgésico, etc
  partesUtilizadas?: string[] // Partes: folhas, raízes, cascas, flores, sementes
  dosagem?: string // Dosagem (opcional): quantidade/dosagem se mencionada
  metodoPreparacao?: string // Método de preparação (opcional)
  origem?: string // Origem da informação (opcional)
}

export interface Species {
  vernacular: string // Nome comum
  nomeCientifico: string // Nome científico
  familia?: string // Família botânica
  nomeAceitoValidado?: string // Nome aceito atual (se validado)
  statusValidacao?: 'validado' | 'naoValidado' // Resultado da consulta API (Clarificação Q2)
  confianca?: 'alta' | 'media' | 'baixa' // Qualidade do match (Clarificação Q2)
  usosPorComunidade?: UsoEspeciePorComunidade[] // Uso detalhado por comunidade (Clarificação Q1, Q3)
}

export interface ExtractedMetadata {
  titulo: string
  autores: Author[]
  ano_publicacao: number
  resumo?: string
  doi?: string
  publicacao?: string
  especies?: Species[]
  regioes?: string[]
  comunidades?: string[]
  tipo_de_uso?: string
  metodologia?: string
  pais?: string
  estado?: string
  municipio?: string
  local?: string
  bioma?: string
  periodoEstudo?: {
    dataInicio?: string
    dataFim?: string
  }
}
