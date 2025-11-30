import { useState } from 'react'
import MainLayout from '../components/MainLayout'
import APIConfiguration from '../components/APIConfiguration'
import DatabaseDownload from '../components/DatabaseDownload'

export default function Settings() {
  const [activeTab, setActiveTab] = useState<'config' | 'backup'>('config')

  return (
    <MainLayout>
      <div className="settings-page">
        <h1>Configuracoes</h1>

        <div className="settings-tabs">
          <button
            className={`tab-button ${activeTab === 'config' ? 'active' : ''}`}
            onClick={() => setActiveTab('config')}
          >
            Configuracao MongoDB
          </button>
          <button
            className={`tab-button ${activeTab === 'backup' ? 'active' : ''}`}
            onClick={() => setActiveTab('backup')}
          >
            Fazer Backup
          </button>
        </div>

        <div className="settings-content">
          {activeTab === 'config' && <APIConfiguration />}
          {activeTab === 'backup' && <DatabaseDownload />}
        </div>
      </div>
    </MainLayout>
  )
}
