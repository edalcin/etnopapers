import { useState, useEffect } from 'react'
import PDFUpload from '@components/PDFUpload'
import MetadataDisplay from '@components/MetadataDisplay'
import ManualEditor from '@components/ManualEditor'
import APIKeySetup from '@components/APIKeySetup'
import { extractTextFromPDF } from '@services/pdfExtractor'
import { extractWithGemini } from '@services/ai/geminiClient'
import { extractWithOpenAI } from '@services/ai/openaiClient'
import { extractWithClaude } from '@services/ai/claudeClient'
import { useAPIKey, useSetExtractedData, useSetExtractLoading, useSetExtractError, useAddArticle } from '@store/useStore'
import { articlesAPI } from '@services/api'
import { useAutoSaveDraft, getRecentDraft, clearAllDrafts } from '@hooks/useAutoSaveDraft'
import type { ExtractedMetadata } from '@types'
import './Upload.css'

export default function Upload() {
  const apiKey = useAPIKey()
  const setExtractedData = useSetExtractedData()
  const setExtractLoading = useSetExtractLoading()
  const setExtractError = useSetExtractError()
  const addArticle = useAddArticle()

  const [step, setStep] = useState<'upload' | 'config' | 'extract' | 'review' | 'edit'>('upload')
  const [pdfText, setPdfText] = useState<string>('')
  const [extractedMetadata, setExtractedMetadata] = useState<ExtractedMetadata | null>(null)
  const [isScanned, setIsScanned] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [duplicateWarning, setDuplicateWarning] = useState<any | null>(null)
  const [showRecoverDraft, setShowRecoverDraft] = useState(false)
  const [hasDraft, setHasDraft] = useState(false)

  // Auto-save drafts
  useAutoSaveDraft(extractedMetadata, {
    enabled: step === 'review' || step === 'edit',
  })

  // Check for recovered draft on mount
  useEffect(() => {
    const recentDraft = getRecentDraft()
    if (recentDraft) {
      setHasDraft(true)
      setShowRecoverDraft(true)
    }
  }, [])

  // Clear upload state when returning to upload step
  useEffect(() => {
    if (step === 'upload') {
      setPdfText('')
      setExtractedMetadata(null)
      setIsScanned(false)
      setError(null)
    }
  }, [step])


  const handleUpload = async (file: File) => {
    try {
      setError(null)
      setStep('extract')
      setExtractLoading(true)

      // Extract text from PDF
      const result = await extractTextFromPDF(file, (progress) => {
        console.log(`PDF extraction: ${progress.percentage}%`)
      })

      setPdfText(result.text)
      setIsScanned(result.isScanned)

      // Check if API key is configured
      if (!apiKey || !apiKey.key || !apiKey.isValid) {
        setExtractLoading(false)
        setError('Configure sua chave de API antes de continuar')
        setStep('config')
        return
      }

      // Load AI Instructions from localStorage
      const aiInstructions = localStorage.getItem('etnopapers_ai_instructions') || undefined

      // Extract with AI based on provider
      let metadata: ExtractedMetadata

      switch (apiKey.provider) {
        case 'openai':
          console.log('Extracting metadata with OpenAI ChatGPT...')
          metadata = await extractWithOpenAI(result.text, apiKey.key, aiInstructions)
          break
        case 'claude':
          console.log('Extracting metadata with Claude...')
          metadata = await extractWithClaude(result.text, apiKey.key, aiInstructions)
          break
        case 'gemini':
        default:
          console.log('Extracting metadata with Gemini...')
          metadata = await extractWithGemini(result.text, apiKey.key, aiInstructions)
          break
      }

      setExtractedData(metadata)
      setExtractedMetadata(metadata)
      setStep('review')
      setExtractLoading(false)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Erro desconhecido'
      setExtractError(errorMsg)
      setError(errorMsg)
      setExtractLoading(false)
    }
  }

  const handleSave = async (data: ExtractedMetadata) => {
    try {
      setError(null)

      // Check duplicate before saving
      const dupCheck = await articlesAPI.checkDuplicate({
        titulo: data.titulo,
        ano_publicacao: data.ano_publicacao,
        autores: data.autores,
        doi: data.doi,
      })

      if (dupCheck.data.is_duplicate) {
        const confirmed = window.confirm(
          `Artigo duplicado detectado:\n${dupCheck.data.duplicate.titulo}\n\nDeseja sobrescrever?`
        )
        if (!confirmed) {
          setError('Salvamento cancelado')
          return
        }
      }

      // Save article
      const response = await articlesAPI.create({
        titulo: data.titulo,
        doi: data.doi,
        ano_publicacao: data.ano_publicacao,
        autores: data.autores,
        resumo: data.resumo,
        status: 'rascunho',
      })

      addArticle(response.data)
      setError(null)

      // Show success message and reset form immediately
      alert('✅ Artigo salvo com sucesso!')

      // Reset form state immediately after save (useEffect will clean up when step changes)
      setStep('upload')
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Erro ao salvar'
      setError(errorMsg)
    }
  }

  const handleDiscard = () => {
    if (window.confirm('Descartar metadados extraídos?')) {
      setStep('upload')
    }
  }

  const handleEdit = () => {
    setStep('edit')
  }

  const handleEditorSave = async (editedData: ExtractedMetadata) => {
    setExtractedMetadata(editedData)
    setStep('review')
  }

  const handleEditorCancel = () => {
    setStep('review')
  }

  const recoverDraft = () => {
    const draft = getRecentDraft()
    if (draft) {
      setExtractedMetadata(draft)
      setStep('review')
      setShowRecoverDraft(false)
    }
  }

  const discardDraft = () => {
    clearAllDrafts()
    setShowRecoverDraft(false)
    setHasDraft(false)
  }

  return (
    <div className="upload-page">
      <h2>📄 Upload de Artigo</h2>

      {/* Step Indicator */}
      <div className="step-indicator">
        <div className={`step ${step === 'upload' ? 'active' : step > 'upload' ? 'done' : ''}`}>
          <span>1</span>
          <span>Upload</span>
        </div>
        <div className={`step ${step === 'config' ? 'active' : step > 'config' ? 'done' : ''}`}>
          <span>2</span>
          <span>Configuração</span>
        </div>
        <div className={`step ${step === 'extract' ? 'active' : step > 'extract' ? 'done' : ''}`}>
          <span>3</span>
          <span>Extração</span>
        </div>
        <div className={`step ${step === 'review' ? 'active' : ''}`}>
          <span>4</span>
          <span>Revisão</span>
        </div>
      </div>

      {error && (
        <div className="error-banner">
          <span>❌ {error}</span>
          <button onClick={() => setError(null)} className="close-btn">✕</button>
        </div>
      )}

      {/* Draft Recovery Banner */}
      {showRecoverDraft && (
        <div className="draft-recovery-banner">
          <div className="draft-message">
            <span>📝 Rascunho recuperado</span>
            <p>Detectamos um rascunho salvo automaticamente. Deseja recuperá-lo?</p>
          </div>
          <div className="draft-actions">
            <button onClick={recoverDraft} className="btn-recover">
              Recuperar
            </button>
            <button onClick={discardDraft} className="btn-discard">
              Descartar
            </button>
          </div>
        </div>
      )}

      {/* Step 1: Upload */}
      {step === 'upload' && (
        <section className="step-section">
          <PDFUpload onUpload={handleUpload} />
        </section>
      )}

      {/* Step 2: Configuration */}
      {step === 'config' && (
        <section className="step-section">
          <APIKeySetup />
          <div className="action-buttons">
            <button onClick={() => setStep('upload')} className="btn-back">
              ← Voltar
            </button>
            <button
              onClick={() => handleUpload(new File([], 'reupload'))}
              className="btn-continue"
            >
              Continuar →
            </button>
          </div>
        </section>
      )}

      {/* Step 3 & 4: Metadata */}
      {(step === 'extract' || step === 'review') && extractedMetadata && (
        <section className="step-section">
          {duplicateWarning && (
            <div className="duplicate-warning">
              <span>⚠️ Artigo Duplicado Detectado</span>
              <p>
                Um artigo similar já existe no banco de dados:
                <strong>{duplicateWarning.titulo}</strong>
              </p>
              <button onClick={() => setDuplicateWarning(null)} className="close-btn">✕</button>
            </div>
          )}

          <MetadataDisplay
            data={extractedMetadata}
            isScanned={isScanned}
            onSave={handleSave}
            onEdit={handleEdit}
            onDiscard={handleDiscard}
          />
        </section>
      )}

      {/* Step 5: Manual Editor */}
      {step === 'edit' && extractedMetadata && (
        <section className="step-section">
          <ManualEditor
            initialData={extractedMetadata}
            onSave={handleEditorSave}
            onCancel={handleEditorCancel}
            isDraft={hasDraft}
          />
        </section>
      )}
    </div>
  )
}
