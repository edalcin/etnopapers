import { useState, useEffect } from 'react'
import './ConfigurationDialog.css'

interface ConfigurationDialogProps {
  onConfigured?: () => void
  isOpen?: boolean
}

export default function ConfigurationDialog({ onConfigured, isOpen = true }: ConfigurationDialogProps) {
  const [mongoUri, setMongoUri] = useState('mongodb://localhost:27017/etnopapers')
  const [loading, setLoading] = useState(false)
  const [validating, setValidating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [isValid, setIsValid] = useState(false)

  const handleValidate = async () => {
    setValidating(true)
    setError(null)

    try {
      const response = await fetch('/api/config/validate-mongo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mongo_uri: mongoUri })
      })

      const data = await response.json()

      if (response.ok && data.valido) {
        setIsValid(true)
        setError(null)
      } else {
        setIsValid(false)
        setError(data.mensagem || 'Conexao invalida')
      }
    } catch (err) {
      setIsValid(false)
      setError('Erro ao validar conexao')
    } finally {
      setValidating(false)
    }
  }

  const handleSave = async () => {
    if (!isValid) {
      setError('Por favor, valide a conexao primeiro')
      return
    }

    setLoading(true)
    setError(null)

    try {
      const response = await fetch('/api/config/save', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mongo_uri: mongoUri })
      })

      if (response.ok) {
        setSuccess(true)
        setTimeout(() => {
          onConfigured?.()
        }, 1500)
      } else {
        const data = await response.json()
        setError(data.mensagem || 'Erro ao salvar')
      }
    } catch (err) {
      setError('Erro ao salvar configuracao')
    } finally {
      setLoading(false)
    }
  }

  if (!isOpen) return null

  return (
    <div className="configuration-dialog-overlay">
      <div className="configuration-dialog">
        <div className="dialog-header">
          <h2>Configurar MongoDB</h2>
          <p>Configure a conexao com o MongoDB para comecar</p>
        </div>

        <div className="dialog-content">
          <div className="form-group">
            <label htmlFor="mongo-uri">URI do MongoDB:</label>
            <input
              id="mongo-uri"
              type="text"
              value={mongoUri}
              onChange={(e) => {
                setMongoUri(e.target.value)
                setIsValid(false)
              }}
              placeholder="mongodb://localhost:27017/etnopapers"
              disabled={loading || validating}
              className="mongo-uri-input"
            />
            <p className="help-text">
              Local: mongodb://localhost:27017/etnopapers<br/>
              Atlas: mongodb+srv://user:pass@cluster.mongodb.net/etnopapers
            </p>
          </div>

          {error && (
            <div className="error-message">
              <span className="error-icon">!</span>
              <p>{error}</p>
            </div>
          )}

          {isValid && (
            <div className="success-message">
              <span className="success-icon">OK</span>
              <p>Conexao validada com sucesso!</p>
            </div>
          )}

          {success && (
            <div className="success-message">
              <span className="success-icon">OK</span>
              <p>Configuracao salva! Carregando aplicacao...</p>
            </div>
          )}
        </div>

        <div className="dialog-actions">
          <button
            onClick={handleValidate}
            disabled={loading || validating || !mongoUri}
            className="btn-secondary"
          >
            {validating ? 'Validando...' : 'Validar'}
          </button>

          <button
            onClick={handleSave}
            disabled={loading || !isValid}
            className="btn-primary"
          >
            {loading ? 'Salvando...' : 'Salvar e Continuar'}
          </button>
        </div>
      </div>
    </div>
  )
}
