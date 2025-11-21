import { Article } from '@types'

export const csvExport = {
  exportArticles: (articles: Article[]): string => {
    if (articles.length === 0) return 'No articles'
    const headers = ['ID', 'Título', 'Autores', 'Ano']
    const rows = articles.map((a) => [a._id, a.titulo, a.autores.map(x => x.nome).join(';'), a.ano_publicacao])
    return [headers.join(','), ...rows.map((r) => r.join(','))].join('\n')
  },

  downloadCSV: (csv: string, filename: string = 'articles.csv'): void => {
    const blob = new Blob([csv], { type: 'text/csv' })
    const link = document.createElement('a')
    link.href = URL.createObjectURL(blob)
    link.download = filename
    link.click()
  },
}
