# Etnopapers Quick Start Guide

**Version**: 2.0 Standalone Desktop Application
**Time to Setup**: 15 minutes

---

## What is Etnopapers?

Etnopapers automatically extracts ethnobotanical metadata from PDF articles:
- Plant names (vernacular + scientific)
- Traditional uses (medicinal, food, rituals)
- Indigenous communities and locations
- Biomes and ecosystems

All processing happens **locally and privately** on your computer.

---

## 30-Second Installation

### Windows
1. Download: https://github.com/edalcin/etnopapers/releases → `etnopapers-windows-vX.Y.Z.exe`
2. Install **Ollama**: https://ollama.com/download
3. Run `etnopapers.exe`
4. Enter MongoDB connection (use default for local)
5. Done! ✅

### macOS
```bash
# Download from releases
# Or using Homebrew (if available)
brew install etnopapers

# Install Ollama
# Download from https://ollama.com/download

# Run
./Etnopapers.app
```

### Linux
```bash
# Download from releases
chmod +x etnopapers-linux-vX.Y.Z
./etnopapers-linux-vX.Y.Z
```

---

## System Requirements

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| RAM | 4 GB | 8 GB |
| Disk | 5 GB | 10 GB |
| CPU | Dual-core | Quad-core |
| Internet | Download only | Offline capable* |

*After initial setup, Etnopapers works completely offline

---

## Step 1: Install Prerequisites

### Install Ollama (Required for AI)

1. Visit: https://ollama.com/download
2. Download your OS version
3. Run installer
4. Restart computer
5. Open Ollama (check system tray)

**Verify Ollama is running**:
```bash
curl http://localhost:11434/api/tags
# Should return list of models
```

### Install MongoDB (Choose One)

**Option A: Local MongoDB (Easiest)**
1. Download: https://www.mongodb.com/try/download/community
2. Run installer (Windows/macOS)
3. Or: `brew install mongodb-community` (macOS)
4. Default connection: `mongodb://localhost:27017/etnopapers`

**Option B: MongoDB Atlas (Cloud, Free 512MB)**
1. Create account: https://www.mongodb.com/cloud/atlas
2. Create cluster (free tier)
3. Get connection string from Atlas dashboard
4. Use that string in Etnopapers setup

---

## Step 2: Run Etnopapers

### First Run

1. **Ensure Ollama is running** (visible in system tray)
2. Double-click `etnopapers` application
3. You'll see a configuration dialog:

```
MongoDB Configuration

Enter connection string:
[mongodb://localhost:27017/etnopapers]

[Test Connection] [Save]
```

4. Click "Test Connection"
5. If green checkmark, click "Save"
6. App launches automatically

---

## Step 3: Upload Your First PDF

### Simple Workflow

1. **Find a scientific article** (any PDF with ethnobotany content)
2. **Drag and drop** onto the gray box in Etnopapers, or click to select
3. **Wait** for "Processing with Ollama..." (usually 2-5 minutes)
4. **Review** extracted metadata
5. **Correct** any errors (scientific names, locations)
6. **Save** to database

### What Gets Extracted?

```
✓ Article title & authors
✓ Publication year & journal
✓ Plant species (vernacular + scientific names)
✓ Types of use (medicinal, food, etc.)
✓ Indigenous communities
✓ Geographic locations (country, state, city)
✓ Biomes (Mata Atlântica, Cerrado, etc.)
✓ Research methodology
```

---

## Step 4: Manage Your Collection

### Search Articles
- Type in search box to find by title, author, species
- Results appear in real-time

### Filter by Year/Country
- Click column headers to sort
- Use dropdown filters if available

### Delete Articles
- Find article in table
- Click "Delete"
- Confirm deletion

### Download Backup
1. Click "Configurações" (Settings)
2. Select "Fazer Backup" (Make Backup)
3. Click "Fazer Download do Backup"
4. Save ZIP file somewhere safe

---

## Common Tasks

### Task: Bulk Upload Multiple PDFs

Currently Etnopapers processes one PDF at a time:

1. Upload PDF #1 → Review → Save
2. Upload PDF #2 → Review → Save
3. Repeat...

**Future**: Batch processing queue (planned for v2.1)

### Task: Export Data

**Option A**: Download backup ZIP
- Contains all articles as JSON
- Can be imported elsewhere

**Option B**: Convert to CSV (manual)
1. Open MongoDB compass
2. Export collection as CSV
3. Open in Excel/Google Sheets

**Option C**: API access
```bash
curl http://localhost:8000/api/articles?limit=1000 > articles.json
```

### Task: Team Collaboration

**Option 1**: Use MongoDB Atlas
- Set `MONGO_URI` to Atlas connection
- Multiple users access same database
- Cloud backup included

**Option 2**: File sharing
- Export backup ZIP
- Share with team
- Someone imports to their MongoDB

---

## Troubleshooting

### "Ollama Unavailable"

```
✓ Ollama installed? https://ollama.com/download
✓ Ollama running? (check system tray)
✓ Try: ollama serve
✓ Restart Ollama application
```

### "MongoDB Connection Error"

```
✓ MongoDB running locally?
  mongosh (should connect)
✓ MongoDB Atlas connection string correct?
✓ Network/firewall blocking?
```

### "PDF Processing Very Slow"

```
✓ Large file (>50 MB)?
  Split into smaller parts
✓ First run?
  Ollama may be downloading model (10 min+)
✓ Low RAM?
  Close other applications
```

### "Invalid or Scanned PDF"

The PDF is an image scan, not text. Solution:
1. Use online OCR tool (Free: https://www.ilovepdf.com/ocr)
2. Upload OCR'd PDF to Etnopapers
3. Or manually enter data

---

## Tips for Best Results

### 1. Verify Scientific Names
- Check against GBIF: https://www.gbif.org/
- Use standardized Latin names
- Include botanical family

### 2. Standardize Locations
- Countries: Brazil, Peru, Colombia
- States: Use official abbreviations (SP, RJ, BA for Brazil)
- Cities: Full official name

### 3. Consistent Data
- Plant uses: medicinal, alimentary, ritual, cosmetic
- Biomes: Use standard names (Mata Atlântica, Cerrado, Amazon)
- Research methods: Interviews, observation, participatory research

### 4. Regular Backups
- Download backup weekly if actively adding articles
- Store in cloud storage (Google Drive, OneDrive, Dropbox)
- Keep password-protected

---

## Next Steps

### For Researchers
1. ✅ Read User Guide: `/docs/GUIA_USUARIO.md` (Portuguese)
2. ✅ Explore advanced filtering options
3. ✅ Set up team collaboration with MongoDB Atlas

### For Developers
1. ✅ Read Developer Guide: `/docs/DEVELOPER_GUIDE.md`
2. ✅ Review API docs: `/docs/API_DOCUMENTATION.md`
3. ✅ Access API: http://localhost:8000/docs
4. ✅ GitHub: https://github.com/edalcin/etnopapers

### For DevOps/Deployment
1. ✅ Build executables: `./build-windows.bat` (or macOS/Linux)
2. ✅ GitHub Actions handles automated builds
3. ✅ Releases available at: https://github.com/edalcin/etnopapers/releases

---

## System Status Indicators

### Ollama Status (Header)

| Indicator | Meaning | Action |
|-----------|---------|--------|
| 🟢 Green | Connected | Ready to process PDFs |
| 🟡 Yellow | Checking | Wait a moment |
| 🔴 Red | Unavailable | Open Ollama application |

### Processing Status

| Message | Meaning |
|---------|---------|
| "Processando PDF com Ollama..." | Extracting metadata (2-5 min) |
| "Aguardando verificação Ollama" | Ollama starting up |
| "Ollama indisponível" | Start Ollama from tray |

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+S (Windows) or Cmd+S (Mac) | Save article |
| Escape | Cancel / Close dialog |
| Enter | Confirm action |

---

## File Locations

### Configuration
- Windows: `C:\Users\[USERNAME]\.etnopapers\.env`
- macOS: `/Users/[USERNAME]/.etnopapers/.env`
- Linux: `/home/[USERNAME]/.etnopapers/.env`

### Database
- **Local**: `C:\Program Files\MongoDB\Server\[VERSION]\data`
- **Atlas**: Cloud-hosted

### Backups
- Default: Your computer's Downloads folder
- Recommended: Store in cloud (Google Drive, Dropbox)

---

## Performance Benchmarks

| Operation | Time | Notes |
|-----------|------|-------|
| PDF upload | <1 sec | File transfer |
| Metadata extraction | 2-5 min | Depends on PDF length, Ollama model |
| Article save | <1 sec | Database write |
| Search | <100 ms | Full-text search |
| Backup creation | 1-30 sec | Depends on article count |

---

## What's Included

✅ **Etnopapers Application** (150 MB)
- React 18 frontend
- FastAPI backend
- Ollama integration
- MongoDB driver

✅ **Local AI** (Ollama)
- Qwen 2.5 7B model
- 100% private processing
- GPU acceleration (if available)

✅ **Documentation**
- User guide (Portuguese)
- API documentation
- Developer guide
- This quickstart

---

## Getting Help

### Issues
- Report bugs: https://github.com/edalcin/etnopapers/issues

### Questions
- Discussions: https://github.com/edalcin/etnopapers/discussions

### Documentation
- User Guide: `docs/GUIA_USUARIO.md`
- API Docs: `docs/API_DOCUMENTATION.md`
- Developer: `docs/DEVELOPER_GUIDE.md`

---

## One-Minute Test

Try this now:

1. **Open Etnopapers** (if not already open)
2. **Find any PDF** on your computer
3. **Drag it** to the upload area
4. **Review** extracted metadata
5. **Change one field** (country, plant name)
6. **Click Save**
7. **Search** for what you just saved

That's the core workflow! 🎉

---

## Version Info

- **Version**: 2.0
- **Release Date**: 2024-01-15
- **Status**: Stable
- **License**: Open Source (check repository)

---

**Need help?** Check `/docs/GUIA_USUARIO.md` or visit the GitHub discussions!

Happy researching! 🌿📚
