import { useCallback, useState } from 'react'
import { useSetUploadFile, useSetUploadError, useUploadState } from '@store/useStore'
import './PDFUpload.css'

const MAX_FILE_SIZE = 50 * 1024 * 1024 // 50 MB

export default function PDFUpload({ onUpload }: { onUpload?: (file: File) => void }) {
  const setUploadFile = useSetUploadFile()
  const setUploadError = useSetUploadError()
  const uploadState = useUploadState()
  const [dragActive, setDragActive] = useState(false)

  const validateFile = (file: File): string | null => {
    if (!file.type.includes('pdf')) {
      return 'Apenas arquivos PDF são aceitos'
    }
    if (file.size > MAX_FILE_SIZE) {
      return `Arquivo muito grande (máx. 50 MB, seu arquivo tem ${(file.size / 1024 / 1024).toFixed(2)} MB)`
    }
    return null
  }

  const handleFile = (file: File) => {
    const error = validateFile(file)
    if (error) {
      setUploadError(error)
      return
    }

    setUploadFile(file)
    setUploadError(null)

    if (onUpload) {
      onUpload(file)
    }
  }

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true)
    } else if (e.type === 'dragleave') {
      setDragActive(false)
    }
  }, [])

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setDragActive(false)

    const files = e.dataTransfer.files
    if (files && files[0]) {
      handleFile(files[0])
    }
  }, [])

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.currentTarget.files
    if (files && files[0]) {
      handleFile(files[0])
    }
  }

  return (
    <div className="pdf-upload">
      <div
        className={`upload-area ${dragActive ? 'active' : ''}`}
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
      >
        <div className="upload-content">
          <div className="upload-icon">📄</div>
          <h3>Envie seu artigo em PDF</h3>
          <p>Arraste e solte ou clique para selecionar</p>
          <p className="file-size-info">Máximo 50 MB</p>

          <input
            type="file"
            accept=".pdf"
            onChange={handleChange}
            className="file-input"
            id="pdf-input"
            disabled={!!uploadState.file}
          />
          <label htmlFor="pdf-input" className="file-label">
            Selecionar arquivo
          </label>
        </div>
      </div>

      {uploadState.error && (
        <div className="error-message">
          <span>❌ {uploadState.error}</span>
        </div>
      )}

      {uploadState.file && (
        <div className="file-info">
          <div className="file-details">
            <span className="file-icon">📋</span>
            <div className="file-text">
              <p className="file-name">{uploadState.file.name}</p>
              <p className="file-size">
                {(uploadState.file.size / 1024 / 1024).toFixed(2)} MB
              </p>
            </div>
          </div>
          <button
            onClick={() => {
              setUploadFile(null)
              setUploadError(null)
            }}
            className="btn-remove"
            title="Remover arquivo"
          >
            ✕
          </button>
        </div>
      )}

      {uploadState.progress > 0 && uploadState.progress < 100 && (
        <div className="progress-bar">
          <div className="progress-fill" style={{ width: `${uploadState.progress}%` }} />
          <span className="progress-text">{uploadState.progress}%</span>
        </div>
      )}
    </div>
  )
}
