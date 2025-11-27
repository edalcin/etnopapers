# Multi-stage build for Etnopapers v2.0 (Frontend + Backend + Ollama + MongoDB)
# This single container includes:
# - Frontend React (served via FastAPI)
# - Backend FastAPI API
# - Ollama + Qwen2.5-7B-Instruct (AI local inference)
# - MongoDB connection (external via MONGO_URI)

# Stage 1: Frontend builder
FROM node:18-alpine AS frontend-builder

WORKDIR /app/frontend

# Copy frontend dependencies
COPY frontend/package*.json ./

# Install dependencies
RUN npm install --prefer-offline --no-audit

# Copy frontend source and config files
COPY frontend/src ./src
COPY frontend/public ./public
COPY frontend/index.html ./
COPY frontend/tsconfig.json ./
COPY frontend/tsconfig.node.json ./
COPY frontend/vite.config.ts ./
COPY frontend/.eslintrc.json ./
COPY frontend/.prettierrc.json ./

# Build frontend
RUN npm run build

# Stage 2: Backend runtime with Ollama
FROM python:3.11-slim

WORKDIR /app

# Install system dependencies:
# - curl: for Ollama healthcheck and model pull
# - wget: alternative for downloading
# - git: required by some Python packages
# - build-essential: for compiling Python C extensions (levenshtein, etc.)
# - ca-certificates: for HTTPS requests
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    wget \
    git \
    build-essential \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Install Ollama
# Ollama will run as a service in the background started by entrypoint.sh
RUN curl -fsSL https://ollama.ai/install.sh | sh || echo "Ollama install completed"

# Install Python dependencies
COPY backend/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy backend
COPY backend ./backend

# Copy built frontend from builder
COPY --from=frontend-builder /app/frontend/dist ./frontend/dist

# Copy entrypoint script
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Expose ports
EXPOSE 8000
EXPOSE 11434

# Health check for frontend, backend, and Ollama
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD bash -c 'curl -f http://localhost:8000/health && curl -f http://localhost:11434/api/tags' || exit 1

# Environment defaults (can be overridden at runtime)
# NOTE: MONGO_URI MUST be provided at runtime via -e MONGO_URI=...
# There is NO default - the application will fail to start without it
# This is intentional to prevent accidental use of wrong database
ENV PORT=8000
ENV LOG_LEVEL=info
ENV ENVIRONMENT=production
ENV OLLAMA_URL=http://localhost:11434
ENV OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M

# Start container with orchestration script
ENTRYPOINT ["/entrypoint.sh"]
