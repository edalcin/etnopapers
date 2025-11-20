# Especificação da Funcionalidade: Sistema de Extração de Metadados de Artigos Etnobotânicos

**Branch da Funcionalidade**: `001-pdf-metadata-extraction`
**Criado**: 2025-11-20
**Status**: Rascunho
**Entrada**: Sistema de extração de metadados de artigos científicos em PDF sobre etnobotânica usando IA local em Docker/UNRAID com banco SQLite

## Cenários de Usuário e Testes *(obrigatório)*

### História de Usuário 1 - Upload e Extração Básica de PDF (Prioridade: P1)

Um pesquisador acessa o sistema através de uma página web, faz o upload de um artigo científico em PDF sobre etnobotânica, e o sistema extrai automaticamente os metadados disponíveis no artigo usando um modelo de IA que roda localmente. Após o processamento, o sistema exibe um resumo estruturado dos metadados extraídos na mesma página, sem armazenar o arquivo PDF original.

**Por que esta prioridade**: Esta é a funcionalidade central do sistema. Sem ela, nenhum outro componente tem valor. Representa o MVP completo e entregável.

**Teste Independente**: Pode ser totalmente testado fazendo upload de um único PDF e verificando se os metadados são extraídos e exibidos corretamente. Entrega valor imediato ao permitir que usuários extraiam dados de artigos sem processamento manual.

**Cenários de Aceitação**:

1. **Dado** que o usuário está na página principal do sistema, **Quando** o usuário seleciona um arquivo PDF válido e clica em upload, **Então** o sistema processa o arquivo e exibe os metadados extraídos em formato estruturado
2. **Dado** que o processamento foi concluído, **Quando** o sistema exibe os resultados, **Então** os metadados incluem título, autores, ano, resumo, DOI, local de publicação, região do estudo, comunidades envolvidas, espécies de plantas (nomes científicos e vernaculares), período do estudo, métodos de coleta de dados e tipo de amostragem
3. **Dado** que os metadados foram extraídos, **Quando** o usuário visualiza o resumo, **Então** o sistema indica claramente quais campos não puderam ser extraídos do artigo
4. **Dado** que o processamento foi concluído, **Quando** o sistema armazena os dados, **Então** nenhum arquivo PDF é armazenado, apenas os metadados no banco de dados SQLite

---

### História de Usuário 2 - Validação e Tratamento de Erros no Upload (Prioridade: P2)

Um pesquisador tenta fazer upload de diferentes tipos de arquivos e o sistema valida adequadamente, rejeitando arquivos não-PDF ou PDFs corrompidos, fornecendo mensagens de erro claras e orientando o usuário sobre como corrigir o problema.

**Por que esta prioridade**: Melhora significativamente a experiência do usuário ao fornecer feedback claro sobre problemas, mas não é essencial para a funcionalidade básica. Pode ser implementada e testada após o fluxo principal estar funcionando.

**Teste Independente**: Pode ser testado independentemente tentando upload de arquivos inválidos (imagens, documentos corrompidos, formatos incorretos) e verificando se as mensagens de erro apropriadas são exibidas.

**Cenários de Aceitação**:

1. **Dado** que o usuário está na página de upload, **Quando** o usuário tenta fazer upload de um arquivo que não é PDF, **Então** o sistema rejeita o arquivo e exibe mensagem explicativa informando que apenas arquivos PDF são aceitos
2. **Dado** que o usuário selecionou um arquivo PDF corrompido, **Quando** o sistema tenta processar o arquivo, **Então** o sistema detecta o problema e informa ao usuário que o arquivo está corrompido ou ilegível
3. **Dado** que ocorreu um erro durante o processamento, **Quando** o sistema exibe a mensagem de erro, **Então** a mensagem é clara, em português brasileiro, e sugere possíveis soluções

---

### História de Usuário 3 - Consulta e Visualização de Histórico (Prioridade: P3)

Um pesquisador deseja revisar artigos processados anteriormente e acessa uma interface para visualizar o histórico de extrações realizadas, podendo filtrar e buscar por diferentes campos de metadados.

**Por que esta prioridade**: Adiciona valor significativo ao permitir reuso dos dados extraídos, mas não é necessária para o processamento inicial. É uma funcionalidade de conveniência que pode ser adicionada após o sistema básico estar operacional.

**Teste Independente**: Pode ser testada independentemente após vários PDFs terem sido processados, verificando se o histórico é exibido corretamente e se os filtros funcionam adequadamente.

**Cenários de Aceitação**:

1. **Dado** que múltiplos artigos foram processados e armazenados, **Quando** o usuário acessa a página de histórico, **Então** o sistema exibe uma lista de todos os artigos processados com seus metadados principais
2. **Dado** que o usuário está visualizando o histórico, **Quando** o usuário busca por um termo específico (ex: nome de espécie, região, comunidade), **Então** o sistema filtra os resultados exibindo apenas artigos que correspondem ao critério de busca
3. **Dado** que o usuário selecionou um artigo do histórico, **Quando** o usuário clica para visualizar detalhes, **Então** o sistema exibe todos os metadados extraídos daquele artigo específico

---

### Casos Extremos

- O que acontece quando o PDF está em formato de imagem (escaneado) sem texto pesquisável?
- Como o sistema lida com artigos em múltiplos idiomas (inglês, português, espanhol)?
- O que acontece quando o PDF tem centenas de páginas ou é muito grande?
- Como o sistema trata artigos que não seguem a estrutura tradicional de papers científicos?
- O que acontece quando campos obrigatórios (como título ou autores) não podem ser identificados?
- Como o sistema processa artigos com tabelas complexas ou dados em formatos não-textuais?
- O que acontece quando o servidor UNRAID fica sem espaço em disco para o banco de dados?
- Como o sistema se comporta se múltiplos usuários fazem upload simultaneamente e a GPU está em uso?

## Requisitos *(obrigatório)*

### Requisitos Funcionais

- **RF-001**: O sistema DEVE fornecer uma interface web com funcionalidade de upload de arquivos PDF
- **RF-002**: O sistema DEVE processar arquivos PDF usando um modelo de inteligência artificial que roda localmente
- **RF-003**: O sistema DEVE extrair os seguintes metadados quando disponíveis no artigo: título, ano de publicação, lista de autores, resumo/abstract, DOI (Digital Object Identifier), local/jornal de publicação, região geográfica do estudo, comunidades tradicionais envolvidas, espécies de plantas identificadas (nomes científicos e vernaculares), ano ou período em que o estudo foi conduzido, métodos de coleta de dados das comunidades, e tipo de amostragem utilizado
- **RF-004**: O sistema DEVE armazenar os metadados extraídos em um banco de dados SQLite
- **RF-005**: O sistema DEVE exibir um resumo estruturado dos metadados extraídos após o processamento do PDF
- **RF-006**: O sistema NÃO DEVE armazenar os arquivos PDF após a extração dos metadados
- **RF-007**: O sistema DEVE indicar claramente quais metadados não foram encontrados ou não puderam ser extraídos de cada artigo
- **RF-008**: O sistema DEVE ser empacotado e executado em um container Docker
- **RF-009**: O sistema DEVE ser compatível com instalação em servidor UNRAID
- **RF-010**: O sistema DEVE utilizar a GPU NVIDIA disponível no servidor UNRAID para processamento com o modelo de IA
- **RF-011**: O sistema DEVE aceitar o caminho do banco de dados SQLite como variável de ambiente durante a criação do container Docker
- **RF-012**: O sistema DEVE utilizar um modelo de IA de código aberto disponível no Hugging Face que possa rodar completamente offline dentro do container Docker
- **RF-013**: Toda a documentação do sistema DEVE ser gerada e mantida em português brasileiro
- **RF-014**: O sistema DEVE operar exclusivamente na branch main, sem branches de desenvolvimento separadas

### Entidades Principais

- **Artigo Científico**: Representa um paper acadêmico processado pelo sistema. Atributos incluem: identificador único, data de processamento, título, ano de publicação, autores (lista), resumo, DOI, local de publicação, status de processamento
- **Região de Estudo**: Representa a localização geográfica onde o estudo foi conduzido. Atributos incluem: identificador único, descrição da região, país, estado/província, coordenadas geográficas (se disponíveis), relacionamento com múltiplos artigos
- **Comunidade Tradicional**: Representa grupos comunitários estudados nos artigos. Atributos incluem: identificador único, nome ou descrição da comunidade, tipo de comunidade (indígena, quilombola, ribeirinha, etc.), relacionamento com artigos e regiões
- **Espécie de Planta**: Representa plantas identificadas nos estudos. Atributos incluem: identificador único, nome científico (binomial), nomes vernaculares (lista de nomes populares em diferentes idiomas/dialetos), família botânica, usos reportados, relacionamento com artigos onde foi mencionada
- **Dados de Estudo**: Representa informações metodológicas do estudo. Atributos incluem: identificador único, período do estudo (data de início e fim), métodos de coleta de dados, tipo de amostragem, tamanho da amostra, relacionamento com o artigo correspondente

## Critérios de Sucesso *(obrigatório)*

### Resultados Mensuráveis

- **CS-001**: Usuários conseguem fazer upload de um PDF e visualizar os metadados extraídos em menos de 2 minutos para artigos de até 30 páginas
- **CS-002**: O sistema extrai com sucesso pelo menos 80% dos metadados disponíveis em artigos que seguem formato científico padrão
- **CS-003**: O sistema identifica e extrai corretamente nomes científicos de plantas com precisão mínima de 85% em relação à extração manual
- **CS-004**: O sistema processa artigos em PDF sem armazenar os arquivos originais, reduzindo o uso de armazenamento em 100% comparado a sistemas que mantêm os PDFs
- **CS-005**: A interface web permite que usuários sem conhecimento técnico façam upload e visualizem resultados sem treinamento prévio (taxa de conclusão de tarefa de 90% no primeiro uso)
- **CS-006**: O sistema opera completamente offline dentro do container Docker, sem dependências de APIs ou serviços externos
- **CS-007**: Toda a documentação técnica e de usuário está disponível em português brasileiro, facilitando adoção por pesquisadores brasileiros
- **CS-008**: O tempo de configuração inicial no servidor UNRAID não ultrapassa 15 minutos para usuários familiarizados com Docker

## Dependências *(opcional)*

### Dependências Externas

- Servidor UNRAID com GPU NVIDIA disponível
- Docker instalado e configurado no servidor UNRAID
- Drivers NVIDIA compatíveis instalados no host UNRAID para suporte a CUDA/GPU
- Espaço em disco suficiente para armazenamento do banco de dados SQLite

### Dependências de Conhecimento

- Modelo de IA de código aberto do Hugging Face compatível com extração de informações de documentos PDF (sugestões de pesquisa: modelos de document understanding, layout analysis, ou information extraction)
- Conhecimento sobre empacotamento de modelos de IA em containers Docker
- Compreensão de taxonomia botânica e nomenclatura científica para validação de extração de nomes de espécies

## Suposições *(opcional)*

- Os artigos científicos em PDF estão majoritariamente em formato de texto pesquisável (não exclusivamente imagens escaneadas)
- O servidor UNRAID tem recursos de hardware suficientes (RAM, CPU) além da GPU para executar o modelo de IA e a aplicação web simultaneamente
- Os artigos seguem, em sua maioria, estruturas convencionais de papers científicos (abstract, métodos, resultados, etc.)
- O volume de processamento não requer múltiplas instâncias ou paralelização complexa (um container é suficiente)
- Usuários têm acesso de rede ao servidor UNRAID para acessar a interface web
- O modelo de IA selecionado pode ser configurado para rodar com a memória de GPU disponível no servidor
- Artigos estarão predominantemente em português, inglês ou espanhol
- A precisão de extração de 80-85% é aceitável, considerando que revisão manual será necessária para uso científico rigoroso

## Exclusões de Escopo *(opcional)*

- Sistema de autenticação e controle de acesso de usuários (sistema será de acesso aberto na rede local)
- Funcionalidades de edição manual ou correção dos metadados extraídos
- Exportação de dados para outros formatos (CSV, JSON, etc.)
- Integração com sistemas externos ou APIs de bases de dados científicas
- Processamento de documentos em formatos diferentes de PDF
- Backup automático do banco de dados
- Interface de administração para gerenciar o banco de dados
- Análises estatísticas ou visualizações dos dados agregados
- Sistema de notificações ou alertas
- Processamento em lote de múltiplos PDFs simultaneamente
- Versionamento ou histórico de alterações dos metadados

## Notas e Considerações *(opcional)*

### Seleção do Modelo de IA

A escolha do modelo de IA do Hugging Face é crítica para o sucesso do projeto. Modelos candidatos devem:
- Suportar document understanding e information extraction
- Ser capazes de processar texto em múltiplos idiomas (especialmente português, inglês, espanhol)
- Ter tamanho compatível com as limitações de memória da GPU disponível
- Ter licença permissiva para uso local e offline

Exemplos de modelos para investigar:
- LayoutLM/LayoutLMv3 para análise de layout de documentos
- BERT multilíngue para extração de entidades nomeadas
- Modelos específicos de document AI como Donut ou DocFormer

### Considerações Técnicas de Docker/UNRAID

- A variável de ambiente para o caminho do banco SQLite deve ser claramente documentada
- O container deve expor a porta da aplicação web de forma configurável
- Considerar volume Docker para persistência do banco de dados
- Garantir que o container tem acesso adequado à GPU NVIDIA (nvidia-docker runtime)

### Estrutura de Documentação

Toda a documentação deverá incluir:
- README em português com instruções de instalação no UNRAID
- Guia de configuração das variáveis de ambiente
- Documentação de usuário sobre como usar a interface de upload
- Descrição do schema do banco de dados SQLite
- Troubleshooting de problemas comuns
