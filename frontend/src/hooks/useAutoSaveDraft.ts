import { useEffect } from 'react'
import { useSetCurrentDraft } from '@store/useStore'
import type { ExtractedMetadata } from '@types'

const DRAFT_STORAGE_KEY = 'etnopapers_draft'
const DRAFT_TIMESTAMP_KEY = 'etnopapers_draft_timestamp'

export const useAutoSaveDraft = (metadata: ExtractedMetadata | null) => {
  const setCurrentDraft = useSetCurrentDraft()

  useEffect(() => {
    if (!metadata) return
    const interval = setInterval(() => {
      saveDraft(metadata)
      setCurrentDraft(metadata)
    }, 5000)
    return () => clearInterval(interval)
  }, [metadata, setCurrentDraft])
}

export const saveDraft = (metadata: ExtractedMetadata) => {
  try {
    localStorage.setItem(DRAFT_STORAGE_KEY, JSON.stringify(metadata))
    localStorage.setItem(DRAFT_TIMESTAMP_KEY, new Date().toISOString())
  } catch (error) {
    console.error('Failed to save draft:', error)
  }
}

export const getRecentDraft = (): ExtractedMetadata | null => {
  try {
    const draftStr = localStorage.getItem(DRAFT_STORAGE_KEY)
    return draftStr ? JSON.parse(draftStr) : null
  } catch (error) {
    console.error('Failed to retrieve draft:', error)
    return null
  }
}

export const clearAllDrafts = () => {
  try {
    localStorage.removeItem(DRAFT_STORAGE_KEY)
    localStorage.removeItem(DRAFT_TIMESTAMP_KEY)
  } catch (error) {
    console.error('Failed to clear drafts:', error)
  }
}

export const getDraftTimestamp = (): string | null => {
  try {
    return localStorage.getItem(DRAFT_TIMESTAMP_KEY)
  } catch {
    return null
  }
}
