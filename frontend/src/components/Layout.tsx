import { Outlet, Link } from 'react-router-dom'
import './Layout.css'

export default function Layout() {
  return (
    <div className="layout">
      <header className="header">
        <div className="container">
          <div className="logo">
            <h1>Etnopapers</h1>
            <p>Sistema de Extração de Metadados de Artigos Etnobotânicos</p>
          </div>
          <nav className="nav">
            <Link to="/">Início</Link>
            <Link to="/upload">Upload</Link>
            <Link to="/history">Histórico</Link>
            <Link to="/settings">Configurações</Link>
          </nav>
        </div>
      </header>

      <main className="main">
        <div className="container">
          <Outlet />
        </div>
      </main>

      <footer className="footer">
        <div className="container">
          <p>&copy; 2025 Etnopapers. Código aberto. Documentação em <code>/docs</code></p>
        </div>
      </footer>
    </div>
  )
}
