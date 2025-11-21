import type { Article } from '@types'

export const exportArticlesToCSV = (articles: Article[]): string => {
  if (articles.length === 0) return ''
  const headers = ['ID', 'Título', 'Autores', 'Ano de Publicação', 'DOI', 'Status']
  const rows = articles.map((a) => [
    a._id,
    `"${a.titulo}"`,
    `"${a.autores.map((x) => `${x.nome} ${x.sobrenome}`).join(';')}"`,
    a.ano_publicacao,
    a.doi || '',
    a.status,
  ])
  return [headers.join(','), ...rows.map((r) => r.join(','))].join('\n')
}

export const exportFilteredArticles = (articles: Article[], filters: any): void => {
  const csv = exportArticlesToCSV(articles)
  const dateStr = new Date().toISOString().split('T')[0]
  downloadCSV(csv, `artigos_${dateStr}.csv`)
}

export const downloadCSV = (csv: string, filename: string = 'articles.csv'): void => {
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  const url = URL.createObjectURL(blob)
  link.href = url
  link.download = filename
  link.click()
  URL.revokeObjectURL(url)
}

export const csvExport = {
  exportArticles: exportArticlesToCSV,
  downloadCSV,
  exportFilteredArticles,
}
