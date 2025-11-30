import { create } from 'zustand'

export interface ExtractedMetadata {
  id?: string
  titulo: string
  autores: string[]
  ano?: number
  publicacao?: string
  doi?: string
  resumo?: string
  especies: Array<{ vernacular: string; nomeCientifico: string }>
  tipo_de_uso?: string
  metodologia?: string
  pais?: string
  estado?: string
  municipio?: string
  local?: string
  bioma?: string
  status?: string
}

interface ExtractionState {
  extractedData: ExtractedMetadata | null
  isExtracting: boolean
  error: string | null
  
  setExtractedData: (data: ExtractedMetadata | null) => void
  setIsExtracting: (extracting: boolean) => void
  setError: (error: string | null) => void
  
  updateField: (field: keyof ExtractedMetadata, value: any) => void
  addSpecies: (species: { vernacular: string; nomeCientifico: string }) => void
  removeSpecies: (index: number) => void
  
  clearExtraction: () => void
}

export const useExtractionStore = create<ExtractionState>((set) => ({
  extractedData: null,
  isExtracting: false,
  error: null,
  
  setExtractedData: (data) => set({ extractedData: data }),
  setIsExtracting: (isExtracting) => set({ isExtracting }),
  setError: (error) => set({ error }),
  
  updateField: (field, value) =>
    set((state) => ({
      extractedData: state.extractedData
        ? { ...state.extractedData, [field]: value }
        : null
    })),
  
  addSpecies: (species) =>
    set((state) => ({
      extractedData: state.extractedData
        ? {
            ...state.extractedData,
            especies: [...(state.extractedData.especies || []), species]
          }
        : null
    })),
  
  removeSpecies: (index) =>
    set((state) => ({
      extractedData: state.extractedData
        ? {
            ...state.extractedData,
            especies: state.extractedData.especies.filter((_, i) => i !== index)
          }
        : null
    })),
  
  clearExtraction: () =>
    set({
      extractedData: null,
      isExtracting: false,
      error: null
    })
}))
