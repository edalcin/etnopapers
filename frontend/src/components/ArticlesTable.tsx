import { useEffect, useState } from 'react'
import { useArticlesTable } from '@hooks/useArticlesTable'
import { useArticles, useSetArticles } from '@store/useStore'
import { articlesAPI } from '@services/api'
import { exportArticlesToCSV, exportFilteredArticles } from '@utils/csvExport'
import AdvancedFilter from './AdvancedFilter'
import type { Article } from '@types'
import './ArticlesTable.css'

export default function ArticlesTable() {
  const { articles, loaded } = useArticles()
  const setArticles = useSetArticles()
  const [loading, setLoading] = useState(!loaded)
  const [error, setError] = useState<string | null>(null)
  const [displayArticles, setDisplayArticles] = useState<Article[]>([])

  const { table, globalFilter, setGlobalFilter } = useArticlesTable(displayArticles.length > 0 ? displayArticles : articles)

  useEffect(() => {
    if (!loaded) {
      loadArticles()
    }
  }, [loaded])

  const loadArticles = async () => {
    try {
      setLoading(true)
      const response = await articlesAPI.list(1, 1000)
      setArticles(response.data.items)
      setError(null)
    } catch (err) {
      setError('Erro ao carregar artigos')
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="articles-table">
        <div className="loading">
          <div className="spinner" />
          <p>Carregando artigos...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="articles-table">
        <div className="error">
          <p>❌ {error}</p>
          <button onClick={loadArticles} className="btn-retry">
            Tentar Novamente
          </button>
        </div>
      </div>
    )
  }

  if (articles.length === 0) {
    return (
      <div className="articles-table">
        <div className="empty">
          <p>📭 Nenhum artigo processado ainda</p>
          <p className="subtitle">Clique em "Upload" para começar</p>
        </div>
      </div>
    )
  }

  const handleExportAll = () => {
    exportArticlesToCSV(articles)
  }

  const handleExportFiltered = () => {
    exportFilteredArticles(articles, { searchTerm: globalFilter || undefined })
  }

  const handleAdvancedFilter = (filtered: Article[]) => {
    setDisplayArticles(filtered)
  }

  const handleClearAdvancedFilter = () => {
    setDisplayArticles([])
  }

  const currentArticles = displayArticles.length > 0 ? displayArticles : articles
  const displayCount = currentArticles.length

  return (
    <div className="articles-table">
      <div className="filter-container">
        <AdvancedFilter
          articles={articles}
          onFilter={handleAdvancedFilter}
          onClear={handleClearAdvancedFilter}
        />
      </div>

      <div className="table-header">
        <h3>
          Histórico de Artigos ({displayCount})
          {displayCount !== articles.length && (
            <span className="filter-indicator"> - Filtrado de {articles.length}</span>
          )}
        </h3>
        <div className="table-controls">
          <div className="search-box">
            <input
              type="text"
              placeholder="🔍 Buscar por título ou autor..."
              value={globalFilter}
              onChange={e => setGlobalFilter(e.target.value)}
              className="search-input"
            />
          </div>
          <div className="export-buttons">
            <button
              onClick={handleExportAll}
              className="btn-export"
              title="Exportar todos os artigos"
            >
              📥 Exportar Tudo
            </button>
            {globalFilter && (
              <button
                onClick={handleExportFiltered}
                className="btn-export btn-export-filtered"
                title="Exportar artigos filtrados"
              >
                📤 Exportar Filtrados
              </button>
            )}
          </div>
        </div>
      </div>

      <div className="table-wrapper">
        <table className="articles-table-element">
          <thead>
            {table.getHeaderGroups().map(headerGroup => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map(header => (
                  <th
                    key={header.id}
                    onClick={header.column.getToggleSortingHandler()}
                    className={header.column.getCanSort() ? 'sortable' : ''}
                    style={{ width: `${header.getSize()}px` }}
                  >
                    <div className="header-content">
                      <span>{header.column.columnDef.header as string}</span>
                      <span className="sort-indicator">
                        {header.column.getIsSorted()
                          ? header.column.getIsSorted() === 'desc'
                            ? ' ↓'
                            : ' ↑'
                          : ''}
                      </span>
                    </div>
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody>
            {table.getRowModel().rows.map((row, idx) => (
              <tr key={row.id} className={idx % 2 === 0 ? 'even' : 'odd'}>
                {row.getVisibleCells().map(cell => (
                  <td
                    key={cell.id}
                    style={{ width: `${cell.column.getSize()}px` }}
                    className="cell"
                  >
                    {cell.renderCell()}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="table-footer">
        <div className="pagination-info">
          <span>
            Mostrando {table.getRowModel().rows.length} de {displayCount} artigos
            {displayCount !== articles.length && (
              <span className="filter-note"> (filtrado de {articles.length})</span>
            )}
          </span>
        </div>

        <div className="pagination-controls">
          <button
            onClick={() => table.previousPage()}
            disabled={!table.getCanPreviousPage()}
            className="pagination-btn"
          >
            ← Anterior
          </button>

          <span className="page-info">
            Página {table.getState().pagination.pageIndex + 1} de{' '}
            {table.getPageCount()}
          </span>

          <button
            onClick={() => table.nextPage()}
            disabled={!table.getCanNextPage()}
            className="pagination-btn"
          >
            Próxima →
          </button>

          <select
            value={table.getState().pagination.pageSize}
            onChange={e => table.setPageSize(Number(e.target.value))}
            className="page-size-select"
          >
            {[10, 25, 50, 100].map(size => (
              <option key={size} value={size}>
                {size} itens
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  )
}
