# EtnoPapers

<img src="docs/etnopapers.png" alt="EtnoPapers" width="150" />

**Aplicação Desktop para Extração Automatizada de Metadados Etnobotânicos**

---

## Status do Projeto

🔄 **Em Migração**: Este projeto está sendo refatorado de Electron (Node.js/TypeScript) para **C# WPF** para melhor integração nativa com Windows, melhor performance e melhor experiência do usuário. Todos os recursos e funcionalidades serão preservados na nova versão.

Para acompanhar o progresso da migração, consulte a especificação em `specs/migrate-to-wpf/spec.md`.

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

### 🤖 Extração Inteligente com IA

Carregue seus artigos em PDF e deixe a inteligência artificial extrair automaticamente:

- **Metadados obrigatórios**: título (normalizado), autores (formato APA), ano de publicação, resumo (em português brasileiro)
- **Dados etnobotânicos**: espécies de plantas (nomes vernaculares e científicos), tipos de uso, comunidades estudadas
- **Dados geográficos**: país, estado, município, localização específica, bioma
- **Informações do estudo**: fonte de publicação, metodologia aplicada

### 📚 Gestão Completa de Referências

Interface intuitiva para gerenciar todas as suas referências processadas:

- Visualize todas as fichas extraídas em formato organizado
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

## Instalação

> ⚠️ **Nota**: A versão atual do EtnoPapers está em transição para C# WPF. As instruções de instalação abaixo são para a versão final. Por enquanto, consulte a documentação da especificação em `specs/migrate-to-wpf/` para acompanhar o desenvolvimento.

1. **Instale o OLLAMA** (pré-requisito obrigatório)
   - Visite o site oficial do OLLAMA e siga as instruções de instalação
   - Certifique-se de que o serviço está rodando antes de usar o EtnoPapers

2. **Instale o .NET 8 Runtime**
   - Baixe e instale o .NET 8 Runtime do site oficial da Microsoft
   - Necessário para executar a aplicação C# WPF

3. **Instale o EtnoPapers**
   - Execute o instalador do EtnoPapers (arquivo .msi ou .exe)
   - Siga as instruções do assistente de instalação
   - O instalador verifica e instala automaticamente o .NET 8 se necessário

4. **Configure o MongoDB** (opcional, mas recomendado)
   - Crie uma conta gratuita no MongoDB Atlas ou instale um servidor local
   - Obtenha a URI de conexão do seu banco de dados
   - Configure a URI nas configurações do EtnoPapers

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
2. Aguarde o processamento - o sistema mostrará o status em tempo real
3. Revise os dados extraídos automaticamente
4. Edite qualquer campo conforme necessário
5. Adicione informações complementares ou atributos personalizados
6. Salve o registro

### Gerenciar Referências

1. Acesse a área de **Gerenciamento de Referências**
2. Visualize todas as fichas processadas
3. Clique em qualquer registro para editar
4. Use os botões para criar novo registro ou deletar registros existentes
5. Marque as fichas que deseja sincronizar com o MongoDB

### Sincronizar com MongoDB

1. Selecione os registros que deseja enviar para o banco de dados
2. Clique em **Sincronizar com MongoDB**
3. Aguarde a confirmação de upload
4. Registros enviados com sucesso serão removidos do armazenamento local

> ⚠️ **Importante**: Faça upload regular dos seus dados para o MongoDB para garantir backup e bom desempenho do sistema. O armazenamento local tem limite de registros.

## Dados Extraídos

### Campos Obrigatórios

Sempre extraídos de cada artigo:

- Título (normalizado)
- Autores (formato APA)
- Ano de publicação
- Resumo (em português brasileiro)

### Campos Opcionais

Extraídos quando disponíveis no documento:

- Fonte de publicação
- Espécies de plantas (nome vernacular, nome científico, tipo de uso)
- Comunidades estudadas (nome, localização)
- Dados geográficos (país, estado, município, local específico)
- Bioma
- Metodologia do estudo

## Notas Importantes

- 📄 **PDFs não são armazenados**: Todos os arquivos PDF enviados são descartados após o processamento por questões de armazenamento e privacidade
- 💾 **Backup regular**: Sempre sincronize seus dados com o MongoDB para evitar perda de informações
- 🎯 **Limite de armazenamento local**: Há um número máximo de registros no arquivo local. O sistema avisará quando se aproximar do limite
- 🔌 **OLLAMA obrigatório**: Sem o OLLAMA instalado e rodando, não é possível processar PDFs
- 🚀 **Use GPU**: Uma placa de vídeo dedicada melhora muito o desempenho da IA

---

## Suporte

Para questões, problemas ou sugestões sobre o EtnoPapers, use o [Issues](https://github.com/edalcin/etnopapers/issues).

---

**Versão**: 1.0.0
**Licença**: [A definir]
**Última atualização**: Dezembro 2025
