import { useState, useEffect } from 'react'
import { useForm, Controller, useFieldArray } from 'react-hook-form'
import type { ExtractedMetadata, Author } from '@types'
import { speciesAPI } from '@services/api'
import './ManualEditor.css'

interface ManualEditorProps {
  initialData: ExtractedMetadata
  onSave: (data: ExtractedMetadata) => void
  onCancel: () => void
  isDraft?: boolean
}

export default function ManualEditor({
  initialData,
  onSave,
  onCancel,
  isDraft = false,
}: ManualEditorProps) {
  const { control, handleSubmit, watch, formState: { errors } } = useForm<ExtractedMetadata>({
    defaultValues: initialData,
    mode: 'onChange',
  })

  const { fields: authorFields, append: appendAuthor, remove: removeAuthor } = useFieldArray({
    control,
    name: 'autores',
  })

  const { fields: speciesFields, append: appendSpecies, remove: removeSpecies } = useFieldArray({
    control,
    name: 'especies',
  })

  const { fields: locationFields, append: appendLocation, remove: removeLocation } = useFieldArray({
    control,
    name: 'regioes',
  })

  const { fields: communityFields, append: appendCommunity, remove: removeCommunity } = useFieldArray({
    control,
    name: 'comunidades',
  })

  const [speciesValidation, setSpeciesValidation] = useState<Record<string, boolean>>({})
  const [validatingSpecies, setValidatingSpecies] = useState<Record<string, boolean>>({})
  const [saveLoading, setSaveLoading] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)

  const watchedSpecies = watch('especies')

  // Validate species when changed
  useEffect(() => {
    const validateSpeciesNames = async () => {
      if (!watchedSpecies || watchedSpecies.length === 0) return

      for (const species of watchedSpecies) {
        if (!species || speciesValidation[species] !== undefined) continue

        setValidatingSpecies(prev => ({ ...prev, [species]: true }))

        try {
          const response = await speciesAPI.validate(species)
          setSpeciesValidation(prev => ({
            ...prev,
            [species]: response.data.status_validacao === 'validado',
          }))
        } catch (error) {
          setSpeciesValidation(prev => ({
            ...prev,
            [species]: false,
          }))
        } finally {
          setValidatingSpecies(prev => ({ ...prev, [species]: false }))
        }
      }
    }

    validateSpeciesNames()
  }, [watchedSpecies, speciesValidation])

  const onSubmit = async (data: ExtractedMetadata) => {
    setSaveLoading(true)
    setSaveError(null)

    try {
      // Validate required fields
      if (!data.titulo || !data.ano_publicacao) {
        setSaveError('Título e ano de publicação são obrigatórios')
        setSaveLoading(false)
        return
      }

      if (!data.autores || data.autores.length === 0) {
        setSaveError('Pelo menos um autor é obrigatório')
        setSaveLoading(false)
        return
      }

      // Clean up empty arrays
      const cleanData: ExtractedMetadata = {
        ...data,
        autores: data.autores.filter(a => a.nome || a.sobrenome),
        especies: (data.especies || []).filter(s => s && s.nomeCientifico && s.nomeCientifico.trim() !== ''),
        regioes: (data.regioes || []).filter(r => r && r.trim() !== ''),
        comunidades: (data.comunidades || []).filter(c => c && c.trim() !== ''),
      }

      onSave(cleanData)
    } catch (error) {
      setSaveError(error instanceof Error ? error.message : 'Erro ao salvar')
    } finally {
      setSaveLoading(false)
    }
  }

  return (
    <div className="manual-editor">
      <div className="editor-header">
        <h3>✏️ Editar Metadados</h3>
        {isDraft && <span className="draft-badge">Rascunho</span>}
      </div>

      {saveError && (
        <div className="editor-error">
          <p>❌ {saveError}</p>
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="editor-form">
        {/* Bibliographic Section */}
        <section className="editor-section">
          <h4>📚 Informações Bibliográficas</h4>

          <div className="form-group">
            <label htmlFor="titulo">Título *</label>
            <Controller
              name="titulo"
              control={control}
              rules={{ required: 'Título é obrigatório' }}
              render={({ field }) => (
                <>
                  <input
                    {...field}
                    id="titulo"
                    type="text"
                    placeholder="Título do artigo"
                    className={errors.titulo ? 'error' : ''}
                  />
                  {errors.titulo && (
                    <span className="error-message">{errors.titulo.message}</span>
                  )}
                </>
              )}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="ano_publicacao">Ano de Publicação *</label>
              <Controller
                name="ano_publicacao"
                control={control}
                rules={{
                  required: 'Ano é obrigatório',
                  min: { value: 1900, message: 'Ano deve ser >= 1900' },
                  max: { value: new Date().getFullYear(), message: 'Ano não pode estar no futuro' },
                }}
                render={({ field }) => (
                  <>
                    <input
                      {...field}
                      id="ano_publicacao"
                      type="number"
                      placeholder="YYYY"
                      className={errors.ano_publicacao ? 'error' : ''}
                    />
                    {errors.ano_publicacao && (
                      <span className="error-message">{errors.ano_publicacao.message}</span>
                    )}
                  </>
                )}
              />
            </div>

            <div className="form-group">
              <label htmlFor="doi">DOI</label>
              <Controller
                name="doi"
                control={control}
                render={({ field }) => (
                  <input
                    {...field}
                    id="doi"
                    type="text"
                    placeholder="10.xxxx/xxxxx"
                  />
                )}
              />
            </div>
          </div>

          <div className="form-group">
            <label htmlFor="resumo">Resumo</label>
            <Controller
              name="resumo"
              control={control}
              render={({ field }) => (
                <textarea
                  {...field}
                  id="resumo"
                  placeholder="Resumo do artigo"
                  rows={4}
                />
              )}
            />
          </div>
        </section>

        {/* Authors Section */}
        <section className="editor-section">
          <div className="section-header">
            <h4>👥 Autores *</h4>
            <button
              type="button"
              onClick={() => appendAuthor({ nome: '', sobrenome: '' })}
              className="btn-add"
            >
              + Adicionar Autor
            </button>
          </div>

          {authorFields.length === 0 && errors.autores && (
            <p className="error-message">Pelo menos um autor é obrigatório</p>
          )}

          {authorFields.map((field, index) => (
            <div key={field.id} className="array-item">
              <div className="form-row">
                <Controller
                  name={`autores.${index}.nome`}
                  control={control}
                  rules={{ required: 'Nome obrigatório' }}
                  render={({ field }) => (
                    <div className="form-group">
                      <label>Nome</label>
                      <input
                        {...field}
                        type="text"
                        placeholder="Nome"
                      />
                    </div>
                  )}
                />

                <Controller
                  name={`autores.${index}.sobrenome`}
                  control={control}
                  render={({ field }) => (
                    <div className="form-group">
                      <label>Sobrenome</label>
                      <input
                        {...field}
                        type="text"
                        placeholder="Sobrenome"
                      />
                    </div>
                  )}
                />

                <Controller
                  name={`autores.${index}.email`}
                  control={control}
                  render={({ field }) => (
                    <div className="form-group">
                      <label>Email</label>
                      <input
                        {...field}
                        type="email"
                        placeholder="email@exemplo.com"
                      />
                    </div>
                  )}
                />
              </div>

              {authorFields.length > 1 && (
                <button
                  type="button"
                  onClick={() => removeAuthor(index)}
                  className="btn-remove"
                >
                  Remover
                </button>
              )}
            </div>
          ))}
        </section>

        {/* Species Section */}
        <section className="editor-section">
          <div className="section-header">
            <h4>🌿 Espécies de Plantas</h4>
            <button
              type="button"
              onClick={() => appendSpecies({ vernacular: '', nomeCientifico: '' })}
              className="btn-add"
            >
              + Adicionar Espécie
            </button>
          </div>

          {speciesFields.map((field, index) => {
            // Nested array for usosPorComunidade (Clarificação 2025-11-27)
            const { fields: usosFields, append: appendUso, remove: removeUso } = useFieldArray({
              control,
              name: `especies.${index}.usosPorComunidade`,
            })

            return (
              <div key={field.id} className="array-item species-item">
                {/* Species Identification */}
                <div className="form-row">
                  <Controller
                    name={`especies.${index}.vernacular`}
                    control={control}
                    render={({ field }) => (
                      <div className="form-group">
                        <label>Nome Comum</label>
                        <input
                          {...field}
                          type="text"
                          placeholder="ex: Açaí"
                        />
                      </div>
                    )}
                  />

                  <Controller
                    name={`especies.${index}.nomeCientifico`}
                    control={control}
                    render={({ field }) => (
                      <div className="form-group flex-grow">
                        <label>Nome Científico</label>
                        <input
                          {...field}
                          type="text"
                          placeholder="ex: Euterpe oleracea"
                        />
                      </div>
                    )}
                  />
                </div>

                {/* Additional Species Details (Clarificação Q2) */}
                <div className="form-row">
                  <Controller
                    name={`especies.${index}.familia`}
                    control={control}
                    render={({ field }) => (
                      <div className="form-group">
                        <label>Família Botânica</label>
                        <input
                          {...field}
                          type="text"
                          placeholder="ex: Arecaceae"
                        />
                      </div>
                    )}
                  />

                  <Controller
                    name={`especies.${index}.statusValidacao`}
                    control={control}
                    render={({ field }) => (
                      <div className="form-group">
                        <label>Status Validação</label>
                        <select {...field}>
                          <option value="">Nenhum</option>
                          <option value="validado">Validado</option>
                          <option value="naoValidado">Não Validado</option>
                        </select>
                      </div>
                    )}
                  />

                  <Controller
                    name={`especies.${index}.confianca`}
                    control={control}
                    render={({ field }) => (
                      <div className="form-group">
                        <label>Confiança</label>
                        <select {...field}>
                          <option value="">Nenhuma</option>
                          <option value="alta">Alta</option>
                          <option value="media">Média</option>
                          <option value="baixa">Baixa</option>
                        </select>
                      </div>
                    )}
                  />
                </div>

                {watchedSpecies?.[index]?.nomeCientifico && (
                  <div className={`validation-badge ${
                    validatingSpecies[watchedSpecies[index].nomeCientifico] ? 'validating' : ''
                  } ${
                    speciesValidation[watchedSpecies[index].nomeCientifico] ? 'valid' : 'invalid'
                  }`}>
                    {validatingSpecies[watchedSpecies[index].nomeCientifico] ? (
                      <span>⏳ Validando...</span>
                    ) : speciesValidation[watchedSpecies[index].nomeCientifico] ? (
                      <span>✅ Validado</span>
                    ) : (
                      <span>⚠️ Não validado</span>
                    )}
                  </div>
                )}

                {/* Usage by Community (Clarificação Q1, Q3 - All fields optional) */}
                <div className="nested-section">
                  <div className="section-header">
                    <h5>📍 Uso por Comunidade</h5>
                    <button
                      type="button"
                      onClick={() => appendUso({
                        comunidade: { nome: '', tipo: '', país: '', estado: '', município: '' },
                        formaDeUso: '',
                        tipoDeUso: '',
                        propositoEspecifico: '',
                        partesUtilizadas: [],
                      })}
                      className="btn-add-small"
                    >
                      + Adicionar Uso
                    </button>
                  </div>

                  {usosFields.map((usoField, usoIndex) => (
                    <div key={usoField.id} className="uso-item nested-item">
                      <div className="form-row">
                        <Controller
                          name={`especies.${index}.usosPorComunidade.${usoIndex}.comunidade.nome`}
                          control={control}
                          render={({ field }) => (
                            <div className="form-group">
                              <label>Comunidade *</label>
                              <input
                                {...field}
                                type="text"
                                placeholder="ex: Povo Yanomami"
                              />
                            </div>
                          )}
                        />

                        <Controller
                          name={`especies.${index}.usosPorComunidade.${usoIndex}.comunidade.tipo`}
                          control={control}
                          render={({ field }) => (
                            <div className="form-group">
                              <label>Tipo Comunidade</label>
                              <select {...field}>
                                <option value="">Selecione...</option>
                                <option value="indígena">Indígena</option>
                                <option value="quilombola">Quilombola</option>
                                <option value="ribeirinha">Ribeirinha</option>
                                <option value="caiçara">Caiçara</option>
                                <option value="outro">Outro</option>
                              </select>
                            </div>
                          )}
                        />
                      </div>

                      {/* Optional uso fields (Clarificação Q3) */}
                      <div className="form-row">
                        <Controller
                          name={`especies.${index}.usosPorComunidade.${usoIndex}.formaDeUso`}
                          control={control}
                          render={({ field }) => (
                            <div className="form-group">
                              <label>Forma de Uso</label>
                              <input
                                {...field}
                                type="text"
                                placeholder="ex: Chá, Pó, Óleo..."
                              />
                            </div>
                          )}
                        />

                        <Controller
                          name={`especies.${index}.usosPorComunidade.${usoIndex}.tipoDeUso`}
                          control={control}
                          render={({ field }) => (
                            <div className="form-group">
                              <label>Tipo de Uso</label>
                              <input
                                {...field}
                                type="text"
                                placeholder="ex: Medicinal, Alimentar..."
                              />
                            </div>
                          )}
                        />

                        <Controller
                          name={`especies.${index}.usosPorComunidade.${usoIndex}.propositoEspecifico`}
                          control={control}
                          render={({ field }) => (
                            <div className="form-group">
                              <label>Propósito</label>
                              <input
                                {...field}
                                type="text"
                                placeholder="ex: Febre, Tosse..."
                              />
                            </div>
                          )}
                        />
                      </div>

                      <div className="form-row">
                        <Controller
                          name={`especies.${index}.usosPorComunidade.${usoIndex}.metodoPreparacao`}
                          control={control}
                          render={({ field }) => (
                            <div className="form-group">
                              <label>Método Preparação</label>
                              <textarea
                                {...field}
                                placeholder="ex: Fervir por 10 minutos..."
                                rows={2}
                              />
                            </div>
                          )}
                        />

                        <Controller
                          name={`especies.${index}.usosPorComunidade.${usoIndex}.dosagem`}
                          control={control}
                          render={({ field }) => (
                            <div className="form-group">
                              <label>Dosagem</label>
                              <input
                                {...field}
                                type="text"
                                placeholder="ex: 1 colher de chá"
                              />
                            </div>
                          )}
                        />
                      </div>

                      {usosFields.length > 0 && (
                        <button
                          type="button"
                          onClick={() => removeUso(usoIndex)}
                          className="btn-remove-small"
                        >
                          Remover Uso
                        </button>
                      )}
                    </div>
                  ))}
                </div>

                {speciesFields.length > 0 && (
                  <button
                    type="button"
                    onClick={() => removeSpecies(index)}
                    className="btn-remove"
                  >
                    Remover Espécie
                  </button>
                )}
              </div>
            )
          })}
        </section>

        {/* Locations Section */}
        <section className="editor-section">
          <div className="section-header">
            <h4>📍 Regiões</h4>
            <button
              type="button"
              onClick={() => appendLocation('')}
              className="btn-add"
            >
              + Adicionar Região
            </button>
          </div>

          {locationFields.map((field, index) => (
            <div key={field.id} className="array-item">
              <Controller
                name={`regioes.${index}`}
                control={control}
                render={({ field }) => (
                  <div className="form-group flex-grow">
                    <input
                      {...field}
                      type="text"
                      placeholder="ex: Amazônia Legal"
                    />
                  </div>
                )}
              />

              {locationFields.length > 0 && (
                <button
                  type="button"
                  onClick={() => removeLocation(index)}
                  className="btn-remove"
                >
                  Remover
                </button>
              )}
            </div>
          ))}
        </section>

        {/* Communities Section */}
        <section className="editor-section">
          <div className="section-header">
            <h4>👨‍🤝‍👨 Comunidades</h4>
            <button
              type="button"
              onClick={() => appendCommunity('')}
              className="btn-add"
            >
              + Adicionar Comunidade
            </button>
          </div>

          {communityFields.map((field, index) => (
            <div key={field.id} className="array-item">
              <Controller
                name={`comunidades.${index}`}
                control={control}
                render={({ field }) => (
                  <div className="form-group flex-grow">
                    <input
                      {...field}
                      type="text"
                      placeholder="ex: Povo Yanomami"
                    />
                  </div>
                )}
              />

              {communityFields.length > 0 && (
                <button
                  type="button"
                  onClick={() => removeCommunity(index)}
                  className="btn-remove"
                >
                  Remover
                </button>
              )}
            </div>
          ))}
        </section>

        {/* Action Buttons */}
        <div className="editor-actions">
          <button
            type="button"
            onClick={onCancel}
            className="btn btn-secondary"
            disabled={saveLoading}
          >
            Cancelar
          </button>

          <button
            type="submit"
            className="btn btn-primary"
            disabled={saveLoading}
          >
            {saveLoading ? 'Salvando...' : 'Salvar Alterações'}
          </button>
        </div>
      </form>
    </div>
  )
}
