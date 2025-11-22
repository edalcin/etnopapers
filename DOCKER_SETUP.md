# Docker Setup for Etnopapers

## Important: MONGO_URI Environment Variable

The application **requires** the `MONGO_URI` environment variable to start. Without it, the application will fail to initialize.

### After Code Update: Rebuild Docker Image

If you have already pulled the image before, you **must rebuild it** after code updates:

```bash
# Pull latest code
git pull origin main

# Rebuild the Docker image
docker build -t etnopapers:latest .

# Stop and remove old container
docker stop etnopapers
docker rm etnopapers

# Run with new image
docker run -d \
  --name='etnopapers' \
  --net='bridge' \
  -e MONGO_URI='mongodb://useflora:yfH8WaEp9Ccf@192.168.1.10:27017/useflora?authSource=useflora' \
  -p '8007:8000/tcp' \
  'etnopapers:latest'
```

**Or on Unraid:** Use the WebUI to rebuild and restart the container with the latest changes.

### Correct Docker Run Command

```bash
docker run \
  -d \
  --name='etnopapers' \
  --net='bridge' \
  -e TZ="America/Sao_Paulo" \
  -e HOST_OS="Unraid" \
  -e HOST_HOSTNAME="Asilo" \
  -e HOST_CONTAINERNAME="etnopapers" \
  -e MONGO_URI='mongodb://useflora:yfH8WaEp9Ccf@192.168.1.10:27017/useflora?authSource=useflora' \
  -p '8007:8000/tcp' \
  'ghcr.io/edalcin/etnopapers'
```

**KEY POINTS:**
- ✅ `-e MONGO_URI='...'` (correct)
- ❌ `-e 'MONGO_URI:'='...'` (wrong - has colon)
- The MONGO_URI must include the database name in the path (e.g., `/useflora`)
- Always use `?authSource=useflora` if your MongoDB user is scoped to a specific database

### MONGO_URI Format

```
mongodb://username:password@host:port/database?authSource=database
```

Example:
```
mongodb://useflora:yfH8WaEp9Ccf@192.168.1.10:27017/useflora?authSource=useflora
```

### Environment Variables Reference

| Variable | Required | Example | Description |
|----------|----------|---------|-------------|
| `MONGO_URI` | **YES** | `mongodb://user:pass@host/db?authSource=db` | MongoDB connection string |
| `PORT` | No | `8000` | API server port (default: 8000) |
| `HOST` | No | `0.0.0.0` | Server host (default: 0.0.0.0) |
| `ENVIRONMENT` | No | `production` | Environment mode (default: development) |
| `LOG_LEVEL` | No | `info` | Logging level (default: info) |
| `TZ` | No | `America/Sao_Paulo` | Timezone |

### Testing the Connection

After starting the container, check the logs:

```bash
docker logs etnopapers
```

Look for:
- ✅ `MongoDB database initialized successfully` - Connection successful
- ❌ `MONGO_URI environment variable not set` - Missing MONGO_URI
- ❌ `Failed to connect to MongoDB` - Connection error

### Debugging Connection Issues

#### Error: `mongo:27017: Name or service not known`

This error means the application tried to connect to `mongo:27017` instead of your MONGO_URI.

**Causes:**
1. ❌ Old Docker image with hardcoded default (FIXED - rebuild image)
2. ❌ MONGO_URI environment variable not passed to container
3. ❌ MONGO_URI syntax error (missing `/database` or `?authSource`)

**Solutions:**

1. **Verify environment variable is being passed:**
   ```bash
   # Check what the container received
   docker inspect etnopapers | grep -A 20 "Env"

   # Look for MONGO_URI in the output
   # Should show: MONGO_URI=mongodb://...
   ```

2. **Check container logs:**
   ```bash
   docker logs etnopapers | grep -i mongo

   # Should see: MONGO_URI configured: YES
   # Should NOT see: mongo:27017
   ```

3. **Rebuild image (if using old version):**
   ```bash
   docker build -t etnopapers:latest .
   docker stop etnopapers
   docker rm etnopapers
   # Then run again with -e MONGO_URI=...
   ```

4. **Test MONGO_URI syntax:**
   ```bash
   # Format: mongodb://user:pass@host:port/database?authSource=database
   # Example: mongodb://useflora:yfH8WaEp9Ccf@192.168.1.10:27017/useflora?authSource=useflora
   ```

### Health Check Endpoint

Once the container is running with correct MONGO_URI:

```bash
curl http://192.168.1.10:8007/health
```

Expected response:
```json
{
  "status": "healthy",
  "version": "0.1.0",
  "environment": "production",
  "database": {
    "size_mb": 0.048,
    "collections": 1
  }
}
```

### Docker Compose Alternative

For easier management, use `docker-compose.yml`:

```yaml
version: '3.8'

services:
  etnopapers:
    image: ghcr.io/edalcin/etnopapers:latest
    container_name: etnopapers
    ports:
      - "8007:8000"
    environment:
      TZ: America/Sao_Paulo
      MONGO_URI: mongodb://useflora:yfH8WaEp9Ccf@192.168.1.10:27017/useflora?authSource=useflora
      ENVIRONMENT: production
      LOG_LEVEL: info
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

# Run with: docker-compose up -d
```

### Security Notes

⚠️ **IMPORTANT**: Never commit credentials to version control!

- Store `MONGO_URI` in Docker secrets or environment variable files outside version control
- For UNRAID, use the Unraid UI to set environment variables
- Never share your MongoDB credentials in logs or documentation
