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
- **OLLAMA**: Serviço de IA local (deve ser instalado separadamente)
  - O OLLAMA é fundamental para o funcionamento do programa
  - Upload de PDFs só é habilitado com OLLAMA conectado
- **Conexão com Internet**: Necessária para sincronização com MongoDB Atlas

### Recomendações

- **GPU (Placa de vídeo dedicada)**: Melhora significativamente o desempenho da extração de dados pela IA
- **MongoDB**: Conta no MongoDB Atlas (gratuita) ou servidor MongoDB local para backup de dados

---

## Instalação

1. **Instale o OLLAMA** (pré-requisito obrigatório)
   - Visite o site oficial do OLLAMA (https://ollama.ai) e siga as instruções de instalação
   - Certifique-se de que o serviço está rodando antes de usar o EtnoPapers
   - **Recomendado**: Baixe o modelo Qwen 2.5 7B: `ollama pull qwen2.5:7b`
   - Veja a seção [Modelo de IA Recomendado](#-modelo-de-ia-recomendado) abaixo para detalhes

2. **Baixe o EtnoPapers**
   - Acesse a seção de Releases no GitHub
   - Baixe a versão mais recente do executável

3. **Execute o EtnoPapers**
   - O aplicativo é distribuído como executável único (single-file)
   - Não requer instalação - basta executar o arquivo `.exe`
   - Todas as dependências estão incluídas no executável

4. **Configure o MongoDB** (opcional, mas recomendado)
   - Crie uma conta gratuita no MongoDB Atlas ou instale um servidor local
   - Obtenha a URI de conexão do seu banco de dados
   - Configure a URI nas configurações do EtnoPapers

---

## Como Usar

### Primeira Configuração

1. Abra o EtnoPapers
2. Vá para a área de **Configurações**
3. Verifique o status de conexão com o OLLAMA (deve estar verde/conectado)
4. Configure o prompt de IA (opcional - um prompt padrão é fornecido)
5. Informe a URI de conexão com o MongoDB (se disponível)
6. Teste a conexão com o MongoDB

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

## 🤖 Modelo de IA Recomendado

### Qwen 2.5 7B (Altamente Recomendado)

Para melhor desempenho da extração de dados etnobotânicos, **recomendamos fortemente o uso do modelo Qwen 2.5 7B**.

**Instalação:**
```bash
ollama pull qwen2.5:7b
```

**Por que Qwen 2.5 7B?**

| Aspecto | Qwen 2.5 7B | Llama 2 7B | Outros |
|--------|-----------|-----------|---------|
| **Tamanho** | 7B (otimizado) | 7B | 13B+ (pesado) |
| **Memória RAM** | ~8-10 GB | ~8-10 GB | 16+ GB |
| **Suporte a Português** | ⭐⭐⭐⭐⭐ (excelente) | ⭐⭐⭐ (bom) | ⭐⭐⭐ (variável) |
| **JSON Estruturado** | ⭐⭐⭐⭐⭐ (especializado) | ⭐⭐⭐ (bom) | ⭐⭐ (fraco) |
| **Velocidade** | ⚡⚡⚡⚡ (muito rápido) | ⚡⚡⚡ (rápido) | ⚡⚡ (lento) |
| **Precisão** | ⭐⭐⭐⭐⭐ (alta) | ⭐⭐⭐⭐ (boa) | ⭐⭐⭐ (média) |
| **Consumo GPU** | ✅ Baixo | ✅ Baixo | ❌ Alto |

**Vantagens Específicas para EtnoPapers:**

1. **Multilíngue Robusto**
   - Treinamento aprofundado em português brasileiro
   - Melhor compreensão de termos científicos e etnobotânicos
   - Tratamento correto de nomes vernaculares e científicos

2. **Especializado em Extração Estruturada**
   - Ótimo para extrair JSON limpo e válido
   - Melhor compreensão de estruturas de dados
   - Menos erros de formatação na resposta

3. **Eficiência de Recursos**
   - Roda em máquinas com 8-10 GB de RAM
   - Tempo de resposta: 15-30 segundos por PDF (vs. 30-60s em outros)
   - Pode rodar com ou sem GPU dedicada

4. **Qualidade de Resposta**
   - Mais "inteligente" na extração de metadados
   - Melhor compreensão de contexto etnobotânico
   - Menos alucinações (falsas informações geradas)

5. **Custo-Benefício**
   - Melhor relação entre tamanho, qualidade e velocidade
   - Reduz frustração de timeouts
   - Menos erros = menos edição manual

**Modelos Alternativos (em ordem de preferência):**

- **Qwen 2.5 14B**: Se sua máquina tem 16+ GB de RAM, melhor ainda! Maior precisão
- **Mistral 7B**: Boa alternativa, um pouco mais rápido que Qwen 2.5 7B
- **Neural Chat 7B**: Alternativa razoável, menos preciso para tarefas estruturadas
- **Llama 2 7B**: Funciona, mas requer mais edição manual de dados extraídos

---

## Tecnologias Utilizadas

- **Framework**: .NET 8.0
- **Interface**: WPF (Windows Presentation Foundation)
- **Arquitetura**: MVVM (Model-View-ViewModel)
- **IA Local**: OLLAMA (API REST) com modelo **Qwen 2.5 7B** recomendado
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
- 🔌 **OLLAMA obrigatório**: Sem o OLLAMA instalado e rodando, não é possível processar PDFs
- 🚀 **Use GPU**: Uma placa de vídeo dedicada melhora muito o desempenho da IA
- ✏️ **Edição sempre disponível**: Após a extração, a janela de edição sempre abre para você revisar os dados, independente de estarem completos ou não

---

## Suporte

Para questões, problemas ou sugestões sobre o EtnoPapers, use o [Issues](https://github.com/edalcin/etnopapers/issues).

---

**Versão**: 1.0.0
**Licença**: [A definir]
**Última atualização**: Dezembro 2024
