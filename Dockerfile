# Multi-stage build for Etnopapers (with MongoDB)

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

# Stage 2: Backend runtime
FROM python:3.11-slim

WORKDIR /app

# Install system dependencies (minimal for MongoDB)

# Install Python dependencies
COPY backend/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy backend
COPY backend ./backend

# Copy built frontend from builder
COPY --from=frontend-builder /app/frontend/dist ./frontend/dist

# Expose port
EXPOSE 8000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import requests; requests.get('http://localhost:8000/health')" || exit 1

# Environment defaults (can be overridden at runtime)
# NOTE: MONGO_URI MUST be provided at runtime via -e MONGO_URI=...
# There is NO default - the application will fail to start without it
# This is intentional to prevent accidental use of wrong database
ENV PORT=8000
ENV LOG_LEVEL=info
ENV ENVIRONMENT=production

# Start backend with Uvicorn
CMD ["uvicorn", "backend.main:app", "--host", "0.0.0.0", "--port", "8000"]
