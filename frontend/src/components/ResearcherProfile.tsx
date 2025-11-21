import { useState, useEffect } from 'react'
import { useForm, Controller, useFieldArray } from 'react-hook-form'
import type { ResearcherProfile } from '@types'
import './ResearcherProfile.css'

interface ResearcherProfileProps {
  onSave?: (profile: ResearcherProfile) => void
}

const STORAGE_KEY = 'etnopapers_researcher_profile'

export default function ResearcherProfileComponent({ onSave }: ResearcherProfileProps) {
  const { control, handleSubmit, watch, formState: { errors } } = useForm<ResearcherProfile>({
    defaultValues: {
      nome: '',
      instituicao: '',
      foco_pesquisa: '',
      regioes_interesse: [],
      comunidades_interesse: [],
    },
    mode: 'onChange',
  })

  const { fields: regionFields, append: appendRegion, remove: removeRegion } = useFieldArray({
    control,
    name: 'regioes_interesse',
  })

  const { fields: communityFields, append: appendCommunity, remove: removeCommunity } = useFieldArray({
    control,
    name: 'comunidades_interesse',
  })

  const [saveMessage, setSaveMessage] = useState<string | null>(null)
  const [saveLoading, setSaveLoading] = useState(false)

  const watchedData = watch()

  // Load profile from localStorage on mount
  useEffect(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY)
      if (stored) {
        const profile = JSON.parse(stored) as ResearcherProfile
        // TODO: populate form with stored data
        // This is a limitation of react-hook-form - we'd need to use reset() method
      }
    } catch (error) {
      console.error('Failed to load researcher profile:', error)
    }
  }, [])

  const onSubmit = async (data: ResearcherProfile) => {
    setSaveLoading(true)
    setSaveMessage(null)

    try {
      // Validate required fields
      if (!data.nome || !data.instituicao || !data.foco_pesquisa) {
        setSaveMessage('Nome, instituição e foco de pesquisa são obrigatórios')
        setSaveLoading(false)
        return
      }

      // Save to localStorage
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data))

      setSaveMessage('✅ Perfil salvo com sucesso!')
      onSave?.(data)

      // Clear message after 3 seconds
      setTimeout(() => setSaveMessage(null), 3000)
    } catch (error) {
      setSaveMessage('❌ Erro ao salvar perfil')
    } finally {
      setSaveLoading(false)
    }
  }

  return (
    <div className="researcher-profile">
      <div className="profile-header">
        <h3>👨‍🔬 Perfil do Pesquisador</h3>
        <p className="profile-subtitle">
          Personalize suas informações para melhorar a extração de metadados
        </p>
      </div>

      {saveMessage && (
        <div className={`profile-message ${saveMessage.includes('sucesso') ? 'success' : 'error'}`}>
          {saveMessage}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="profile-form">
        {/* Basic Information */}
        <section className="profile-section">
          <h4>ℹ️ Informações Básicas</h4>

          <div className="form-group">
            <label htmlFor="nome">Nome Completo *</label>
            <Controller
              name="nome"
              control={control}
              rules={{ required: 'Nome é obrigatório' }}
              render={({ field }) => (
                <>
                  <input
                    {...field}
                    id="nome"
                    type="text"
                    placeholder="Seu nome completo"
                    className={errors.nome ? 'error' : ''}
                  />
                  {errors.nome && (
                    <span className="error-message">{errors.nome.message}</span>
                  )}
                </>
              )}
            />
          </div>

          <div className="form-group">
            <label htmlFor="instituicao">Instituição *</label>
            <Controller
              name="instituicao"
              control={control}
              rules={{ required: 'Instituição é obrigatória' }}
              render={({ field }) => (
                <>
                  <input
                    {...field}
                    id="instituicao"
                    type="text"
                    placeholder="Universidade / Organização"
                    className={errors.instituicao ? 'error' : ''}
                  />
                  {errors.instituicao && (
                    <span className="error-message">{errors.instituicao.message}</span>
                  )}
                </>
              )}
            />
          </div>

          <div className="form-group">
            <label htmlFor="foco_pesquisa">Foco de Pesquisa *</label>
            <Controller
              name="foco_pesquisa"
              control={control}
              rules={{ required: 'Foco de pesquisa é obrigatório' }}
              render={({ field }) => (
                <>
                  <textarea
                    {...field}
                    id="foco_pesquisa"
                    placeholder="Descreva seu foco de pesquisa e áreas de interesse"
                    rows={4}
                    className={errors.foco_pesquisa ? 'error' : ''}
                  />
                  {errors.foco_pesquisa && (
                    <span className="error-message">{errors.foco_pesquisa.message}</span>
                  )}
                </>
              )}
            />
          </div>
        </section>

        {/* Research Interests */}
        <section className="profile-section">
          <div className="section-header">
            <h4>📍 Regiões de Interesse</h4>
            <button
              type="button"
              onClick={() => appendRegion('')}
              className="btn-add-small"
            >
              + Adicionar
            </button>
          </div>

          {regionFields.map((field, index) => (
            <div key={field.id} className="interest-item">
              <Controller
                name={`regioes_interesse.${index}`}
                control={control}
                render={({ field }) => (
                  <input
                    {...field}
                    type="text"
                    placeholder="ex: Amazônia Legal, Cerrado"
                  />
                )}
              />
              {regionFields.length > 0 && (
                <button
                  type="button"
                  onClick={() => removeRegion(index)}
                  className="btn-remove-small"
                >
                  ✕
                </button>
              )}
            </div>
          ))}
        </section>

        <section className="profile-section">
          <div className="section-header">
            <h4>👨‍🤝‍👨 Comunidades de Interesse</h4>
            <button
              type="button"
              onClick={() => appendCommunity('')}
              className="btn-add-small"
            >
              + Adicionar
            </button>
          </div>

          {communityFields.map((field, index) => (
            <div key={field.id} className="interest-item">
              <Controller
                name={`comunidades_interesse.${index}`}
                control={control}
                render={({ field }) => (
                  <input
                    {...field}
                    type="text"
                    placeholder="ex: Povo Yanomami, Quilombo Ivaporunduva"
                  />
                )}
              />
              {communityFields.length > 0 && (
                <button
                  type="button"
                  onClick={() => removeCommunity(index)}
                  className="btn-remove-small"
                >
                  ✕
                </button>
              )}
            </div>
          ))}
        </section>

        {/* Action Button */}
        <div className="profile-actions">
          <button
            type="submit"
            className="btn-save"
            disabled={saveLoading}
          >
            {saveLoading ? 'Salvando...' : '💾 Salvar Perfil'}
          </button>
        </div>
      </form>
    </div>
  )
}
