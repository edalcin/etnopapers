#!/bin/bash
set -e

# Etnopapers Docker Entrypoint Script
# Orchestrates:
# 1. Ollama background service
# 2. Qwen2.5 model loading
# 3. FastAPI startup

OLLAMA_URL="${OLLAMA_URL:-http://localhost:11434}"
OLLAMA_MODEL="${OLLAMA_MODEL:-qwen2.5:7b-instruct-q4_K_M}"
OLLAMA_TIMEOUT=60

echo "==================================="
echo "Etnopapers v2.0 - Startup Script"
echo "==================================="

# Function to wait for Ollama to be ready
wait_for_ollama() {
    local counter=0
    local max_attempts=$1

    echo "Waiting for Ollama to be ready (max ${max_attempts}s)..."

    while [ $counter -lt $max_attempts ]; do
        if curl -s "${OLLAMA_URL}/api/tags" > /dev/null 2>&1; then
            echo "✓ Ollama is ready"
            return 0
        fi

        counter=$((counter + 1))
        echo "  [$counter/$max_attempts] Waiting for Ollama..."
        sleep 1
    done

    echo "✗ Ollama failed to start within ${max_attempts} seconds"
    return 1
}

# Function to check if model is available
check_model_available() {
    local model=$1
    echo "Checking if model $model is available..."

    if curl -s "${OLLAMA_URL}/api/tags" | grep -q "$model"; then
        echo "✓ Model $model is available"
        return 0
    else
        echo "⚠ Model $model not found, will attempt to pull..."
        return 1
    fi
}

# Start Ollama in background
echo ""
echo "Starting Ollama service..."
/bin/ollama serve &
OLLAMA_PID=$!
echo "Ollama PID: $OLLAMA_PID"

# Wait for Ollama to be ready
if wait_for_ollama $OLLAMA_TIMEOUT; then
    echo ""
    echo "Ollama is running at $OLLAMA_URL"

    # Check if model is available
    if ! check_model_available "$OLLAMA_MODEL"; then
        echo ""
        echo "Pulling model $OLLAMA_MODEL..."
        curl -s -X POST "${OLLAMA_URL}/api/pull" \
            -H "Content-Type: application/json" \
            -d "{\"name\": \"$OLLAMA_MODEL\"}" \
            --max-time 600 || echo "Warning: Model pull may have timed out or failed"
    fi

    # Verify model is ready
    if check_model_available "$OLLAMA_MODEL"; then
        echo ""
        echo "==================================="
        echo "✓ Ollama and model ready!"
        echo "==================================="

        # Start FastAPI backend
        echo ""
        echo "Starting FastAPI backend..."
        exec uvicorn backend.main:app --host 0.0.0.0 --port 8000
    else
        echo "✗ Model $OLLAMA_MODEL is still not available"
        exit 1
    fi
else
    echo "✗ Ollama startup failed"
    exit 1
fi
