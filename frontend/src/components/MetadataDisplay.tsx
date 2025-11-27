import { useState } from 'react'
import type { ExtractedMetadata } from '@types'
import './MetadataDisplay.css'

interface MetadataDisplayProps {
  data: ExtractedMetadata
  loading?: boolean
  error?: string
  onSave?: (data: ExtractedMetadata) => void
  onEdit?: () => void
  onDiscard?: () => void
  isScanned?: boolean
}

export default function MetadataDisplay({
  data,
  loading = false,
  error,
  onSave,
  onEdit,
  onDiscard,
  isScanned = false,
}: MetadataDisplayProps) {
  const [saved, setSaved] = useState(false)

  const handleSave = async () => {
    if (onSave) {
      await onSave(data)
      setSaved(true)
      setTimeout(() => setSaved(false), 3000)
    }
  }

  if (loading) {
    return (
      <div className="metadata-display">
        <div className="loading">
          <div className="spinner" />
          <p>Extraindo metadados com IA...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="metadata-display">
        <div className="error">
          <p>❌ Erro ao extrair metadados</p>
          <p className="error-message">{error}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="metadata-display">
      {isScanned && (
        <div className="warning-banner">
          <span>⚠️ PDF Escaneado Detectado</span>
          <p>
            Este documento é um PDF escaneado. A qualidade da extração pode estar
            reduzida. Revise os dados com atenção.
          </p>
        </div>
      )}

      <div className="metadata-card">
        <div className="metadata-header">
          <h3>Metadados Extraídos</h3>
          {saved && <span className="saved-badge">✅ Salvo!</span>}
        </div>

        {/* Bibliographic Section */}
        <section className="metadata-section">
          <h4>📚 Informações Bibliográficas</h4>

          <div className="field">
            <label>Título</label>
            <p className={!data.titulo ? 'missing' : ''}>
              {data.titulo || '⚠️ Não extraído'}
            </p>
          </div>

          <div className="field-row">
            <div className="field">
              <label>Ano</label>
              <p>{data.ano_publicacao || '⚠️ Não extraído'}</p>
            </div>
            <div className="field">
              <label>DOI</label>
              <p>{data.doi ? <code>{data.doi}</code> : '⚠️ Não encontrado'}</p>
            </div>
          </div>

          <div className="field">
            <label>Autores ({data.autores?.length || 0})</label>
            {data.autores && data.autores.length > 0 ? (
              <ul className="authors-list">
                {data.autores.map((author, i) => (
                  <li key={i}>
                    <strong>{author.nome}</strong> {author.sobrenome}
                    {author.email && <span className="email">{author.email}</span>}
                  </li>
                ))}
              </ul>
            ) : (
              <p className="missing">⚠️ Nenhum autor extraído</p>
            )}
          </div>

          <div className="field">
            <label>Resumo</label>
            <p className={!data.resumo ? 'missing' : 'resumo'}>
              {data.resumo || '⚠️ Não extraído'}
            </p>
          </div>
        </section>

        {/* Botanical Section */}
        <section className="metadata-section">
          <h4>🌿 Espécies de Plantas</h4>
          {data.especies && data.especies.length > 0 ? (
            <div className="species-list">
              {data.especies.map((species, i) => (
                <div key={i} className="species-item">
                  <div className="species-names">
                    <span className="vernacular">{species.vernacular || '⚠️ Sem nome comum'}</span>
                    <span className="scientific"><em>{species.nomeCientifico || '⚠️ Sem nome científico'}</em></span>
                    {species.familia && <span className="familia">Fam: {species.familia}</span>}
                  </div>

                  {/* Validation Status (Clarificação 2025-11-27) */}
                  {(species.statusValidacao || species.confianca) && (
                    <div className="species-validation">
                      {species.statusValidacao === 'validado' && (
                        <span className="badge badge-validated">✅ Validado</span>
                      )}
                      {species.statusValidacao === 'naoValidado' && (
                        <span className="badge badge-not-validated">⚠️ Não validado</span>
                      )}
                      {species.confianca && (
                        <span className={`confidence confidence-${species.confianca}`}>
                          {species.confianca === 'alta' && '🟢 Alta'}
                          {species.confianca === 'media' && '🟡 Média'}
                          {species.confianca === 'baixa' && '🔴 Baixa'}
                        </span>
                      )}
                    </div>
                  )}

                  {/* Usage Details by Community (Clarificação Q1, Q3) */}
                  {species.usosPorComunidade && species.usosPorComunidade.length > 0 && (
                    <div className="usos-por-comunidade">
                      <div className="usos-header">📍 Uso por Comunidade:</div>
                      {species.usosPorComunidade.map((uso, j) => (
                        <div key={j} className="uso-item">
                          <div className="comunidade-info">
                            <strong>{uso.comunidade.nome}</strong>
                            {uso.comunidade.tipo && <span className="tipo-comunidade">({uso.comunidade.tipo})</span>}
                          </div>
                          {uso.formaDeUso && <div className="uso-field">📋 Forma: {uso.formaDeUso}</div>}
                          {uso.tipoDeUso && <div className="uso-field">🏷️ Tipo: {uso.tipoDeUso}</div>}
                          {uso.propositoEspecifico && <div className="uso-field">🎯 Propósito: {uso.propositoEspecifico}</div>}
                          {uso.partesUtilizadas && uso.partesUtilizadas.length > 0 && (
                            <div className="uso-field">🌱 Partes: {uso.partesUtilizadas.join(', ')}</div>
                          )}
                          {uso.dosagem && <div className="uso-field">💊 Dosagem: {uso.dosagem}</div>}
                          {uso.metodoPreparacao && <div className="uso-field">🔬 Método: {uso.metodoPreparacao}</div>}
                          {uso.origem && <div className="uso-field">📚 Origem: {uso.origem}</div>}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : (
            <p className="missing">⚠️ Nenhuma espécie extraída</p>
          )}
        </section>

        {/* Geographic Section */}
        <section className="metadata-section">
          <h4>🗺️ Regiões</h4>
          {data.regioes && data.regioes.length > 0 ? (
            <div className="tags-list">
              {data.regioes.map((regiao, i) => (
                <span key={i} className="tag regiao">
                  {regiao}
                </span>
              ))}
            </div>
          ) : (
            <p className="missing">⚠️ Nenhuma região extraída</p>
          )}
        </section>

        {/* Communities Section */}
        <section className="metadata-section">
          <h4>👥 Comunidades</h4>
          {data.comunidades && data.comunidades.length > 0 ? (
            <div className="tags-list">
              {data.comunidades.map((comunidade, i) => (
                <span key={i} className="tag comunidade">
                  {comunidade}
                </span>
              ))}
            </div>
          ) : (
            <p className="missing">⚠️ Nenhuma comunidade extraída</p>
          )}
        </section>

        {/* Action Buttons */}
        <div className="action-buttons">
          <button onClick={handleSave} className="btn-save" disabled={!data.titulo}>
            💾 Salvar
          </button>
          <button onClick={onEdit} className="btn-edit">
            ✏️ Editar
          </button>
          <button onClick={onDiscard} className="btn-discard">
            🗑️ Descartar
          </button>
        </div>
      </div>
    </div>
  )
}
