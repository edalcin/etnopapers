# EtnoPapers

<img src="docs/etnopapers.png" alt="EtnoPapers" width="150" />

**Aplicação Desktop para Extração Automatizada de Metadados Etnobotânicos**

**Versão Atual**: 1.1.0 | [Ver Histórico de Versões](VERSION_HISTORY.md)

> **✨ Novidade na v1.1.0**: Extração de PDFs agora usa conversão para Markdown estruturado, eliminando alucinações de metadados pelo modelo de IA. [Saiba mais](VERSION_HISTORY.md#versão-110---dezembro-2025)

---

## Sobre o EtnoPapers

O EtnoPapers é uma aplicação desktop nativa para Windows desenvolvida para pesquisadores em etnobotânica que precisam catalogar e organizar dados sobre o uso tradicional de plantas por comunidades indígenas e tradicionais.

Com o EtnoPapers, você pode:

- ✨ **Extrair automaticamente** metadados de artigos científicos em PDF usando inteligência artificial
- 📝 **Gerenciar** suas referências com interface completa de edição (criar, visualizar, editar, deletar)
- ☁️ **Sincronizar** seus dados com MongoDB (Atlas ou servidor local) para backup e segurança
- 🔧 **Personalizar** a extração com prompts configuráveis para o modelo de IA
- 🌿 **Catalogar** espécies de plantas, comunidades estudadas, localizações geográficas e metodologias


## Funcionalidades Principais

### 🤖 Extração Inteligente com IA (Melhorada na v1.1!)

Carregue seus artigos em PDF e deixe a inteligência artificial extrair automaticamente com **maior precisão**:

**💡 Nova tecnologia v1.1**: PDFs são convertidos para Markdown estruturado antes da extração, preservando hierarquia de seções, tabelas e formatação. Isso reduz drasticamente alucinações de dados pelo modelo de IA.

- **Metadados obrigatórios**: título (normalizado), autores (formato APA), ano de publicação, resumo (em português brasileiro)
- **Dados etnobotânicos**: espécies de plantas (nomes vernaculares e científicos), tipos de uso, comunidades estudadas
- **Dados geográficos**: país, estado, município, localização específica, bioma
- **Informações do estudo**: fonte de publicação, metodologia aplicada

### 📚 Gestão Completa de Referências

Interface intuitiva para gerenciar todas as suas referências processadas:

- Visualize todas as fichas extraídas em formato de tabela organizada
- Edite qualquer campo dos registros, incluindo adição de novos atributos personalizados
- Crie novos registros manualmente quando necessário
- Delete referências que não são mais necessárias
- Marque fichas para envio ao banco de dados remoto

### ☁️ Sincronização com MongoDB

Mantenha seus dados seguros e acessíveis:

- Conecte-se ao MongoDB Atlas (nuvem) ou servidor local
- Selecione quais fichas deseja enviar para o banco de dados
- Upload automático com confirmação de sucesso
- Registros enviados com sucesso são removidos do armazenamento local
- Avisos automáticos para lembrar você de fazer backup regular

### ⚙️ Configuração Flexível

- Configure o prompt de IA para personalizar a extração de dados
- Informe a URI de conexão com seu MongoDB
- Configurações persistem entre sessões
- Indicadores de status de conexão para IA e banco de dados

---

## Requisitos do Sistema

### Requisitos Obrigatórios

- **Sistema Operacional**: Windows 10 ou superior
- **Provedor de IA em Nuvem**: Chave de API de um dos seguintes:
  - Google Gemini API ([obter chave](https://ai.google.dev/))
  - OpenAI API ([obter chave](https://platform.openai.com/))
  - Anthropic Claude API ([obter chave](https://console.anthropic.com/))
- **Conexão com Internet**: Necessária para:
  - Extração de metadados usando IA em nuvem
  - Sincronização com MongoDB Atlas

### Recomendações

- **MongoDB**: Conta no MongoDB Atlas (gratuita) ou servidor MongoDB local para backup de dados

---

## Instalação

1. **Baixe o EtnoPapers**
   - Acesse a seção de Releases no GitHub
   - Baixe a versão mais recente do instalador
   - Execute o instalador e siga as instruções

2. **Obtenha uma Chave de API de IA**

   Escolha **um** dos seguintes provedores:

   **Opção 1: Google Gemini** (Recomendado - gratuito até 15 requisições/minuto)
   - Acesse [Google AI Studio](https://ai.google.dev/)
   - Crie uma conta Google (se não tiver)
   - Clique em "Get API Key"
   - Copie sua chave de API

   **Opção 2: OpenAI**
   - Acesse [OpenAI Platform](https://platform.openai.com/)
   - Crie uma conta
   - Navegue até "API Keys" e crie uma nova chave
   - Adicione créditos à conta (pago por uso)

   **Opção 3: Anthropic Claude**
   - Acesse [Anthropic Console](https://console.anthropic.com/)
   - Crie uma conta
   - Gere uma API key
   - Adicione créditos à conta (pago por uso)

3. **Configure o EtnoPapers**
   - Abra o EtnoPapers
   - Vá para **Configurações**
   - Selecione seu provedor de IA (Gemini, OpenAI ou Anthropic)
   - Cole sua chave de API
   - Clique em **Salvar**

4. **Configure o MongoDB** (opcional, mas recomendado)
   - Crie uma conta gratuita no MongoDB Atlas ou instale um servidor local
   - Obtenha a URI de conexão do seu banco de dados
   - Configure a URI nas configurações do EtnoPapers

---

## Como Usar

### Primeira Configuração

1. Abra o EtnoPapers
2. Vá para a área de **Configurações**
3. Selecione seu provedor de IA em nuvem (Gemini, OpenAI ou Anthropic)
4. Cole sua chave de API do provedor escolhido
5. Clique em **Salvar** para armazenar as configurações
6. Informe a URI de conexão com o MongoDB (opcional)
7. Teste a conexão com o MongoDB

### Processar um Artigo

1. Na tela principal, clique em **Upload de PDF** ou arraste um arquivo para a área designada
2. Aguarde o processamento - o sistema mostrará uma janela de progresso
3. Após a extração, a janela de edição abrirá automaticamente
4. Revise os dados extraídos pela IA
5. Edite qualquer campo conforme necessário
6. Adicione informações complementares ou atributos personalizados
7. Clique em **Salvar** para armazenar o registro localmente

### Gerenciar Referências

1. Acesse a aba **Registros**
2. Visualize todas as fichas processadas em formato de tabela
3. A lista é atualizada automaticamente sempre que você visita a página
4. Veja as principais informações: Título, Ano, Autores, País e Bioma
5. Selecione registros para editar ou sincronizar com MongoDB

### Sincronizar com MongoDB

1. Na aba **Registros**, selecione os registros que deseja enviar para o banco de dados
2. Clique em **Sincronizar com MongoDB**
3. Aguarde a confirmação de upload
4. Registros enviados com sucesso serão removidos do armazenamento local

> ⚠️ **Importante**: Faça upload regular dos seus dados para o MongoDB para garantir backup e bom desempenho do sistema. O armazenamento local tem limite de registros.

---

## Dados Extraídos

### Campos Obrigatórios

Sempre extraídos de cada artigo:

- **Título** (normalizado)
- **Autores** (formato APA)
- **Ano** de publicação
- **Resumo** (sempre em português brasileiro)

### Campos Opcionais

Extraídos quando disponíveis no documento:

- Fonte de publicação
- **Espécies de plantas** (nome vernacular, nome científico, tipo de uso)
- **Comunidades estudadas** (nome, localização)
- **Dados geográficos** (país, estado, município, local específico)
- **Bioma**
- **Metodologia** do estudo

### Exemplo Real - Dados Extraídos com Qwen 2.5 7B

Abaixo um exemplo real de artigo processado pelo EtnoPapers usando o modelo **Qwen 2.5 7B**:

![Exemplo de dados extraídos](docs/dataSampleJSON.png)

**Neste exemplo:**
- ✅ Título normalizado em inglês
- ✅ 2 autores em formato APA
- ✅ Ano de publicação extraído
- ✅ Resumo completo em português brasileiro (traduzido automaticamente)
- ✅ 2 espécies de plantas identificadas com nomes vernaculares e científicos
- ✅ Comunidade indígena (Xavante) e localização
- ✅ Dados geográficos completos (país, estado, município, bioma)
- ✅ Metodologia documentada
- ✅ **Tempo de extração: 37.97 segundos** (com Qwen 2.5 7B)

### Estrutura de Dados

A estrutura completa dos dados extraídos está documentada em `docs/estrutura.json`.

---

## ☁️ Provedores de IA em Nuvem

### Comparação de Provedores

O EtnoPapers suporta três provedores de IA em nuvem para extração de metadados:

| Aspecto | Google Gemini | OpenAI | Anthropic Claude |
|---------|--------------|--------|------------------|
| **Modelo Padrão** | Gemini 1.5 Flash | GPT-4o-mini | Claude 3.5 Haiku |
| **Custo** | ✅ Gratuito (até 15/min) | 💰 Pago por uso | 💰 Pago por uso |
| **Velocidade** | ⚡⚡⚡⚡ (muito rápido) | ⚡⚡⚡ (rápido) | ⚡⚡⚡⚡ (muito rápido) |
| **Precisão** | ⭐⭐⭐⭐⭐ (excelente) | ⭐⭐⭐⭐⭐ (excelente) | ⭐⭐⭐⭐⭐ (excelente) |
| **Suporte a Português** | ⭐⭐⭐⭐⭐ (nativo) | ⭐⭐⭐⭐⭐ (nativo) | ⭐⭐⭐⭐⭐ (nativo) |
| **Extração Estruturada** | ⭐⭐⭐⭐⭐ (JSON nativo) | ⭐⭐⭐⭐⭐ (JSON nativo) | ⭐⭐⭐⭐⭐ (JSON nativo) |
| **Registro** | Conta Google | Email + cartão | Email + cartão |

### Recomendações por Uso

**Para iniciantes / uso ocasional:**
- **Google Gemini** - Gratuito, rápido, sem necessidade de cartão de crédito
- Ideal para testar o EtnoPapers sem custos
- Limite generoso: até 15 requisições por minuto

**Para uso profissional / alto volume:**
- **OpenAI GPT-4o-mini** - Custo muito baixo, alta qualidade
- Aproximadamente $0.15 por 1000 páginas processadas
- API madura e estável

**Para máxima qualidade:**
- **Anthropic Claude 3.5 Haiku** - Melhor compreensão de contexto científico
- Aproximadamente $0.25 por 1000 páginas processadas
- Excelente para termos técnicos e nomenclatura científica

### Versões Anteriores (OLLAMA Local)

Nas versões anteriores do EtnoPapers (v1.0), a extração era feita usando OLLAMA localmente instalado. Essa abordagem foi descontinuada em favor dos provedores em nuvem pelos seguintes motivos:

- **Desempenho**: IA em nuvem é 50% mais rápida que modelos locais
- **Qualidade**: Menos alucinações e melhor compreensão de contexto
- **Facilidade**: Não requer instalação de software adicional ou GPU
- **Manutenção**: Modelos sempre atualizados pelos provedores

Se você usava OLLAMA anteriormente, seus dados existentes continuam compatíveis. Basta configurar um provedor de IA em nuvem nas Configurações

---

## Tecnologias Utilizadas

- **Framework**: .NET 8.0
- **Interface**: WPF (Windows Presentation Foundation)
- **Arquitetura**: MVVM (Model-View-ViewModel)
- **IA em Nuvem**: Google Gemini, OpenAI ou Anthropic Claude (APIs REST)
- **Armazenamento Local**: JSON
- **Banco de Dados**: MongoDB (Atlas ou local)
- **Linguagem**: C#

---

## Arquitetura

Para entender a arquitetura detalhada do sistema, incluindo diagramas C4 Model e fluxos de trabalho completos, consulte o documento de **[Arquitetura do Sistema (Arquitetrura.md)](Arquitetrura.md)**.

---

## Notas Importantes

- 📄 **PDFs não são armazenados**: Todos os arquivos PDF enviados são descartados após o processamento por questões de armazenamento e privacidade
- 💾 **Backup regular**: Sempre sincronize seus dados com o MongoDB para evitar perda de informações
- 🎯 **Limite de armazenamento local**: Há um número máximo de registros no arquivo local. O sistema avisará quando se aproximar do limite
- ☁️ **Provedor de IA obrigatório**: Configure um provedor de IA em nuvem (Gemini, OpenAI ou Anthropic) antes de processar PDFs
- 🔑 **Segurança da API Key**: Sua chave de API é criptografada usando DPAPI do Windows e armazenada localmente de forma segura
- ✏️ **Edição sempre disponível**: Após a extração, a janela de edição sempre abre para você revisar os dados, independente de estarem completos ou não

---

## Suporte

Para questões, problemas ou sugestões sobre o EtnoPapers, use o [Issues](https://github.com/edalcin/etnopapers/issues).

---

**Versão**: 1.0.0
**Licença**: [A definir]
**Última atualização**: Dezembro 2024
