# 🚀 Etnopapers v2.0 - Deployment Guide

**Comprehensive guide for deploying Etnopapers on UNRAID with GPU acceleration**

---

## Table of Contents

1. [Hardware Requirements](#hardware-requirements)
2. [UNRAID GPU Setup](#unraid-gpu-setup)
3. [Docker Deployment](#docker-deployment)
4. [Verification & Troubleshooting](#verification--troubleshooting)
5. [Performance Optimization](#performance-optimization)

---

## Hardware Requirements

### Minimum Specification

| Component | Requirement | Notes |
|-----------|-------------|-------|
| **GPU** | NVIDIA RTX 3060 or better | 6-12 GB VRAM required |
| **RAM** | 16 GB | Minimum 8 GB, 32 GB recommended |
| **Storage** | 50 GB free | ~8.5 GB Docker image + databases |
| **CPU** | Modern (2015+) | Dual-core minimum, quad-core recommended |
| **Network** | Gigabit recommended | For model download (~4.8 GB) |

### Supported NVIDIA GPUs

✅ **Recommended (Tested)**
- NVIDIA RTX 3060 (12 GB) - Excellent performance
- NVIDIA RTX 3070 (8 GB) - Very good
- NVIDIA RTX 3080 (10 GB) - Excellent
- NVIDIA RTX 4060 Ti (8 GB) - Good

⚠️ **Limited Support (Tight VRAM)**
- NVIDIA RTX 2080 (8 GB) - Works, slower
- NVIDIA RTX 2070 (8 GB) - Works with optimization
- NVIDIA GTX 1080 Ti (11 GB) - Works, requires tuning

❌ **Not Supported**
- GPUs with < 6 GB VRAM (model won't fit)
- Intel Arc GPUs (not supported by Ollama)
- AMD GPUs (use CPU mode - very slow)

### Check Your GPU

```bash
# On UNRAID terminal:
nvidia-smi

# Expected output:
# +----------+----------+----------+
# | NVIDIA GPU (RTX 3060) | 12 GB VRAM
# +----------+----------+----------+
```

---

## UNRAID GPU Setup

### Step 1: Install NVIDIA Driver Plugin

1. Open UNRAID WebUI → **Apps**
2. Search for **"nvidia"**
3. Click **"Nvidia-Driver"** (Community Applications)
4. Click **"Install"**
5. **Reboot UNRAID server** when prompted

### Step 2: Verify GPU Is Available

After reboot, open UNRAID terminal:

```bash
# Check GPU visibility
nvidia-smi

# Expected output (RTX 3060):
# +-------------+----------+
# | NVIDIA-SMI 535.00 | Driver Version: 535.00 |
# +-------------+----------+
# | GPU Name | Compute Capability |
# |  0 NVIDIA RTX 3060 | 8.6 |
# | Memory: 12GB | Free: 11.8GB |
# +-------------+----------+
```

### Step 3: Install Docker GPU Support

The Nvidia Driver Plugin should automatically handle this, but verify:

```bash
# Check Docker GPU runtime is available
docker run --rm --gpus all nvidia/cuda:12.0.1-base-ubuntu22.04 nvidia-smi
```

### Step 4: Configure UNRAID Container Settings (GUI Method)

When creating the Etnopapers container via UNRAID GUI:

1. **Extra Parameters**: Add exactly:
   ```
   --runtime=nvidia --gpus all -e NVIDIA_VISIBLE_DEVICES=all
   ```

2. **Environment Variables**: Set in container settings:
   ```
   MONGO_URI=mongodb://mongo:27017/etnopapers
   OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M
   OLLAMA_URL=http://localhost:11434
   NVIDIA_VISIBLE_DEVICES=all
   CUDA_VISIBLE_DEVICES=0
   ```

3. Click **Save** and start container

### Step 5: Test GPU Access Inside Container

```bash
# SSH into UNRAID, then:
docker exec etnopapers nvidia-smi

# Should show GPU with ample free memory (> 6GB for safe operation)
```

---

## Docker Deployment

### Option 1: Docker Compose (Recommended)

```bash
# Clone repository (or cd to existing clone)
git clone https://github.com/etnopapers/etnopapers.git
cd etnopapers

# Edit docker-compose.yml for GPU support (uncomment GPU section)
# Then deploy:
docker-compose up -d

# Monitor startup (wait for Ollama model load - 5-20 minutes)
docker-compose logs -f etnopapers
```

### Option 2: Manual Docker Command (UNRAID CLI)

```bash
docker run -d \
  --name etnopapers \
  --runtime=nvidia \
  --gpus all \
  -e NVIDIA_VISIBLE_DEVICES=all \
  -e MONGO_URI=mongodb://localhost:27017/etnopapers \
  -e OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M \
  -e OLLAMA_URL=http://localhost:11434 \
  -p 8000:8000 \
  -p 11434:11434 \
  -v /mnt/user/data/etnopapers:/app/data \
  etnopapers:latest
```

### Option 3: UNRAID Community Applications

1. **Apps** → Search "Etnopapers"
2. Click **Install**
3. Configure GPU and MongoDB settings
4. Click **Install** again
5. Container starts automatically

---

## Verification & Troubleshooting

### Health Check - Complete System

```bash
# 1. Check container is running
docker ps | grep etnopapers
# Expected: etnopapers container listed with "Up" status

# 2. Check GPU is accessible
docker exec etnopapers nvidia-smi
# Expected: GPU memory available (> 6GB free)

# 3. Check Ollama is ready
docker exec etnopapers curl -s http://localhost:11434/api/tags | jq .
# Expected: List of available models including qwen2.5

# 4. Check FastAPI backend
curl -s http://localhost:8000/health | jq .
# Expected: {"status": "healthy", ...}

# 5. Check Frontend
curl -s http://localhost:8000 | head -20
# Expected: HTML content (React app)

# 6. Check MongoDB connection
curl -s http://localhost:8000/api/articles | jq .
# Expected: JSON response (even if empty array)
```

### Common Issues

#### ❌ GPU Not Visible in Container

**Problem**: `docker exec etnopapers nvidia-smi` fails

**Solutions**:
1. Verify UNRAID GPU driver: `nvidia-smi` on host
2. Restart nvidia driver: Settings → Docker → Nvidia-Driver → Restart
3. Check container runtime: `docker run --rm --runtime=nvidia ubuntu nvidia-smi`
4. Rebuild container without GPU cache:
   ```bash
   docker-compose build --no-cache
   ```

#### ❌ Ollama Won't Start

**Problem**: Container logs show "Ollama startup failed"

**Solutions**:
1. Check GPU memory: `docker exec etnopapers nvidia-smi`
   - Model needs ≥ 6GB VRAM free
   - If < 6GB, stop other containers
2. Check Ollama manually:
   ```bash
   docker exec etnopapers /bin/ollama serve &
   sleep 10
   curl http://localhost:11434/api/tags
   ```
3. Pre-load model (if not auto-loaded):
   ```bash
   docker exec etnopapers ollama pull qwen2.5:7b-instruct-q4_K_M
   ```

#### ❌ Slow Inference (> 5 seconds per article)

**Problem**: AI inference takes too long

**Solutions**:
1. Verify GPU is being used:
   ```bash
   # Run sample request and monitor GPU
   watch -n 1 'docker exec etnopapers nvidia-smi'
   # GPU utilization should jump to 50-90%
   ```
2. Check GPU memory allocation:
   ```bash
   docker exec etnopapers nvidia-smi -q
   # Look for "Memory Usage" - should show model loaded
   ```
3. If slow with GPU available:
   - GPU may be too small for model
   - Try RTX 3070+ or reduce batch size
4. Enable GPU persistence (reduces load time):
   ```bash
   sudo nvidia-smi -pm 1
   ```

#### ❌ MongoDB Connection Fails

**Problem**: "MONGO_URI environment variable not set"

**Solutions**:
1. Check environment variables:
   ```bash
   docker exec etnopapers env | grep MONGO
   # Should show: MONGO_URI=mongodb://mongo:27017/etnopapers
   ```
2. Verify MongoDB is running:
   ```bash
   docker ps | grep mongo
   # If missing, ensure mongo service is running
   docker-compose up -d mongo
   ```
3. Test MongoDB connection:
   ```bash
   docker exec etnopapers python -c "
   from pymongo import MongoClient
   client = MongoClient('mongodb://mongo:27017')
   client.admin.command('ping')
   print('Connected!')
   "
   ```

#### ❌ Port Already in Use

**Problem**: "bind: address already in use"

**Solutions**:
1. Find what's using port 8000:
   ```bash
   sudo lsof -i :8000
   ```
2. Stop conflicting container:
   ```bash
   docker stop <container_name>
   ```
3. Change ports in docker-compose.yml:
   ```yaml
   ports:
     - "8001:8000"  # Changed from 8000:8000
     - "11435:11434" # Changed from 11434:11434
   ```

---

## Performance Optimization

### GPU Memory Tuning

**Default Model** (Qwen2.5-7B-Instruct-Q4):
- Memory requirement: 4.8 GB
- Peak usage: 6-7 GB (with context)
- Safe margin: 8 GB GPU minimum

**If VRAM Constrained** (< 6 GB GPU):

1. Use smaller model variant:
   ```bash
   # Edit Dockerfile or env variable:
   OLLAMA_MODEL=qwen2.5:3.5b-instruct-q4_K_M  # 2.5 GB

   # Or smallest variant:
   OLLAMA_MODEL=qwen2.5:1.5b-instruct-q4_K_M  # 1.2 GB
   ```

2. Enable memory mapping:
   ```bash
   docker run ... -e OLLAMA_NUM_GC=1 ...
   ```

### Inference Speed Tuning

**For Faster Inference**:

```bash
# Set via environment variables in docker-compose.yml
OLLAMA_NUM_THREADS=8        # Use 8 CPU threads (adjust to your CPU cores)
OLLAMA_NUM_GC=1            # Faster garbage collection
CUDA_VISIBLE_DEVICES=0     # Use GPU 0 (if multi-GPU setup)
```

**Temperature Settings** (for deterministic output):

In extraction service, use:
```python
temperature=0.1  # Very deterministic (default)
temperature=0.3  # Slightly more creative
# Higher = more varied, more hallucinations
```

### Batch Processing

For multiple PDFs, Etnopapers processes one at a time. For **bulk processing**:

```bash
# Manual batch via curl:
for pdf in *.pdf; do
  curl -X POST http://localhost:8000/api/extract/metadata \
    -F "pdf_file=@$pdf" \
    --max-time 180  # 3 minute timeout per file
done
```

---

## Monitoring

### View Live Logs

```bash
# Follow container logs in real-time
docker-compose logs -f etnopapers

# View only extraction endpoint logs:
docker-compose logs etnopapers | grep extract
```

### Monitor GPU Usage During Processing

```bash
# Real-time GPU monitoring while processing:
watch -n 1 'docker exec etnopapers nvidia-smi --query-gpu=index,name,utilization.gpu,utilization.memory,memory.used,memory.free --format=csv,noheader'

# Expected during inference:
# 0, RTX 3060, 85%, 45%, 5800MB, 6200MB
```

### Check Model is Loaded

```bash
# Verify model in GPU memory:
docker exec etnopapers ollama list
# Expected output:
# NAME                             ID              SIZE    MODIFIED
# qwen2.5:7b-instruct-q4_K_M      abc123...       4.8GB   1 hour ago
```

---

## Production Checklist

Before deploying to production, verify:

- [ ] GPU visible: `nvidia-smi` shows available VRAM
- [ ] NVIDIA driver: v535+ recommended
- [ ] Docker GPU runtime: `docker run --gpus all nvidia/cuda:12.0 nvidia-smi` works
- [ ] Container starts: `docker-compose up -d` completes
- [ ] Health check passes: `curl http://localhost:8000/health`
- [ ] Ollama ready: `curl http://localhost:11434/api/tags`
- [ ] Sample extraction works: Upload test PDF
- [ ] MongoDB backup: Volume mounted for persistence
- [ ] Log rotation: Configured for long-running server
- [ ] Memory pressure: GPU/RAM utilization < 80%

---

## Support & Troubleshooting

For additional help:
- Check logs: `docker-compose logs etnopapers`
- Test GPU: `nvidia-smi` and `docker exec etnopapers nvidia-smi`
- Forum: UNRAID Community Applications → Etnopapers discussion
- GitHub Issues: https://github.com/etnopapers/etnopapers/issues

---

**Last Updated**: 2025-11-27
**Version**: v2.0
**Status**: Production-ready with UNRAID GPU support
