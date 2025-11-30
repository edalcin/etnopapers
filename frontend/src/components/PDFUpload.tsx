import { useState } from 'react'
import { useExtractionStore } from '../store/extractionStore'
import { useOllamaHealth } from '../hooks/useOllamaHealth'
import MetadataDisplay from './MetadataDisplay'
import './PDFUpload.css'

interface PDFUploadProps {
  onSuccess?: () => void
}

export default function PDFUpload({ onSuccess }: PDFUploadProps) {
  const [isDragging, setIsDragging] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const { extractedData, setExtractedData, setError: setStoreError, clearExtraction } = useExtractionStore()
  const { health } = useOllamaHealth(30000)

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault()
    setIsDragging(true)
  }

  const handleDragLeave = () => {
    setIsDragging(false)
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setIsDragging(false)
    const files = e.dataTransfer.files
    if (files.length > 0) {
      handleFile(files[0])
    }
  }

  const handleFileInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      handleFile(e.target.files[0])
    }
  }

  const handleFile = async (file: File) => {
    setError(null)

    // Check Ollama status before processing
    if (health.status !== 'ok') {
      if (health.status === 'checking') {
        setError('Aguardando verificação do Ollama... Tente novamente em alguns momentos.')
      } else {
        setError('Ollama está indisponível. Por favor, inicie o Ollama em https://ollama.com/download e tente novamente.')
      }
      return
    }

    setIsLoading(true)

    try {
      const maxSize = 100 * 1024 * 1024
      if (file.size > maxSize) {
        setError('PDF muito grande (>100 MB) - Divida o arquivo em partes menores')
        setIsLoading(false)
        return
      }

      const formData = new FormData()
      formData.append('file', file)

      const response = await fetch('/api/extract/metadata', {
        method: 'POST',
        body: formData
      })

      if (response.ok) {
        const data = await response.json()
        setExtractedData(data)
        setError(null)
      } else {
        const errorData = await response.json()
        const errorMsg = errorData.detail || 'Erro ao extrair metadados'
        setError(errorMsg)
        setStoreError(errorMsg)
      }
    } catch (err) {
      const errorMsg = 'Erro ao processar arquivo'
      setError(errorMsg)
      setStoreError(errorMsg)
    } finally {
      setIsLoading(false)
    }
  }

  if (extractedData) {
    return <MetadataDisplay onNewExtraction={() => clearExtraction()} onSuccess={onSuccess} />
  }

  return (
    <div className="pdf-upload">
      <div className={`drop-zone ${isDragging ? 'dragging' : ''} ${health.status !== 'ok' ? 'disabled' : ''}`} onDragOver={handleDragOver} onDragLeave={handleDragLeave} onDrop={handleDrop}>
        <div className="drop-zone-content">
          <h3>Enviar Arquivo PDF</h3>
          {health.status === 'ok' ? (
            <>
              <p>Arraste um arquivo PDF aqui ou clique para selecionar</p>
              <p className="file-size-hint">Maximo 100 MB</p>
            </>
          ) : health.status === 'checking' ? (
            <p className="status-warning">Verificando Ollama...</p>
          ) : (
            <p className="status-warning">Ollama indisponível. Inicie o Ollama para processar PDFs.</p>
          )}
          <input type="file" accept=".pdf" onChange={handleFileInput} disabled={isLoading || health.status !== 'ok'} className="file-input" />
        </div>
      </div>

      {isLoading && (
        <div className="loading-indicator">
          <div className="spinner"></div>
          <p>Processando PDF com Ollama...</p>
        </div>
      )}

      {error && (
        <div className="error-message">
          <p>{error}</p>
        </div>
      )}
    </div>
  )
}
