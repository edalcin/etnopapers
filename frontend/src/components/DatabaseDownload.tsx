import { useState } from 'react'
import './DatabaseDownload.css'

export default function DatabaseDownload() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [stats, setStats] = useState<any>(null)
  const [showStats, setShowStats] = useState(false)

  const handleGetStats = async () => {
    setLoading(true)
    setError(null)

    try {
      const response = await fetch('/api/database/stats')
      if (response.ok) {
        const data = await response.json()
        setStats(data)
        setShowStats(true)
      } else {
        setError('Erro ao obter informacoes do banco de dados')
      }
    } catch (err) {
      setError('Erro ao conectar com o servidor')
    } finally {
      setLoading(false)
    }
  }

  const handleDownload = async () => {
    setLoading(true)
    setError(null)

    try {
      const response = await fetch('/api/database/download')
      
      if (response.ok) {
        // Get filename from headers
        const contentDisposition = response.headers.get('content-disposition')
        let filename = 'etnopapers_backup.zip'
        
        if (contentDisposition) {
          const filenameMatch = contentDisposition.match(/filename=([^;]+)/)
          if (filenameMatch && filenameMatch[1]) {
            filename = filenameMatch[1]
          }
        }

        // Download file
        const blob = await response.blob()
        const url = window.URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = filename
        document.body.appendChild(a)
        a.click()
        window.URL.revokeObjectURL(url)
        document.body.removeChild(a)

        setError(null)
      } else {
        const errorData = await response.json()
        setError(errorData.detail || 'Erro ao fazer download do backup')
      }
    } catch (err) {
      setError('Erro ao fazer download do backup')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="database-download-container">
      <div className="download-card">
        <div className="card-header">
          <h3>Fazer Backup da Base de Dados</h3>
          <p>Exporte todos os artigos e configuracoes como arquivo ZIP</p>
        </div>

        <div className="card-content">
          <p className="description">
            O backup contem todos os artigos salvos com metadados completos. 
            Voce pode usar este backup para:
          </p>
          <ul>
            <li>Manter uma copia de seguranca de seus dados</li>
            <li>Transferir dados para outro computador</li>
            <li>Restaurar dados apos problemas</li>
          </ul>

          {stats && showStats && (
            <div className="stats-box">
              <h4>Informacoes do Backup:</h4>
              <p>Total de artigos: <strong>{stats.total_articles}</strong></p>
              <p>Base de dados: <strong>{stats.database_name}</strong></p>
            </div>
          )}

          {error && (
            <div className="error-message">
              <p>{error}</p>
            </div>
          )}
        </div>

        <div className="card-actions">
          <button
            onClick={handleGetStats}
            className="btn-secondary"
            disabled={loading}
          >
            {loading && !error ? 'Verificando...' : 'Verificar Tamanho'}
          </button>
          <button
            onClick={handleDownload}
            className="btn-primary"
            disabled={loading}
          >
            {loading ? 'Baixando...' : 'Fazer Download do Backup'}
          </button>
        </div>

        <div className="card-footer">
          <p className="hint">
            Dica: Guarde o arquivo ZIP em um local seguro para proteger seus dados.
          </p>
        </div>
      </div>
    </div>
  )
}
