import { useState, useEffect } from 'react'
import './AIInstructions.css'

const STORAGE_KEY = 'etnopapers_ai_instructions'

const DEFAULT_INSTRUCTIONS = `Você é um especialista em extração de metadados de artigos científicos sobre etnobotânica.

Sua tarefa é extrair as seguintes informações do texto do artigo:
- Título: Nome completo do artigo
- Autores: Lista de nomes dos autores
- Ano de Publicação: Ano em que o artigo foi publicado
- Resumo: Resumo ou abstrato do artigo (se disponível)
- DOI: Digital Object Identifier (se disponível)

Por favor, extraia apenas informações que estão explicitamente mencionadas no texto do artigo. Se alguma informação não estiver disponível, indique como não encontrada.`

export default function AIInstructions() {
  const [instructions, setInstructions] = useState(DEFAULT_INSTRUCTIONS)
  const [saveMessage, setSaveMessage] = useState<string | null>(null)
  const [saveLoading, setSaveLoading] = useState(false)

  // Load instructions from localStorage on mount
  useEffect(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY)
      if (stored) {
        setInstructions(stored)
      }
    } catch (error) {
      console.error('Failed to load AI instructions:', error)
    }
  }, [])

  const handleSave = async () => {
    setSaveLoading(true)
    setSaveMessage(null)

    try {
      if (!instructions.trim()) {
        setSaveMessage('❌ Instruções não podem estar vazias')
        setSaveLoading(false)
        return
      }

      // Save to localStorage
      localStorage.setItem(STORAGE_KEY, instructions)

      setSaveMessage('✅ Instruções salvas com sucesso!')

      // Clear message after 3 seconds
      setTimeout(() => setSaveMessage(null), 3000)
    } catch (error) {
      setSaveMessage('❌ Erro ao salvar instruções')
    } finally {
      setSaveLoading(false)
    }
  }

  const handleReset = () => {
    if (window.confirm('Deseja restaurar as instruções padrão?')) {
      setInstructions(DEFAULT_INSTRUCTIONS)
      localStorage.setItem(STORAGE_KEY, DEFAULT_INSTRUCTIONS)
      setSaveMessage('✅ Instruções restauradas para o padrão!')
      setTimeout(() => setSaveMessage(null), 3000)
    }
  }

  return (
    <div className="ai-instructions">
      <div className="instructions-header">
        <h3>📋 Instruções para a I.A.</h3>
        <p className="instructions-subtitle">
          Customizar as instruções que serão enviadas ao agente de I.A. para melhorar a extração de metadados
        </p>
      </div>

      {saveMessage && (
        <div className={`instructions-message ${saveMessage.includes('sucesso') ? 'success' : 'error'}`}>
          {saveMessage}
        </div>
      )}

      <div className="instructions-section">
        <label htmlFor="instructions-textarea">
          <strong>Seu Prompt Customizado:</strong>
        </label>
        <p className="help-text">
          Estas instruções serão usadas como sistema de prompt para o agente de I.A. ao extrair metadados dos artigos.
          Você pode customizar para melhorar a qualidade da extração conforme suas necessidades.
        </p>
        <textarea
          id="instructions-textarea"
          value={instructions}
          onChange={(e) => setInstructions(e.target.value)}
          placeholder="Digite suas instruções customizadas..."
          rows={12}
          disabled={saveLoading}
        />
      </div>

      <div className="instructions-actions">
        <button
          onClick={handleSave}
          className="btn-save"
          disabled={saveLoading}
        >
          {saveLoading ? 'Salvando...' : '💾 Salvar Instruções'}
        </button>
        <button
          onClick={handleReset}
          className="btn-reset"
          disabled={saveLoading}
        >
          🔄 Restaurar Padrão
        </button>
      </div>

      <div className="info-box">
        <h4>💡 Dicas para Melhorar a Extração:</h4>
        <ul>
          <li>Especifique claramente quais campos são obrigatórios</li>
          <li>Indique o formato esperado para cada campo</li>
          <li>Mencione campos específicos de etnobotânica se relevante</li>
          <li>Defina como lidar com informações não encontradas</li>
          <li>Use exemplos se necessário para clareza</li>
        </ul>
      </div>
    </div>
  )
}
