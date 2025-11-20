# Especificação da Funcionalidade: Sistema de Extração de Metadados de Artigos Etnobotânicos

**Branch da Funcionalidade**: `main`
**Criado**: 2025-11-20
**Status**: Rascunho
**Entrada**: Sistema de extração de metadados de artigos científicos em PDF sobre etnobotânica usando APIs externas de IA (Gemini, ChatGPT, Claude) com chaves fornecidas pelo usuário, rodando em Docker com banco SQLite

## Clarifications

### Session 2025-11-20

- Q: Qual o limite de tamanho de arquivo PDF? → A: 50 MB por arquivo
- Q: Como tratar PDFs escaneados (sem texto pesquisável)? → A: Tentar processar mas exibir aviso de qualidade reduzida, e após extração mostrar tela de edição para correção/complementação manual dos metadados
- Q: Como garantir unicidade de espécies de plantas no banco de dados? → A: Nome científico (binomial) como chave única, com validação via API externa para obter nome aceito atual, família botânica e autores do nome científico
- Q: O que acontece se usuário fechar janela sem salvar? → A: Salvar automaticamente como rascunho. Após extração, apresentar botões "Salvar" (finalizar no BD), "Editar" (abrir interface de edição) e "Descartar" (excluir dados extraídos)
- Q: Como será feita a extração de metadados? → A: Usando APIs externas (Gemini, ChatGPT, Claude) com chave fornecida pelo usuário. A chave é armazenada apenas no navegador (browser storage), nunca no servidor
- Q: Como evitar duplicação de artigos na base de dados? → A: Sistema verifica duplicatas após extração de metadados usando DOI (se disponível) ou combinação título+ano+primeiro autor. Se duplicata for detectada, usuário é informado e pode optar por descartar ou sobrescrever o registro existente

## Cenários de Usuário e Testes *(obrigatório)*

### História de Usuário 1 - Upload e Extração Básica de PDF (Prioridade: P1)

Um pesquisador acessa o sistema através de uma página web, fornece sua chave de API para um serviço de IA (Gemini, ChatGPT ou Claude) que fica armazenada apenas no navegador, faz o upload de um artigo científico em PDF sobre etnobotânica, e o sistema extrai automaticamente os metadados disponíveis no artigo usando a API de IA selecionada. A página principal exibe uma tabela com os artigos já processados, permitindo busca e ordenação. Após o processamento do novo PDF, o sistema exibe um resumo estruturado dos metadados extraídos na mesma página, sem armazenar o arquivo PDF original nem a chave de API no servidor. O pesquisador também pode fazer download completo do banco de dados SQLite para backup ou análise externa.

**Por que esta prioridade**: Esta é a funcionalidade central do sistema. Sem ela, nenhum outro componente tem valor. Representa o MVP completo e entregável. A visualização dos artigos já processados e a funcionalidade de download são essenciais para o pesquisador ter visibilidade completa dos dados coletados.

**Teste Independente**: Pode ser totalmente testado fazendo upload de um único PDF e verificando se os metadados são extraídos e exibidos corretamente. Entrega valor imediato ao permitir que usuários extraiam dados de artigos sem processamento manual. A tabela e funcionalidade de download podem ser testadas após processar múltiplos artigos.

**Cenários de Aceitação**:

1. **Dado** que o usuário acessa o sistema pela primeira vez, **Quando** o usuário visualiza a página de upload, **Então** o sistema solicita que o usuário selecione um provedor de IA (Gemini, ChatGPT ou Claude) e insira sua chave de API
2. **Dado** que o usuário inseriu uma chave de API válida, **Quando** o usuário confirma, **Então** a chave é armazenada apenas no navegador (localStorage) e nunca enviada ao servidor para armazenamento
3. **Dado** que o usuário já configurou sua chave de API, **Quando** o usuário retorna ao sistema, **Então** a chave é recuperada do navegador e o usuário pode fazer upload diretamente
4. **Dado** que o usuário está na página principal do sistema com chave configurada, **Quando** o usuário seleciona um arquivo PDF válido e clica em upload, **Então** o sistema processa o arquivo usando a API de IA selecionada e exibe os metadados extraídos em formato estruturado
5. **Dado** que o processamento foi concluído, **Quando** o sistema exibe os resultados, **Então** os metadados incluem título, autores, ano, resumo, DOI, local de publicação, região do estudo, comunidades envolvidas, espécies de plantas (nomes científicos e vernaculares), período do estudo, métodos de coleta de dados e tipo de amostragem
6. **Dado** que os metadados foram extraídos, **Quando** o usuário visualiza o resumo, **Então** o sistema indica claramente quais campos não puderam ser extraídos do artigo E apresenta três botões de ação: "Salvar", "Editar" e "Descartar"
7. **Dado** que o usuário visualiza os metadados extraídos, **Quando** o usuário clica em "Salvar", **Então** os metadados são salvos finalizados no banco de dados SQLite e o PDF não é armazenado
8. **Dado** que o usuário visualiza os metadados extraídos, **Quando** o usuário clica em "Editar", **Então** o sistema abre a interface de edição manual permitindo correções e complementações
9. **Dado** que o usuário visualiza os metadados extraídos, **Quando** o usuário clica em "Descartar", **Então** o sistema exclui os dados extraídos e solicita confirmação antes de descartar
10. **Dado** que o usuário fechou a janela/navegador sem clicar em nenhum botão, **Quando** o usuário retorna ao sistema, **Então** os dados extraídos estão salvos como rascunho e podem ser recuperados
11. **Dado** que o usuário está na página principal, **Quando** a página carrega, **Então** o sistema exibe uma tabela com todos os artigos já processados, mostrando colunas: título, ano, autores, status, data de processamento e número de espécies
12. **Dado** que a tabela de artigos está visível, **Quando** o usuário clica no cabeçalho de uma coluna, **Então** a tabela é ordenada por aquela coluna (crescente/decrescente alternadamente)
13. **Dado** que a tabela de artigos está visível, **Quando** o usuário digita texto no campo de busca/filtro, **Então** a tabela filtra em tempo real mostrando apenas artigos que contenham o texto buscado em qualquer campo visível
14. **Dado** que o usuário está na página principal, **Quando** o usuário clica no botão "Download Base de Dados", **Então** o sistema inicia o download do arquivo SQLite completo (.db) contendo todos os artigos e metadados
15. **Dado** que o usuário clicou em "Download Base de Dados", **Quando** o download é concluído, **Então** o arquivo tem nome descritivo incluindo data (ex: etnopapers_20251120.db) e pode ser aberto em qualquer ferramenta SQLite

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
3. **Dado** que o usuário está na tela de edição, **Quando** o usuário modifica um ou mais campos e salva as alterações, **Então** os metadados atualizados são armazenados no banco de dados SQLite
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
- **RF-002**: O sistema DEVE permitir que o usuário selecione entre provedores de IA: Google Gemini, OpenAI ChatGPT ou Anthropic Claude
- **RF-003**: O sistema DEVE solicitar ao usuário que forneça sua chave de API para o provedor de IA selecionado
- **RF-004**: O sistema DEVE armazenar a chave de API fornecida APENAS no localStorage do navegador do usuário
- **RF-005**: O sistema NÃO DEVE enviar ou armazenar a chave de API do usuário no servidor em nenhuma circunstância
- **RF-006**: O sistema DEVE processar arquivos PDF usando a API de IA selecionada pelo usuário com a chave fornecida
- **RF-007**: O sistema DEVE extrair os seguintes metadados quando disponíveis no artigo: título, ano de publicação, lista de autores, resumo/abstract, DOI (Digital Object Identifier), local/jornal de publicação, região geográfica do estudo, comunidades tradicionais envolvidas, espécies de plantas identificadas (nomes científicos e vernaculares), ano ou período em que o estudo foi conduzido, métodos de coleta de dados das comunidades, e tipo de amostragem utilizado
- **RF-008**: O sistema DEVE armazenar os metadados extraídos em um banco de dados SQLite
- **RF-009**: O sistema DEVE exibir um resumo estruturado dos metadados extraídos após o processamento do PDF
- **RF-010**: O sistema NÃO DEVE armazenar os arquivos PDF após a extração dos metadados
- **RF-011**: O sistema DEVE indicar claramente quais metadados não foram encontrados ou não puderam ser extraídos de cada artigo
- **RF-012**: O sistema DEVE ser empacotado e executado em um container Docker
- **RF-013**: O sistema DEVE ser compatível com instalação em servidor UNRAID
- **RF-014**: O sistema DEVE aceitar o caminho do banco de dados SQLite como variável de ambiente durante a criação do container Docker
- **RF-015**: Toda a documentação do sistema DEVE ser gerada e mantida em português brasileiro
- **RF-016**: O sistema DEVE operar exclusivamente na branch main, sem branches de desenvolvimento separadas
- **RF-017**: O sistema DEVE rejeitar arquivos PDF maiores que 50 MB com mensagem de erro clara informando o limite de tamanho
- **RF-018**: O sistema DEVE fornecer interface de edição manual permitindo aos usuários corrigir e complementar metadados extraídos
- **RF-019**: O sistema DEVE processar PDFs escaneados (formato de imagem) mas DEVE exibir aviso informando que a qualidade da extração pode estar reduzida
- **RF-020**: Ao detectar PDF escaneado ou extração de baixa qualidade, o sistema DEVE direcionar o usuário automaticamente para a tela de edição manual após exibir os resultados
- **RF-021**: O sistema DEVE garantir unicidade de espécies de plantas no banco de dados usando o nome científico (binomial) como chave primária
- **RF-022**: O sistema DEVE validar a chave de API fornecida fazendo uma chamada de teste antes de permitir o upload de PDFs
- **RF-023**: O sistema DEVE exibir mensagens de erro claras quando a chave de API for inválida, expirada ou quando a API estiver indisponível
- **RF-024**: O sistema DEVE permitir ao usuário visualizar, editar ou remover a chave de API armazenada no navegador a qualquer momento
- **RF-025**: O sistema DEVE validar nomes científicos de plantas extraídos consultando API de taxonomia botânica para obter o nome aceito atual
- **RF-026**: O sistema DEVE armazenar automaticamente família botânica e autores do nome científico obtidos da API de taxonomia
- **RF-027**: Quando múltiplos artigos mencionam a mesma espécie (mesmo nome científico), o sistema DEVE reutilizar o registro existente ao invés de criar duplicata
- **RF-028**: Se a API de taxonomia estiver indisponível, o sistema DEVE permitir salvamento dos metadados mas DEVE marcar o status de validação como "não validado"
- **RF-029**: O sistema DEVE implementar cache local de consultas à API de taxonomia para reduzir requisições repetidas e melhorar performance
- **RF-030**: Após extração de metadados, o sistema DEVE apresentar interface com três botões de ação claramente visíveis: "Salvar", "Editar" e "Descartar"
- **RF-031**: Ao clicar em "Salvar", o sistema DEVE armazenar os metadados com status "finalizado" no banco de dados
- **RF-032**: Ao clicar em "Editar", o sistema DEVE abrir interface de edição manual com todos os campos editáveis
- **RF-033**: Ao clicar em "Descartar", o sistema DEVE solicitar confirmação e então excluir permanentemente os dados extraídos
- **RF-034**: Se o usuário fechar navegador/janela sem selecionar ação, o sistema DEVE salvar automaticamente os metadados com status "rascunho"
- **RF-035**: O sistema DEVE permitir recuperação de rascunhos salvos automaticamente para finalização posterior
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
- **RF-049**: Ao clicar em "Download Base de Dados", o sistema DEVE gerar e baixar o arquivo SQLite completo (.db) contendo todos os dados
- **RF-050**: O arquivo de download DEVE ter nome descritivo incluindo data no formato: etnopapers_YYYYMMDD.db (ex: etnopapers_20251120.db)
- **RF-051**: O download do banco de dados DEVE incluir todas as tabelas e dados: artigos, espécies, regiões, comunidades, dados de estudo e relacionamentos
- **RF-052**: A tabela de artigos DEVE exibir mensagem apropriada quando não houver artigos processados (ex: "Nenhum artigo processado ainda. Faça upload do primeiro PDF!")

### Entidades Principais

- **Artigo Científico**: Representa um paper acadêmico processado pelo sistema. Atributos incluem: identificador único, data de processamento, título, ano de publicação, autores (lista), resumo, DOI, local de publicação, status de finalização (rascunho/finalizado), status de processamento, indicador se foi editado manualmente, timestamp de última modificação
- **Região de Estudo**: Representa a localização geográfica onde o estudo foi conduzido. Atributos incluem: identificador único, descrição da região, país, estado/província, coordenadas geográficas (se disponíveis), relacionamento com múltiplos artigos
- **Comunidade Tradicional**: Representa grupos comunitários estudados nos artigos. Atributos incluem: identificador único, nome ou descrição da comunidade, tipo de comunidade (indígena, quilombola, ribeirinha, etc.), relacionamento com artigos e regiões
- **Espécie de Planta**: Representa plantas identificadas nos estudos. Atributos incluem: identificador único, nome científico (binomial) como chave de unicidade, autores do nome científico, nomes vernaculares (lista de nomes populares em diferentes idiomas/dialetos), família botânica, nome aceito atual validado via API, status de validação taxonômica, usos reportados, relacionamento com artigos onde foi mencionada
- **Dados de Estudo**: Representa informações metodológicas do estudo. Atributos incluem: identificador único, período do estudo (data de início e fim), métodos de coleta de dados, tipo de amostragem, tamanho da amostra, relacionamento com o artigo correspondente

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

### Dependências Externas

- Servidor (pode ser UNRAID ou qualquer servidor compatível com Docker, não requer GPU)
- Docker instalado e configurado no servidor
- Espaço em disco suficiente para armazenamento do banco de dados SQLite
- Acesso à internet para consulta a APIs externas:
  - API de IA selecionada pelo usuário (Gemini, ChatGPT ou Claude) com chave de API válida fornecida pelo usuário
  - API de taxonomia botânica acessível (sugestões: GBIF Species API, Tropicos, POWO - Plants of the World Online)
- Usuários devem possuir conta e chave de API válida em pelo menos um dos provedores de IA:
  - Google AI Studio (Gemini API): https://makersuite.google.com/app/apikey
  - OpenAI Platform (ChatGPT API): https://platform.openai.com/api-keys
  - Anthropic Console (Claude API): https://console.anthropic.com/settings/keys

### Dependências de Conhecimento

- Integração com APIs de IA externas (Gemini, ChatGPT, Claude) para extração de informações de documentos PDF
- Gerenciamento seguro de chaves de API no frontend (localStorage do navegador)
- Compreensão de taxonomia botânica e nomenclatura científica para validação de extração de nomes de espécies
- Conhecimento básico sobre obtenção de chaves de API dos provedores de IA (processo de cadastro e geração de chaves)

## Suposições *(opcional)*

- Os artigos científicos em PDF estão majoritariamente em formato de texto pesquisável (não exclusivamente imagens escaneadas)
- O servidor tem recursos de hardware suficientes (RAM, CPU) para executar a aplicação web e gerenciar o banco de dados
- Os artigos seguem, em sua maioria, estruturas convencionais de papers científicos (abstract, métodos, resultados, etc.)
- O volume de processamento não requer múltiplas instâncias ou paralelização complexa (um container é suficiente)
- Usuários têm acesso de rede ao servidor para acessar a interface web
- Usuários possuem ou estão dispostos a criar contas nos provedores de IA para obter chaves de API
- Usuários compreendem que o uso das APIs de IA pode incorrer em custos conforme os planos de cada provedor
- Artigos estarão predominantemente em português, inglês ou espanhol
- A precisão de extração de 80-85% é aceitável, considerando que edição manual estará disponível para correção
- Usuários têm acesso à internet para chamadas às APIs de IA e taxonomia durante o processamento

## Exclusões de Escopo *(opcional)*

- Sistema de autenticação e controle de acesso de usuários (sistema será de acesso aberto na rede local)
- Exportação de dados para outros formatos (CSV, JSON, etc.)
- Integração com sistemas externos ou APIs de bases de dados científicas (exceto API de taxonomia botânica para validação de nomes científicos)
- Processamento de documentos em formatos diferentes de PDF
- Backup automático do banco de dados
- Interface de administração para gerenciar o banco de dados
- Análises estatísticas ou visualizações dos dados agregados
- Sistema de notificações ou alertas
- Processamento em lote de múltiplos PDFs simultaneamente
- Versionamento ou histórico de alterações dos metadados
- Funcionalidade de OCR nativo (o sistema tentará processar PDFs escaneados mas não aplicará OCR para conversão)
- Armazenamento ou gerenciamento de chaves de API no servidor (chaves são gerenciadas exclusivamente pelo usuário no navegador)
- Fornecimento de créditos ou pagamento de custos de API de IA (usuários são responsáveis por seus próprios custos de API)

## Notas e Considerações *(opcional)*

### Seleção de Provedor de IA

O sistema suporta três provedores principais de IA, cada um com suas características:

**Google Gemini (recomendado para começar)**:
- API gratuita com quota generosa para uso experimental
- Excelente suporte a múltiplos idiomas incluindo português
- Boa capacidade de análise de documentos
- Facilidade de obtenção de chave de API

**OpenAI ChatGPT**:
- Alta qualidade de extração e compreensão de contexto
- Ótimo desempenho com documentos científicos
- Requer créditos pagos após cota gratuita inicial
- API bem documentada e madura

**Anthropic Claude**:
- Excelente para análise de documentos longos e complexos
- Bom raciocínio sobre estrutura de papers científicos
- Boa capacidade multilíngue
- Oferece modelos com diferentes níveis de custo/performance

Considerações para usuários:
- Começar com Gemini devido à quota gratuita generosa
- Avaliar qualidade de extração e custos antes de escalar
- Possibilidade de usar diferentes provedores para diferentes tipos de artigos
- Monitorar uso de API para controlar custos

### Considerações Técnicas de Docker

- A variável de ambiente para o caminho do banco SQLite deve ser claramente documentada
- O container deve expor a porta da aplicação web de forma configurável
- Considerar volume Docker para persistência do banco de dados
- Container Docker simplificado (não requer nvidia-docker runtime, apenas Docker padrão)
- Requisitos de hardware muito reduzidos sem necessidade de GPU

### Gerenciamento de Chaves de API e Privacidade

**Armazenamento no Navegador**:
- Chaves de API são armazenadas exclusivamente no localStorage do navegador do usuário
- Nunca são transmitidas ao servidor backend
- Permanecem no dispositivo do usuário
- Podem ser visualizadas, editadas ou removidas a qualquer momento

**Segurança**:
- Frontend faz chamadas diretas às APIs de IA usando a chave armazenada localmente
- CORS (Cross-Origin Resource Sharing) configurado adequadamente nas APIs
- Chave visível apenas no navegador do usuário em uso
- Se usuário limpar cache/dados do navegador, chave é perdida (precisa reconfigurar)

**Fluxo de Configuração**:
1. Usuário seleciona provedor de IA (Gemini/ChatGPT/Claude)
2. Sistema exibe instruções de como obter chave de API do provedor selecionado
3. Usuário cola chave de API na interface
4. Sistema valida chave fazendo chamada de teste à API
5. Se válida, chave é salva no localStorage
6. Sistema exibe confirmação e permite upload de PDFs

**Gerenciamento**:
- Botão "Configurações" na interface para visualizar/editar chave
- Opção de trocar de provedor (requer nova chave)
- Indicador visual de provedor em uso
- Aviso se chave estiver inválida ou expirada

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

### Estrutura de Documentação

Toda a documentação deverá incluir:
- README em português com instruções de instalação simplificadas (Docker padrão, sem GPU)
- Guia passo-a-passo de como obter chave de API de cada provedor (Gemini, ChatGPT, Claude)
- Guia de configuração das variáveis de ambiente do Docker
- Documentação de usuário sobre configuração inicial da chave de API
- Documentação de usuário sobre como usar a interface de upload
- Guia sobre como usar a interface de edição manual
- FAQ sobre privacidade e segurança das chaves de API
- Informações sobre custos estimados de uso das APIs de IA
- Descrição do schema do banco de dados SQLite
- Troubleshooting de problemas comuns (incluindo erros de API, chaves inválidas, etc.)
