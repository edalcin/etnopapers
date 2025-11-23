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
  titulo: string
  ano_publicacao: number
  autores: Author[]
  doi?: string
  resumo?: string
  status: 'rascunho' | 'finalizado'
}

export interface Species {
  vernacular: string
  nomeCientifico: string
}

export interface ExtractedMetadata {
  titulo: string
  autores: Author[]
  ano_publicacao: number
  resumo?: string
  doi?: string
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
}
