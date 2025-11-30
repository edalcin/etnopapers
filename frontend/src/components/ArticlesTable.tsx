import { useState, useEffect } from 'react'
import './ArticlesTable.css'

interface Article {
  _id?: string
  titulo: string
  autores: string[]
  ano?: number
  especies?: Array<{ vernacular: string; nomeCientifico: string }>
  local?: string
  pais?: string
  tipo_de_uso?: string
}

interface TableState {
  articles: Article[]
  total: number
  page: number
  limit: number
  search: string
  sortBy: string
  sortOrder: 'asc' | 'desc'
}

export default function ArticlesTable() {
  const [state, setState] = useState<TableState>({
    articles: [],
    total: 0,
    page: 1,
    limit: 20,
    search: '',
    sortBy: 'titulo',
    sortOrder: 'asc'
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetchArticles()
  }, [state.page, state.limit, state.search, state.sortBy, state.sortOrder])

  const fetchArticles = async () => {
    setLoading(true)
    setError(null)

    try {
      let url = `/api/articles?limit=${state.limit}&skip=${(state.page - 1) * state.limit}&sort_by=${state.sortBy}&order=${state.sortOrder === 'desc' ? 'desc' : 'asc'}`

      if (state.search.trim()) {
        url = `/api/articles/search?q=${encodeURIComponent(state.search)}`
      }

      const response = await fetch(url)
      if (response.ok) {
        const data = await response.json()
        setState(prev => ({
          ...prev,
          articles: Array.isArray(data.artigos) ? data.artigos : data,
          total: data.total || data.length || 0
        }))
      }
    } catch (err) {
      setError('Erro ao carregar artigos')
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (articleId: string) => {
    if (!confirm('Deletar este artigo?')) return

    try {
      const response = await fetch(`/api/articles/${articleId}`, { method: 'DELETE' })
      if (response.ok) {
        fetchArticles()
      } else {
        setError('Erro ao deletar artigo')
      }
    } catch (err) {
      setError('Erro ao deletar artigo')
    }
  }

  const handleSearch = (searchTerm: string) => {
    setState(prev => ({ ...prev, search: searchTerm, page: 1 }))
  }

  const handleSort = (column: string) => {
    setState(prev => ({
      ...prev,
      sortBy: column,
      sortOrder: prev.sortBy === column && prev.sortOrder === 'asc' ? 'desc' : 'asc'
    }))
  }

  if (loading && state.articles.length === 0) return <div className="loading">Carregando artigos...</div>
  if (error) return <div className="error-message">{error}</div>
  if (state.articles.length === 0) return <div className="empty-state">Nenhum artigo salvo ainda</div>

  const totalPages = Math.ceil(state.total / state.limit)

  return (
    <div className="articles-table-container">
      <div className="table-header">
        <h3>Artigos Salvos ({state.total})</h3>
        <input
          type="text"
          placeholder="Buscar por titulo, autor, especie..."
          value={state.search}
          onChange={(e) => handleSearch(e.target.value)}
          className="search-input"
        />
      </div>

      <div className="table-wrapper">
        <table className="articles-table">
          <thead>
            <tr>
              <th onClick={() => handleSort('titulo')} className="sortable">
                Titulo {state.sortBy === 'titulo' && (state.sortOrder === 'asc' ? '↑' : '↓')}
              </th>
              <th onClick={() => handleSort('ano')} className="sortable">
                Ano {state.sortBy === 'ano' && (state.sortOrder === 'asc' ? '↑' : '↓')}
              </th>
              <th>Autores</th>
              <th>Especies</th>
              <th>Pais</th>
              <th>Tipo de Uso</th>
              <th>Acoes</th>
            </tr>
          </thead>
          <tbody>
            {state.articles.map((article) => (
              <tr key={article._id}>
                <td className="title-cell">{article.titulo}</td>
                <td>{article.ano || '-'}</td>
                <td className="authors-cell">{article.autores?.slice(0, 2).join(', ')}{article.autores && article.autores.length > 2 ? '...' : ''}</td>
                <td>{article.especies?.length || 0}</td>
                <td>{article.pais || '-'}</td>
                <td>{article.tipo_de_uso || '-'}</td>
                <td>
                  <button onClick={() => handleDelete(article._id || '')} className="btn-delete" title="Deletar">
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="table-footer">
        <div className="pagination">
          <button onClick={() => setState(prev => ({ ...prev, page: prev.page - 1 }))} disabled={state.page === 1}>
            Anterior
          </button>
          <span>Pagina {state.page} de {totalPages}</span>
          <button onClick={() => setState(prev => ({ ...prev, page: prev.page + 1 }))} disabled={state.page === totalPages}>
            Proxima
          </button>
        </div>
      </div>
    </div>
  )
}
