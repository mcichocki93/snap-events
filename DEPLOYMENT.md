# Wedding Photos - Deployment Guide

## Environment Variables Configuration

This application uses environment variables for sensitive configuration. **Never commit secrets to the repository.**

### Required Environment Variables

#### MongoDB Configuration
```bash
MONGODB_CONNECTION_STRING=mongodb+srv://username:password@cluster.mongodb.net/
MONGODB_DATABASE_NAME=WeddingPhotos
MONGODB_CLIENTS_COLLECTION=Clients
```

#### Google Cloud Configuration
```bash
GOOGLE_SERVICE_ACCOUNT_KEY_PATH=/app/secrets/google-credentials.json
GOOGLE_PROJECT_ID=your-project-id
```

#### CORS Configuration
```bash
ALLOWED_ORIGINS=https://yourweddingphotos.pl,https://www.yourweddingphotos.pl
```

### Optional Environment Variables

#### Redis Cache (Optional)
```bash
REDIS_ENABLED=false
REDIS_CONNECTION_STRING=localhost:6379
REDIS_DEFAULT_EXPIRATION_MINUTES=30
REDIS_GALLERY_CACHE_EXPIRATION_MINUTES=15
```

#### Sentry Error Tracking (Optional)
```bash
SENTRY_DSN=https://your-sentry-dsn@sentry.io/project-id
SENTRY_ENVIRONMENT=production
SENTRY_TRACES_SAMPLE_RATE=0.1
```

#### Hangfire Background Jobs
```bash
HANGFIRE_ENABLED=true
HANGFIRE_DASHBOARD_ENABLED=false
HANGFIRE_DASHBOARD_PATH=/hangfire
```

#### ASP.NET Core Settings
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
```

## Local Development Setup

1. **Copy environment template:**
   ```bash
   cp .env.example .env
   ```

2. **Edit `.env` with your local credentials**

3. **Place Google credentials file:**
   ```bash
   mkdir -p secrets
   # Copy your google-credentials.json to secrets/
   ```

4. **Run the application:**
   ```bash
   dotnet run --project src/WeddingPhotos.Api
   ```

## Docker Deployment

1. **Build the Docker image:**
   ```bash
   docker build -t wedding-photos-api .
   ```

2. **Run with environment variables:**
   ```bash
   docker run -d \
     -p 5000:5000 \
     -e MONGODB_CONNECTION_STRING="your-connection-string" \
     -e MONGODB_DATABASE_NAME="WeddingPhotos" \
     -e GOOGLE_SERVICE_ACCOUNT_KEY_PATH="/app/secrets/google-credentials.json" \
     -e GOOGLE_PROJECT_ID="your-project-id" \
     -e ALLOWED_ORIGINS="https://yoursite.com" \
     -e SENTRY_DSN="your-sentry-dsn" \
     -v /path/to/secrets:/app/secrets:ro \
     wedding-photos-api
   ```

3. **Or use Docker Compose with `.env` file:**
   ```bash
   docker-compose up -d
   ```

## Kubernetes Deployment

1. **Create Kubernetes secrets:**
   ```bash
   kubectl create secret generic wedding-photos-secrets \
     --from-literal=mongodb-connection-string="your-connection-string" \
     --from-literal=google-project-id="your-project-id" \
     --from-file=google-credentials=/path/to/google-credentials.json
   ```

2. **Apply deployment:**
   ```bash
   kubectl apply -f k8s/deployment.yaml
   ```

## Health Check Endpoints

- `/health` - Simple health check (for load balancers)
- `/health/detailed` - Detailed diagnostics (MongoDB, Redis status)
- `/health/ready` - Readiness probe (API + MongoDB)
- `/health/live` - Liveness probe (API only)

Example Kubernetes probes:
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 5000
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 5000
  initialDelaySeconds: 10
  periodSeconds: 5
```

## Security Best Practices

### 1. Secrets Management

**DO:**
- ✅ Use environment variables for all secrets
- ✅ Use Kubernetes Secrets or Azure Key Vault in production
- ✅ Keep `secrets/` directory in `.gitignore`
- ✅ Use `.env.example` as template (without real values)

**DON'T:**
- ❌ Never commit `appsettings.json` with real credentials
- ❌ Never commit `.env` files with real values
- ❌ Never hardcode secrets in source code

### 2. CORS Configuration

Production CORS should only allow your frontend domains:
```bash
ALLOWED_ORIGINS=https://yourweddingphotos.pl,https://www.yourweddingphotos.pl
```

### 3. MongoDB Connection String

**Format:**
```
mongodb+srv://username:password@cluster.mongodb.net/?retryWrites=true&w=majority
```

**Security:**
- Use strong passwords (20+ characters, mixed case, symbols)
- Enable MongoDB Network Access IP whitelist
- Use MongoDB Atlas for automatic backups

### 4. Google Cloud Service Account

**Permissions needed:**
- `roles/storage.objectViewer` (read files)
- `roles/storage.objectCreator` (upload files)
- `roles/storage.objectAdmin` (delete files)

**Mount as read-only in Docker:**
```bash
-v /path/to/secrets:/app/secrets:ro
```

### 5. Rate Limiting

Default limits (configured in code):
- General API: 60 requests/minute
- Photo upload: 100 requests/hour
- Gallery view: 30 requests/minute
- Photo proxy: 100 requests/minute

### 6. File Upload Limits

- Max file size: 100 MB (configurable via `ApplicationConstants.MaxFileSizeBytes`)
- Max concurrent uploads: 100 connections
- Allowed file types: `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.webp`, `.heic`, `.tiff`

## Monitoring

### Sentry Integration

Set `SENTRY_DSN` to enable error tracking and performance monitoring:
- Errors are automatically captured
- Performance traces sample at 10% in production (configurable)
- Health check endpoints are filtered out to reduce noise

### Logging

Logs are written to:
- Console (structured JSON in production)
- File: `logs/wedding-photos-YYYY-MM-DD.log` (30 days retention, 10MB max per file)
- Sentry (if configured)

## Troubleshooting

### MongoDB Connection Issues

1. **Check connection string format:**
   ```bash
   echo $MONGODB_CONNECTION_STRING
   ```

2. **Verify network access:**
   - Add your server IP to MongoDB Atlas Network Access
   - Check firewall rules

3. **Test connection:**
   ```bash
   curl http://localhost:5000/health/detailed
   ```

### Google Drive Upload Issues

1. **Verify service account credentials:**
   ```bash
   ls -la $GOOGLE_SERVICE_ACCOUNT_KEY_PATH
   cat $GOOGLE_SERVICE_ACCOUNT_KEY_PATH | jq
   ```

2. **Check permissions:**
   - Service account must have access to the folder
   - Folder ID must be correct in client configuration

### Redis Connection Issues

If Redis is unavailable, the app automatically falls back to memory cache. Check logs:
```bash
docker logs wedding-photos-api | grep -i redis
```

## Production Checklist

Before deploying to production:

- [ ] All environment variables configured
- [ ] MongoDB connection string secured
- [ ] Google service account key deployed securely
- [ ] CORS origins set to production domains only
- [ ] Sentry DSN configured for error tracking
- [ ] Health checks verified (`/health/detailed`)
- [ ] Rate limiting tested
- [ ] File upload limits verified
- [ ] Logs configured and monitored
- [ ] Backups enabled (MongoDB Atlas)
- [ ] SSL/TLS certificate configured (reverse proxy)
- [ ] Hangfire dashboard disabled or secured

## Support

For issues or questions, check:
1. Application logs: `logs/wedding-photos-*.log`
2. Health check status: `/health/detailed`
3. Sentry dashboard (if configured)
