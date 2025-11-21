import type { Article } from '@types'

/**
 * Converts an array of articles to CSV format and triggers download
 */
export function exportArticlesToCSV(articles: Article[], filename?: string): void {
  if (!articles || articles.length === 0) {
    console.warn('No articles to export')
    return
  }

  // Define CSV headers
  const headers = [
    'ID',
    'Título',
    'DOI',
    'Ano de Publicação',
    'Autores',
    'Resumo',
    'Status',
    'Editado Manualmente',
    'Data de Processamento',
    'Data de Última Modificação',
  ]

  // Convert articles to CSV rows
  const rows = articles.map(article => [
    article.id,
    escapeCSVField(article.titulo),
    article.doi || '',
    article.ano_publicacao,
    escapeCSVField(article.autores.map(a => `${a.nome} ${a.sobrenome || ''}`).join('; ')),
    escapeCSVField(article.resumo || ''),
    article.status,
    article.editado_manualmente ? 'Sim' : 'Não',
    new Date(article.data_processamento).toLocaleString('pt-BR'),
    new Date(article.data_ultima_modificacao).toLocaleString('pt-BR'),
  ])

  // Combine headers and rows
  const csvContent = [
    headers.join(','),
    ...rows.map(row => row.join(',')),
  ].join('\n')

  // Download file
  downloadCSV(csvContent, filename || `artigos_${new Date().toISOString().split('T')[0]}.csv`)
}

/**
 * Escapes special characters in CSV fields
 */
function escapeCSVField(field: string | number | boolean): string {
  const stringField = String(field)

  // If field contains comma, newline, or quotes, wrap in quotes and escape inner quotes
  if (stringField.includes(',') || stringField.includes('\n') || stringField.includes('"')) {
    return `"${stringField.replace(/"/g, '""')}"`
  }

  return stringField
}

/**
 * Triggers CSV file download in the browser
 */
function downloadCSV(content: string, filename: string): void {
  const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  const url = URL.createObjectURL(blob)

  link.setAttribute('href', url)
  link.setAttribute('download', filename)
  link.style.visibility = 'hidden'

  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)

  URL.revokeObjectURL(url)
}

/**
 * Exports articles with filters applied (e.g., by status, year, etc.)
 */
export function exportFilteredArticles(
  articles: Article[],
  filters: {
    status?: 'rascunho' | 'finalizado'
    minYear?: number
    maxYear?: number
    searchTerm?: string
  },
  filename?: string
): void {
  let filtered = articles

  if (filters.status) {
    filtered = filtered.filter(a => a.status === filters.status)
  }

  if (filters.minYear) {
    filtered = filtered.filter(a => a.ano_publicacao >= filters.minYear!)
  }

  if (filters.maxYear) {
    filtered = filtered.filter(a => a.ano_publicacao <= filters.maxYear!)
  }

  if (filters.searchTerm) {
    const term = filters.searchTerm.toLowerCase()
    filtered = filtered.filter(
      a =>
        a.titulo.toLowerCase().includes(term) ||
        a.autores.some(author =>
          `${author.nome} ${author.sobrenome || ''}`.toLowerCase().includes(term)
        )
    )
  }

  exportArticlesToCSV(
    filtered,
    filename || `artigos_filtrados_${new Date().toISOString().split('T')[0]}.csv`
  )
}
