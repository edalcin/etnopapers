import { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useOllamaHealth } from '../hooks/useOllamaHealth'
import './MainLayout.css'

interface MainLayoutProps {
  children: ReactNode
}

export default function MainLayout({ children }: MainLayoutProps) {
  const { health } = useOllamaHealth(30000)

  const getStatusIcon = () => {
    switch (health.status) {
      case 'ok':
        return '●'
      case 'checking':
        return '◯'
      case 'unavailable':
        return '●'
      default:
        return '●'
    }
  }

  const getStatusClass = () => {
    switch (health.status) {
      case 'ok':
        return 'status-ok'
      case 'checking':
        return 'status-checking'
      case 'unavailable':
        return 'status-unavailable'
      default:
        return 'status-checking'
    }
  }

  const getStatusText = () => {
    switch (health.status) {
      case 'ok':
        return `Ollama Conectado (${health.modelsCount} modelos)`
      case 'checking':
        return 'Verificando Ollama...'
      case 'unavailable':
        return 'Ollama Indisponível'
      default:
        return 'Status desconhecido'
    }
  }

  const getStatusTooltip = () => {
    if (health.status === 'ok') {
      return `Ollama: ${health.url}\nModelos disponíveis: ${health.modelsCount}`
    } else if (health.error) {
      return `Erro: ${health.error}`
    }
    return ''
  }

  return (
    <div className="main-layout">
      <header className="header">
        <div className="header-content">
          <h1 className="logo">Etnopapers</h1>
          <nav className="nav">
            <Link to="/" className="nav-link">Início</Link>
            <Link to="/settings" className="nav-link">Configurações</Link>
          </nav>
          <div className={`ollama-status ${getStatusClass()}`} title={getStatusTooltip()}>
            <span className="status-icon">{getStatusIcon()}</span>
            <span className="status-text">{getStatusText()}</span>
          </div>
        </div>
      </header>

      <main className="main-content">
        {children}
      </main>

      <footer className="footer">
        <p>Etnopapers v2.0 - Extração de Metadados Etnobotânicos</p>
      </footer>
    </div>
  )
}
