import ArticlesTable from '@components/ArticlesTable'
import './History.css'

export default function History() {
  return (
    <div className="history-page">
      <h2>📚 Histórico de Artigos</h2>
      <p className="subtitle">Visualize, busque e gerencie todos os artigos processados</p>
      <ArticlesTable />
    </div>
  )
}
