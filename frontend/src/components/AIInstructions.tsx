import { useState, useEffect } from 'react'
import './AIInstructions.css'

const STORAGE_KEY = 'etnopapers_ai_instructions'

const DEFAULT_INSTRUCTIONS = `Você é um especialista em extração de metadados de artigos científicos sobre etnobotânica.

Sua tarefa é extrair as seguintes informações do texto do artigo e retornar um JSON estruturado:

**INFORMAÇÕES BIBLIOGRÁFICAS:**
- titulo: Nome completo do artigo (obrigatório)
- autores: Lista de objetos com {nome, sobrenome, email (opcional)} (obrigatório, mínimo 1)
- ano_publicacao: Ano de publicação (obrigatório, número 1900-2100)
- resumo: Resumo ou abstrato do artigo (opcional)
- doi: Digital Object Identifier (opcional)

**INFORMAÇÕES ETNOBOTÂNICAS:**
- especies: Array de objetos com {vernacular (nome comum), nomeCientifico} (opcional)
- tipo_de_uso: Tipo de uso das plantas (ex: medicinal, alimentar, ritual, combustível) (opcional)
- metodologia: Metodologia de pesquisa (ex: entrevistas, observação participante, levantamento) (opcional)
- comunidades: Array de nomes de comunidades indígenas ou tradicionais mencionadas (opcional)

**INFORMAÇÕES GEOGRÁFICAS:**
- pais: País onde o estudo foi realizado (opcional)
- estado: Estado ou região (opcional)
- municipio: Município (opcional)
- local: Local específico ou nome da comunidade/territorialidade (opcional)
- bioma: Bioma onde o estudo foi realizado (ex: Mata Atlântica, Cerrado, Amazônia) (opcional)
- regioes: Array de regiões mencionadas no estudo (opcional)

**IMPORTANTE:**
- Retorne APENAS um objeto JSON válido
- Extraia apenas informações que estão EXPLICITAMENTE mencionadas no texto
- Para campos não encontrados, omita-os do JSON ou use null
- Nomes científicos de plantas devem estar em formato binomial (Gênero species)
- Mantenha a ordem dos autores conforme aparecem no artigo`

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
