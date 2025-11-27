# Especificação da Funcionalidade: Sistema de Extração de Metadados de Artigos Etnobotânicos

**Branch da Funcionalidade**: `main`
**Criado**: 2025-11-20
**Atualizado**: 2025-11-24 (v2.0 - AI Local + Uso Detalhado de Plantas por Comunidades)
**Status**: Em Planejamento
**Requisitos**: 72 Requisitos Funcionais (RF-001 a RF-072)
**Arquitetura**: Sistema de extração de metadados de artigos científicos em PDF sobre etnobotânica usando **modelo de AI local** (Ollama + Qwen2.5-7B-Instruct) com inferência em GPU, rodando em Docker com banco MongoDB. Captura detalhada de **uso de plantas por comunidades tradicionais** com estrutura de forma, tipo e propósito de uso

## Clarifications

### Session 2025-01-23 (v2.0 - Migração para AI Local)

- **Q: Por que migrar de APIs externas para AI local?** → A: Privacidade total (dados nunca saem do servidor), custo fixo (sem custos por requisição), sem limites de quota, latência consistente, e simplicidade (um único modelo em vez de gerenciar múltiplas APIs)
- **Q: Qual modelo de AI será usado?** → A: Qwen2.5-7B-Instruct quantizado (Q4) via Ollama. Escolhido por: excelente português, suporte nativo a JSON estruturado, 4.8 GB de tamanho, e 128K tokens de contexto
- **Q: Quais os requisitos de GPU?** → A: NVIDIA GPU com 6-8 GB VRAM mínimo (RTX 3060 ou superior). Inferência leva 1-3 segundos por artigo
- **Q: Qual o tamanho total do Docker?** → A: ~8.5 GB (MongoDB ~700 MB + Ollama ~2 GB + Modelo ~4.8 GB + Backend ~800 MB + Frontend ~10 MB)
- **Q: E se não houver GPU disponível?** → A: Sistema pode usar CPU mas será muito mais lento (~30-60s por extração). Recomenda-se GPU para produção
- **Q: Como fica a segurança dos dados?** → A: Dados nunca saem do servidor UNRAID. Totalmente privado e offline-capable após download do modelo
- **Q: Banco de dados SQLite ou MongoDB?** → A: Migrado para MongoDB (NoSQL) para melhor flexibilidade de schema e crescimento sem migrações
- **Q: Usuário ainda precisa configurar API keys?** → A: Não! Sistema é totalmente self-contained. Sem configuração de API keys

### Session 2025-11-20

- Q: Qual o limite de tamanho de arquivo PDF? → A: 50 MB por arquivo
- Q: Como tratar PDFs escaneados (sem texto pesquisável)? → A: Tentar processar mas exibir aviso de qualidade reduzida, e após extração mostrar tela de edição para correção/complementação manual dos metadados
- Q: Como garantir unicidade de espécies de plantas no banco de dados? → A: Nome científico (binomial) como chave única, com validação via API externa para obter nome aceito atual, família botânica e autores do nome científico
- Q: O que acontece se usuário fechar janela sem salvar? → A: ~~Salvar automaticamente como rascunho~~ **[ATUALIZADO 2025-11-27]** Reter em localStorage; usuário deve clicar "Salvar" para persistir em MongoDB. Após extração, apresentar botões "Salvar" (finalizar no BD), "Editar" (abrir interface de edição) e "Descartar" (deletar de localStorage)
- Q: Como será feita a extração de metadados? → A: ~~Usando APIs externas (Gemini, ChatGPT, Claude) com chave fornecida pelo usuário~~ **[ATUALIZADO v2.0]** Usando modelo de AI local (Qwen2.5-7B-Instruct) rodando no próprio servidor via Ollama com inferência em GPU. Sem necessidade de API keys
- Q: Como evitar duplicação de artigos na base de dados? → A: Sistema verifica duplicatas após extração de metadados usando DOI (se disponível) ou combinação título+ano+primeiro autor. Se duplicata for detectada, usuário é informado e pode optar por descartar ou sobrescrever o registro existente
- Q: O sistema deve usar contexto do pesquisador para melhorar extração? → A: Sim, pesquisador pode configurar perfil opcional (área de especialização, região de foco, idiomas/línguas indígenas relevantes, comunidades estudadas) que é incluído como contexto no prompt enviado ao Ollama, melhorando precisão da extração

### Session 2025-11-27 (Clarificações de Implementação - Data Model + UX)

- Q: Como estruturar `comunidade` dentro de `usosPorComunidade`? → A: Como objeto inline denormalizado {nome, tipo, país, estado, município}. Sem referência externa. Dados da comunidade copiados em cada registro de uso.
- Q: Como representar confiança de validação taxonômica? → A: Dois campos separados: `statusValidacao` ("validado" | "naoValidado") para resultado da consulta API, E `confianca` ("alta" | "media" | "baixa") para qualidade do match. Ambos presentes em cada espécie.
- Q: Quais campos em `usosPorComunidade` são obrigatórios? → A: Todos os 7 campos são OPCIONAIS. Registros podem conter qualquer subconjunto de {forma, tipo, propósito, partes, dosagem, metodoPreparacao, origem}. Sistema captura o que está disponível no artigo.
- Q: Quando persistir metadados extraídos: localStorage vs MongoDB? → A: localStorage após extração (sem auto-save ao BD). Botão "Salvar" → MongoDB status="finalizado"; "Editar" → abre editor; "Descartar" → deleta de localStorage. Browser close = perda de dados em localStorage.
- Q: Prompt de extração deve suportar múltiplos modelos Ollama? → A: Não. Prompt hardcoded para Qwen2.5-7B-Instruct. Nenhum suporte a parameterização ou fallback de modelos. Simplicidade conforme Constitution IV.

## Cenários de Usuário e Testes *(obrigatório)*

### História de Usuário 1 - Upload e Extração Básica de PDF (Prioridade: P0) **[ATUALIZADO v2.0]**

Um pesquisador acessa o sistema através de uma página web e faz o upload de um artigo científico em PDF sobre etnobotânica. O sistema extrai automaticamente os metadados disponíveis no artigo usando **modelo de AI local** (Qwen2.5-7B-Instruct) rodando no servidor UNRAID com inferência em GPU. A página principal exibe uma tabela com os artigos já processados, permitindo busca e ordenação. Após o processamento do novo PDF (1-3 segundos), o sistema exibe um resumo estruturado dos metadados extraídos na mesma página, sem armazenar o arquivo PDF original. O pesquisador também pode fazer download completo do banco de dados MongoDB para backup ou análise externa. **Não há necessidade de configurar API keys - o sistema é totalmente self-contained e privado.**

**Por que esta prioridade**: Esta é a funcionalidade central do sistema. Sem ela, nenhum outro componente tem valor. Representa o MVP completo e entregável. A visualização dos artigos já processados e a funcionalidade de download são essenciais para o pesquisador ter visibilidade completa dos dados coletados. **Atualizado para P0 (prioridade máxima) pois inclui setup de infraestrutura de AI local.**

**Teste Independente**: Pode ser totalmente testado fazendo upload de um único PDF e verificando se os metadados são extraídos e exibidos corretamente. Entrega valor imediato ao permitir que usuários extraiam dados de artigos sem processamento manual. A tabela e funcionalidade de download podem ser testadas após processar múltiplos artigos.

**Cenários de Aceitação**:

1. ~~**Dado** que o usuário acessa o sistema pela primeira vez, **Quando** o usuário visualiza a página de upload, **Então** o sistema solicita que o usuário selecione um provedor de IA~~ **[REMOVIDO v2.0 - Sem API keys]**
2. ~~**Dado** que o usuário inseriu uma chave de API válida, **Quando** o usuário confirma, **Então** a chave é armazenada apenas no navegador~~ **[REMOVIDO v2.0 - Sem API keys]**
3. ~~**Dado** que o usuário já configurou sua chave de API, **Quando** o usuário retorna ao sistema, **Então** a chave é recuperada do navegador~~ **[REMOVIDO v2.0 - Sem API keys]**
4. **Dado** que o usuário está na página principal do sistema, **Quando** o usuário seleciona um arquivo PDF válido e clica em upload, **Então** o sistema processa o arquivo usando **AI local (Ollama + Qwen2.5)** no servidor e exibe os metadados extraídos em formato estruturado (inferência: 1-3 segundos)
5. **Dado** que o processamento foi concluído, **Quando** o sistema exibe os resultados, **Então** os metadados incluem título, autores, ano, resumo, DOI, local de publicação, região do estudo, comunidades envolvidas, espécies de plantas (nomes científicos e vernaculares), período do estudo, métodos de coleta de dados e tipo de amostragem
6. **Dado** que os metadados foram extraídos, **Quando** o usuário visualiza o resumo, **Então** o sistema indica claramente quais campos não puderam ser extraídos do artigo E apresenta três botões de ação: "Salvar", "Editar" e "Descartar"
7. **Dado** que o usuário visualiza os metadados extraídos, **Quando** o usuário clica em "Salvar", **Então** os metadados são salvos finalizados no banco de dados **MongoDB** e o PDF não é armazenado
8. **Dado** que o usuário visualiza os metadados extraídos, **Quando** o usuário clica em "Editar", **Então** o sistema abre a interface de edição manual permitindo correções e complementações
9. **Dado** que o usuário visualiza os metadados extraídos, **Quando** o usuário clica em "Descartar", **Então** o sistema exclui os dados extraídos e solicita confirmação antes de descartar
10. **Dado** que o usuário fechou a janela/navegador sem clicar em nenhum botão, **Quando** o usuário retorna ao sistema **na mesma sessão do navegador**, **Então** os dados extraídos ainda estão em localStorage e podem ser salvos em MongoDB clicando "Salvar", ou descartados clicando "Descartar". **[CLARIFICADO 2025-11-27: localStorage apenas — se navegador fechado/cookies apagados, dados são perdidos]**
11. **Dado** que o usuário está na página principal, **Quando** a página carrega, **Então** o sistema exibe uma tabela com todos os artigos já processados, mostrando colunas: título, ano, autores, status, data de processamento e número de espécies
12. **Dado** que a tabela de artigos está visível, **Quando** o usuário clica no cabeçalho de uma coluna, **Então** a tabela é ordenada por aquela coluna (crescente/decrescente alternadamente)
13. **Dado** que a tabela de artigos está visível, **Quando** o usuário digita texto no campo de busca/filtro, **Então** a tabela filtra em tempo real mostrando apenas artigos que contenham o texto buscado em qualquer campo visível
14. **Dado** que o usuário está na página principal, **Quando** o usuário clica no botão "Download Base de Dados", **Então** o sistema inicia o download de um **arquivo ZIP** contendo backup completo do banco MongoDB (formato BSON/JSON) com todos os artigos e metadados
15. **Dado** que o usuário clicou em "Download Base de Dados", **Quando** o download é concluído, **Então** o arquivo tem nome descritivo incluindo data (ex: etnopapers_backup_20251120.zip) e pode ser importado em qualquer instância MongoDB ou visualizado como JSON

---

### História de Usuário 2 - Validação e Tratamento de Erros no Upload (Prioridade: P2)

Um pesquisador tenta fazer upload de diferentes tipos de arquivos e o sistema valida adequadamente, rejeitando arquivos não-PDF ou PDFs corrompidos, fornecendo mensagens de erro claras e orientando o usuário sobre como corrigir o problema. O sistema também detecta artigos duplicados e informa o pesquisador, permitindo que ele decida se deseja descartar ou sobrescrever o registro existente.

**Por que esta prioridade**: Melhora significativamente a experiência do usuário ao fornecer feedback claro sobre problemas, mas não é essencial para a funcionalidade básica. Pode ser implementada e testada após o fluxo principal estar funcionando.

**Teste Independente**: Pode ser testado independentemente tentando upload de arquivos inválidos (imagens, documentos corrompidos, formatos incorretos) e verificando se as mensagens de erro apropriadas são exibidas. Também pode ser testado fazendo upload do mesmo artigo duas vezes.

**Cenários de Aceitação**:

1. **Dado** que o usuário está na página de upload, **Quando** o usuário tenta fazer upload de um arquivo que não é PDF, **Então** o sistema rejeita o arquivo e exibe mensagem explicativa informando que apenas arquivos PDF são aceitos
2. **Dado** que o usuário selecionou um arquivo PDF corrompido, **Quando** o sistema tenta processar o arquivo, **Então** o sistema detecta o problema e informa ao usuário que o arquivo está corrompido ou ilegível
3. **Dado** que ocorreu um erro durante o processamento, **Quando** o sistema exibe a mensagem de erro, **Então** a mensagem é clara, em português brasileiro, e sugere possíveis soluções
4. **Dado** que o sistema extraiu metadados de um PDF, **Quando** o sistema verifica duplicatas no banco de dados, **Então** o sistema compara usando DOI (se disponível) ou combinação de título + ano + primeiro autor
5. **Dado** que o artigo extraído já existe no banco de dados (duplicata detectada), **Quando** o sistema identifica a duplicata, **Então** o sistema exibe mensagem informando que o artigo já foi processado anteriormente, mostrando a data do processamento original e o status (finalizado/rascunho)
6. **Dado** que uma duplicata foi detectada, **Quando** a mensagem de duplicata é exibida, **Então** o sistema apresenta duas opções: "Descartar" (ignora novo upload) e "Sobrescrever" (substitui registro existente pelos novos metadados extraídos)
7. **Dado** que o usuário escolheu "Descartar" na detecção de duplicata, **Quando** a ação é confirmada, **Então** os metadados recém-extraídos são descartados e o registro original permanece inalterado no banco de dados
8. **Dado** que o usuário escolheu "Sobrescrever" na detecção de duplicata, **Quando** a ação é confirmada, **Então** o sistema substitui o registro existente pelos novos metadados e atualiza o timestamp de última modificação

---

### História de Usuário 3 - Edição e Correção Manual de Metadados (Prioridade: P2)

Após a extração automática dos metadados, um pesquisador revisa os dados extraídos e identifica informações faltantes, imprecisas ou campos vazios. O sistema fornece uma interface de edição que permite ao pesquisador corrigir e complementar manualmente os metadados antes de salvá-los no banco de dados.

**Por que esta prioridade**: Essencial para garantir qualidade dos dados, especialmente em PDFs escaneados ou de baixa qualidade. Permite que usuários corrijam erros da IA e preencham lacunas, aumentando a confiabilidade do banco de dados. Pode ser implementada após o fluxo básico de extração estar funcional.

**Teste Independente**: Pode ser testado fazendo upload de um PDF, aguardando a extração, e então editando manualmente campos específicos na interface, verificando se as alterações são persistidas corretamente no banco de dados.

**Cenários de Aceitação**:

1. **Dado** que o sistema concluiu a extração de metadados, **Quando** os resultados são exibidos, **Então** o sistema apresenta uma interface de edição permitindo modificar todos os campos de metadados
2. **Dado** que o PDF era escaneado ou de baixa qualidade, **Quando** os metadados são exibidos, **Então** o sistema exibe um aviso informando que a qualidade da extração pode estar reduzida e recomenda revisão manual
3. **Dado** que o usuário está na tela de edição, **Quando** o usuário modifica um ou mais campos e salva as alterações, **Então** os metadados atualizados são armazenados no banco de dados **MongoDB**
4. **Dado** que o usuário editou metadados, **Quando** o usuário visualiza o histórico posteriormente, **Então** os valores editados manualmente são exibidos (não os valores originais extraídos pela IA)

---

### História de Usuário 4 - Consulta e Visualização de Histórico (Prioridade: P3)

Um pesquisador deseja revisar artigos processados anteriormente e acessa uma interface para visualizar o histórico de extrações realizadas, podendo filtrar e buscar por diferentes campos de metadados.

**Por que esta prioridade**: Adiciona valor significativo ao permitir reuso dos dados extraídos, mas não é necessária para o processamento inicial. É uma funcionalidade de conveniência que pode ser adicionada após o sistema básico estar operacional.

**Teste Independente**: Pode ser testada independentemente após vários PDFs terem sido processados, verificando se o histórico é exibido corretamente e se os filtros funcionam adequadamente.

**Cenários de Aceitação**:

1. **Dado** que múltiplos artigos foram processados e armazenados, **Quando** o usuário acessa a página de histórico, **Então** o sistema exibe uma lista de todos os artigos processados com seus metadados principais
2. **Dado** que o usuário está visualizando o histórico, **Quando** o usuário busca por um termo específico (ex: nome de espécie, região, comunidade), **Então** o sistema filtra os resultados exibindo apenas artigos que correspondem ao critério de busca
3. **Dado** que o usuário selecionou um artigo do histórico, **Quando** o usuário clica para visualizar detalhes, **Então** o sistema exibe todos os metadados extraídos daquele artigo específico

---

### Casos Extremos

- O que acontece quando o PDF está em formato de imagem (escaneado) sem texto pesquisável? O sistema processa o arquivo mas exibe aviso sobre qualidade reduzida e direciona o usuário para a tela de edição manual para revisão e complementação
- Como o sistema lida com artigos em múltiplos idiomas (inglês, português, espanhol)?
- O que acontece quando o PDF excede o limite de 50 MB? O sistema rejeita imediatamente com mensagem clara informando o tamanho máximo permitido
- Como o sistema trata artigos que não seguem a estrutura tradicional de papers científicos?
- O que acontece quando campos obrigatórios (como título ou autores) não podem ser identificados?
- Como o sistema processa artigos com tabelas complexas ou dados em formatos não-textuais?
- O que acontece quando o servidor fica sem espaço em disco para o banco de dados?
- O que acontece se o usuário não forneceu uma chave de API válida? O sistema exibe mensagem de erro clara solicitando que o usuário configure sua chave de API
- O que acontece se a chave de API fornecida está inválida ou expirada? O sistema exibe mensagem de erro específica e permite ao usuário atualizar a chave
- O que acontece se a API de IA selecionada (Gemini/ChatGPT/Claude) estiver temporariamente indisponível? O sistema exibe mensagem de erro e sugere tentar novamente mais tarde
- O que acontece se o usuário fechar a janela sem clicar em nenhum botão (Salvar/Editar/Descartar)? O sistema salva automaticamente os metadados como rascunho que pode ser recuperado posteriormente
- O que acontece se a API de taxonomia botânica estiver offline ou inacessível? O sistema permite salvamento dos dados mas marca o status de validação taxonômica como "não validado"
- Como o sistema trata nomes científicos sinônimos ou desatualizados? A API retorna o nome aceito atual e o sistema armazena ambos (extraído e validado)
- Como o sistema protege a chave de API do usuário? A chave nunca é enviada ao servidor, apenas armazenada no localStorage do navegador e usada diretamente pelo frontend para chamadas à API
- O que acontece se o usuário tentar fazer upload de um artigo que já está no banco de dados? O sistema detecta duplicata após extração usando DOI ou título+ano+autor, exibe mensagem informativa com detalhes do registro existente, e permite que usuário escolha entre descartar ou sobrescrever
- Como o sistema detecta duplicatas se o artigo não tem DOI? Sistema usa combinação de título + ano de publicação + primeiro autor como chave de comparação alternativa
- O que acontece se um artigo for ligeiramente modificado (nova versão do mesmo artigo)? Sistema detecta como duplicata se título e autores forem idênticos; usuário pode sobrescrever para atualizar metadados
- Como o sistema trata duplicatas em rascunhos? Rascunhos também são verificados para evitar duplicação; se duplicata de rascunho for detectada, sistema sugere finalizar o rascunho existente ao invés de criar novo
- O que acontece se não houver artigos processados ainda? Tabela exibe mensagem amigável "Nenhum artigo processado ainda. Faça upload do primeiro PDF!" e botão de download fica desabilitado
- Como a tabela se comporta com muitos artigos (ex: 1000+ artigos)? Sistema implementa paginação automática (50 artigos por página) para manter performance
- O que acontece se o usuário filtrar a tabela e não houver resultados? Sistema exibe mensagem "Nenhum artigo encontrado com esse filtro" e permite limpar o filtro facilmente
- Como funciona o download se o banco de dados for muito grande (ex: 100MB+)? Sistema gera arquivo e inicia download normalmente; navegador gerencia o progresso do download
- O que acontece se o download do banco de dados falhar no meio? Usuário pode tentar novamente clicando no botão de download; arquivo é gerado a cada requisição
- Como o sistema garante que o arquivo baixado não está corrompido? Backend usa checksum SQLite PRAGMA integrity_check antes de disponibilizar download
- Usuário pode baixar o banco de dados enquanto processa um novo artigo? Sim, download é independente do processamento; ambos podem ocorrer simultaneamente

## Requisitos *(obrigatório)*

### Requisitos Funcionais

- **RF-001**: O sistema DEVE fornecer uma interface web com funcionalidade de upload de arquivos PDF
- ~~**RF-002**: O sistema DEVE permitir que o usuário selecione entre provedores de IA~~ **[REMOVIDO v2.0 - AI local]**
- ~~**RF-003**: O sistema DEVE solicitar ao usuário que forneça sua chave de API~~ **[REMOVIDO v2.0 - Sem API keys]**
- ~~**RF-004**: O sistema DEVE armazenar a chave de API fornecida APENAS no localStorage~~ **[REMOVIDO v2.0 - Sem API keys]**
- ~~**RF-005**: O sistema NÃO DEVE enviar ou armazenar a chave de API do usuário no servidor~~ **[REMOVIDO v2.0 - Sem API keys]**
- **RF-006**: O sistema DEVE processar arquivos PDF usando **modelo de AI local (Qwen2.5-7B-Instruct via Ollama)** rodando no servidor com inferência em GPU **[ATUALIZADO v2.0]**
- **RF-007**: O sistema DEVE extrair os seguintes metadados quando disponíveis no artigo: título, ano de publicação, lista de autores, resumo/abstract, DOI (Digital Object Identifier), local/jornal de publicação, região geográfica do estudo, comunidades tradicionais envolvidas, espécies de plantas identificadas (nomes científicos e vernaculares), ano ou período em que o estudo foi conduzido, métodos de coleta de dados das comunidades, e tipo de amostragem utilizado
- **RF-008**: O sistema DEVE armazenar os metadados extraídos em um banco de dados **MongoDB (NoSQL)** **[ATUALIZADO v2.0 de SQLite para MongoDB]**
- **RF-009**: O sistema DEVE exibir um resumo estruturado dos metadados extraídos após o processamento do PDF
- **RF-010**: O sistema NÃO DEVE armazenar os arquivos PDF após a extração dos metadados
- **RF-011**: O sistema DEVE indicar claramente quais metadados não foram encontrados ou não puderam ser extraídos de cada artigo
- **RF-012**: O sistema DEVE ser empacotado e executado em um container Docker
- **RF-013**: O sistema DEVE ser compatível com instalação em servidor UNRAID **com GPU NVIDIA (6-8 GB VRAM mínimo)** **[ATUALIZADO v2.0]**
- **RF-014**: O sistema DEVE aceitar **URI de conexão MongoDB (MONGO_URI)** e **URL do serviço Ollama (OLLAMA_URL)** como variáveis de ambiente durante a criação do container Docker **[ATUALIZADO v2.0]**
- **RF-014a**: **[NOVO v2.0]** O sistema DEVE executar **frontend React + backend FastAPI + Ollama em um único container Docker** (exceto MongoDB, que será conectado via variável de ambiente MONGO_URI como serviço externo). MongoDB pode estar em 3 configurações suportadas: (1) container MongoDB separado no mesmo host UNRAID (hostname `mongo`), (2) MongoDB em máquina diferente na rede local (IP/hostname configurável), ou (3) MongoDB Atlas (cloud) com connection string mongodb+srv://. O sistema DEVE funcionar identicamente em qualquer dessas 3 configurações—não há preferência de implementação
- **RF-014b**: **[NOVO v2.0]** Para GPU NVIDIA no UNRAID, o container Docker DEVE:
  - Configurar `--runtime=nvidia` nos parâmetros extras do container
  - Definir variável de ambiente `NVIDIA_VISIBLE_DEVICES=all` para exposição de GPU
- **RF-015**: Toda a documentação do sistema DEVE ser gerada e mantida em português brasileiro
- **RF-016**: O sistema DEVE operar exclusivamente na branch main, sem branches de desenvolvimento separadas
- **RF-017**: O sistema DEVE rejeitar arquivos PDF maiores que 50 MB com mensagem de erro clara informando o limite de tamanho
- **RF-018**: O sistema DEVE fornecer interface de edição manual permitindo aos usuários corrigir e complementar metadados extraídos
- **RF-019**: O sistema DEVE processar PDFs escaneados (formato de imagem) mas DEVE exibir aviso informando que a qualidade da extração pode estar reduzida
- **RF-020**: Ao detectar PDF escaneado ou extração de baixa qualidade, o sistema DEVE direcionar o usuário automaticamente para a tela de edição manual após exibir os resultados
- **RF-021**: O sistema DEVE garantir unicidade de espécies de plantas no banco de dados usando o nome científico (binomial) como chave primária
- ~~**RF-022**: O sistema DEVE validar a chave de API fornecida~~ **[REMOVIDO v2.0 - Sem API keys]**
- ~~**RF-023**: O sistema DEVE exibir mensagens de erro claras quando a chave de API for inválida~~ **[REMOVIDO v2.0 - Sem API keys]**
- ~~**RF-024**: O sistema DEVE permitir ao usuário visualizar, editar ou remover a chave de API~~ **[REMOVIDO v2.0 - Sem API keys]**
- **RF-025**: O sistema DEVE validar nomes científicos de plantas extraídos consultando API de taxonomia botânica (GBIF Species API primária, Tropicos API como fallback) para obter o nome aceito atual
- **RF-025a**: **[NOVO v2.0]** O sistema DEVE implementar `TaxonomyService` (backend) que: (a) Consulta GBIF Species API com timeout 5s; (b) Se GBIF timeout/indisponível, tenta Tropicos API; (c) Retorna resultado com nome aceito, família, autores, e confiança (alta=100% match, média=fuzzy match, baixa=nenhuma match); (d) Registra "não validado" se ambas APIs falharem
- **RF-026**: O sistema DEVE armazenar automaticamente família botânica e autores do nome científico obtidos da API de taxonomia
- **RF-027**: Quando múltiplos artigos mencionam a mesma espécie (mesmo nome científico), o sistema DEVE reutilizar o registro existente ao invés de criar duplicata
- **RF-028**: Se a API de taxonomia estiver indisponível, o sistema DEVE permitir salvamento dos metadados mas DEVE marcar o status de validação como "não validado"
- **RF-029**: O sistema DEVE implementar cache local de consultas à API de taxonomia (em memória, TTL 30 dias) para reduzir requisições repetidas e melhorar performance offline
- **RF-030**: Após extração de metadados, o sistema DEVE apresentar interface com três botões de ação claramente visíveis: "Salvar", "Editar" e "Descartar"
- **RF-031**: Ao clicar em "Salvar", o sistema DEVE armazenar os metadados com status "finalizado" no banco de dados
- **RF-032**: Ao clicar em "Editar", o sistema DEVE abrir interface de edição manual com todos os campos editáveis
- **RF-033**: Ao clicar em "Descartar", o sistema DEVE solicitar confirmação e então excluir permanentemente os dados extraídos
- **RF-034**: Se o usuário fechar navegador/janela sem selecionar ação, o sistema DEVE reter os metadados extraídos em localStorage do navegador. **[CLARIFICADO 2025-11-27: localStorage apenas; sem auto-save em MongoDB]**
- **RF-035**: O sistema DEVE permitir que metadados retidos em localStorage sejam recuperados e salvos em MongoDB durante a mesma sessão do navegador, clicando no botão "Salvar"
- **RF-036**: O sistema DEVE detectar artigos duplicados após a extração de metadados, antes de permitir salvamento
- **RF-037**: O sistema DEVE verificar duplicatas usando DOI como critério primário (se DOI estiver disponível nos metadados extraídos)
- **RF-038**: Se DOI não estiver disponível, o sistema DEVE verificar duplicatas usando a combinação de título + ano de publicação + primeiro autor
- **RF-039**: Quando duplicata for detectada, o sistema DEVE exibir mensagem informativa contendo: título do artigo existente, data do processamento original, status (finalizado/rascunho), e duas opções de ação ("Descartar" e "Sobrescrever")
- **RF-040**: Ao escolher "Descartar" em duplicata detectada, o sistema DEVE descartar os metadados recém-extraídos e manter o registro original inalterado
- **RF-041**: Ao escolher "Sobrescrever" em duplicata detectada, o sistema DEVE substituir completamente o registro existente pelos novos metadados extraídos e atualizar o timestamp de última modificação
- **RF-042**: O sistema DEVE verificar duplicatas tanto em artigos finalizados quanto em rascunhos pendentes
- **RF-043**: O sistema DEVE exibir na página principal uma tabela com todos os artigos já processados e armazenados no banco de dados
- **RF-044**: A tabela de artigos DEVE exibir as seguintes colunas: título, ano de publicação, lista de autores, status (finalizado/rascunho), data de processamento e número de espécies mencionadas
- **RF-045**: Cada coluna da tabela DEVE ser ordenável (clicável), alternando entre ordem crescente e decrescente
- **RF-046**: A tabela DEVE incluir campo de busca/filtro que permite filtrar artigos em tempo real por qualquer conteúdo visível nas colunas
- **RF-047**: O filtro de busca DEVE ser case-insensitive (não distinguir maiúsculas/minúsculas) e buscar por substring (conteúdo parcial)
- **RF-048**: O sistema DEVE fornecer botão "Download Base de Dados" visível na página principal
- **RF-049**: Ao clicar em "Download Base de Dados", o sistema DEVE gerar e baixar um **arquivo ZIP** contendo backup completo do banco MongoDB (formato BSON/JSON) com todos os dados **[ATUALIZADO v2.0]**
- **RF-050**: O arquivo de download DEVE ter nome descritivo incluindo data no formato: etnopapers_backup_YYYYMMDD.zip (ex: etnopapers_backup_20251120.zip) **[ATUALIZADO v2.0]**
- **RF-051**: O download do banco de dados DEVE incluir todos os dados da **collection MongoDB `referencias`** (artigos com dados denormalizados: espécies, comunidades, localização) em formato BSON/JSON **[ATUALIZADO v2.0]**
- **RF-052**: A tabela de artigos DEVE exibir mensagem apropriada quando não houver artigos processados (ex: "Nenhum artigo processado ainda. Faça upload do primeiro PDF!")
- **RF-053**: O sistema DEVE armazenar localizações geográficas usando hierarquia administrativa de três níveis: País → Estado/Província → Município/Cidade
- **RF-054**: O sistema DEVE armazenar territórios comunitários como entidades separadas, independentes da hierarquia geográfica administrativa
- **RF-055**: Um artigo PODE estar associado a múltiplas localizações, incluindo qualquer combinação de municípios (hierárquicos) e territórios (comunitários)
- **RF-056**: O sistema DEVE permitir que territórios estejam associados a comunidades tradicionais, mas esta associação é opcional
- **RF-057**: O sistema DEVE armazenar nomes vernaculares de plantas em tabela separada com relação N:M com espécies
- **RF-058**: Cada nome vernacular DEVE incluir opcionalmente o idioma ou língua (ex: português, Yanomami, Guarani) e região de uso
- **RF-059**: O sistema DEVE permitir que um nome vernacular esteja associado a múltiplas espécies (homonímia) e que uma espécie tenha múltiplos nomes vernaculares
- **RF-060**: Cada associação entre espécie e nome vernacular DEVE incluir nível de confiança (alta, média, baixa) e opcionalmente a fonte da informação
- **RF-061**: Ao extrair metadados, o sistema DEVE tentar identificar se a localização mencionada refere-se a município/cidade (hierárquico) ou território tradicional/comunitário
- **RF-062**: O sistema DEVE normalizar e reutilizar registros de países, estados e municípios para evitar duplicação
- **RF-063**: O sistema DEVE permitir associação de coordenadas geográficas a municípios e territórios de forma opcional
- **RF-064**: O sistema DEVE permitir que o usuário configure opcionalmente um perfil de pesquisador contendo: área de especialização, região geográfica de foco, idiomas/línguas indígenas relevantes e tipos de comunidades estudadas
- **RF-065**: O perfil do pesquisador DEVE ser armazenado apenas no navegador (localStorage), nunca no servidor
- **RF-066**: Ao extrair metadados de um PDF, o sistema DEVE incluir o perfil do pesquisador (se configurado) como contexto adicional no prompt enviado ao Ollama. Formato: JSON block de max 500 tokens contendo especialização, regiões de interesse, idiomas/línguas indígenas e tipos de comunidades. Exemplo: `"Pesquisador: especialização em etnobotânica amazônica, interesse em comunidades indígenas do Alto Rio Negro, conhece Yanomami e Baniwa. Procure especialmente por plantas medicinais e alimentares usadas nessas comunidades."`
- **RF-067**: O sistema DEVE permitir que o usuário edite, visualize ou desabilite temporariamente seu perfil de pesquisador a qualquer momento
- **RF-068**: Se o perfil estiver desabilitado, o sistema DEVE fazer extração genérica sem contexto personalizado
- **RF-069**: **[NOVO v2.0]** O sistema DEVE capturar e estruturar informações detalhadas sobre o **uso de plantas por comunidades tradicionais**, incluindo para cada espécie: comunidade utilizadora, forma de uso, tipo de uso, e propósito específico
- **RF-070**: **[NOVO v2.0]** Para cada espécie mencionada no artigo, o sistema DEVE extrair e armazenar uma lista de **contextos de uso por comunidade**, onde cada contexto inclui:
  - Comunidade tradicional utilizadora (nome e tipo: indígena, quilombola, ribeirinha, etc.)
  - Forma de uso (pó, chá, infusão, decocção, cataplasma, óleo, tinctura, etc.)
  - Tipo de uso (medicinal, alimentar, ritual, cosmético, construção, etc.)
  - Propósito específico (febre, tosse, digestão, analgésico, afrodisíaco, etc.)
  - Partes da planta utilizadas (folhas, raízes, cascas, flores, sementes, etc.)
  - Dosagem/quantidade (se mencionado no artigo)
  - Método de preparação detalhado (se disponível)
  - Origem/fonte da informação (se indicado)
- **RF-071**: **[NOVO v2.0]** O sistema DEVE permitir busca e filtro por comunidade tradicional, tipo de uso (medicinal, alimentar, etc.) e propósito específico, permitindo pesquisadores localizar rapidamente plantas e conhecimento relevante para comunidades ou usos específicos
- **RF-072**: **[NOVO v2.0]** O prompt de extração enviado ao Ollama DEVE instruir explicitamente o modelo de IA a extrair e estruturar informações detalhadas de uso de plantas, incluindo:
  - Identificação precisa da comunidade tradicional utilizadora (nome, tipo, localização)
  - Para cada espécie, todos os contextos de uso relatados no artigo (uma planta pode ter múltiplos usos em mesma comunidade ou diferentes comunidades)
  - Forma de uso, tipo de uso, propósito específico, partes utilizadas, dosagem, método de preparação
  - Origem/fonte da informação etnobotânica relatada
  - Garantir que informações de diferentes comunidades ou usos não sejam consolidadas incorretamente

### Entidades Principais

- **Artigo Científico**: Representa um paper acadêmico processado pelo sistema. Atributos incluem: identificador único, data de processamento, título, ano de publicação, autores (lista), resumo, DOI, local de publicação, status de finalização (rascunho/finalizado), status de processamento, indicador se foi editado manualmente, timestamp de última modificação
- **Hierarquia Geográfica**: Representa localizações administrativas tradicionais organizadas hierarquicamente:
  - **País**: Nível mais alto da hierarquia (ex: Brasil, Peru). Atributos: identificador único, nome, código ISO 3166-1 alpha-2
  - **Estado/Província**: Divisão administrativa de primeiro nível dentro de um país (ex: Amazonas, Acre). Atributos: identificador único, nome, sigla, referência ao país
  - **Município/Cidade**: Divisão administrativa de segundo nível dentro de um estado (ex: Manaus, São Gabriel da Cachoeira). Atributos: identificador único, nome, coordenadas geográficas (opcional), referência ao estado
- **Território**: Representa espaços comunitários tradicionais sem definição espacial precisa ou relação com hierarquia geográfica administrativa. Territórios podem sobrepor ou transcender limites administrativos formais. Atributos incluem: identificador único, nome do território (ex: "Terra Indígena Yanomami", "Território Quilombola Ivaporunduva"), descrição detalhada, descrição textual da localização, coordenadas aproximadas (opcional), referência à comunidade associada (opcional), relacionamento com múltiplos artigos
- **Comunidade Tradicional**: Representa grupos comunitários estudados nos artigos e suas práticas de uso de plantas. Atributos incluem: identificador único, nome ou descrição da comunidade, tipo de comunidade (indígena, quilombola, ribeirinha, caiçara, seringueira, pantaneira, outro), localização geográfica (país, estado, município, território), idioma/língua falada (ex: Yanomami, Guarani, português), relacionamento com territórios específicos, histórico de relacionamento com pesquisa etnobotânica (data de criação de registro, estudos mencionados), conhecimento tradicional de plantas associado (agregado de todos os usos de plantas documentados para essa comunidade)
- **Localização de Artigo**: Tabela de associação que vincula artigos a localizações, permitindo que um artigo esteja associado a municípios (hierarquia geográfica) OU a territórios comunitários. Um artigo pode ter múltiplas localizações de ambos os tipos. Atributos: identificador único, referência ao artigo, referência ao município (exclusivo com território), referência ao território (exclusivo com município)
- **Espécie de Planta**: Representa plantas identificadas nos estudos. Atributos incluem: identificador único, nome científico (binomial) como chave de unicidade, autores do nome científico, família botânica, nome aceito atual validado via API, sinônimo (referência a outra espécie se aplicável), status de validação taxonômica, usos reportados (agregado de todos os artigos), relacionamento N:M com nomes vernaculares, relacionamento com artigos onde foi mencionada
- **Nome Vernacular**: Representa nomes populares de plantas utilizados por comunidades tradicionais. Uma espécie pode ter múltiplos nomes vernaculares, e um nome vernacular pode referir-se a múltiplas espécies (homonímia). Atributos incluem: identificador único, nome popular (ex: "unha-de-gato", "ipê-roxo", "yãpinã"), idioma ou língua (ex: "português", "Yanomami", "Guarani"), descrição da região onde o nome é usado, relacionamento N:M com espécies de plantas
- **Associação Espécie-Nome Vernacular**: Tabela que conecta espécies a nomes vernaculares. Atributos incluem: referência à espécie, referência ao nome vernacular, fonte da informação, nível de confiança da associação (alta/média/baixa)
- **Dados de Estudo**: Representa informações metodológicas do estudo. Atributos incluem: identificador único, período do estudo (data de início e fim), métodos de coleta de dados, tipo de amostragem, tamanho da amostra, relacionamento 1:1 com o artigo correspondente
- **Perfil do Pesquisador**: Configuração opcional armazenada no navegador para personalizar extração de metadados. Atributos incluem: área de especialização/foco (ex: "etnobotânica amazônica", "plantas medicinais"), região geográfica de interesse (ex: "Amazônia brasileira", "Alto Rio Negro"), idiomas e línguas indígenas relevantes (lista, ex: ["Yanomami", "Baniwa", "Tukano"]), tipos de comunidades estudadas (lista, ex: ["indígena", "ribeirinha"]), estado do perfil (ativo/desabilitado)

### MongoDB Collection Structure **[NOVO v2.0]**

O sistema usa **denormalization pattern** com **1 collection única** (`referencias`) contendo todos os dados de artigos em documentos auto-contidos, seguindo o exemplo em `docs/estrutura.json`:

**Collection: `referencias`** (Artigos científicos com dados embutidos)

Estrutura de documento exemplo:
```json
{
  "_id": "ObjectId",
  "ano": 2010,
  "titulo": "Uso e conhecimento tradicional de plantas medicinais...",
  "publicacao": "Acta bot. bras. 24(2): 395-406",
  "autores": ["Giraldi, M.", "Hanazaki, N."],
  "resumo": "O objetivo desta pesquisa...",
  "doi": "10.1590/...",
  "data_processamento": "2025-11-24T10:30:00Z",
  "status": "finalizado|rascunho",
  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita",
      "familia": "Asteraceae",
      "nomeAceitoValidado": "Chamomilla recutita (L.) Rydb.",
      "statusValidacao": "validado",
      "confianca": "alta",
      "usosPorComunidade": [
        {
          "comunidade": {
            "nome": "Comunidade Açoriana do Sertão do Ribeirão",
            "tipo": "tradicional",
            "pais": "Brasil",
            "estado": "SC",
            "municipio": "Florianópolis"
          },
          "formaDeUso": "chá",
          "tipoDeUso": "medicinal",
          "propositoEspecifico": "digestão, anti-inflamação",
          "partesUtilizadas": ["flores", "folhas"],
          "dosagem": "1 xícara 2-3 vezes ao dia",
          "metodoPreparacao": "infusão em água quente por 5-10 minutos",
          "origem": "conhecimento tradicional passado oralmente"
        },
        {
          "comunidade": {
            "nome": "Comunidade Açoriana do Sertão do Ribeirão",
            "tipo": "tradicional",
            "pais": "Brasil",
            "estado": "SC",
            "municipio": "Florianópolis"
          },
          "formaDeUso": "banho",
          "tipoDeUso": "medicinal",
          "propositoEspecifico": "inflamação de pele",
          "partesUtilizadas": ["flores"],
          "dosagem": "conforme necessário",
          "metodoPreparacao": "decocção de flores em água, aplicação tópica",
          "origem": "conhecimento tradicional"
        }
      ]
    },
    {
      "vernacular": "hortelã-branca",
      "nomeCientifico": "Mentha sp.",
      "familia": "Lamiaceae",
      "nomeAceitoValidado": "Mentha spicata L.",
      "statusValidacao": "validado",
      "confianca": "media",
      "usosPorComunidade": [
        {
          "comunidade": {
            "nome": "Comunidade Açoriana do Sertão do Ribeirão",
            "tipo": "tradicional",
            "pais": "Brasil",
            "estado": "SC",
            "municipio": "Florianópolis"
          },
          "formaDeUso": "chá",
          "tipoDeUso": "medicinal",
          "propositoEspecifico": "digestão, gases intestinais",
          "partesUtilizadas": ["folhas", "ramos"],
          "dosagem": "1 xícara após refeições",
          "metodoPreparacao": "infusão de folhas frescas em água quente",
          "origem": "uso comum entre moradores da comunidade"
        }
      ]
    }
  ],
  "tipoUso": "medicinal|alimentar|ritual|outro",
  "metodologia": "entrevistas|observacao|outro",
  "pais": "Brasil",
  "estado": "SC",
  "municipio": "Florianópolis",
  "local": "Sertão do Ribeirão",
  "bioma": "Mata Atlântica",
  "comunidades": [
    {
      "nome": "Comunidade Açoriana",
      "tipo": "indígena|quilombola|ribeirinha|caiçara|tradicional"
    }
  ],
  "periodoEstudo": {
    "dataInicio": "2009-01-01",
    "dataFim": "2010-12-31"
  }
}
```

**Campos principais**:
- `_id`: ObjectId único (MongoDB auto-gerado)
- `ano`, `titulo`, `publicacao`, `autores`: Metadados básicos do artigo
- `resumo`: Abstract/resumo do artigo
- `doi`: Digital Object Identifier (unique index para evitar duplicação)
- `data_processamento`: ISO 8601 timestamp de quando foi extraído
- `status`: "finalizado" (salvo pelo usuário) ou "rascunho" (auto-saved)
- `especies[]`: Array de objetos com:
  - Identificação: `vernacular`, `nomeCientifico`, `familia`, `nomeAceitoValidado`, `statusValidacao` ("validado" | "naoValidado"), `confianca` ("alta" | "media" | "baixa")
  - **Uso detalhado por comunidade** (`usosPorComunidade[]`): Para cada comunidade tradicional que usa a planta, array com **todos os 7 campos opcionais** — registros podem conter qualquer subconjunto:
    - `comunidade`: {nome, tipo, país, estado, município} - **Comunidade utilizadora (inline, denormalizada)**
    - `formaDeUso`: Forma (chá, pó, óleo, infusão, decocção, cataplasma, tinctura, banho, etc.) — OPCIONAL
    - `tipoDeUso`: Tipo (medicinal, alimentar, ritual, cosmético, construção, etc.) — OPCIONAL
    - `propositoEspecifico`: Propósito (febre, tosse, digestão, analgésico, etc.) — OPCIONAL
    - `partesUtilizadas`: Partes da planta (folhas, raízes, cascas, flores, sementes, etc.) — OPCIONAL
    - `dosagem`: Quantidade/dosagem (se mencionado) — OPCIONAL
    - `metodoPreparacao`: Como é preparada detalhadamente — OPCIONAL
    - `origem`: Origem da informação — OPCIONAL
- `tipoUso`, `metodologia`, `pais`, `estado`, `municipio`, `local`, `bioma`: Contexto geográfico/temático do artigo
- `comunidades[]`: Array de comunidades tradicionais mencionadas no artigo (nome, tipo, localização)
- `periodoEstudo`: Período do estudo (dataInicio, dataFim) - opcional

**Índices para Performance**:
- `{doi: 1}` (unique) → evita duplicação exata
- `{ano: 1}` → filtro por ano de publicação
- `{status: 1}` → separar finalizado de rascunho
- `{titulo: "text"}` → busca full-text no título
- `{pais: 1, estado: 1, municipio: 1}` → filtro geográfico
- `{"especies.usosPorComunidade.comunidade.nome": 1}` → busca por comunidade tradicional
- `{"especies.usosPorComunidade.tipoDeUso": 1}` → filtro por tipo de uso (medicinal, alimentar, etc.)
- `{"especies.usosPorComunidade.propositoEspecifico": 1}` → busca por propósito específico (febre, tosse, etc.)

**Vantagens da Denormalização**:
- ✅ **Simplicidade**: Um documento = uma referência. Sem JOINs, sem relacionamentos complexos
- ✅ **Queries rápidas**: Tudo em um document — `find({doi: "..."})` retorna artigo completo
- ✅ **Flexibilidade schema**: Novos campos podem ser adicionados sem migrações
- ✅ **Offline-friendly**: Backup/restore de um único documento é simples
- ✅ **Adequado para volume**: 1000-10000 artigos é manageable em single collection
- ✅ **Escalabilidade futura**: Se crescer muito, pode-se normalizaralém, mas não é necessário agora

## Critérios de Sucesso *(obrigatório)*

### Resultados Mensuráveis

- **CS-001**: Usuários conseguem fazer upload de um PDF e visualizar os metadados extraídos em menos de 2 minutos para artigos de até 30 páginas
- **CS-002**: O sistema extrai com sucesso pelo menos 80% dos metadados disponíveis em artigos que seguem formato científico padrão
- **CS-003**: O sistema identifica e extrai corretamente nomes científicos de plantas com precisão mínima de 85% em relação à extração manual
- **CS-004**: O sistema processa artigos em PDF sem armazenar os arquivos originais, reduzindo o uso de armazenamento em 100% comparado a sistemas que mantêm os PDFs
- **CS-005**: A interface web permite que usuários sem conhecimento técnico façam upload e visualizem resultados sem treinamento prévio (taxa de conclusão de tarefa de 90% no primeiro uso)
- **CS-006**: O sistema utiliza APIs externas de IA (Gemini, ChatGPT, Claude) para extração de metadados, permitindo que usuários usem suas próprias chaves de API sem custo adicional de infraestrutura
- **CS-007**: A chave de API do usuário permanece 100% privada, armazenada apenas no navegador e nunca transmitida ou armazenada no servidor
- **CS-008**: Toda a documentação técnica e de usuário está disponível em português brasileiro, facilitando adoção por pesquisadores brasileiros
- **CS-009**: O tempo de configuração inicial do servidor não ultrapassa 10 minutos para usuários familiarizados com Docker (simplificado sem necessidade de GPU)
- **CS-010**: Usuários conseguem editar e salvar correções em metadados extraídos em menos de 30 segundos por campo
- **CS-011**: Usuários conseguem configurar sua chave de API em menos de 1 minuto na primeira utilização do sistema
- **CS-012**: O sistema detecta 100% de artigos duplicados quando DOI está disponível e pelo menos 95% quando usa combinação título+ano+autor, evitando duplicação indevida na base de dados
- **CS-013**: A tabela de artigos carrega e exibe até 1000 artigos com tempo de resposta inferior a 2 segundos
- **CS-014**: O filtro/busca da tabela responde em tempo real (menos de 500ms) ao digitar, mesmo com 1000+ artigos
- **CS-015**: Usuários conseguem ordenar a tabela por qualquer coluna com 1 clique, alternando entre crescente/decrescente
- **CS-016**: O download completo do banco de dados SQLite é iniciado em menos de 3 segundos após clicar no botão
- **CS-017**: O arquivo SQLite baixado pode ser aberto em qualquer ferramenta SQLite sem erros de integridade (100% de compatibilidade)
- **CS-018**: Usuários conseguem localizar um artigo específico usando o filtro em menos de 10 segundos, independentemente do tamanho da base

## Dependências *(opcional)*

### Dependências Externas **[ATUALIZADO v2.0]**

- **Servidor UNRAID** (ou qualquer servidor compatível com Docker) **COM GPU NVIDIA** (6-8 GB VRAM mínimo, ex: RTX 3060+)
- **Docker + Docker Compose** instalado e configurado no servidor
- **NVIDIA Container Toolkit** (nvidia-docker) para GPU passthrough nos containers
- **NVIDIA GPU Driver** compatível instalado no host UNRAID
- **Espaço em disco:** Mínimo 20 GB, recomendado 50+ GB (Docker images ~8.5 GB + MongoDB data + modelos)
- **RAM:** Mínimo 8 GB, recomendado 16+ GB
- **Acesso à internet** (uma vez) para:
  - Download do modelo Qwen2.5-7B-Instruct-Q4 (~4.8 GB) via Ollama
  - Consulta opcional à API de taxonomia botânica (GBIF Species API, Tropicos, POWO)
- **Após setup inicial**: Sistema é **offline-capable** (não requer internet para extração de metadados)
- ~~Usuários devem possuir conta e chave de API válida em provedores de IA~~ **[REMOVIDO v2.0 - Sem API keys necessárias]**

### Dependências de Conhecimento **[ATUALIZADO v2.0]**

- **Integração com Ollama** (framework de inferência local) para extração de informações de documentos PDF usando Qwen2.5-7B-Instruct
- **MongoDB** (NoSQL database) para armazenamento de metadados em formato BSON/JSON
- **Instructor + Pydantic** (Python) para outputs estruturados garantidos do modelo de AI
- **GPU passthrough em Docker** (NVIDIA Container Toolkit) para permitir que containers usem GPU do host
- **pdfplumber** (Python) para extração de texto de PDFs
- Compreensão de taxonomia botânica e nomenclatura científica para validação de extração de nomes de espécies
- ~~Gerenciamento seguro de chaves de API no frontend~~ **[REMOVIDO v2.0 - Sem API keys]**
- ~~Conhecimento básico sobre obtenção de chaves de API dos provedores de IA~~ **[REMOVIDO v2.0 - Sem API keys]**

## Suposições *(opcional)* **[ATUALIZADO v2.0]**

- Os artigos científicos em PDF estão majoritariamente em formato de texto pesquisável (não exclusivamente imagens escaneadas)
- **O servidor UNRAID possui GPU NVIDIA com 6-8 GB VRAM mínimo** (ex: RTX 3060, RTX 3070, RTX 3080, ou superior)
- O servidor tem recursos de hardware suficientes (RAM 8+ GB, CPU multi-core, disco 20+ GB) para executar a aplicação web, MongoDB e Ollama
- **NVIDIA Container Toolkit está instalado e configurado** no servidor UNRAID para permitir GPU passthrough em Docker
- Os artigos seguem, em sua maioria, estruturas convencionais de papers científicos (abstract, métodos, resultados, etc.)
- O volume de processamento não requer múltiplas instâncias ou paralelização complexa (um container de cada serviço é suficiente)
- Usuários têm acesso de rede ao servidor para acessar a interface web
- ~~Usuários possuem ou estão dispostos a criar contas nos provedores de IA para obter chaves de API~~ **[REMOVIDO v2.0]**
- ~~Usuários compreendem que o uso das APIs de IA pode incorrer em custos conforme os planos de cada provedor~~ **[REMOVIDO v2.0]**
- Artigos estarão predominantemente em português, inglês ou espanhol (Qwen2.5 suporta 29+ idiomas)
- A precisão de extração de 80-85% é aceitável, considerando que edição manual estará disponível para correção
- **Usuários têm acesso à internet na configuração inicial** (para download do modelo ~4.8 GB), mas sistema funciona offline após setup
- **Inferência local leva 1-3 segundos por artigo** em GPU adequada (RTX 3060+), aceitável para uso single-user

## Exclusões de Escopo *(opcional)*

- Sistema de autenticação e controle de acesso de usuários (sistema será de acesso aberto na rede local)
- Exportação de dados para outros formatos (CSV, JSON, etc.) além do backup ZIP do MongoDB
- Integração com sistemas externos ou APIs de bases de dados científicas (exceto API de taxonomia botânica para validação de nomes científicos)
- Processamento de documentos em formatos diferentes de PDF
- Backup automático do banco de dados (apenas download manual via interface)
- Interface de administração para gerenciar o banco de dados
- Análises estatísticas ou visualizações dos dados agregados
- Sistema de notificações ou alertas
- Processamento em lote de múltiplos PDFs simultaneamente (apenas upload single-file)
- Versionamento ou histórico de alterações dos metadados
- Funcionalidade de OCR nativo (o sistema tentará processar PDFs escaneados mas não aplicará OCR para conversão)
- ~~Armazenamento ou gerenciamento de chaves de API no servidor~~ **[REMOVIDO v2.0 - Sem API keys]**
- ~~Fornecimento de créditos ou pagamento de custos de API de IA~~ **[REMOVIDO v2.0 - AI local sem custos por requisição]**
- **Fine-tuning ou retreinamento do modelo de AI** (usa modelo Qwen2.5 pré-treinado)
- **Suporte a múltiplos modelos de AI simultaneamente** (apenas Qwen2.5-7B-Instruct)
- **Aceleração por múltiplas GPUs** (usa apenas 1 GPU)
- **Inferência em CPU otimizada** (requer GPU para performance aceitável)

## Notas e Considerações *(opcional)*

### ~~Seleção de Provedor de IA~~ **[REMOVIDO v2.0]**

~~O sistema suporta três provedores principais de IA~~

**[ATUALIZADO v2.0]** O sistema agora usa **AI Local (Ollama + Qwen2.5-7B-Instruct)** rodando no próprio servidor UNRAID. Sem necessidade de API keys, quotas ou custos por requisição.

### Modelo de AI Local: Qwen2.5-7B-Instruct **[NOVO v2.0]**

**Por que Qwen2.5-7B-Instruct?**
- ✅ **Multilíngue excelente:** Treinado em 29+ idiomas incluindo português (MMLU 81.6%)
- ✅ **JSON nativo:** Suporte built-in a structured outputs (garante JSON válido)
- ✅ **Context window grande:** 128K tokens (artigos longos cabem inteiros)
- ✅ **Tamanho otimizado:** 4.8 GB (Q4 quantizado) vs. 17 GB (full precision)
- ✅ **Performance:** Inferência 1-3s em RTX 3060+ (6-8 GB VRAM)
- ✅ **Atualizado:** Lançado em 2025, arquitetura moderna

**Alternativas consideradas:**
- **NuExtract-1.5:** Purpose-built para extração, mas português mais fraco
- **Sabiá-7B:** Nativo português, mas sem instruction tuning
- **Mistral-7B-Instruct:** Excelente qualidade, mas maior que Qwen
- **Gemma 2-9B:** Boa performance, mas português limitado

**Recomendação:** Qwen2.5 oferece o melhor balanço para etnobotânica brasileira.

### Prompt de Extração: Hardcoded para Qwen2.5 **[NOVO 2025-11-27]**

**Decisão:** O prompt de extração de metadados é **hardcoded especificamente para Qwen2.5-7B-Instruct**. Nenhum suporte a parameterização de prompt ou fallback para modelos alternativos.

**Rationale:**
- Simplicidade (Princípio IV da Constituição): Um prompt, um modelo, uma estratégia clara
- Otimização: Prompt tuned para strengths específicos de Qwen (português excelente, JSON nativo, instruction-tuned)
- Sem necessidade de versioning ou compatibilidade entre modelos
- Suporte a múltiplos modelos pode ser adicionado no futuro se explicitamente requisitado

**Implicação:** Se no futuro necessário usar modelo diferente (ex: Mistral para latência menor), prompt será completamente reescrito e testado.

### Considerações Técnicas de Docker **[ATUALIZADO v2.0]**

- **Variáveis de ambiente obrigatórias:**
  - `MONGO_URI`: URI de conexão MongoDB (ex: mongodb://mongo:27017/etnopapers)
  - `OLLAMA_URL`: URL do serviço Ollama (ex: http://ollama:11434)
  - `OLLAMA_MODEL`: Nome do modelo (ex: qwen2.5:7b-instruct-q4_K_M)
- **GPU passthrough obrigatório:** Containers Ollama e Etnopapers precisam de acesso à GPU NVIDIA via nvidia-docker runtime
- **Volumes Docker para persistência:**
  - `mongodb_data`: Banco de dados MongoDB
  - `ollama_models`: Modelos de AI baixados (~4.8 GB)
- **Portas expostas:**
  - 8000: Interface web + API backend
  - 11434: Ollama API (interno)
  - 27017: MongoDB (interno)
- **Healthchecks:** Implementados para MongoDB e Ollama (garante serviços estão prontos antes de iniciar aplicação)
- **Tamanho total Docker:** ~8.5 GB (vs. 180 MB anterior)

### ~~Gerenciamento de Chaves de API e Privacidade~~ **[REMOVIDO v2.0]**

~~Chaves de API são armazenadas exclusivamente no localStorage do navegador~~

**[ATUALIZADO v2.0]** Não há mais necessidade de chaves de API! O sistema usa AI local (Ollama + Qwen2.5) rodando no próprio servidor.

**Privacidade Garantida:**
- ✅ Dados de artigos nunca saem do servidor UNRAID
- ✅ Nenhuma transmissão para APIs externas
- ✅ Sistema totalmente offline-capable após download do modelo
- ✅ Zero dependências de serviços terceiros para extração
- ✅ Sem riscos de vazamento de dados sensíveis de pesquisa

**Fluxo Simplificado:**
1. ~~Usuário seleciona provedor de IA~~ **[REMOVIDO]**
2. ~~Sistema exibe instruções de como obter chave de API~~ **[REMOVIDO]**
3. **Usuário acessa interface e faz upload direto** (sem configuração prévia)
4. **Backend processa com AI local em 1-3 segundos**
5. **Metadados são exibidos instantaneamente**

### Interface de Edição Manual

- Interface deve ser intuitiva e responsiva
- Todos os campos de metadados devem ser editáveis
- Validações básicas devem ser implementadas (ex: formato de ano, DOI)
- Sistema deve indicar visualmente quais campos foram editados manualmente
- Considerar funcionalidade de "desfazer" para reverter edições

### Fluxo de Salvamento e Gestão de Rascunhos

Após a extração de metadados, o sistema apresenta três ações principais:

**Botão "Salvar"**:
- Finaliza o processamento e armazena com status "finalizado"
- Dados ficam disponíveis no histórico
- Validação taxonômica executada se ainda não foi feita
- Irreversível via interface (seria necessário editar posteriormente via histórico)

**Botão "Editar"**:
- Abre interface de edição completa
- Permite correção e complementação de todos os campos
- Ao salvar na interface de edição, status muda para "finalizado"
- Ideal para PDFs escaneados ou extrações de baixa qualidade

**Botão "Descartar"**:
- Solicita confirmação: "Tem certeza? Esta ação não pode ser desfeita"
- Remove permanentemente os dados extraídos
- Libera recursos
- Útil quando upload foi feito por engano

**Auto-salvamento como Rascunho**:
- Acionado automaticamente se usuário fechar janela/navegador
- Status "rascunho" no banco de dados
- Recuperável via seção "Rascunhos Pendentes" na interface
- Timeout configurável (sugestão: 7 dias) para limpeza automática de rascunhos antigos
- Rascunhos não aparecem no histórico principal, apenas em área dedicada

### Integração com API de Taxonomia Botânica

A validação de nomes científicos é crítica para garantir qualidade e consistência dos dados. APIs recomendadas:

**GBIF Species API** (Global Biodiversity Information Facility):
- Cobertura global abrangente
- API REST gratuita e bem documentada
- Retorna nome aceito, autores, família, sinônimos
- URL: https://www.gbif.org/developer/species

**Tropicos** (Missouri Botanical Garden):
- Especializado em plantas tropicais e neotropicais
- API gratuita com registro
- Dados nomenclaturais detalhados incluindo autores
- URL: https://www.tropicos.org/home

**POWO - Plants of the World Online** (Kew Royal Botanic Gardens):
- Base taxonômica autorizada para plantas
- API disponível para consultas
- Cobertura mundial de famílias de plantas

Considerações técnicas:
- Implementar cache local para reduzir chamadas repetidas à API
- Definir timeout apropriado para chamadas de API (sugestão: 5 segundos)
- Tratamento de falhas: se API estiver indisponível, permitir salvamento sem validação mas marcar como "não validado"
- Rate limiting: respeitar limites de requisições da API escolhida
- Considerar fallback entre múltiplas APIs se uma estiver indisponível

### Estrutura de Documentação **[ATUALIZADO v2.0]**

Toda a documentação deverá incluir:
- **README em português** com instruções de instalação para UNRAID + GPU
- **Guia de Setup de GPU** (NVIDIA Container Toolkit, driver installation)
- **Guia de configuração do docker-compose.yml** (variáveis de ambiente: MONGO_URI, OLLAMA_URL, OLLAMA_MODEL)
- **Guia de download do modelo Qwen2.5** via Ollama (pull inicial ~4.8 GB)
- **Documentação de usuário** sobre interface de upload simplificada (sem configuração de API keys)
- **Guia sobre interface de edição manual** de metadados
- **FAQ sobre privacidade** (dados nunca saem do servidor, totalmente offline)
- ~~Informações sobre custos estimados de uso das APIs de IA~~ **[REMOVIDO v2.0 - Custo fixo inicial, zero por requisição]**
- **Descrição do schema MongoDB** (coleções: referencias, especies_plantas, etc.)
- **Troubleshooting especializado:**
  - GPU não detectada (nvidia-smi, nvidia-docker)
  - Modelo Ollama não carrega (download manual, verificação de VRAM)
  - Inferência muito lenta (verificar se GPU está sendo usada, não caiu para CPU)
  - Erro de memória (VRAM insuficiente, usar quantização menor)
- **Requisitos de hardware detalhados** (GPU models compatíveis, VRAM, RAM, disco)
