# Build Portátil - EtnoPapers

## Descrição

Esta documentação explica como gerar a versão portátil (self-contained) da aplicação EtnoPapers que funciona em qualquer máquina Windows sem necessidade de instalação do .NET Framework.

## Pré-requisitos

- .NET 8.0 SDK instalado (`dotnet` no PATH)
- Windows 10/11 (ou Windows 7+ para executar a build)
- Git (para versão do código)

## Como Gerar a Build Portátil

### Opção 1: Usando PowerShell (Recomendado)

Execute este comando na raiz do repositório:

```powershell
cd D:\git\etnopapers

# Restaurar dependências
dotnet restore

# Publicar como self-contained
dotnet publish src/EtnoPapers.UI/EtnoPapers.UI.csproj `
    --configuration Release `
    --output EtnoPapers-Portable `
    --self-contained `
    --runtime win-x64 `
    --no-restore
```

### Opção 2: Usando Bash/Shell

```bash
cd D:\git\etnopapers

# Restaurar e publicar
dotnet restore
dotnet publish src/EtnoPapers.UI/EtnoPapers.UI.csproj \
    --configuration Release \
    --output EtnoPapers-Portable \
    --self-contained \
    --runtime win-x64 \
    --no-restore
```

## Estrutura da Build Portátil

Após executar o comando acima, você terá:

```
EtnoPapers-Portable/
├── EtnoPapers.UI.exe              # Executável principal
├── EtnoPapers.Core.dll            # Biblioteca core
├── EtnoPapers.UI.deps.json        # Dependências
├── LEIA-ME.txt                    # Instruções em português
├── INICIAR.bat                    # Script de inicialização
├── [Runtime .NET 8.0]             # Todos os arquivos do runtime
└── [Todas as dependências]        # NuGet packages
```

**Tamanho:** ~179 MB (inclui runtime completo)

## Limpeza (Apenas Português)

Por padrão, o .NET inclui recursos de vários idiomas. Para manter apenas português:

```powershell
cd EtnoPapers-Portable

# Remover pastas de idiomas
Remove-Item en-US, es, de, fr, it, ja, ko, pt-BR, ru, zh-CN, zh-TW -Recurse -Force -ErrorAction SilentlyContinue

# Remover documentação em inglês
Remove-Item README.txt -Force -ErrorAction SilentlyContinue
```

## Distribuição

### Criar Arquivo ZIP

```powershell
# No Windows Explorer ou PowerShell
Compress-Archive -Path EtnoPapers-Portable -DestinationPath EtnoPapers-Portable.zip
```

**Resultado:** `EtnoPapers-Portable.zip` (~179 MB)

### Enviar para Usuários

1. Hospede o ZIP em um servidor (GitHub Releases, OneDrive, Google Drive, etc)
2. Forneça o link de download
3. Usuários extraem em qualquer local
4. Executam `EtnoPapers.UI.exe` ou `INICIAR.bat`

## Uso da Build Portátil

### Primeira Execução

1. Extrair o ZIP em qualquer pasta
2. Clicar duplo em `EtnoPapers.UI.exe` ou `INICIAR.bat`
3. Ir para "Configurações"
4. Selecionar provedor de IA (Gemini, OpenAI ou Anthropic)
5. Inserir chave de API
6. Testar conexão
7. Usar normalmente

### Dados do Usuário

Todos os dados são armazenados em:
```
C:\Users\[usuario]\AppData\Roaming\EtnoPapers\
```

Incluindo:
- `config.json` - Configurações (chaves de API criptografadas)
- Registros de extração
- Logs da aplicação

Os dados persistem entre execuções e não estão na pasta portátil.

## Versionamento

### Incluir Versão

Para incluir número de versão na build, adicione a versão ao comando:

```powershell
dotnet publish src/EtnoPapers.UI/EtnoPapers.UI.csproj `
    --configuration Release `
    --output EtnoPapers-Portable-v1.1.0 `
    --self-contained `
    --runtime win-x64 `
    /p:Version=1.1.0 `
    --no-restore
```

## Troubleshooting

### "Restauração de dependências falha"
```powershell
# Limpar cache NuGet
dotnet nuget locals all --clear

# Tentar novamente
dotnet restore
```

### "Arquivo bloqueado durante build"
```powershell
# Fechar o executável
taskkill /F /IM EtnoPapers.UI.exe

# Remover pasta anterior
Remove-Item EtnoPapers-Portable -Recurse -Force

# Tentar novamente
```

### "Erro de runtime"
Certificar-se de que o .NET 8.0 SDK está instalado:
```powershell
dotnet --version
# Deve retornar algo como: 8.0.xxx
```

## Performance

- **Primeira execução:** ~5-10 segundos (aplicação carrega runtime)
- **Execuções subsequentes:** ~2-3 segundos
- **Memória:** ~150-200 MB em idle
- **Disco:** ~200 MB (descompactado)

## Segurança

A versão portátil:
- ✅ Mantém chaves de API criptografadas (Windows DPAPI)
- ✅ Não coleta dados pessoais
- ✅ É código aberto (pode ser auditado)
- ✅ Não requer conexão com servidores nossos
- ⚠️ Requer internet para conectar com provedores de IA

## Notas

- A build é self-contained, sem dependências externas
- Compatível com Windows 7 ou superior (x64)
- Não requer instalação do Visual C++ Redistributable
- Não requer .NET Framework pré-instalado
- Pode rodar em ambientes offline após configuração inicial

## Suporte

Para problemas ou perguntas:
- 📧 Abra uma issue no GitHub
- 🔗 https://github.com/edalcin/etnopapers

---

**Última atualização:** 15 de dezembro de 2025
**Versão da documentação:** 1.0
