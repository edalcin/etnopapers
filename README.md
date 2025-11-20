# 🌿 Etnopapers

**Sistema de Extração de Metadados de Artigos Etnobotânicos**

Etnopapers é uma ferramenta que automatiza a extração de informações sobre o uso de plantas por comunidades tradicionais a partir de artigos científicos em PDF. O sistema usa inteligência artificial para ler artigos e criar um banco de dados organizado com essas informações.

---

## 📖 O que é Etnobotânica?

Etnobotânica é o estudo da relação entre pessoas e plantas. Pesquisadores documentam como comunidades tradicionais (indígenas, quilombolas, ribeirinhas) usam plantas para medicina, alimentação, rituais e outras finalidades. Esses conhecimentos são publicados em artigos científicos, mas ficam dispersos e difíceis de consultar.

## 🎯 Para que serve o Etnopapers?

O Etnopapers ajuda pesquisadores a:

- **Economizar tempo**: Extrai automaticamente metadados de PDFs que levariam horas para serem catalogados manualmente
- **Organizar conhecimento**: Cria um banco de dados estruturado com informações sobre plantas, comunidades e regiões de estudo
- **Evitar duplicação**: Garante que a mesma espécie de planta não seja cadastrada múltiplas vezes
- **Facilitar pesquisas**: Permite consultar rapidamente quais plantas são usadas por quais comunidades e para quais finalidades

## 💡 Como funciona?

```
1. Você faz upload de um artigo científico (PDF)
         ↓
2. Sistema extrai o texto do PDF
         ↓
3. Inteligência artificial lê e identifica:
   • Espécies de plantas mencionadas
   • Comunidades tradicionais estudadas
   • Regiões geográficas
   • Métodos de pesquisa
         ↓
4. Você revisa e corrige os dados extraídos
         ↓
5. Informações são salvas em banco de dados
```

### 🔒 Sua privacidade está protegida

- **PDFs não são armazenados**: Apenas os metadados extraídos ficam salvos
- **Chave de API segura**: Fica apenas no seu navegador, nunca é enviada ao servidor
- **Sem custos ocultos**: Você usa sua própria chave de API de IA (custo típico: menos de 2 centavos por artigo)

## 🛠️ Tecnologias Usadas

### Para Usuários
- **Interface Web**: Simples e intuitiva, funciona em qualquer navegador moderno
- **Inteligência Artificial**: Usa APIs externas (Google Gemini, ChatGPT ou Claude) - você escolhe
- **Validação Taxonômica**: Confirma nomes científicos de plantas automaticamente

### Para Desenvolvedores
- **Frontend**: React com TypeScript
- **Backend**: Python FastAPI
- **Banco de Dados**: SQLite (arquivo único, fácil de fazer backup)
- **Docker**: Instalação simplificada, funciona em qualquer servidor
- **Sem GPU**: Não precisa de hardware especial

## 🚀 Como começar?

### Opção 1: Instalação Rápida no UNRAID

1. Abra **Community Applications** no UNRAID
2. Busque por "Etnopapers"
3. Clique em "Install"
4. Acesse pelo navegador: `http://seu-servidor:8000`

### Opção 2: Docker (qualquer servidor)

```bash
# Clonar repositório
git clone https://github.com/etnopapers/etnopapers.git
cd etnopapers

# Criar diretório de dados
mkdir data

# Executar com Docker Compose
docker-compose up -d

# Acessar no navegador
# http://localhost:8000
```

### Primeiro Uso

1. **Obter chave de API** (escolha uma opção):
   - Google Gemini (gratuito): https://makersuite.google.com/app/apikey
   - OpenAI ChatGPT (pago): https://platform.openai.com/api-keys
   - Anthropic Claude (pago): https://console.anthropic.com/settings/keys

2. **Configurar no sistema**:
   - Selecione o provedor (Gemini, ChatGPT ou Claude)
   - Cole sua chave de API
   - Sistema valida e salva no navegador

3. **Processar primeiro artigo**:
   - Faça upload de um PDF científico sobre etnobotânica
   - Aguarde 1-2 minutos para extração
   - Revise e salve os metadados

## 📁 Estrutura do Projeto

```
etnopapers/
├── specs/main/               # Documentação técnica
│   ├── spec.md              # Especificação completa do sistema
│   ├── research.md          # Decisões tecnológicas
│   ├── data-model.md        # Estrutura do banco de dados
│   ├── quickstart.md        # Guia de instalação detalhado
│   └── contracts/           # APIs e integrações
├── frontend/                # Interface web (React)
├── backend/                 # Servidor (Python FastAPI)
├── data/                    # Banco de dados SQLite
├── docker-compose.yml       # Configuração Docker
└── README.md               # Este arquivo
```

## 📚 Documentação

- **[Guia de Instalação Completo](specs/main/quickstart.md)**: Instruções detalhadas, troubleshooting e FAQ
- **[Especificação Técnica](specs/main/spec.md)**: Requisitos funcionais, histórias de usuário e critérios de sucesso
- **[Modelo de Dados](specs/main/data-model.md)**: Estrutura do banco SQLite com exemplos de queries
- **[API REST](specs/main/contracts/api-rest.yaml)**: Documentação OpenAPI dos endpoints
- **[Integração com IA](specs/main/contracts/ai-integration.md)**: Como funciona a extração com Gemini/ChatGPT/Claude

## 🌟 Principais Funcionalidades

### ✅ Já Disponível
- Upload de PDFs científicos (até 50 MB)
- Extração automática de metadados via IA
- Interface de edição manual para correções
- Validação taxonômica de espécies de plantas
- Histórico de artigos processados
- Rascunhos automáticos (não perde trabalho se fechar o navegador)
- Exportação de dados para CSV
- Backup simples (copiar um arquivo)

### 🔄 Metadados Extraídos
- **Bibliográficos**: Título, autores, ano, DOI, revista/jornal
- **Geográficos**: Regiões de estudo (país, estado, coordenadas)
- **Sociais**: Comunidades tradicionais estudadas (tipo, nome)
- **Botânicos**: Espécies de plantas (nome científico, vernacular, família, usos)
- **Metodológicos**: Período do estudo, métodos de coleta, tipo de amostragem

## 💰 Custos

### Grátis
- Software Etnopapers (código aberto)
- Servidor próprio (se já tiver UNRAID ou Docker)
- Google Gemini (quota gratuita generosa)

### Custos Possíveis
- **APIs de IA** (após quota gratuita):
  - Gemini: ~$0.01 por artigo
  - ChatGPT: ~$0.02 por artigo
  - Claude: ~$0.003 por artigo (mais econômico)
- **Servidor** (se não tiver): A partir de ~R$ 30/mês (VPS básica)

**Exemplo**: Processar 100 artigos com Gemini = ~R$ 5,00

## 🔐 Segurança e Privacidade

- ✅ Chave de API armazenada **apenas no navegador**
- ✅ PDFs **não são salvos** no servidor
- ✅ Banco de dados **local** (você controla)
- ✅ Sem telemetria ou rastreamento
- ✅ Código aberto (auditável)

## 🤝 Contribuindo

Contribuições são bem-vindas! Veja como ajudar:

1. **Reportar bugs**: Abra uma issue descrevendo o problema
2. **Sugerir melhorias**: Compartilhe suas ideias
3. **Contribuir código**: Fork, desenvolva e envie pull request
4. **Melhorar documentação**: Corrija erros ou adicione exemplos

## 📄 Licença

MIT License - Veja [LICENSE](LICENSE) para detalhes.

## 🙏 Agradecimentos

Este projeto foi desenvolvido para apoiar pesquisadores em etnobotânica, preservando e organizando conhecimentos tradicionais sobre o uso de plantas.

---

## 📞 Suporte

- **Documentação**: [specs/main/](specs/main/)
- **Issues**: [GitHub Issues](https://github.com/etnopapers/etnopapers/issues)
- **Guia Rápido**: [quickstart.md](specs/main/quickstart.md)

---

**Desenvolvido com ❤️ para pesquisadores em etnobotânica**

🤖 *Documentação gerada com [Claude Code](https://claude.com/claude-code)*
