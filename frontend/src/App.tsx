import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import { useEffect, useState } from 'react'
import Home from './pages/Home'
import Settings from './pages/Settings'
import ConfigurationDialog from './components/ConfigurationDialog'
import { useAppStore } from './store/appStore'
import './App.css'

function App() {
  const { appState, initialize, setAppState } = useAppStore()
  const [showConfigDialog, setShowConfigDialog] = useState(false)

  useEffect(() => {
    initialize()
  }, [initialize])

  useEffect(() => {
    if (appState === 'unconfigured') {
      setShowConfigDialog(true)
    }
  }, [appState])

  const handleConfigurationComplete = () => {
    setShowConfigDialog(false)
    setAppState('configured')
  }

  if (appState === 'loading') {
    return (
      <div className="app-loading">
        <div className="spinner"></div>
        <p>Iniciando Etnopapers...</p>
      </div>
    )
  }

  if (appState === 'error') {
    return (
      <div className="app-error">
        <h2>Erro ao Iniciar</h2>
        <p>Verifique se o backend esta rodando e tente novamente.</p>
      </div>
    )
  }

  return (
    <>
      <ConfigurationDialog isOpen={showConfigDialog} onConfigured={handleConfigurationComplete} />
      <Router>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/settings" element={<Settings />} />
        </Routes>
      </Router>
    </>
  )
}

export default App
