import { useEffect, useState } from 'react'
import Analytics from '@components/Analytics'
import { useArticles, useSetArticles } from '@store/useStore'
import { articlesAPI } from '@services/api'
import './Analytics.css'

export default function AnalyticsPage() {
  const { articles, loaded } = useArticles()
  const setArticles = useSetArticles()
  const [loading, setLoading] = useState(!loaded)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!loaded) {
      loadArticles()
    }
  }, [loaded])

  const loadArticles = async () => {
    try {
      setLoading(true)
      setError(null)
      const response = await articlesAPI.list(1, 1000)

      // Validate response structure
      if (!response.data || !Array.isArray(response.data.items)) {
        console.error('Invalid response structure:', response.data)
        setError('Resposta inválida do servidor. Tente novamente.')
        return
      }

      setArticles(response.data.items)
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Erro ao carregar artigos'
      console.error('Error loading articles:', err)
      setError(`Erro ao carregar artigos: ${errorMsg}`)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="analytics-page">
        <div className="loading">
          <div className="spinner" />
          <p>Carregando estatísticas...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="analytics-page">
        <div className="error">
          <p>❌ {error}</p>
          <button onClick={loadArticles} className="btn-retry">
            Tentar Novamente
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="analytics-page">
      <Analytics articles={articles} />
    </div>
  )
}
