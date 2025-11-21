export default function Home() {
  return (
    <div className="home">
      <h2>Bem-vindo ao Etnopapers</h2>
      <p>Sistema de extração automática de metadados de artigos científicos sobre etnobotânica.</p>
      <div className="features">
        <div className="feature">
          <h3>📄 Upload de PDFs</h3>
          <p>Envie artigos científicos em PDF e deixe o sistema extrair os metadados automaticamente.</p>
        </div>
        <div className="feature">
          <h3>🤖 Extração com IA</h3>
          <p>Utilize Google Gemini, OpenAI ChatGPT ou Anthropic Claude para extração inteligente de dados.</p>
        </div>
        <div className="feature">
          <h3>🌿 Validação Taxonômica</h3>
          <p>Valide nomes científicos de plantas através do GBIF e Tropicos com cache inteligente.</p>
        </div>
        <div className="feature">
          <h3>💾 Banco de Dados Local</h3>
          <p>Todos os dados são armazenados em um banco SQLite portável - sem servidor necessário.</p>
        </div>
      </div>
    </div>
  )
}
