import { useState, useEffect } from 'react'
import apiClient from '@services/api'
import './DatabaseDownload.css'

interface DatabaseInfo {
  size_mb: number
  collections: number
  collection_info?: Record<string, number>
}

export default function DatabaseDownload() {
  const [info, setInfo] = useState<DatabaseInfo | null>(null)
  const [loading, setLoading] = useState(true)
  const [downloading, setDownloading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)

  useEffect(() => {
    loadDatabaseInfo()
  }, [])

  const loadDatabaseInfo = async () => {
    try {
      setLoading(true)
      const response = await apiClient.get('/health')
      setInfo(response.data.database)
      setError(null)
    } catch (err) {
      setError('Erro ao carregar informações do banco de dados')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleDownload = async () => {
    try {
      setDownloading(true)
      setError(null)
      const response = await apiClient.get('/database/download', {
        responseType: 'blob',
      })

      // Create download link
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute(
        'download',
        `etnopapers_${new Date().toISOString().split('T')[0]}.zip`
      )
      document.body.appendChild(link)
      link.click()
      link.parentNode?.removeChild(link)
      window.URL.revokeObjectURL(url)

      setSuccess(true)
      setTimeout(() => setSuccess(false), 3000)
    } catch (err) {
      setError('Erro ao baixar banco de dados')
      console.error(err)
    } finally {
      setDownloading(false)
    }
  }

  if (loading) {
    return (
      <div className="database-download">
        <div className="loading">Carregando informações...</div>
      </div>
    )
  }

  return (
    <div className="database-download">
      <div className="download-card">
        <div className="card-header">
          <h3>💾 Banco de Dados MongoDB</h3>
          <p>Faça download do dump completo de suas coleções MongoDB em arquivo .zip</p>
        </div>

        {error && <div className="error-message">{error}</div>}

        {success && (
          <div className="success-message">✅ Download iniciado com sucesso!</div>
        )}

        {info && (
          <>
            <div className="info-section">
              <div className="info-item">
                <span className="info-label">Tamanho</span>
                <span className="info-value">{info.size_mb.toFixed(2)} MB</span>
              </div>
              <div className="info-item">
                <span className="info-label">Coleções</span>
                <span className="info-value">{info.collections}</span>
              </div>
            </div>

            {info.collection_info && Object.keys(info.collection_info).length > 0 && (
              <div className="tables-info">
                <h4>Documentos por Coleção</h4>
                <div className="table-stats">
                  {Object.entries(info.collection_info).map(([collection, count]) => (
                    <div key={collection} className="stat-row">
                      <span className="table-name">{collection}</span>
                      <span className="record-count">{count} documentos</span>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <div className="features">
              <p>✅ Dump completo de todas as coleções MongoDB</p>
              <p>✅ Arquivo .zip comprimido e portável</p>
              <p>✅ Pode ser restaurado em qualquer instância MongoDB</p>
              <p>✅ Integridade de dados verificada</p>
            </div>

            <div className="action-buttons">
              <button
                onClick={handleDownload}
                disabled={downloading}
                className="btn-download"
              >
                {downloading ? '📥 Baixando...' : '📥 Baixar Banco de Dados'}
              </button>
              <button
                onClick={loadDatabaseInfo}
                disabled={downloading}
                className="btn-refresh"
              >
                🔄 Atualizar Info
              </button>
            </div>
          </>
        )}

        <div className="help-section">
          <h4>Como usar o backup?</h4>
          <p>
            O arquivo .zip contém um dump das coleções MongoDB. Você pode restaurá-lo com:
          </p>
          <ul>
            <li>
              <strong>mongorestore</strong> - Ferramenta CLI oficial do MongoDB
              <br />
              <code>mongorestore --archive=dump.zip --gzip</code>
            </li>
            <li>
              <strong>MongoDB Compass</strong> - Interface gráfica oficial
            </li>
            <li>
              <strong>Python</strong> - <code>pymongo</code> para processamento customizado
            </li>
            <li>
              <strong>Node.js</strong> - <code>mongodb</code> driver para processamento
            </li>
          </ul>
          <p style={{ marginTop: '12px', fontSize: '12px', color: '#666' }}>
            💡 Dica: Extraia o arquivo .zip e use <code>mongorestore</code> para restaurar em seu servidor MongoDB.
          </p>
        </div>
      </div>
    </div>
  )
}
