import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div className="not-found">
      <h2>404 - Página Não Encontrada</h2>
      <p>A página que você procura não existe.</p>
      <Link to="/">Voltar para início</Link>
    </div>
  )
}
