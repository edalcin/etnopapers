# 🧪 Guia de Teste - Etnopapers MVP

Seu MVP está **100% funcional**! Siga este guia para testar todas as funcionalidades.

## 📋 Pré-requisitos

- **Python 3.11+**
- **Node.js 18+**
- **Google Gemini API Key** (obtenha em https://ai.google.dev)
- **Git**

## 🚀 Opção 1: Testar Localmente (Recomendado)

### Terminal 1 - Backend

```bash
cd backend

# Windows
python -m venv venv
venv\Scripts\activate

# Linux/Mac
python3 -m venv venv
source venv/bin/activate

# Instalar dependências
pip install -r requirements.txt

# Inicializar banco de dados
python database/init_db.py

# Executar servidor
uvicorn main:app --reload
```

✅ Backend rodando em: **http://localhost:8000**
📖 API Docs em: **http://localhost:8000/docs**

### Terminal 2 - Frontend

```bash
cd frontend

# Instalar dependências
npm install

# Executar dev server
npm run dev
```

✅ Frontend rodando em: **http://localhost:5173**

---

## 🐳 Opção 2: Testar com Docker

```bash
# Na raiz do projeto
docker-compose up

# Aguarde até ver: "Uvicorn running on http://0.0.0.0:8000"
```

✅ Aplicação rodando em: **http://localhost:8000**

---

## 🧪 Fluxo Completo de Teste

### Passo 1️⃣ : Verificar Backend Rodando

Abra no navegador:
```
http://localhost:8000/health
```

Esperado:
```json
{
  "status": "healthy",
  "version": "0.1.0",
  "environment": "development",
  "database": {
    "size_mb": 0.05,
    "tables": 12
  }
}
```

### Passo 2️⃣: Acessar Frontend

Abra: **http://localhost:5173**

Você verá:
- 🏠 Página inicial com features
- 📝 Menu de navegação (Home, Upload, Histórico, Configurações)

### Passo 3️⃣: Configurar API Key (Google Gemini)

1. Clique em **"Configurações"**
2. Veja o formulário **"Configurar Chave de API"**
3. Garanta que **"Google Gemini"** está selecionado
4. Clique em **"Como obter uma chave →"**
5. Siga as instruções:
   - Acesse https://ai.google.dev
   - Clique em "Get API Key"
   - Crie um novo projeto (ou use existente)
   - Copie a chave
6. Cole a chave no campo do formulário
7. Clique em **"Validar e Salvar"**

Esperado:
```
✅ Chave validada com sucesso!
Provedor: gemini
```

### Passo 4️⃣: Testar Upload e Extração

1. Clique em **"Upload"**
2. **Opção A**: Selecionar um PDF de teste
   - Crie um arquivo de teste simples em PDF ou
   - Use qualquer PDF científico que tenha
3. **Opção B**: Drag-and-drop um PDF
   - Arraste um arquivo .pdf para a área de upload

Esperado:
```
✅ Arquivo selecionado: seu_arquivo.pdf
   Tamanho: X.XX MB
```

### Passo 5️⃣: Extração Automática com IA

Após selecionar o arquivo:

1. Sistema extrai texto do PDF automaticamente
2. Envia para Google Gemini
3. Exibe metadados extraídos em 30-60 segundos

Esperado:
```
📚 Metadados Extraídos
- Título: [Título do artigo]
- Ano: 2024
- Autores: Nome Sobrenome
- Espécies: [Lista de plantas]
- Regiões: [Lista de locais]
- Comunidades: [Grupos identificados]
```

### Passo 6️⃣: Revisar e Salvar

1. Revise os metadados extraídos
2. Verifique se detecta duplicatas automaticamente
3. Clique em **"💾 Salvar"**

Esperado:
```
✅ Artigo salvo com sucesso!
```

Você retorna ao upload pronto para o próximo artigo.

### Passo 7️⃣: Visualizar Histórico

1. Clique em **"Histórico"**
2. Veja tabela com todos os artigos

Recursos para testar:
- 🔍 **Busca**: Digite no campo "🔍 Buscar por título..."
- ⬆️/⬇️ **Ordenação**: Clique no header da coluna
- 📄 **Paginação**: Selecione 10, 25, 50 ou 100 itens por página

Esperado:
```
Mostrando X de Y artigos
Página 1 de Z
```

### Passo 8️⃣: Download do Banco de Dados

1. Vá em **"Configurações"**
2. Scroll até **"💾 Banco de Dados"**
3. Veja estatísticas:
   - Tamanho total (MB)
   - Número de tabelas
   - Registros por tabela
4. Clique em **"📥 Baixar Banco de Dados"**

Esperado:
```
✅ Download iniciado com sucesso!
Arquivo: etnopapers_2025-01-15.db
```

O arquivo será salvo em Downloads com nome:
```
etnopapers_YYYYMMDD.db
```

---

## 🔌 Testes de API (Curl/Postman)

### Health Check
```bash
curl http://localhost:8000/health
```

### Listar Artigos
```bash
curl "http://localhost:8000/api/articles?page=1&page_size=10"
```

### Criar Artigo
```bash
curl -X POST http://localhost:8000/api/articles \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Plantas Medicinais da Amazônia",
    "ano_publicacao": 2024,
    "autores": [{"nome": "João", "sobrenome": "Silva"}],
    "doi": "10.1234/test.001",
    "resumo": "Estudo etnobotânico"
  }'
```

### Validar Espécie
```bash
curl -X POST http://localhost:8000/api/species/validate \
  -H "Content-Type: application/json" \
  -d '{"nome_cientifico": "Areca catechu"}'
```

### Verificar Duplicata
```bash
curl -X POST http://localhost:8000/api/articles/check-duplicate \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Plantas Medicinais da Amazônia",
    "ano_publicacao": 2024,
    "autores": [{"nome": "João"}],
    "doi": "10.1234/test.001"
  }'
```

### Informações do Banco
```bash
curl http://localhost:8000/api/database/info
```

---

## ⚡ Checklist de Testes

- [ ] Backend iniciou sem erros
- [ ] Frontend acessível em localhost:5173
- [ ] Health check retorna status "healthy"
- [ ] API Key (Gemini) validada com sucesso
- [ ] PDF upload funciona
- [ ] Extração de texto do PDF funciona
- [ ] Metadados extraídos aparecem corretamente
- [ ] Duplicatas são detectadas
- [ ] Artigo pode ser salvo
- [ ] Artigo aparece no histórico
- [ ] Busca funciona na tabela
- [ ] Ordenação funciona
- [ ] Paginação funciona
- [ ] Download do banco funciona
- [ ] Info do banco mostra estatísticas corretas

---

## 🐛 Troubleshooting

### "API Key inválida"
- Verifique se a chave está correta em https://ai.google.dev
- Confirme que tem quota disponível (free tier tem limite)
- Tente validar novamente

### "Erro ao extrair PDF"
- Verifique se o PDF é válido
- Tente com um PDF diferente
- Máximo 50 MB - seu arquivo pode ser muito grande

### "Erro ao salvar artigo"
- Verifique se há espaço em disco
- Tente refresh da página
- Verifique logs do backend

### "Banco de dados não encontrado"
Solução:
```bash
python backend/database/init_db.py
```

### Frontend não conecta com Backend
Verifique em `frontend/vite.config.ts`:
```typescript
proxy: {
  '/api': {
    target: 'http://localhost:8000',
    changeOrigin: true,
  },
}
```

---

## 📊 Dados de Teste

Se quiser testar com dados reais:

### Artigos Científicos de Exemplo
- Busque em Google Scholar: "ethnobotany"
- PubMed: "medicinal plants indigenous"
- Journals grátis: PLOS ONE, Scientific Reports

### Dados Pré-carregados (Opcional)

```bash
# Script para popular banco com dados de teste
python backend/scripts/seed_test_data.py  # (futuro)
```

---

## 🎯 Próximas Features (Não MVP)

- TASK-014: Integração OpenAI ChatGPT
- TASK-015: Integração Anthropic Claude
- TASK-017: Editor manual de metadados
- TASK-018: Auto-save de rascunhos
- TASK-021: Validação melhorada de uploads
- TASK-022: Detecção de PDFs escaneados (OCR)
- TASK-028: Dashboard de configurações avançadas

---

## 📞 Suporte

Se encontrar problemas:

1. Veja os logs do backend (Terminal 1)
2. Veja o console do navegador (DevTools - F12)
3. Abra uma issue no GitHub com:
   - Descrição do problema
   - Logs relevantes
   - Sistema operacional

---

## ✅ Status MVP

**Etnopapers MVP está completo com:**
- ✅ Upload drag-and-drop de PDFs
- ✅ Extração automática de texto com pdf.js
- ✅ Integração com Google Gemini para IA
- ✅ Detecção automática de duplicatas
- ✅ Validação de espécies botânicas (GBIF/Tropicos)
- ✅ Banco de dados SQLite local
- ✅ Interface responsiva e intuitiva
- ✅ Download/backup do banco

**Aproveite!** 🎉
