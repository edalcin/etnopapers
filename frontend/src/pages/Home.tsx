import { useState, useEffect } from 'react'
import { articlesAPI } from '@services/api'
import { useArticles, useSetArticles } from '@store/useStore'
import ArticleModal from '@components/ArticleModal'
import type { Article } from '@types'
import './Home.css'

export default function Home() {
  const articles = useArticles()
  const setArticles = useSetArticles()

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedArticle, setSelectedArticle] = useState<Article | null>(null)
  const [modalOpen, setModalOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [sortBy, setSortBy] = useState<'titulo' | 'ano' | 'autores'>('ano')
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('desc')

  // Load articles on mount
  useEffect(() => {
    const loadArticles = async () => {
      try {
        setLoading(true)
        const response = await articlesAPI.list(1, 1000)
        setArticles(response.data.data || [])
        setError(null)
      } catch (err) {
        const msg = err instanceof Error ? err.message : 'Erro ao carregar artigos'
        setError(msg)
      } finally {
        setLoading(false)
      }
    }

    loadArticles()
  }, [setArticles])

  // Filter and sort articles
  const filteredArticles = articles
    .filter(article =>
      article.titulo.toLowerCase().includes(searchTerm.toLowerCase()) ||
      article.autores?.some(a =>
        `${a.nome} ${a.sobrenome}`.toLowerCase().includes(searchTerm.toLowerCase())
      ) ||
      String(article.ano_publicacao).includes(searchTerm)
    )
    .sort((a, b) => {
      let compareA: any = a[sortBy]
      let compareB: any = b[sortBy]

      if (sortBy === 'autores') {
        compareA = a.autores?.[0]?.sobrenome || ''
        compareB = b.autores?.[0]?.sobrenome || ''
      }

      if (compareA < compareB) return sortDir === 'asc' ? -1 : 1
      if (compareA > compareB) return sortDir === 'asc' ? 1 : -1
      return 0
    })

  const handleRowClick = (article: Article) => {
    setSelectedArticle(article)
    setModalOpen(true)
  }

  const toggleSort = (column: 'titulo' | 'ano' | 'autores') => {
    if (sortBy === column) {
      setSortDir(sortDir === 'asc' ? 'desc' : 'asc')
    } else {
      setSortBy(column)
      setSortDir('asc')
    }
  }

  const SortIcon = ({ column }: { column: 'titulo' | 'ano' | 'autores' }) => {
    if (sortBy !== column) return <span className="sort-icon">↕️</span>
    return <span className="sort-icon">{sortDir === 'asc' ? '↑' : '↓'}</span>
  }

  if (loading) {
    return (
      <div className="home">
        <div className="loading">
          <div className="spinner" />
          <p>Carregando artigos...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="home">
      <div className="home-header">
        <div className="header-content">
          <img src="/favicon.png" alt="Etnopapers Logo" className="home-logo" width="100" height="100" />
          <div>
            <h2>Bem-vindo ao Etnopapers</h2>
            <p>Sistema de extração automática de metadados de artigos científicos sobre etnobotânica.</p>
          </div>
        </div>
      </div>

      <div className="articles-section">
        <div className="section-header">
          <h3>📚 Artigos na Base de Dados</h3>
          <p className="count">{filteredArticles.length} artigo(s) encontrado(s)</p>
        </div>

        {error && (
          <div className="error-message">
            ❌ Erro ao carregar artigos: {error}
          </div>
        )}

        {filteredArticles.length === 0 ? (
          <div className="empty-state">
            <p>Nenhum artigo encontrado.</p>
            <p className="hint">Comece enviando um PDF na seção de Upload!</p>
          </div>
        ) : (
          <>
            <div className="search-box">
              <input
                type="text"
                placeholder="Buscar por título, autor ou ano..."
                value={searchTerm}
                onChange={e => setSearchTerm(e.target.value)}
                className="search-input"
              />
              <span className="search-icon">🔍</span>
            </div>

            <div className="table-wrapper">
              <table className="articles-table">
                <thead>
                  <tr>
                    <th onClick={() => toggleSort('titulo')} className="sortable">
                      Título <SortIcon column="titulo" />
                    </th>
                    <th onClick={() => toggleSort('ano')} className="sortable">
                      Ano <SortIcon column="ano" />
                    </th>
                    <th onClick={() => toggleSort('autores')} className="sortable">
                      Primeiro Autor <SortIcon column="autores" />
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {filteredArticles.map(article => (
                    <tr
                      key={article._id}
                      onClick={() => handleRowClick(article)}
                      className="article-row"
                    >
                      <td className="titulo">{article.titulo}</td>
                      <td className="ano">{article.ano_publicacao}</td>
                      <td className="autores">
                        {article.autores?.[0]
                          ? `${article.autores[0].nome} ${article.autores[0].sobrenome}`
                          : '-'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </>
        )}
      </div>

      <ArticleModal
        article={selectedArticle}
        isOpen={modalOpen}
        onClose={() => setModalOpen(false)}
      />

      <div className="features">
        <div className="feature">
          <h3>📄 Upload de PDFs</h3>
          <p>Envie artigos científicos em PDF e deixe o sistema extrair os metadados automaticamente.</p>
        </div>
        <div className="feature">
          <h3>🤖 Extração com IA</h3>
          <p>Utilize Google Gemini, OpenAI ChatGPT ou Anthropic Claude para extração inteligente de dados.</p>
        </div>
        <div className="feature">
          <h3>🌿 Validação Taxonômica</h3>
          <p>Valide nomes científicos de plantas através do GBIF e Tropicos com cache inteligente.</p>
        </div>
        <div className="feature">
          <h3>💾 Banco de Dados MongoDB</h3>
          <p>Armazene e consulte dados com MongoDB - escalável, flexível e pronto para nuvem.</p>
        </div>
      </div>
    </div>
  )
}
