# Etnopapers - Guia do Usuário

**Versão**: 2.0
**Idioma**: Português (Brasil)
**Atualizado**: 2024-01-15

---

## Bem-vindo ao Etnopapers!

Etnopapers é um aplicativo desktop especializado em **extração automática de metadados etnobotânicos** a partir de artigos científicos em PDF.

Com Etnopapers você pode:
- ✅ Enviar arquivos PDF de artigos científicos
- ✅ Extrair automaticamente informações sobre plantas medicinais tradicionais
- ✅ Catalogar e organizar seus artigos por país, região, comunidade
- ✅ Fazer backup completo da base de dados
- ✅ Pesquisar e filtrar artigos por espécie, uso, localização

---

## Requisitos do Sistema

### Antes de Começar

**Você precisa ter instalado:**

1. **Ollama** (aplicativo desktop para IA local)
   - Download: https://ollama.com/download
   - Disponível para Windows, macOS e Linux
   - Funciona 100% offline e privado

2. **MongoDB** (banco de dados)
   - Opção 1: Local (localhost)
   - Opção 2: MongoDB Atlas (gratuito com 512 MB)

**Requisitos Mínimos do Computador:**
- 4 GB de RAM
- 5 GB de espaço em disco
- Processador dual-core
- Conexão com internet (apenas para download inicial)

---

## Instalação e Primeiro Acesso

### Passo 1: Baixar o Etnopapers

1. Acesse: https://github.com/edalcin/etnopapers/releases
2. Selecione a versão para seu sistema operacional:
   - **Windows**: `etnopapers-windows-vX.Y.Z.exe`
   - **macOS**: `Etnopapers-macos-vX.Y.Z.zip`
   - **Linux**: `etnopapers-linux-vX.Y.Z`

3. Faça download e salve em um local seguro

### Passo 2: Instalar Ollama

1. Visite https://ollama.com/download
2. Baixe a versão para seu sistema operacional
3. Execute o instalador e complete a instalação
4. **Reinicie seu computador** após a instalação
5. Abra o Ollama para confirmar que está funcionando

### Passo 3: Preparar MongoDB

**Opção A: MongoDB Local (Recomendado para Iniciantes)**

1. Baixe MongoDB Community: https://www.mongodb.com/try/download/community
2. Execute o instalador
3. Durante a instalação, marque a opção "Install MongoDB as a Service"
4. Após terminar, MongoDB iniciará automaticamente

**Opção B: MongoDB Atlas (Recomendado para Equipes)**

1. Crie conta gratuita em https://www.mongodb.com/cloud/atlas
2. Crie um cluster gratuito
3. Obtenha a string de conexão (será usado no próximo passo)

### Passo 4: Executar Etnopapers pela Primeira Vez

1. **Certifique-se de que Ollama está rodando** (você verá o ícone na bandeja)
2. Abra o arquivo `etnopapers.exe` (Windows) ou `Etnopapers` (macOS/Linux)
3. O aplicativo mostrará uma janela de configuração:

```
┌─────────────────────────────────────────┐
│ Configurar MongoDB                      │
│                                         │
│ String de conexão MongoDB:              │
│ [mongodb://localhost:27017/etnopapers]  │
│                                         │
│ [ Testar Conexão ] [ Salvar ]           │
└─────────────────────────────────────────┘
```

4. **MongoDB Local**: Use a string padrão (`mongodb://localhost:27017/etnopapers`)
5. **MongoDB Atlas**: Cole sua string de conexão
6. Clique em "Testar Conexão" para verificar
7. Se ok, clique em "Salvar"
8. O aplicativo iniciará normalmente

---

## Interface Principal

```
┌─────────────────────────────────────────────────────────┐
│ Etnopapers                    Início  Configurações     │
│ ● Ollama Conectado (1 modelos)                          │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ╔═══════════════════════════════════════════════════╗  │
│  ║ Enviar Arquivo PDF                              ║  │
│  ║                                                 ║  │
│  ║ Arraste um PDF aqui ou clique para selecionar   ║  │
│  ║ Máximo 100 MB                                   ║  │
│  ║                                                 ║  │
│  ║        [Selecionar arquivo...]                 ║  │
│  ╚═══════════════════════════════════════════════════╝  │
│                                                         │
│  ┌─ Artigos Salvos ────────────────────────────────┐   │
│  │ Título               │ Ano │ País  │ Espécies   │   │
│  ├──────────────────────┼─────┼───────┼────────────┤   │
│  │ Etnobotânica Brasil  │ 2020│ Brasil│ 5          │   │
│  │ Plantas Medicinais   │ 2019│ Peru  │ 3          │   │
│  └──────────────────────┴─────┴───────┴────────────┘   │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Seções Principais

1. **Cabeçalho**
   - Logo e nome do aplicativo
   - Links de navegação (Início, Configurações)
   - Indicador de status do Ollama

2. **Área de Upload**
   - Arraste e solte um PDF
   - Ou clique para selecionar arquivo
   - Máximo 100 MB por arquivo

3. **Tabela de Artigos**
   - Lista de todos os artigos salvos
   - Ordenar por coluna
   - Buscar por título, autor, espécie
   - Filtrar por ano, país, tipo de uso

---

## Usando o Etnopapers - Passo a Passo

### Tarefa 1: Enviar um PDF e Extrair Metadados

**Tempo estimado: 2-5 minutos**

1. **Prepare seu arquivo PDF**
   - Pode ser um artigo científico em qualquer idioma
   - Deve ser texto extraível (não scaneado em imagem)
   - Tamanho máximo: 100 MB

2. **Envie o PDF**
   - Arraste o arquivo para a caixa "Enviar Arquivo PDF"
   - OU clique na caixa e selecione o arquivo
   - Espere a mensagem "Processando PDF com Ollama..."

3. **Revise os Metadados Extraídos**

   O aplicativo mostará um formulário com:
   - **Título** do artigo
   - **Autores** (lista)
   - **Ano de publicação**
   - **Resumo** (se disponível)
   - **Espécies** (plantas mencionadas)
     - Nome vernacular
     - Nome científico
   - **Tipo de uso** (medicinal, alimentar, etc.)
   - **Metodologia** (entrevistas, observação, etc.)
   - **Localização** (país, estado, município)
   - **Bioma** (Mata Atlântica, Cerrado, etc.)
   - **Status** (rascunho ou finalizado)

4. **Edite se Necessário**
   - Corrija qualquer informação incorreta
   - Adicione espécies que faltaram
   - Complete campos em branco
   - Clique em "+ Adicionar Espécie" para incluir mais plantas

5. **Salve o Artigo**
   - Clique em "Salvar Artigo"
   - Você verá uma confirmação de sucesso
   - O artigo aparecerá na tabela "Artigos Salvos"

### Tarefa 2: Buscar e Filtrar Artigos

**Tempo estimado: 1 minuto**

1. **Buscar por Texto**
   - Digite no campo "Buscar artigos..."
   - Busca em: título, autores, espécies
   - Resultados aparecem em tempo real

2. **Filtrar por Coluna**
   - Clique no cabeçalho de coluna para ordenar
   - Primeira clique: A-Z
   - Segunda clique: Z-A

3. **Paginação**
   - A tabela mostra 20 artigos por página
   - Use os botões "< Anterior" e "Próxima >" para navegar
   - Total de artigos mostrado no rodapé

### Tarefa 3: Deletar um Artigo

**Tempo estimado: 30 segundos**

1. Encontre o artigo na tabela "Artigos Salvos"
2. Clique em "Deletar" no final da linha
3. Confirme que deseja deletar
4. Artigo será removido permanentemente

### Tarefa 4: Fazer Backup da Base de Dados

**Tempo estimado: 1-5 minutos** (depende da quantidade de artigos)

1. Clique em "Configurações" no menu superior
2. Selecione a aba "Fazer Backup"
3. Clique em "Verificar Tamanho" para ver informações:
   - Total de artigos
   - Tamanho da base de dados
4. Clique em "Fazer Download do Backup"
5. Um arquivo `.zip` será baixado (ex: `etnopapers_backup_2024-01-15.zip`)
6. **Guarde este arquivo em um local seguro!**

---

## Resolução de Problemas

### "Ollama está indisponível"

**Problema**: Não consegue processar PDF, mostra erro "Ollama indisponível"

**Solução**:
1. Abra o Ollama (procure na bandeja do sistema)
2. Espere aparecer a mensagem "Running on http://localhost:11434"
3. Tente enviar o PDF novamente

**Se ainda não funcionar**:
```
Windows:
1. Pressione Ctrl+Alt+Delete
2. Vá para "Gerenciador de Tarefas"
3. Procure por "ollama" e clique "Finalizar tarefa"
4. Abra o Ollama novamente

macOS/Linux:
1. Abra Terminal
2. Execute: killall ollama
3. Reinicie o Ollama
```

### "MongoDB indisponível" / "Erro de conexão com banco de dados"

**Problema**: Não consegue salvar artigos, mostra erro de MongoDB

**Solução para MongoDB Local**:
```
Windows:
1. Pressione Windows+R, digite "services.msc"
2. Procure por "MongoDB" e confirme que está "Ativado"
3. Se não estiver, clique em "Iniciar"

macOS:
brew services restart mongodb-community

Linux:
sudo systemctl restart mongodb
```

**Solução para MongoDB Atlas**:
1. Verifique que sua string de conexão está correta
2. Verifique que sua senha está correta (inclui caracteres especiais?)
3. Confira que seu IP está na whitelist do MongoDB Atlas
4. Tente reconectar em Configurações > Configuracao MongoDB

### "PDF inválido" / "Não contém texto extraível"

**Problema**: Arquivo é rejeitado como "não contém texto"

**Causa**: O PDF é uma imagem escaneada em vez de texto real

**Solução**:
1. Tente um PDF diferente
2. Se o PDF foi escaneado, você precisa fazer OCR primeiro:
   - Use ferramentas como Adobe Acrobat, PDFtk, ou Google Drive
   - Converta o PDF com OCR
   - Tente novamente

### Aplicativo muito lento / PDF demora muito para processar

**Problema**: Extraction está lenta (>10 minutos)

**Causas possíveis**:
- Ollama précisa de mais tempo na primeira execução
- Computador não tem RAM suficiente
- Arquivo PDF é muito grande (>50 MB)

**Soluções**:
1. Feche outros aplicativos para liberar RAM
2. Divida PDFs grandes em partes
3. Aguarde mais tempo na primeira execução (Ollama pode estar baixando modelo)
4. Verifique em Configurações se está usando modelo correto

### Onde ficam meus dados salvos?

**Localização dos dados**:

**Configurações (.env)**:
- Windows: `C:\Users\[USUARIO]\.etnopapers\.env`
- macOS: `/Users/[USUARIO]/.etnopapers/.env`
- Linux: `/home/[USUARIO]/.etnopapers/.env`

**Base de dados MongoDB**:
- Se local: `C:\Program Files\MongoDB\Server\[VERSION]\data` (Windows)
- Se Atlas: Armazenado na nuvem MongoDB

**Como fazer backup manualmente**:
1. Use o botão "Fazer Backup" no aplicativo (recomendado)
2. Ou selecione Configurações > Fazer Backup
3. Arquivo ZIP será salvo em sua pasta Downloads

---

## Dicas e Boas Práticas

### 1. Estruture seus dados corretamente

- **País**: Use formato padrão ISO (Brasil, Peru, Colômbia)
- **Estado**: Use siglas (SP, RJ, BA para Brasil)
- **Município**: Nome da cidade
- **Bioma**: Use nomes padronizados (Mata Atlântica, Cerrado, Amazônia)
- **Tipo de uso**: medicinal, alimentar, cosmético, rituais

### 2. Normalize os nomes científicos

- Use base de dados GBIF ou Tropicos
- Inclua a família botânica quando possível
- Exemplo: "Chamomilla recutita (Asteraceae)"

### 3. Revise sempre a extração automática

Olhama é poderosa mas não é perfeita:
- Verifique nomes científicos (confira em GBIF)
- Confirme o país e região
- Revise espécies mencionadas
- Corrija erros antes de salvar

### 4. Faça backup regularmente

- Faça backup semanal se adicionar muitos artigos
- Guarde em local seguro (pen drive, nuvem)
- Use password se armazenar na nuvem

### 5. Organize suas pesquisas

- Use uma convenção de nomenclatura para autores
- Mantenha consistência de país/região
- Padronize tipos de uso
- Use comentários para contexto especial

---

## Keyboard Shortcuts

| Atalho | Função |
|--------|--------|
| `Ctrl+L` (Windows) ou `Cmd+L` (Mac) | Ir para seção de busca |
| `Ctrl+S` | Salvar artigo (quando em edição) |
| `Esc` | Cancelar e voltar |
| `Enter` | Confirmar ação |

---

## Suporte e Documentação

### Documentação Online
- API REST: `/docs/API_DOCUMENTATION.md`
- Guia de Desenvolvedor: `/docs/DEVELOPER_GUIDE.md`
- Especificação Técnica: `/CLAUDE.md`

### Relatar Bugs

Encontrou um problema? Ajude-nos a melhorar!

1. Abra: https://github.com/edalcin/etnopapers/issues
2. Clique em "New issue"
3. Descreva o problema:
   - O que você estava fazendo
   - O que aconteceu
   - Qual era o resultado esperado
   - Seu sistema operacional e versão

### Solicitar Recursos

Tem uma ideia de melhoria?

1. Acesse: https://github.com/edalcin/etnopapers/discussions
2. Clique em "New discussion"
3. Descreva sua ideia

---

## Privacidade e Segurança

### Seus Dados São Privados

✅ **Processamento Local**: Ollama processa PDFs em seu computador
✅ **Sem Envio de Dados**: Seus artigos NUNCA são enviados para servidores
✅ **Banco de Dados Local**: MongoDB fica em seu computador ou sua conta
✅ **Open Source**: Código-fonte público para auditoria

### Recomendações de Segurança

1. **Senha MongoDB Atlas**: Use senha forte se usar nuvem
2. **Backup**: Faça backup regular dos arquivos ZIP
3. **Atualizações**: Mantenha Etnopapers e Ollama atualizados
4. **Firewall**: Configure firewall para bloquear acesso remoto a MongoDB

---

## Próximos Passos

### Para Iniciantes
1. Instale conforme as instruções acima
2. Envie um PDF de teste
3. Revise a extração automática
4. Faça um backup de segurança

### Para Pesquisadores
1. Configure MongoDB Atlas para trabalho em equipe
2. Use as ferramentas de filtro e busca avançada
3. Exporte dados para análise em Excel/R/Python
4. Documente sua metodologia de extração

### Para Desenvolvedores
- Consulte `/docs/DEVELOPER_GUIDE.md`
- API REST disponível em `/docs/API_DOCUMENTATION.md`
- Código-fonte: https://github.com/edalcin/etnopapers

---

## Changelog

### Versão 2.0 (2024-01-15)
- ✅ Integração com Ollama local (100% privado)
- ✅ MongoDB para armazenamento escalável
- ✅ Aplicativo desktop standalone (sem Docker)
- ✅ Interface em português
- ✅ Indicador de status Ollama em tempo real
- ✅ Backup e download de dados
- ✅ Pesquisa e filtro de artigos

### Versão 1.0 (2023-06-01)
- Versão inicial com API Gemini

---

**Versão**: 2.0
**Data**: 2024-01-15
**Idioma**: Português (Brasil)
**Última Atualização**: 2024-01-15

Para suporte, acesse: https://github.com/edalcin/etnopapers/issues
