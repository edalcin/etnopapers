import { useEffect, useRef, useCallback } from 'react'
import { useStore } from '@store/useStore'
import type { ExtractedMetadata, DraftState } from '@types'

const AUTOSAVE_DELAY = 3000 // Save after 3 seconds of inactivity

interface UseAutoSaveDraftOptions {
  enabled: boolean
  onSaved?: () => void
  onError?: (error: Error) => void
}

export function useAutoSaveDraft(
  data: ExtractedMetadata | null,
  options: UseAutoSaveDraftOptions = { enabled: true }
) {
  const { enabled = true, onSaved, onError } = options
  const addDraft = useStore(state => state.addDraft)
  const timeoutRef = useRef<NodeJS.Timeout | null>(null)
  const lastSavedRef = useRef<string>('')

  const saveDraft = useCallback(() => {
    if (!data || !enabled) return

    try {
      const dataString = JSON.stringify(data)

      // Only save if data has changed
      if (dataString === lastSavedRef.current) {
        return
      }

      lastSavedRef.current = dataString

      const draft: DraftState = {
        data,
        savedAt: new Date().toISOString(),
      }

      addDraft(draft)
      onSaved?.()

      console.log('Draft auto-saved')
    } catch (error) {
      const err = error instanceof Error ? error : new Error('Failed to save draft')
      onError?.(err)
      console.error('Draft auto-save failed:', err)
    }
  }, [data, enabled, addDraft, onSaved, onError])

  useEffect(() => {
    if (!enabled || !data) return

    // Clear existing timeout
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }

    // Set new timeout for auto-save
    timeoutRef.current = setTimeout(saveDraft, AUTOSAVE_DELAY)

    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
    }
  }, [data, enabled, saveDraft])

  // Save on unmount if there are unsaved changes
  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
      // Perform final save
      if (data && enabled) {
        saveDraft()
      }
    }
  }, [data, enabled, saveDraft])

  return {
    saveDraft,
    isDraft: !!lastSavedRef.current,
  }
}

export function getRecentDraft(): ExtractedMetadata | null {
  try {
    const stored = localStorage.getItem('etnopapers_drafts')
    if (!stored) return null

    const state = JSON.parse(stored)
    if (!state.drafts || state.drafts.length === 0) return null

    // Return most recent draft
    const latestDraft = state.drafts[state.drafts.length - 1]
    return latestDraft.data
  } catch (error) {
    console.error('Failed to recover draft:', error)
    return null
  }
}

export function clearAllDrafts(): void {
  try {
    const stored = localStorage.getItem('etnopapers_drafts')
    if (stored) {
      const state = JSON.parse(stored)
      state.drafts = []
      localStorage.setItem('etnopapers_drafts', JSON.stringify(state))
    }
  } catch (error) {
    console.error('Failed to clear drafts:', error)
  }
}
