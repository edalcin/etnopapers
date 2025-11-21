import { useEffect } from 'react'
import { useStore } from '@store/useStore'
import { ExtractedMetadata } from '@types'

export const useAutoSaveDraft = (metadata: ExtractedMetadata | null) => {
  const setCurrentDraft = useStore((state) => state.setCurrentDraft)

  useEffect(() => {
    if (!metadata) return
    const interval = setInterval(() => {
      setCurrentDraft(metadata)
    }, 5000)
    return () => clearInterval(interval)
  }, [metadata, setCurrentDraft])
}
