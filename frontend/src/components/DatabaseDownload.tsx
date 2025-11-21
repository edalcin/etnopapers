import { useState, useEffect } from 'react'
import apiClient from '@services/api'
import './DatabaseDownload.css'

interface DatabaseInfo {
  path: string
  size_mb: number
  tables: number
  table_info: Record<string, number>
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
      const response = await apiClient.get('/database/info')
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
        `etnopapers_${new Date().toISOString().split('T')[0]}.db`
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
          <h3>💾 Banco de Dados</h3>
          <p>Faça download completo do seu banco de dados SQLite</p>
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
                <span className="info-label">Tabelas</span>
                <span className="info-value">{info.tables}</span>
              </div>
            </div>

            <div className="tables-info">
              <h4>Registros por Tabela</h4>
              <div className="table-stats">
                {Object.entries(info.table_info).map(([table, count]) => (
                  <div key={table} className="stat-row">
                    <span className="table-name">{table}</span>
                    <span className="record-count">{count} registros</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="features">
              <p>✅ Banco de dados completo em um arquivo único</p>
              <p>✅ Formato SQLite - portável e aberto</p>
              <p>✅ Pode ser aberto com ferramentas SQLite padrão</p>
              <p>✅ Integrado com integridade de dados verificada</p>
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
            Você pode abrir o arquivo .db com qualquer ferramenta SQLite, como:
          </p>
          <ul>
            <li>
              <strong>SQLite Browser</strong> - Interface gráfica gratuita
            </li>
            <li>
              <strong>SQLite CLI</strong> - <code>sqlite3 etnopapers.db</code>
            </li>
            <li>
              <strong>Python</strong> - <code>import sqlite3</code>
            </li>
            <li>
              <strong>DBeaver</strong> - IDE de banco de dados profissional
            </li>
          </ul>
        </div>
      </div>
    </div>
  )
}
