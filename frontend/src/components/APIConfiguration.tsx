import { useState } from 'react'
import './APIConfiguration.css'

export default function APIConfiguration() {
  const [mongoUri, setMongoUri] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  const handleSave = async () => {
    setLoading(true)
    setError(null)
    setSuccess(false)

    try {
      const response = await fetch('/api/config/save', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mongo_uri: mongoUri })
      })

      if (response.ok) {
        setSuccess(true)
        setMongoUri('')
      } else {
        setError('Erro ao salvar configuração')
      }
    } catch (err) {
      setError('Erro de conexão')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="api-configuration">
      <div className="form-group">
        <label>MongoDB URI:</label>
        <input
          type="text"
          value={mongoUri}
          onChange={(e) => setMongoUri(e.target.value)}
          placeholder="mongodb://localhost:27017/etnopapers"
          disabled={loading}
        />
      </div>

      <button onClick={handleSave} disabled={loading}>
        {loading ? 'Salvando...' : 'Salvar'}
      </button>

      {error && <div className="error-msg">{error}</div>}
      {success && <div className="success-msg">Configuração salva com sucesso!</div>}
    </div>
  )
}
