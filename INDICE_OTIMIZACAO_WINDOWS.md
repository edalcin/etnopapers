# Índice - Otimização do Build Windows (v2.1)

**Status**: 📋 Planejamento Completo
**Data**: 2025-11-30
**Commit**: `213e8ba` Adicionar visão geral visual da otimização Windows

---

## 📚 Documentação - Ordem de Leitura Recomendada

### 1️⃣ COMECE AQUI (5 minutos)
**Arquivo**: `RESUMO_OTIMIZACAO_WINDOWS.md`

Resumo executivo para entender o problema e a solução em alto nível:
- Análise do problema atual (39 MB, travamento)
- Solução proposta (MSI Installer)
- Comparação antes/depois
- Próximos passos

👉 **Ideal para**: Entender rapidamente a proposta

---

### 2️⃣ VISÃO VISUAL (10 minutos)
**Arquivo**: `docs/VISAO_GERAL_OTIMIZACAO.txt`

Diagrama visual com ASCII art mostrando:
- Problema atual vs. solução proposta
- Arquitetura nova (MSI + app modular)
- Comparação de métricas
- 5 fases de implementação
- Benefícios principais

👉 **Ideal para**: Ver diagramas e entender visualmente

---

### 3️⃣ ESTRATÉGIA (20 minutos)
**Arquivo**: `docs/PLANO_OTIMIZACAO_WINDOWS.md`

Plano detalhado da abordagem:
- Problema identificado (raiz causa)
- Solução proposta (explicação técnica)
- 4 fases de implementação
- Recursos necessários
- Considerações importantes
- Timeline estimada (10-16 horas)
- Métricas de sucesso

👉 **Ideal para**: Entender a estratégia completa

---

### 4️⃣ IMPLEMENTAÇÃO (1-2 horas)
**Arquivo**: `docs/TAREFAS_OTIMIZACAO_WINDOWS.md`

19 tarefas detalhadas e executáveis:
- **Fase 1**: T1.1 - T1.3 (Preparação: 1-2h)
- **Fase 2**: T2.1 - T2.6 (Implementação: 4-6h)
  - T2.1: build.spec otimizado
  - T2.2: Product.wxs (MSI principal)
  - T2.3: Components.wxs (arquivos)
  - T2.4: check-python.ps1
  - T2.5: post-install.ps1
  - T2.6: requirements-windows.txt
- **Fase 3**: T3.1 - T3.2 (Automação: 2-3h)
  - T3.1: Atualizar build-windows.bat
  - T3.2: GitHub Actions workflow
- **Fase 4**: T4.1 - T4.3 (Testes: 2-3h)
  - T4.1: Plano de testes
  - T4.2: Troubleshooting guide
  - T4.3: Guia do usuário
- **Fase 5**: T5.1 - T5.2 (Documentação: 1-2h)

Cada tarefa inclui:
- ✅ Descrição clara
- ✅ Subtarefas checáveis
- ✅ Scripts e código de exemplo
- ✅ Tempo estimado
- ✅ Saída esperada

👉 **Ideal para**: Implementadores - segue passo-a-passo

---

## 🎯 Resumo Rápido

| Aspecto | Detalhe |
|---------|---------|
| **Problema** | PyInstaller 39MB, travamento, 100% RAM |
| **Solução** | MSI Installer (5-10 MB) + app modular (15 MB) |
| **Benefício** | 75% menor download, 80% mais rápido |
| **Tempo** | 10-16 horas (5 fases) |
| **Status** | 📋 Planejado, aguardando implementação |

---

## 📍 Localização dos Arquivos

```
etnopapers/
├── RESUMO_OTIMIZACAO_WINDOWS.md          ← COMECE AQUI
├── INDICE_OTIMIZACAO_WINDOWS.md          ← Você está aqui
│
└── docs/
    ├── VISAO_GERAL_OTIMIZACAO.txt        ← Visão visual
    ├── PLANO_OTIMIZACAO_WINDOWS.md       ← Estratégia
    └── TAREFAS_OTIMIZACAO_WINDOWS.md     ← Implementação
```

---

## 🚀 Como Começar a Implementação

Quando aprovado, siga este fluxo:

### Passo 1: Instalação de Ferramentas
```bash
# Instalar WiX Toolset
choco install wixtoolset -y

# Verificar
wix --version
```

### Passo 2: Preparação (T1)
Seguir tarefas em `docs/TAREFAS_OTIMIZACAO_WINDOWS.md`:
- [ ] T1.1 - Analisar executável
- [ ] T1.2 - Instalar WiX
- [ ] T1.3 - Preparar estrutura

### Passo 3: Implementação (T2-T3)
Implementar as 6 tarefas da Fase 2 e 2 da Fase 3

### Passo 4: Testes (T4)
Testar em máquina Windows real

### Passo 5: Documentação (T5)
Atualizar guias e documentação

---

## 📊 Métricas de Sucesso

Após implementação, esperamos:

| Métrica | Antes | Depois | Meta |
|---------|-------|--------|------|
| Download | 39 MB | 5-10 MB | ✅ 75% menor |
| Inicialização | 15-30s | 2-5s | ✅ 80% rápido |
| RAM | 100%+ | 20-30% | ✅ 70% menos |
| Taxa Sucesso | 10% | >95% | ✅ 10x melhor |

---

## 🔗 Referências Git

**Commits com Planejamento**:
```bash
# Visualizar commits
git log --oneline -5

# 213e8ba docs: Adicionar visão geral visual
# f535aa7 docs: Adicionar resumo executivo
# 9a09f8e docs: Criar plano de otimização Windows
```

**Para clonar repositório**:
```bash
git clone https://github.com/edalcin/etnopapers.git
cd etnopapers
git log --all | grep -A2 "otimiza"
```

---

## ❓ Perguntas Frequentes

**P: Por onde começo?**
R: Leia `RESUMO_OTIMIZACAO_WINDOWS.md` (5 min)

**P: Qual é a ordem de leitura?**
R: Resumo → Visual → Plano → Tarefas

**P: Quanto tempo leva implementar?**
R: 10-16 horas (5 fases)

**P: Preciso de experiência com WiX?**
R: Não, todos os exemplos estão em `TAREFAS_OTIMIZACAO_WINDOWS.md`

**P: Posso implementar em paralelo?**
R: Sim, algumas tarefas (T2.1, T2.2, T2.3) podem ser paralelas

**P: E se houver problemas?**
R: T4.2 (Troubleshooting) terá soluções para problemas comuns

---

## 📋 Checklist de Implementação

- [ ] Ler este índice (você está aqui)
- [ ] Ler `RESUMO_OTIMIZACAO_WINDOWS.md`
- [ ] Ler `docs/VISAO_GERAL_OTIMIZACAO.txt`
- [ ] Ler `docs/PLANO_OTIMIZACAO_WINDOWS.md`
- [ ] Ler `docs/TAREFAS_OTIMIZACAO_WINDOWS.md`
- [ ] Decidir: Implementar? (S/N)
- [ ] Se SIM → Começar T1.1
- [ ] Completar todas as 19 tarefas
- [ ] Testar em máquina Windows real
- [ ] Fazer release v2.1
- [ ] ✅ Deploy Windows profissional!

---

## 👤 Próximas Ações para o Usuário

1. **Revisar planejamento** (45 minutos)
   - Ler os 4 documentos acima

2. **Decidir** (5 minutos)
   - Prosseguir com implementação?

3. **Aprovar ou sugerir mudanças** (livre)
   - Comentários no planejamento?

4. **Iniciar implementação** (se aprovado)
   - Começar com T1.1

---

**Status**: ✅ Planejamento 100% Completo
**Aguardando**: Aprovação e Início da Implementação

Para dúvidas, revisar documentação correspondente ou criar issue no GitHub.
