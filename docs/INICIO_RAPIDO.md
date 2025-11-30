# Guia de Início Rápido Etnopapers

**Versão**: 2.0 Aplicação Desktop Standalone
**Tempo para Setup**: 15 minutos

---

## O que é Etnopapers?

Etnopapers extrai automaticamente metadados etnobotânicos de artigos em PDF:
- Nomes de plantas (vernacular + científico)
- Usos tradicionais (medicinal, alimentar, rituais)
- Comunidades indígenas e localizações
- Biomas e ecossistemas

Todo processamento acontece **localmente e privadamente** no seu computador.

---

## Instalação em 30 Segundos

### Windows
1. Download: https://github.com/edalcin/etnopapers/releases → `etnopapers-windows-vX.Y.Z.exe`
2. Instale **Ollama**: https://ollama.com/download
3. Execute `etnopapers.exe`
4. Entre na string MongoDB (use o padrão para local)
5. Pronto! ✅

### macOS
```bash
# Download das releases
# Ou usando Homebrew (se disponível)
brew install etnopapers

# Instale Ollama
# Download de https://ollama.com/download

# Execute
./Etnopapers.app
```

### Linux
```bash
# Download das releases
chmod +x etnopapers-linux-vX.Y.Z
./etnopapers-linux-vX.Y.Z
```

---

## Requisitos do Sistema

| Requisito | Mínimo | Recomendado |
|-----------|--------|-------------|
| RAM | 4 GB | 8 GB |
| Disco | 5 GB | 10 GB |
| CPU | Dual-core | Quad-core |
| Internet | Download apenas | Funciona offline* |

*Após setup inicial, Etnopapers funciona completamente offline

---

## Passo 1: Instalar Pré-requisitos

### Instalar Ollama (Obrigatório para IA)

1. Visite: https://ollama.com/download
2. Baixe sua versão de OS
3. Execute o instalador
4. Reinicie o computador
5. Abra Ollama (verifique na bandeja do sistema)

**Verificar se Ollama está rodando**:
```bash
curl http://localhost:11434/api/tags
# Deve retornar lista de modelos
```

### Instalar MongoDB (Escolha Uma)

**Opção A: MongoDB Local (Mais Fácil)**
1. Download: https://www.mongodb.com/try/download/community
2. Execute o instalador (Windows/macOS)
3. Ou: `brew install mongodb-community` (macOS)
4. Conexão padrão: `mongodb://localhost:27017/etnopapers`

**Opção B: MongoDB Atlas (Nuvem, Gratuito 512MB)**
1. Crie conta: https://www.mongodb.com/cloud/atlas
2. Crie cluster (tier gratuito)
3. Obtenha string de conexão do dashboard
4. Use essa string no setup Etnopapers

---

## Passo 2: Executar Etnopapers

### Primeira Execução

1. **Garanta que Ollama está rodando** (visível na bandeja do sistema)
2. Duplo-clique em `etnopapers` aplicativo
3. Você verá um diálogo de configuração:

```
Configuração MongoDB

Entre com string de conexão:
[mongodb://localhost:27017/etnopapers]

[Testar Conexão] [Salvar]
```

4. Clique em "Testar Conexão"
5. Se marca verde, clique em "Salvar"
6. Aplicativo inicia automaticamente

---

## Passo 3: Enviar Seu Primeiro PDF

### Fluxo Simples

1. **Encontre um artigo científico** (qualquer PDF com conteúdo etnobotânico)
2. **Arraste e solte** sobre a caixa cinza em Etnopapers, ou clique para selecionar
3. **Aguarde** por "Processando com Ollama..." (geralmente 2-5 minutos)
4. **Revise** metadados extraídos
5. **Corrija** qualquer erro (nomes científicos, localizações)
6. **Salve** no banco de dados

### O que é Extraído?

```
✓ Título e autores do artigo
✓ Ano de publicação e journal
✓ Espécies de plantas (vernacular + científico)
✓ Tipos de uso (medicinal, alimentar, etc.)
✓ Comunidades indígenas
✓ Localizações geográficas (país, estado, cidade)
✓ Biomas (Mata Atlântica, Cerrado, etc.)
✓ Metodologia de pesquisa
```

---

## Passo 4: Gerenciar Sua Coleção

### Buscar Artigos
- Digite na caixa de busca para encontrar por título, autor, espécie
- Resultados aparecem em tempo real

### Filtrar por Ano/País
- Clique em cabeçalho de coluna para ordenar
- Use dropdowns de filtro se disponível

### Deletar Artigos
- Encontre artigo na tabela
- Clique em "Deletar"
- Confirme deleção

### Fazer Download de Backup
1. Clique em "Configurações" (Settings)
2. Selecione "Fazer Backup"
3. Clique em "Fazer Download do Backup"
4. Arquivo ZIP será salvo em Downloads
5. **Guarde este arquivo em um lugar seguro!**

---

## Tarefas Comuns

### Tarefa: Upload em Lote de Múltiplos PDFs

Atualmente Etnopapers processa um PDF por vez:

1. Envie PDF #1 → Revise → Salve
2. Envie PDF #2 → Revise → Salve
3. Repita...

**Futura**: Fila de processamento em lote (planejado para v2.1)

### Tarefa: Exportar Dados

**Opção A**: Download de backup ZIP
- Contém todos os artigos como JSON
- Pode ser importado em outras ferramentas

**Opção B**: Converter para CSV (manual)
1. Abra MongoDB Compass
2. Exporte coleção como CSV
3. Abra em Excel/Google Sheets

**Opção C**: Acesso via API
```bash
curl http://localhost:8000/api/articles?limit=1000 > artigos.json
```

### Tarefa: Colaboração em Equipe

**Opção 1**: Usar MongoDB Atlas
- Configure `MONGO_URI` para conexão Atlas
- Múltiplos usuários acessam mesmo banco de dados
- Backup em nuvem incluído

**Opção 2**: Compartilhar arquivo
- Exporte backup ZIP
- Compartilhe com equipe
- Alguém importa para seu MongoDB

---

## Solução de Problemas

### "Ollama Indisponível"

```
✓ Ollama instalado? https://ollama.com/download
✓ Ollama rodando? (verifique bandeja do sistema)
✓ Tente: ollama serve
✓ Reinicie aplicativo Ollama
```

### "Erro de Conexão MongoDB"

```
✓ MongoDB rodando localmente?
  mongosh (deve conectar)
✓ String de conexão MongoDB Atlas correta?
✓ Rede/firewall bloqueando?
```

### "Processamento de PDF Muito Lento"

```
✓ Arquivo grande (>50 MB)?
  Divida em partes menores
✓ Primeira execução?
  Ollama pode estar baixando modelo (10+ min)
✓ RAM baixa?
  Feche outras aplicações
```

### "PDF Inválido ou Scaneado"

O PDF é uma varredura de imagem, não texto. Solução:
1. Use ferramenta OCR online (Gratuito: https://www.ilovepdf.com/ocr)
2. Envie PDF com OCR para Etnopapers
3. Ou digite os dados manualmente

---

## Dicas para Melhores Resultados

### 1. Verifique Nomes Científicos
- Confira contra GBIF: https://www.gbif.org/
- Use nomes latinos padronizados
- Inclua família botânica quando possível

### 2. Padronize Localizações
- Países: Brasil, Peru, Colômbia
- Estados: Use abreviações oficiais (SP, RJ, BA para Brasil)
- Cidades: Nome oficial completo

### 3. Dados Consistentes
- Usos de plantas: medicinal, alimentar, ritual, cosmético
- Biomas: Use nomes padrão (Mata Atlântica, Cerrado, Amazônia)
- Metodologias: Entrevistas, observação, pesquisa participativa

### 4. Backup Regular
- Baixe backup semanalmente se adiciona muitos artigos
- Armazene em armazenamento em nuvem (Google Drive, OneDrive, Dropbox)
- Mantenha com senha protegida

---

## Próximos Passos

### Para Pesquisadores
1. ✅ Leia Guia do Usuário: `/docs/GUIA_USUARIO.md` (Português)
2. ✅ Explore opções de filtro avançado
3. ✅ Configure colaboração em equipe com MongoDB Atlas

### Para Desenvolvedores
1. ✅ Leia Guia do Desenvolvedor: `/docs/GUIA_DESENVOLVEDOR.md`
2. ✅ Revise documentação da API: `/docs/API_DOCUMENTACAO.md`
3. ✅ Acesse API: http://localhost:8000/docs
4. ✅ GitHub: https://github.com/edalcin/etnopapers

### Para DevOps/Implantação
1. ✅ Construa executáveis: `./build-windows.bat` (ou macOS/Linux)
2. ✅ GitHub Actions trata builds automatizados
3. ✅ Releases disponíveis em: https://github.com/edalcin/etnopapers/releases

---

## Indicadores de Status do Sistema

### Status do Ollama (Cabeçalho)

| Indicador | Significado | Ação |
|-----------|------------|-------|
| 🟢 Verde | Conectado | Pronto para processar PDFs |
| 🟡 Amarelo | Verificando | Aguarde um momento |
| 🔴 Vermelho | Indisponível | Abra aplicativo Ollama |

### Status de Processamento

| Mensagem | Significado |
|----------|-----------|
| "Processando PDF com Ollama..." | Extraindo metadados (2-5 min) |
| "Aguardando verificação Ollama" | Ollama iniciando |
| "Ollama indisponível" | Inicie Ollama da bandeja |

---

## Atalhos de Teclado

| Atalho | Ação |
|--------|------|
| Ctrl+S (Windows) ou Cmd+S (Mac) | Salvar artigo |
| Escape | Cancelar / Fechar diálogo |
| Enter | Confirmar ação |

---

## Localização de Arquivos

### Configuração
- Windows: `C:\Users\[USUARIO]\.etnopapers\.env`
- macOS: `/Users/[USUARIO]/.etnopapers/.env`
- Linux: `/home/[USUARIO]/.etnopapers/.env`

### Banco de Dados
- **Local**: `C:\Program Files\MongoDB\Server\[VERSION]\data` (Windows)
- **Atlas**: Armazenado em nuvem MongoDB

### Backups
- Padrão: Pasta Downloads do seu computador
- Recomendado: Armazene em nuvem (Google Drive, Dropbox)

---

## Benchmarks de Performance

| Operação | Tempo | Notas |
|----------|-------|-------|
| Upload de PDF | <1 seg | Transferência de arquivo |
| Extração de metadados | 2-5 min | Depende do tamanho do PDF |
| Salvar artigo | <1 seg | Escrita no banco de dados |
| Busca (1000+ artigos) | <100 ms | Busca full-text indexada |
| Criação de backup | 1-30 seg | Escala com quantidade de dados |

---

## Teste em Um Minuto

Tente isto agora:

1. **Abra Etnopapers** (se ainda não aberto)
2. **Encontre qualquer PDF** no seu computador
3. **Arraste para** a área de upload
4. **Revise** metadados extraídos
5. **Altere um campo** (país, nome de planta)
6. **Clique Salvar**
7. **Busque** o que você acabou de salvar

Isso é o fluxo de trabalho principal! 🎉

---

## Informações da Versão

- **Versão**: 2.0
- **Data de Lançamento**: 2024-01-15
- **Status**: Estável
- **Licença**: Aberta (verifique repositório)

---

**Precisa de ajuda?** Verifique `/docs/GUIA_USUARIO.md` ou visite as discussões GitHub!

Feliz pesquisa! 🌿
