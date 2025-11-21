import APIKeySetup from '@components/APIKeySetup'
import DatabaseDownload from '@components/DatabaseDownload'
import './Settings.css'

export default function Settings() {
  return (
    <div className="settings-page">
      <h2>⚙️ Configurações</h2>
      <p className="subtitle">Gerencie suas configurações e dados</p>

      <div className="settings-grid">
        <section className="settings-section">
          <APIKeySetup />
        </section>

        <section className="settings-section">
          <DatabaseDownload />
        </section>
      </div>
    </div>
  )
}
