import { useState } from 'react'
import { validateAPIKey } from '@services/api'
import { useSetAPIKey, useSetAPIKeyValidity, useAPIKey } from '@store/useStore'
import type { AIProvider } from '@types'
import './APIKeySetup.css'

export default function APIKeySetup() {
  const apiKey = useAPIKey()
  const setAPIKey = useSetAPIKey()
  const setAPIKeyValidity = useSetAPIKeyValidity()

  const [tempProvider, setTempProvider] = useState<AIProvider>(apiKey.provider)
  const [tempKey, setTempKey] = useState(apiKey.key)
  const [loading, setLoading] = useState(false)
  const [message, setMessage] = useState('')
  const [showKey, setShowKey] = useState(false)

  const providers: { id: AIProvider; name: string; url: string }[] = [
    {
      id: 'gemini',
      name: 'Google Gemini',
      url: 'https://ai.google.dev/tutorials/python_quickstart',
    },
    {
      id: 'openai',
      name: 'OpenAI ChatGPT',
      url: 'https://platform.openai.com/api-keys',
    },
    {
      id: 'claude',
      name: 'Anthropic Claude',
      url: 'https://console.anthropic.com/account/keys',
    },
  ]

  const handleValidate = async () => {
    if (!tempKey) {
      setMessage('❌ Chave de API não pode estar vazia')
      return
    }

    setLoading(true)
    setMessage('Validando...')

    try {
      const isValid = await validateAPIKey(tempProvider, tempKey)

      if (isValid) {
        setAPIKey(tempProvider, tempKey)
        setAPIKeyValidity(true)
        setMessage('✅ Chave validada com sucesso!')
      } else {
        setMessage('❌ Chave inválida ou sem permissão')
        setAPIKeyValidity(false)
      }
    } catch (error) {
      setMessage('❌ Erro ao validar chave')
      setAPIKeyValidity(false)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="api-key-setup">
      <div className="setup-card">
        <h3>Configurar Chave de API</h3>
        <p className="subtitle">
          Selecione seu provedor de IA preferido e configure a chave de API
        </p>

        <div className="form-group">
          <label>Provedor de IA</label>
          <select
            value={tempProvider}
            onChange={e => setTempProvider(e.target.value as AIProvider)}
            disabled={loading}
          >
            {providers.map(p => (
              <option key={p.id} value={p.id}>
                {p.name}
              </option>
            ))}
          </select>
          <a
            href={providers.find(p => p.id === tempProvider)?.url}
            target="_blank"
            rel="noreferrer"
            className="help-link"
          >
            Como obter uma chave →
          </a>
        </div>

        <div className="form-group">
          <label>Chave de API</label>
          <div className="key-input-wrapper">
            <input
              type={showKey ? 'text' : 'password'}
              value={tempKey}
              onChange={e => setTempKey(e.target.value)}
              placeholder="Cole sua chave de API aqui"
              disabled={loading}
            />
            <button
              type="button"
              onClick={() => setShowKey(!showKey)}
              className="toggle-visibility"
              disabled={loading}
              title={showKey ? 'Ocultar' : 'Mostrar'}
            >
              {showKey ? '👁️' : '👁️‍🗨️'}
            </button>
          </div>
        </div>

        {message && (
          <div
            className={`message ${
              message.startsWith('✅') ? 'success' : 'error'
            }`}
          >
            {message}
          </div>
        )}

        <div className="button-group">
          <button
            onClick={handleValidate}
            disabled={loading || !tempKey}
            className="btn-primary"
          >
            {loading ? 'Validando...' : 'Validar e Salvar'}
          </button>
        </div>

        <div className="info-box">
          <p>
            <strong>⚠️ Privacidade:</strong> Sua chave de API é armazenada
            apenas no navegador (localStorage) e nunca é enviada para nosso
            servidor.
          </p>
        </div>

        {apiKey.isValid && (
          <div className="status-box success">
            <p>✅ Chave validada e ativa</p>
            <p className="provider">Provedor: <strong>{tempProvider}</strong></p>
          </div>
        )}
      </div>
    </div>
  )
}
