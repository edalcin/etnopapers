import type { Article } from '@types'
import './ArticleModal.css'

interface ArticleModalProps {
  article: Article | null
  isOpen: boolean
  onClose: () => void
}

export default function ArticleModal({ article, isOpen, onClose }: ArticleModalProps) {
  if (!isOpen || !article) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{article.titulo}</h2>
          <button className="modal-close" onClick={onClose} title="Fechar">
            ✕
          </button>
        </div>

        <div className="modal-body">
          <div className="modal-field">
            <label>Ano de Publicação</label>
            <p>{article.ano || article.ano_publicacao}</p>
          </div>

          <div className="modal-field">
            <label>Autores ({Array.isArray(article.autores) ? article.autores.length : 0})</label>
            {article.autores && Array.isArray(article.autores) && article.autores.length > 0 ? (
              <ul className="authors-list">
                {article.autores.map((author, i) => {
                  // Handle both string and object formats
                  const authorName = typeof author === 'string' ? author : `${author.nome} ${author.sobrenome}`
                  const email = typeof author === 'string' ? undefined : author.email
                  return (
                    <li key={i}>
                      <strong>{authorName}</strong>
                      {email && <span className="email">({email})</span>}
                    </li>
                  )
                })}
              </ul>
            ) : (
              <p className="empty">Nenhum autor registrado</p>
            )}
          </div>

          {article.doi && (
            <div className="modal-field">
              <label>DOI</label>
              <p>
                <a href={`https://doi.org/${article.doi}`} target="_blank" rel="noreferrer">
                  {article.doi}
                </a>
              </p>
            </div>
          )}

          {article.resumo && (
            <div className="modal-field">
              <label>Resumo</label>
              <p className="resumo">{article.resumo}</p>
            </div>
          )}

          <div className="modal-field">
            <label>Status</label>
            <p>
              <span className={`status-badge ${article.status}`}>
                {article.status === 'rascunho' ? '📝 Rascunho' : '✅ Finalizado'}
              </span>
            </p>
          </div>
        </div>

        <div className="modal-footer">
          <button onClick={onClose} className="btn-close">
            Fechar
          </button>
        </div>
      </div>
    </div>
  )
}
