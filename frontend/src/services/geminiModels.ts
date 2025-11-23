/**
 * Gemini Models Service
 * Handles listing and validating available Gemini models
 */

export interface GeminiModel {
  name: string
  displayName: string
  description: string
}

const GEMINI_MODELS: GeminiModel[] = [
  {
    name: 'gemini-pro',
    displayName: 'Gemini Pro',
    description: 'Fast and accurate model for text analysis'
  },
  {
    name: 'gemini-pro-vision',
    displayName: 'Gemini Pro Vision',
    description: 'Supports both text and images'
  },
]

export const listGeminiModels = (): GeminiModel[] => {
  return GEMINI_MODELS
}

export const getDefaultGeminiModel = (): string => {
  return 'gemini-pro'
}

export const validateGeminiModel = async (
  apiKey: string,
  modelName: string
): Promise<boolean> => {
  try {
    const response = await fetch(
      `https://generativelanguage.googleapis.com/v1/models/${modelName}?key=${apiKey}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      }
    )

    return response.ok
  } catch {
    return false
  }
}

export const listAvailableGeminiModels = async (apiKey: string): Promise<GeminiModel[]> => {
  try {
    const response = await fetch(
      `https://generativelanguage.googleapis.com/v1/models?key=${apiKey}`,
      {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      }
    )

    if (!response.ok) {
      return GEMINI_MODELS
    }

    const data = await response.json()
    
    // Filter for generation models that support generateContent
    const generationModels = data.models
      ?.filter((m: any) => m.supportedGenerationMethods?.includes('generateContent'))
      .map((m: any) => ({
        name: m.name.replace('models/', ''),
        displayName: m.displayName || m.name,
        description: m.description || 'Gemini model',
      })) || GEMINI_MODELS

    return generationModels.length > 0 ? generationModels : GEMINI_MODELS
  } catch {
    return GEMINI_MODELS
  }
}
