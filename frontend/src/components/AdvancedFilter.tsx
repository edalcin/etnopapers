import { useState } from 'react'
import type { Article } from '@types'
import './AdvancedFilter.css'

interface AdvancedFilterProps {
  articles: Article[]
  onFilter: (filtered: Article[]) => void
  onClear: () => void
}

export default function AdvancedFilter({ articles, onFilter, onClear }: AdvancedFilterProps) {
  const [showFilters, setShowFilters] = useState(false)
  const [selectedSpecies, setSelectedSpecies] = useState<Set<string>>(new Set())
  const [selectedRegions, setSelectedRegions] = useState<Set<string>>(new Set())
  const [selectedCommunities, setSelectedCommunities] = useState<Set<string>>(new Set())
  const [selectedStatus, setSelectedStatus] = useState<Set<string>>(new Set())
  const [yearRange, setYearRange] = useState<{ min: number; max: number }>({
    min: 1900,
    max: new Date().getFullYear(),
  })

  // Extract unique values from articles
  const getSpecies = (): string[] => {
    const species = new Set<string>()
    articles.forEach(article => {
      if ((article as any).especies) {
        (article as any).especies.forEach((s: string) => species.add(s))
      }
    })
    return Array.from(species).sort()
  }

  const getRegions = (): string[] => {
    const regions = new Set<string>()
    articles.forEach(article => {
      if ((article as any).regioes) {
        (article as any).regioes.forEach((r: string) => regions.add(r))
      }
    })
    return Array.from(regions).sort()
  }

  const getCommunities = (): string[] => {
    const communities = new Set<string>()
    articles.forEach(article => {
      if ((article as any).comunidades) {
        (article as any).comunidades.forEach((c: string) => communities.add(c))
      }
    })
    return Array.from(communities).sort()
  }

  const applyFilters = () => {
    let filtered = articles

    // Filter by species
    if (selectedSpecies.size > 0) {
      filtered = filtered.filter(article => {
        const articleSpecies = (article as any).especies || []
        return articleSpecies.some((s: string) => selectedSpecies.has(s))
      })
    }

    // Filter by region
    if (selectedRegions.size > 0) {
      filtered = filtered.filter(article => {
        const articleRegions = (article as any).regioes || []
        return articleRegions.some((r: string) => selectedRegions.has(r))
      })
    }

    // Filter by community
    if (selectedCommunities.size > 0) {
      filtered = filtered.filter(article => {
        const articleCommunities = (article as any).comunidades || []
        return articleCommunities.some((c: string) => selectedCommunities.has(c))
      })
    }

    // Filter by status
    if (selectedStatus.size > 0) {
      filtered = filtered.filter(article => selectedStatus.has(article.status))
    }

    // Filter by year range
    filtered = filtered.filter(
      article =>
        article.ano_publicacao >= yearRange.min && article.ano_publicacao <= yearRange.max
    )

    onFilter(filtered)
  }

  const clearFilters = () => {
    setSelectedSpecies(new Set())
    setSelectedRegions(new Set())
    setSelectedCommunities(new Set())
    setSelectedStatus(new Set())
    setYearRange({ min: 1900, max: new Date().getFullYear() })
    onClear()
  }

  const toggleSpecies = (species: string) => {
    const newSet = new Set(selectedSpecies)
    if (newSet.has(species)) {
      newSet.delete(species)
    } else {
      newSet.add(species)
    }
    setSelectedSpecies(newSet)
  }

  const toggleRegion = (region: string) => {
    const newSet = new Set(selectedRegions)
    if (newSet.has(region)) {
      newSet.delete(region)
    } else {
      newSet.add(region)
    }
    setSelectedRegions(newSet)
  }

  const toggleCommunity = (community: string) => {
    const newSet = new Set(selectedCommunities)
    if (newSet.has(community)) {
      newSet.delete(community)
    } else {
      newSet.add(community)
    }
    setSelectedCommunities(newSet)
  }

  const toggleStatus = (status: string) => {
    const newSet = new Set(selectedStatus)
    if (newSet.has(status)) {
      newSet.delete(status)
    } else {
      newSet.add(status)
    }
    setSelectedStatus(newSet)
  }

  const activeFilterCount =
    selectedSpecies.size +
    selectedRegions.size +
    selectedCommunities.size +
    selectedStatus.size +
    (yearRange.min !== 1900 || yearRange.max !== new Date().getFullYear() ? 1 : 0)

  return (
    <div className="advanced-filter">
      <button
        onClick={() => setShowFilters(!showFilters)}
        className={`filter-toggle ${activeFilterCount > 0 ? 'active' : ''}`}
      >
        🔍 Filtros Avançados
        {activeFilterCount > 0 && <span className="filter-count">{activeFilterCount}</span>}
      </button>

      {showFilters && (
        <div className="filter-panel">
          {/* Status Filter */}
          <div className="filter-section">
            <h4>Status</h4>
            <div className="filter-options">
              <label className="filter-checkbox">
                <input
                  type="checkbox"
                  checked={selectedStatus.has('rascunho')}
                  onChange={() => toggleStatus('rascunho')}
                />
                <span>Rascunho</span>
              </label>
              <label className="filter-checkbox">
                <input
                  type="checkbox"
                  checked={selectedStatus.has('finalizado')}
                  onChange={() => toggleStatus('finalizado')}
                />
                <span>Finalizado</span>
              </label>
            </div>
          </div>

          {/* Year Range Filter */}
          <div className="filter-section">
            <h4>Intervalo de Anos</h4>
            <div className="year-range">
              <div className="year-input">
                <label>De:</label>
                <input
                  type="number"
                  min="1900"
                  max={yearRange.max}
                  value={yearRange.min}
                  onChange={e =>
                    setYearRange({ ...yearRange, min: Math.max(1900, Number(e.target.value)) })
                  }
                />
              </div>
              <div className="year-input">
                <label>Até:</label>
                <input
                  type="number"
                  min={yearRange.min}
                  max={new Date().getFullYear()}
                  value={yearRange.max}
                  onChange={e =>
                    setYearRange({
                      ...yearRange,
                      max: Math.min(new Date().getFullYear(), Number(e.target.value)),
                    })
                  }
                />
              </div>
            </div>
          </div>

          {/* Species Filter */}
          {getSpecies().length > 0 && (
            <div className="filter-section">
              <h4>Espécies de Plantas</h4>
              <div className="filter-options scrollable">
                {getSpecies().map(species => (
                  <label key={species} className="filter-checkbox">
                    <input
                      type="checkbox"
                      checked={selectedSpecies.has(species)}
                      onChange={() => toggleSpecies(species)}
                    />
                    <span>{species}</span>
                  </label>
                ))}
              </div>
            </div>
          )}

          {/* Regions Filter */}
          {getRegions().length > 0 && (
            <div className="filter-section">
              <h4>Regiões</h4>
              <div className="filter-options scrollable">
                {getRegions().map(region => (
                  <label key={region} className="filter-checkbox">
                    <input
                      type="checkbox"
                      checked={selectedRegions.has(region)}
                      onChange={() => toggleRegion(region)}
                    />
                    <span>{region}</span>
                  </label>
                ))}
              </div>
            </div>
          )}

          {/* Communities Filter */}
          {getCommunities().length > 0 && (
            <div className="filter-section">
              <h4>Comunidades</h4>
              <div className="filter-options scrollable">
                {getCommunities().map(community => (
                  <label key={community} className="filter-checkbox">
                    <input
                      type="checkbox"
                      checked={selectedCommunities.has(community)}
                      onChange={() => toggleCommunity(community)}
                    />
                    <span>{community}</span>
                  </label>
                ))}
              </div>
            </div>
          )}

          {/* Filter Actions */}
          <div className="filter-actions">
            <button onClick={applyFilters} className="btn-apply">
              ✅ Aplicar Filtros
            </button>
            <button onClick={clearFilters} className="btn-clear">
              ❌ Limpar Filtros
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
