import { useState } from 'react'
import PDFUpload from '@components/PDFUpload'
import MetadataDisplay from '@components/MetadataDisplay'
import APIKeySetup from '@components/APIKeySetup'
import { extractTextFromPDF } from '@services/pdfExtractor'
import { extractWithGemini } from '@services/ai/geminiClient'
import { extractWithOpenAI } from '@services/ai/openaiClient'
import { extractWithClaude } from '@services/ai/claudeClient'
import { useAPIKey, useSetExtractedData, useSetExtractLoading, useSetExtractError, useAddArticle } from '@store/useStore'
import { articlesAPI } from '@services/api'
import type { ExtractedMetadata } from '@types'
import './Upload.css'

export default function Upload() {
  const apiKey = useAPIKey()
  const setExtractedData = useSetExtractedData()
  const setExtractLoading = useSetExtractLoading()
  const setExtractError = useSetExtractError()
  const addArticle = useAddArticle()

  const [step, setStep] = useState<'upload' | 'config' | 'extract' | 'review'>('upload')
  const [pdfText, setPdfText] = useState<string>('')
  const [extractedMetadata, setExtractedMetadata] = useState<ExtractedMetadata | null>(null)
  const [isScanned, setIsScanned] = useState(false)
  const [error, setError] = useState<string | null>(null)

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
      if (!apiKey.key || !apiKey.isValid) {
        setExtractLoading(false)
        setError('Configure sua chave de API antes de continuar')
        setStep('config')
        return
      }

      // Extract with AI based on provider
      let metadata: ExtractedMetadata

      switch (apiKey.provider) {
        case 'openai':
          console.log('Extracting metadata with OpenAI ChatGPT...')
          metadata = await extractWithOpenAI(result.text, apiKey.key)
          break
        case 'claude':
          console.log('Extracting metadata with Claude...')
          metadata = await extractWithClaude(result.text, apiKey.key)
          break
        case 'gemini':
        default:
          console.log('Extracting metadata with Gemini...')
          metadata = await extractWithGemini(result.text, apiKey.key)
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

      // Reset form
      setTimeout(() => {
        setStep('upload')
        setPdfText('')
        setExtractedMetadata(null)
        setIsScanned(false)
      }, 1000)

      alert('✅ Artigo salvo com sucesso!')
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Erro ao salvar'
      setError(errorMsg)
    }
  }

  const handleDiscard = () => {
    if (window.confirm('Descartar metadados extraídos?')) {
      setStep('upload')
      setPdfText('')
      setExtractedMetadata(null)
      setIsScanned(false)
      setError(null)
    }
  }

  const handleEdit = () => {
    alert('Edição de metadados será implementada em TASK-017')
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
          <MetadataDisplay
            data={extractedMetadata}
            isScanned={isScanned}
            onSave={handleSave}
            onEdit={handleEdit}
            onDiscard={handleDiscard}
          />
        </section>
      )}
    </div>
  )
}
