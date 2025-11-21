import { useMemo } from 'react'
import type { Article } from '@types'
import './Analytics.css'

interface AnalyticsProps {
  articles: Article[]
}

interface StatItem {
  label: string
  value: number
  percentage?: number
  color?: string
}

export default function Analytics({ articles }: AnalyticsProps) {
  const stats = useMemo(() => {
    if (!articles || articles.length === 0) {
      return {
        total: 0,
        byStatus: [],
        byYear: [],
        topSpecies: [],
        topRegions: [],
        topCommunities: [],
      }
    }

    // Total articles
    const total = articles.length

    // By status
    const byStatus: StatItem[] = [
      {
        label: 'Rascunho',
        value: articles.filter(a => a.status === 'rascunho').length,
        color: '#ffc107',
      },
      {
        label: 'Finalizado',
        value: articles.filter(a => a.status === 'finalizado').length,
        color: '#28a745',
      },
    ].map(item => ({
      ...item,
      percentage: Math.round((item.value / total) * 100),
    }))

    // By year (last 10 years)
    const yearsMap = new Map<number, number>()
    articles.forEach(article => {
      yearsMap.set(article.ano_publicacao, (yearsMap.get(article.ano_publicacao) || 0) + 1)
    })
    const byYear = Array.from(yearsMap.entries())
      .sort((a, b) => a[0] - b[0])
      .slice(-10)
      .map(([year, count]) => ({
        label: String(year),
        value: count,
      }))

    // Top species
    const speciesMap = new Map<string, number>()
    articles.forEach(article => {
      if ((article as any).especies) {
        (article as any).especies.forEach((species: string) => {
          speciesMap.set(species, (speciesMap.get(species) || 0) + 1)
        })
      }
    })
    const topSpecies = Array.from(speciesMap.entries())
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5)
      .map(([species, count]) => ({
        label: species,
        value: count,
      }))

    // Top regions
    const regionsMap = new Map<string, number>()
    articles.forEach(article => {
      if ((article as any).regioes) {
        (article as any).regioes.forEach((region: string) => {
          regionsMap.set(region, (regionsMap.get(region) || 0) + 1)
        })
      }
    })
    const topRegions = Array.from(regionsMap.entries())
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5)
      .map(([region, count]) => ({
        label: region,
        value: count,
      }))

    // Top communities
    const communitiesMap = new Map<string, number>()
    articles.forEach(article => {
      if ((article as any).comunidades) {
        (article as any).comunidades.forEach((community: string) => {
          communitiesMap.set(community, (communitiesMap.get(community) || 0) + 1)
        })
      }
    })
    const topCommunities = Array.from(communitiesMap.entries())
      .sort((a, b) => b[1] - a[1])
      .slice(0, 5)
      .map(([community, count]) => ({
        label: community,
        value: count,
      }))

    return {
      total,
      byStatus,
      byYear,
      topSpecies,
      topRegions,
      topCommunities,
    }
  }, [articles])

  if (stats.total === 0) {
    return (
      <div className="analytics">
        <div className="empty-state">
          <p>📊 Nenhum artigo para análise</p>
          <p className="subtitle">Comece fazendo upload de artigos para ver as estatísticas</p>
        </div>
      </div>
    )
  }

  return (
    <div className="analytics">
      <h2>📊 Painel de Análise</h2>

      {/* Key Metrics */}
      <div className="metrics-grid">
        <div className="metric-card">
          <div className="metric-icon">📄</div>
          <div className="metric-content">
            <div className="metric-label">Total de Artigos</div>
            <div className="metric-value">{stats.total}</div>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-icon">✅</div>
          <div className="metric-content">
            <div className="metric-label">Finalizados</div>
            <div className="metric-value">
              {stats.byStatus.find(s => s.label === 'Finalizado')?.value || 0}
            </div>
            <div className="metric-percentage">
              {stats.byStatus.find(s => s.label === 'Finalizado')?.percentage || 0}%
            </div>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-icon">📝</div>
          <div className="metric-content">
            <div className="metric-label">Rascunhos</div>
            <div className="metric-value">
              {stats.byStatus.find(s => s.label === 'Rascunho')?.value || 0}
            </div>
            <div className="metric-percentage">
              {stats.byStatus.find(s => s.label === 'Rascunho')?.percentage || 0}%
            </div>
          </div>
        </div>

        <div className="metric-card">
          <div className="metric-icon">🌿</div>
          <div className="metric-content">
            <div className="metric-label">Espécies Únicas</div>
            <div className="metric-value">{stats.topSpecies.length}</div>
          </div>
        </div>
      </div>

      {/* Charts Grid */}
      <div className="charts-grid">
        {/* Status Distribution */}
        <div className="chart-card">
          <h3>Status de Artigos</h3>
          <div className="stat-list">
            {stats.byStatus.map((item, idx) => (
              <div key={idx} className="stat-item">
                <div className="stat-bar-container">
                  <span className="stat-label">{item.label}</span>
                  <div className="stat-bar-wrapper">
                    <div
                      className="stat-bar"
                      style={{
                        width: `${item.percentage}%`,
                        backgroundColor: item.color,
                      }}
                    />
                  </div>
                  <span className="stat-value">
                    {item.value} ({item.percentage}%)
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Articles by Year */}
        {stats.byYear.length > 0 && (
          <div className="chart-card">
            <h3>Artigos por Ano (Últimos 10 Anos)</h3>
            <div className="stat-list">
              {stats.byYear.map((item, idx) => (
                <div key={idx} className="stat-item">
                  <div className="stat-bar-container">
                    <span className="stat-label">{item.label}</span>
                    <div className="stat-bar-wrapper">
                      <div
                        className="stat-bar"
                        style={{
                          width: `${(item.value / Math.max(...stats.byYear.map(s => s.value))) * 100}%`,
                          backgroundColor: '#667eea',
                        }}
                      />
                    </div>
                    <span className="stat-value">{item.value}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {/* Top Items Grid */}
      <div className="top-items-grid">
        {/* Top Species */}
        {stats.topSpecies.length > 0 && (
          <div className="top-item-card">
            <h3>🌿 Espécies Mais Frequentes</h3>
            <div className="top-item-list">
              {stats.topSpecies.map((item, idx) => (
                <div key={idx} className="top-item">
                  <div className="top-item-rank">{idx + 1}</div>
                  <div className="top-item-content">
                    <div className="top-item-label">{item.label}</div>
                    <div className="top-item-count">{item.value} artigos</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Top Regions */}
        {stats.topRegions.length > 0 && (
          <div className="top-item-card">
            <h3>📍 Regiões Mais Estudadas</h3>
            <div className="top-item-list">
              {stats.topRegions.map((item, idx) => (
                <div key={idx} className="top-item">
                  <div className="top-item-rank">{idx + 1}</div>
                  <div className="top-item-content">
                    <div className="top-item-label">{item.label}</div>
                    <div className="top-item-count">{item.value} artigos</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Top Communities */}
        {stats.topCommunities.length > 0 && (
          <div className="top-item-card">
            <h3>👨‍🤝‍👨 Comunidades Mais Documentadas</h3>
            <div className="top-item-list">
              {stats.topCommunities.map((item, idx) => (
                <div key={idx} className="top-item">
                  <div className="top-item-rank">{idx + 1}</div>
                  <div className="top-item-content">
                    <div className="top-item-label">{item.label}</div>
                    <div className="top-item-count">{item.value} artigos</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
