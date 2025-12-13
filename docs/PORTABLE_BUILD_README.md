# EtnoPapers v2.0.0 - Versão Portátil para Pen-Drive

## 📋 Pré-requisitos

Para usar esta versão portátil do EtnoPapers, você precisa ter instalado no seu computador:

- **Windows 10** ou superior
- **.NET 8.0 Runtime** (necessário - [baixar aqui](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- Chave de API de um provedor de IA:
  - Google Gemini ([obter aqui](https://ai.google.dev/)) - GRÁTIS
  - OpenAI ([obter aqui](https://platform.openai.com/))
  - Anthropic Claude ([obter aqui](https://console.anthropic.com/))

## 🚀 Como Usar

### 1. Preparar o Pen-Drive

1. Copie a pasta `publish/` para seu pen-drive
2. A pasta ocupa apenas ~29 MB (bem menor que a versão com .NET embutida)

### 2. Executar o EtnoPapers

1. Conecte o pen-drive ao computador
2. Navegue até a pasta copiada
3. Execute: **`EtnoPapers.UI.exe`**

### 3. Configuração Inicial

1. Abra a aba **Configurações**
2. Escolha um provedor de IA (Gemini, OpenAI ou Anthropic)
3. Cole sua chave de API
4. (Opcional) Configure MongoDB para sincronização
5. Clique em **Salvar Configurações**

## 📊 Tamanho do Build

| Aspecto | Tamanho |
|---------|---------|
| **Executável + Dependências** | ~29 MB |
| **Número de Arquivos** | 53 |
| **Requer .NET 8.0 Runtime** | Sim (~150 MB adicional se não instalado) |

## 💡 Dicas

- **Primeira Vez?** Use Google Gemini - é grátis e não requer cartão de crédito
- **Dados Persistem?** Sim! Todos os registros são salvos em `AppData/Local/EtnoPapers/data.json`
- **Backup?** Recomenda-se sincronizar com MongoDB para backup automático
- **Múltiplos Computadores?** Você pode usar o mesmo pen-drive em diferentes PCs (com .NET 8.0 instalado)

## 🛠️ Linguagem

Este programa é **100% em português brasileiro**. Sem opções de outros idiomas.

## 📝 Registro de Extração

Cada extração registra:
- ✅ Tempo de extração (em segundos)
- ✅ Agente/Provedor utilizado (Gemini, OpenAI, Anthropic)
- ✅ Data/hora de criação
- ✅ Metadados etnobotânicos completos

## 📧 Suporte

Para problemas, visite: https://github.com/edalcin/etnopapers/issues

---

**Versão**: 2.0.0
**Data**: Dezembro 2025
**Licença**: [A definir]
