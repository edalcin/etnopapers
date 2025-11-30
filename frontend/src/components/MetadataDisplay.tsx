import { useState } from 'react'
import { useExtractionStore } from '../store/extractionStore'
import './MetadataDisplay.css'

interface MetadataDisplayProps {
  onNewExtraction?: () => void
  onSuccess?: () => void
}

export default function MetadataDisplay({ onNewExtraction, onSuccess }: MetadataDisplayProps) {
  const { extractedData, updateField, addSpecies, removeSpecies, clearExtraction } = useExtractionStore()
  const [isSaving, setIsSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [newVernacular, setNewVernacular] = useState('')
  const [newScientific, setNewScientific] = useState('')

  if (!extractedData) return null

  const handleAddSpecies = () => {
    if (newVernacular && newScientific) {
      addSpecies({ vernacular: newVernacular, nomeCientifico: newScientific })
      setNewVernacular('')
      setNewScientific('')
    }
  }

  const handleSave = async () => {
    setIsSaving(true)
    setSaveError(null)

    try {
      const response = await fetch('/api/articles', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(extractedData)
      })

      if (response.ok) {
        clearExtraction()
        onSuccess?.()
      } else {
        const error = await response.json()
        setSaveError(error.detail || 'Erro ao salvar')
      }
    } catch (err) {
      setSaveError('Erro ao salvar artigo')
    } finally {
      setIsSaving(false)
    }
  }

  const handleDiscard = () => {
    if (confirm('Descartar os dados extraidos?')) {
      clearExtraction()
      onNewExtraction?.()
    }
  }

  return (
    <div className="metadata-display">
      <div className="display-header">
        <h2>Metadados Extraidos</h2>
        <p>Revise e corrija os dados antes de salvar</p>
      </div>

      <div className="metadata-form">
        <div className="form-row">
          <div className="form-group full-width">
            <label>Titulo:</label>
            <input type="text" value={extractedData.titulo} onChange={(e) => updateField('titulo', e.target.value)} className="form-input" />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Ano:</label>
            <input type="number" value={extractedData.ano || ''} onChange={(e) => updateField('ano', e.target.value ? parseInt(e.target.value) : null)} className="form-input" />
          </div>
          <div className="form-group">
            <label>DOI:</label>
            <input type="text" value={extractedData.doi || ''} onChange={(e) => updateField('doi', e.target.value)} className="form-input" />
          </div>
        </div>

        <div className="form-section">
          <h3>Especies</h3>
          <div className="species-list">
            {extractedData.especies?.map((s, i) => (
              <div key={i} className="species-item">
                <p>{s.vernacular} - {s.nomeCientifico}</p>
                <button onClick={() => removeSpecies(i)} className="btn-remove">X</button>
              </div>
            ))}
          </div>
          <div className="add-species">
            <input type="text" value={newVernacular} onChange={(e) => setNewVernacular(e.target.value)} placeholder="Nome comum" className="form-input" />
            <input type="text" value={newScientific} onChange={(e) => setNewScientific(e.target.value)} placeholder="Nome cientifico" className="form-input" />
            <button onClick={handleAddSpecies} className="btn-secondary">Adicionar</button>
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label>Tipo de Uso:</label>
            <select value={extractedData.tipo_de_uso || ''} onChange={(e) => updateField('tipo_de_uso', e.target.value)} className="form-input">
              <option value="">-</option>
              <option value="medicinal">Medicinal</option>
              <option value="alimentar">Alimentar</option>
              <option value="ritualistico">Ritualistico</option>
            </select>
          </div>
          <div className="form-group">
            <label>Pais:</label>
            <input type="text" value={extractedData.pais || ''} onChange={(e) => updateField('pais', e.target.value)} className="form-input" />
          </div>
        </div>

        {saveError && <div className="error-message"><p>{saveError}</p></div>}

        <div className="form-actions">
          <button onClick={handleDiscard} className="btn-danger" disabled={isSaving}>Descartar</button>
          <button onClick={handleSave} className="btn-primary" disabled={isSaving}>Salvar</button>
        </div>
      </div>
    </div>
  )
}
