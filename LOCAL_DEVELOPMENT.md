# Local Development Setup

## Prerequisites

- .NET 10.0 SDK
- MongoDB (local or Atlas)
- Google Cloud service account credentials
- (Optional) Redis for caching

## Quick Start

### 1. Set Environment Variables

Create a `.env` file in the project root (already in `.gitignore`):

```bash
# MongoDB Configuration
MONGODB_CONNECTION_STRING=mongodb+srv://your-username:your-password@cluster.mongodb.net/
MONGODB_DATABASE_NAME=WeddingPhotos
MONGODB_CLIENTS_COLLECTION=Clients

# Google Cloud Configuration
GOOGLE_SERVICE_ACCOUNT_KEY_PATH=C:/Users/cichy/Documents/Programowanie/WeddingPhotos/secrets/google-credentials.json
GOOGLE_PROJECT_ID=weddingphotos-441512

# CORS Configuration (for local development)
ALLOWED_ORIGINS=http://localhost:5173,http://localhost:3000

# Redis Configuration (optional)
REDIS_ENABLED=false
REDIS_CONNECTION_STRING=localhost:6379

# Sentry Configuration (optional)
SENTRY_DSN=

# Hangfire
HANGFIRE_ENABLED=true
HANGFIRE_DASHBOARD_ENABLED=false
```

### 2. Load Environment Variables

**Windows PowerShell:**
```powershell
# Load .env file
Get-Content .env | ForEach-Object {
    if ($_ -match '^\s*([^#][^=]+)=(.+)$') {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        [Environment]::SetEnvironmentVariable($name, $value, 'Process')
    }
}
```

**Windows CMD:**
```cmd
@echo off
for /f "usebackq tokens=1,* delims==" %%a in (".env") do (
    set "%%a=%%b"
)
```

**Linux/macOS:**
```bash
export $(grep -v '^#' .env | xargs)
```

**Or use dotenv CLI tool:**
```bash
# Install
npm install -g dotenv-cli

# Run with env vars
dotenv -e .env -- dotnet run --project src/WeddingPhotos.Api
```

### 3. Place Google Credentials

```bash
mkdir secrets
# Copy your google-credentials.json to secrets/
```

### 4. Run the Application

```bash
dotnet run --project src/WeddingPhotos.Api
```

Or with watch mode:
```bash
dotnet watch run --project src/WeddingPhotos.Api
```

### 5. Access the Application

- API: http://localhost:5000
- Health check: http://localhost:5000/health/detailed
- Hangfire dashboard (if enabled): http://localhost:5000/hangfire

## Using appsettings.Development.json (Alternative)

If you prefer not to use environment variables locally, create `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:3000"
  ],
  "MongoDbSettings": {
    "ConnectionString": "your-connection-string-here",
    "DatabaseName": "WeddingPhotos",
    "ClientsCollectionName": "Clients"
  },
  "GoogleCloud": {
    "ServiceAccountKeyPath": "C:/Users/cichy/Documents/Programowanie/WeddingPhotos/secrets/google-credentials.json",
    "ProjectId": "weddingphotos-441512"
  },
  "RedisCache": {
    "Enabled": false,
    "ConnectionString": "localhost:6379",
    "DefaultExpirationMinutes": 30,
    "GalleryCacheExpirationMinutes": 15
  },
  "Hangfire": {
    "Enabled": true,
    "DashboardEnabled": true,
    "DashboardPath": "/hangfire"
  },
  "Sentry": {
    "Dsn": "",
    "Environment": "Development"
  }
}
```

**⚠️ IMPORTANT:** `appsettings.Development.json` is in `.gitignore` - never commit it with real credentials!

## Running Tests

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter "FullyQualifiedName~GoogleStorageServiceTests"

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Debugging in Visual Studio Code

Create `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/WeddingPhotos.Api/bin/Debug/net10.0/WeddingPhotos.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/WeddingPhotos.Api",
      "stopAtEntry": false,
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "MONGODB_CONNECTION_STRING": "your-connection-string",
        "GOOGLE_SERVICE_ACCOUNT_KEY_PATH": "C:/path/to/secrets/google-credentials.json",
        "GOOGLE_PROJECT_ID": "your-project-id",
        "ALLOWED_ORIGINS": "http://localhost:5173,http://localhost:3000"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    }
  ]
}
```

## Troubleshooting

### Error: "MongoDB connection string is not configured"

**Solution:** Set `MONGODB_CONNECTION_STRING` environment variable or add it to `appsettings.Development.json`

### Error: "Service account key file not found"

**Solution:**
1. Check the path in `GOOGLE_SERVICE_ACCOUNT_KEY_PATH`
2. Ensure `google-credentials.json` exists in the `secrets/` directory
3. Use absolute path, not relative

### Redis connection failing but app works

**Expected behavior** - the app falls back to memory cache when Redis is unavailable. Check logs:
```
[INF] Redis cache disabled, using memory cache only
```

### CORS errors in browser

1. Check `ALLOWED_ORIGINS` includes your frontend URL
2. Ensure no trailing slashes in origins
3. Check browser console for exact error

### Tests failing after changes

1. Rebuild all projects: `dotnet clean && dotnet build`
2. Clear test results: `rm -rf */bin */obj`
3. Check for compilation errors: `dotnet build`

## Development Tools

### Useful Extensions for VS Code

- C# Dev Kit
- C# Extensions
- REST Client (for testing API endpoints)
- MongoDB for VS Code

### Testing API Endpoints

Create `test.http` file:

```http
### Get health status
GET http://localhost:5000/health/detailed

### Get client by GUID
GET http://localhost:5000/api/client/abc123

### Upload photo
POST http://localhost:5000/api/photo/upload/abc123
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="file"; filename="photo.jpg"
Content-Type: image/jpeg

< ./test-photo.jpg
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

## Git Workflow

### Before committing:

1. **Never commit secrets:**
   ```bash
   git status
   # Ensure .env, appsettings.Development.json, secrets/ are NOT listed
   ```

2. **Run tests:**
   ```bash
   dotnet test
   ```

3. **Check for build warnings:**
   ```bash
   dotnet build
   ```

### Safe files to commit:
- ✅ Source code (*.cs)
- ✅ Project files (*.csproj)
- ✅ `.env.example` (template without real values)
- ✅ `appsettings.json` (with empty secret values)
- ✅ Tests
- ✅ Documentation

### Files to NEVER commit:
- ❌ `.env` (with real values)
- ❌ `appsettings.Development.json` (with real values)
- ❌ `secrets/` directory
- ❌ `logs/` directory
- ❌ Any file with passwords, API keys, tokens

## Performance Testing

### Load testing with k6:

```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  stages: [
    { duration: '30s', target: 20 },
    { duration: '1m', target: 50 },
    { duration: '30s', target: 0 },
  ],
};

export default function () {
  let res = http.get('http://localhost:5000/health');
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
}
```

Run:
```bash
k6 run loadtest.js
```

## Database Management

### MongoDB Atlas Web UI

https://cloud.mongodb.com/

### Connect with MongoDB Compass

```
mongodb+srv://username:password@cluster.mongodb.net/WeddingPhotos
```

### Useful MongoDB queries:

```javascript
// Get all clients
db.Clients.find()

// Find active clients
db.Clients.find({ IsActive: true })

// Find expired clients
db.Clients.find({ DateTo: { $lt: new Date() }, IsActive: true })

// Count total clients
db.Clients.count()
```

## Support

For questions or issues:
1. Check logs: `logs/wedding-photos-*.log`
2. Review health status: http://localhost:5000/health/detailed
3. Check environment variables are loaded correctly
