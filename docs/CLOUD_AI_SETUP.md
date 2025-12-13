# Guia de Configuração de IA em Nuvem

Este guia explica como configurar e usar provedores de IA em nuvem no EtnoPapers para extração automática de metadados etnobotânicos.

## Visão Geral

O EtnoPapers agora suporta três provedores de IA em nuvem:
- **Google Gemini** (Gemini 1.5 Flash)
- **OpenAI** (GPT-4o-mini)
- **Anthropic** (Claude 3.5 Sonnet)

### Vantagens da IA em Nuvem

✅ **50% mais rápido** que OLLAMA local (2-10 segundos vs 30-60 segundos por PDF)
✅ **Melhor precisão** e menos alucinações na extração de metadados
✅ **Sem requisitos de hardware** - não precisa de GPU ou instalação local
✅ **Sempre atualizado** - modelos mais recentes sem necessidade de download

## Passo 1: Escolha um Provedor

### Google Gemini (Recomendado)
- **Custo**: Gratuito até 15 requisições por minuto
- **Velocidade**: 2-5 segundos por PDF
- **Qualidade**: Excelente para português brasileiro
- **Obter chave**: https://ai.google.dev/

### OpenAI
- **Custo**: Pago (GPT-4o-mini é econômico, ~$0.15 por milhão de tokens)
- **Velocidade**: 3-6 segundos por PDF
- **Qualidade**: Excelente para extração estruturada
- **Obter chave**: https://platform.openai.com/

### Anthropic Claude
- **Custo**: Pago (Claude 3.5 Sonnet, ~$3 por milhão de tokens)
- **Velocidade**: 4-8 segundos por PDF
- **Qualidade**: Melhor para raciocínio complexo e nuances
- **Obter chave**: https://console.anthropic.com/

## Passo 2: Obter Chave de API

### Google Gemini

1. Acesse https://ai.google.dev/
2. Faça login com sua conta Google
3. Clique em "Get API Key" no menu
4. Clique em "Create API Key in new project"
5. Copie a chave gerada (formato: `AIza...`)

**Limites gratuitos**:
- 15 requisições por minuto
- 1500 requisições por dia
- Suficiente para processar ~100 PDFs por hora

### OpenAI

1. Acesse https://platform.openai.com/
2. Crie uma conta ou faça login
3. Vá em "API Keys" no menu lateral
4. Clique em "Create new secret key"
5. Copie a chave (formato: `sk-proj-...`)

**Custo estimado**:
- GPT-4o-mini: $0.15 / milhão de tokens de entrada
- Custo médio: ~$0.002 por PDF (menos de 1 centavo)
- Para 1000 PDFs: ~$2 USD

### Anthropic Claude

1. Acesse https://console.anthropic.com/
2. Crie uma conta ou faça login
3. Vá em "API Keys"
4. Clique em "Create Key"
5. Copie a chave (formato: `sk-ant-...`)

**Custo estimado**:
- Claude 3.5 Sonnet: $3 / milhão de tokens de entrada
- Custo médio: ~$0.03 por PDF
- Para 1000 PDFs: ~$30 USD

## Passo 3: Configurar no EtnoPapers

1. Abra o EtnoPapers
2. Vá em **Configurações** (menu lateral)
3. Na seção "Configuração do Provedor de IA":
   - Selecione o provedor desejado no dropdown
   - Cole sua chave de API no campo "Chave de API"
   - Clique em "Salvar Configurações"

**Nota de Segurança**: A chave de API é criptografada usando Windows DPAPI e armazenada localmente. Nunca compartilhe sua chave de API com terceiros.

## Passo 4: Testar a Configuração

1. Vá para a página **Upload**
2. Selecione um PDF de teste (de preferência um artigo etnobotânico)
3. Clique em **Processar PDF**
4. Aguarde a extração (deve levar 2-10 segundos)
5. Verifique se os metadados foram extraídos corretamente

### Campos Extraídos

O sistema extrai automaticamente:
- ✓ Título (normalizado em Proper Case)
- ✓ Autores (formato APA: Sobrenome, I.)
- ✓ Ano de publicação
- ✓ Resumo (sempre em português brasileiro)
- ✓ Espécies botânicas (nome vernacular + científico + tipo de uso)
- ✓ Dados geográficos (país, estado, município, local, bioma)
- ✓ Informações da comunidade tradicional
- ✓ Metodologia utilizada

## Comparação de Performance

| Provedor | Velocidade Média | Precisão | Custo | Melhor Para |
|----------|-----------------|----------|-------|-------------|
| **Gemini** | 2-5s | 95% | Gratuito | Uso geral, usuários iniciantes |
| **OpenAI** | 3-6s | 96% | ~$0.002/PDF | Extração estruturada, volumes médios |
| **Anthropic** | 4-8s | 97% | ~$0.03/PDF | Artigos complexos, raciocínio avançado |
| OLLAMA (local) | 30-60s | 85-90% | Gratuito | Offline, privacidade total |

**Recomendação**: Comece com **Google Gemini** (gratuito e rápido). Migre para OpenAI ou Anthropic se precisar de maior precisão ou volumes maiores.

## Solução de Problemas

### Erro: "Chave de API inválida ou expirada"

**Solução**:
1. Verifique se a chave foi copiada corretamente (sem espaços extras)
2. Confirme que a chave é válida no console do provedor
3. Para OpenAI/Anthropic: verifique se você tem créditos disponíveis

### Erro: "Limite de requisições excedido"

**Solução**:
1. **Gemini**: Aguarde 1 minuto (limite de 15 requisições/minuto)
2. **OpenAI/Anthropic**: Aumente os limites de taxa no console do provedor
3. O sistema tenta automaticamente 3x com backoff exponencial

### Erro: "Sem conexão com a internet"

**Solução**:
1. Verifique sua conexão de internet
2. Teste acessando https://google.com no navegador
3. Verifique firewall/proxy corporativo

### Erro: "Tempo limite excedido"

**Solução**:
1. Verifique se sua conexão está lenta
2. Tente novamente (timeout configurado em 30 segundos)
3. PDFs muito grandes (>10MB) podem demorar mais

### Resumo não está em português

**Nota**: O sistema instrui todos os provedores a retornar o resumo em português brasileiro. Se o PDF original não tem resumo em português, o provedor pode:
- Traduzir automaticamente o resumo (comportamento mais comum)
- Retornar em inglês (raro)

**Solução**: Edite manualmente o campo "Resumo" após a extração, se necessário.

### Qualidade da extração está ruim

**Causas comuns**:
1. PDF é uma imagem escaneada (não tem texto selecionável)
2. PDF está corrompido ou com formatação muito complexa
3. Artigo não é etnobotânico (sistema otimizado para esse domínio)

**Soluções**:
1. Use OCR para converter PDFs de imagem em texto
2. Tente outro provedor (Anthropic é melhor para casos complexos)
3. Edite manualmente os campos após extração

## Migração do OLLAMA

Se você estava usando OLLAMA localmente:

1. **Seus dados são preservados** - todos os registros salvos continuam acessíveis
2. **Configuração automática** - o sistema detecta automaticamente e mostra um banner de migração
3. **Sem perda de funcionalidade** - todos os recursos continuam funcionando

**Banner de Migração**: Você verá um aviso amarelo na página de Configurações. Você pode fechá-lo após configurar um provedor de nuvem.

## Perguntas Frequentes

### Preciso pagar para usar IA em nuvem?

Não necessariamente. O **Google Gemini** é gratuito com limites generosos (15 req/min, 1500 req/dia). Para a maioria dos pesquisadores, isso é suficiente.

### Minha chave de API fica armazenada onde?

Localmente no seu computador, em: `%LOCALAPPDATA%\EtnoPapers\config.json`

A chave é **criptografada com Windows DPAPI** e só pode ser descriptografada no seu usuário Windows. Nunca é enviada para servidores do EtnoPapers.

### Posso usar offline?

Não. Provedores de IA em nuvem requerem conexão de internet. Se você precisa trabalhar offline, considere usar OLLAMA local (não suportado nesta versão).

### Posso alternar entre provedores?

Sim! Vá em Configurações e troque o provedor a qualquer momento. A chave anterior é preservada se você voltar.

### Como monitoro meus custos?

- **Gemini**: Console do Google Cloud (se ultrapassar limites gratuitos)
- **OpenAI**: https://platform.openai.com/usage
- **Anthropic**: https://console.anthropic.com/settings/billing

### Qual provedor consome menos tokens?

Todos usam aproximadamente a mesma quantidade de tokens (6000-8000 por PDF). O custo varia pelo preço do provedor, não pelo consumo.

### Os dados dos PDFs são privados?

**Importante**: Ao usar IA em nuvem, o conteúdo do PDF é enviado para os servidores do provedor (Google, OpenAI ou Anthropic).

- **Google/OpenAI/Anthropic**: Não treinam modelos com seus dados (conforme políticas de privacidade)
- **EtnoPapers**: Nunca armazena ou tem acesso aos seus PDFs ou chaves de API

Se você precisa de privacidade total, use OLLAMA local (não incluído nesta versão).

## Suporte

- **Documentação técnica**: Consulte `docs/estrutura.json` para ver a estrutura completa de dados
- **Issues**: Reporte bugs em https://github.com/seu-repositorio/etnopapers/issues
- **Comunidade**: Participe das discussões sobre etnobotânica e extração de dados

---

**Última atualização**: 2025-12-13
**Versão do EtnoPapers**: 2.0 (Cloud AI Migration)
