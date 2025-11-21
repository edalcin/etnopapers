import { useState } from 'react'
import APIKeySetup from '@components/APIKeySetup'
import DatabaseDownload from '@components/DatabaseDownload'
import ResearcherProfile from '@components/ResearcherProfile'
import './Settings.css'

export default function Settings() {
  const [activeTab, setActiveTab] = useState<'api' | 'profile' | 'database' | 'about'>('api')

  return (
    <div className="settings-page">
      <h2>⚙️ Configurações</h2>
      <p className="subtitle">Gerencie suas configurações e dados</p>

      {/* Tab Navigation */}
      <div className="settings-tabs">
        <button
          className={`tab-button ${activeTab === 'api' ? 'active' : ''}`}
          onClick={() => setActiveTab('api')}
        >
          🔑 Chaves de API
        </button>
        <button
          className={`tab-button ${activeTab === 'profile' ? 'active' : ''}`}
          onClick={() => setActiveTab('profile')}
        >
          👨‍🔬 Perfil do Pesquisador
        </button>
        <button
          className={`tab-button ${activeTab === 'database' ? 'active' : ''}`}
          onClick={() => setActiveTab('database')}
        >
          💾 Banco de Dados
        </button>
        <button
          className={`tab-button ${activeTab === 'about' ? 'active' : ''}`}
          onClick={() => setActiveTab('about')}
        >
          ℹ️ Sobre
        </button>
      </div>

      {/* Tab Content */}
      <div className="settings-content">
        {activeTab === 'api' && (
          <section className="settings-section">
            <APIKeySetup />
          </section>
        )}

        {activeTab === 'profile' && (
          <section className="settings-section">
            <ResearcherProfile />
          </section>
        )}

        {activeTab === 'database' && (
          <section className="settings-section">
            <DatabaseDownload />
          </section>
        )}

        {activeTab === 'about' && (
          <section className="settings-section about-section">
            <h3>📦 Sobre o Etnopapers</h3>
            <div className="about-content">
              <div className="about-item">
                <h4>Versão</h4>
                <p>1.0.0</p>
              </div>

              <div className="about-item">
                <h4>Funcionalidades</h4>
                <ul>
                  <li>✅ Upload de artigos em PDF</li>
                  <li>✅ Extração automática de metadados com IA (Gemini, ChatGPT, Claude)</li>
                  <li>✅ Edição manual de metadados</li>
                  <li>✅ Detecção de artigos duplicados</li>
                  <li>✅ Validação automática de nomes científicos</li>
                  <li>✅ Auto-salvamento de rascunhos</li>
                  <li>✅ Download do banco de dados completo</li>
                  <li>✅ Personalização de perfil de pesquisador</li>
                </ul>
              </div>

              <div className="about-item">
                <h4>Stack Tecnológico</h4>
                <p><strong>Frontend:</strong> React 18 + TypeScript + Zustand</p>
                <p><strong>Backend:</strong> FastAPI + Python 3.11</p>
                <p><strong>Database:</strong> SQLite com 12 tabelas normalizadas</p>
                <p><strong>API de IA:</strong> Google Gemini, OpenAI, Anthropic Claude</p>
              </div>

              <div className="about-item">
                <h4>Recursos de Validação</h4>
                <ul>
                  <li>🔍 Integração com GBIF para validação de espécies</li>
                  <li>🔄 Fallback para Tropicos API</li>
                  <li>💾 Cache de 30 dias para resultados de validação</li>
                </ul>
              </div>

              <div className="about-item">
                <h4>Dicas de Uso</h4>
                <ul>
                  <li>Configure sua chave de API antes de fazer upload</li>
                  <li>Revise os metadados extraídos antes de salvar</li>
                  <li>Use a edição manual para corrigir erros de extração</li>
                  <li>Personalize seu perfil para melhorar a qualidade da extração</li>
                  <li>Faça backup regular do seu banco de dados</li>
                </ul>
              </div>
            </div>
          </section>
        )}
      </div>
    </div>
  )
}
