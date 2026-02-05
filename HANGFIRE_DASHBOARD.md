# Hangfire Dashboard - Setup i Użycie

## Co to jest Hangfire Dashboard?

Hangfire Dashboard to webowy interfejs do monitorowania i zarządzania zadaniami w tle (background jobs). Pozwala na:
- 📊 Podgląd statystyk (succeeded, failed, processing jobs)
- 📋 Listę wszystkich zadań w kolejce
- 🔄 Zarządzanie recurring jobs
- ❌ Retry failed jobs
- 📈 Wykresy wydajności w czasie rzeczywistym

## Jak włączyć Dashboard?

### 1. Ustaw zmienne środowiskowe

Dodaj do pliku `.env` lub ustaw zmienne środowiskowe:

```bash
# Włącz dashboard
HANGFIRE_DASHBOARD_ENABLED=true

# Ustaw credentials (WYMAGANE!)
HANGFIRE_DASHBOARD_USERNAME=admin
HANGFIRE_DASHBOARD_PASSWORD=TwojeHasło123!

# Opcjonalnie - zmień ścieżkę
HANGFIRE_DASHBOARD_PATH=/hangfire
```

### 2. Załaduj zmienne środowiskowe

**Windows PowerShell:**
```powershell
# Załaduj z pliku .env
Get-Content .env | ForEach-Object {
    if ($_ -match '^\s*([^#][^=]+)=(.+)$') {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        [Environment]::SetEnvironmentVariable($name, $value, 'Process')
    }
}
```

**Linux/macOS/WSL:**
```bash
export $(grep -v '^#' .env | xargs)
```

### 3. Uruchom aplikację

```bash
dotnet run --project src/WeddingPhotos.Api
```

### 4. Otwórz Dashboard

Przejdź do:
```
http://localhost:5000/hangfire
```

Pojawi się prompt logowania - użyj credentials z `.env`:
- **Username:** admin (lub co ustawiłeś)
- **Password:** TwojeHasło123!

## Bezpieczeństwo

### ✅ Zabezpieczenia w aplikacji:

1. **Basic Authentication** - Wymagane login i hasło
2. **Environment Variables** - Credentials NIE są w kodzie
3. **Secure by Default** - Jeśli brak credentials → dashboard zablokowany
4. **HTTPS recommended** - Na produkcji używaj HTTPS
5. **Strong passwords** - Użyj silnych haseł (min. 16 znaków)

### 🔒 Generowanie bezpiecznego hasła:

**PowerShell:**
```powershell
# Wygeneruj losowe hasło (20 znaków)
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 20 | % {[char]$_})
```

**Linux/macOS:**
```bash
# Wygeneruj losowe hasło (20 znaków)
openssl rand -base64 20
```

**Online:**
https://passwordsgenerator.net/

### ⚠️ Ważne zasady:

1. **NIE commituj haseł do Git**
   - `.env` jest w `.gitignore`
   - Używaj `.env.example` jako template

2. **Różne hasła dla różnych środowisk**
   - Development: `admin` / `dev123`
   - Production: Silne, losowe hasło

3. **Zmień domyślne hasło**
   - Nigdy nie używaj `admin` / `admin` na produkcji!

4. **HTTPS na produkcji**
   - Basic Auth przez HTTP = hasło wysyłane w plain text
   - Zawsze używaj HTTPS/SSL

## Producja - Docker

W `docker-compose.yml`:

```yaml
services:
  api:
    environment:
      - HANGFIRE_DASHBOARD_ENABLED=true
      - HANGFIRE_DASHBOARD_USERNAME=${HANGFIRE_DASHBOARD_USERNAME}
      - HANGFIRE_DASHBOARD_PASSWORD=${HANGFIRE_DASHBOARD_PASSWORD}
```

W pliku `.env` (poza repozytorium):
```bash
HANGFIRE_DASHBOARD_USERNAME=admin
HANGFIRE_DASHBOARD_PASSWORD=verySecureRandomPassword123!@#
```

## Co pokazuje Dashboard?

### Główna strona (Overview)

```
┌────────────────────────────────────────┐
│ Hangfire Dashboard                     │
├────────────────────────────────────────┤
│ Jobs                                   │
│ ├─ Succeeded: 1,234                    │
│ ├─ Failed: 3                           │
│ ├─ Processing: 1                       │
│ ├─ Scheduled: 5                        │
│ └─ Enqueued: 0                         │
├────────────────────────────────────────┤
│ Servers: 1 active                      │
│ Recurring Jobs: 1                      │
├────────────────────────────────────────┤
│ Real-time Graph                        │
│     [wykres succeeded/failed w czasie] │
└────────────────────────────────────────┘
```

### Jobs Tab

Lista wszystkich zadań:

| Job | State | Created | Duration |
|-----|-------|---------|----------|
| SendContactFormNotification | Succeeded | 2s ago | 0.5s |
| SendContactFormNotification | Succeeded | 5m ago | 0.3s |
| DeactivateExpiredClients | Failed | 1h ago | 10s |

Kliknij na job → Zobaczysz:
- Szczegóły wykonania
- Exception details (jeśli failed)
- History poprzednich wykonań
- Możliwość retry

### Recurring Jobs Tab

```
┌────────────────────────────────────────┐
│ Recurring Jobs                         │
├────────────────────────────────────────┤
│ ID: deactivate-expired-clients         │
│ Cron: 0 0 * * * (daily at midnight)    │
│ Next execution: in 5 hours             │
│ Last execution: 19 hours ago (success) │
│ [Trigger now] [Remove]                 │
└────────────────────────────────────────┘
```

### Retries Tab

Failed jobs z automatycznymi retry:

| Job | Retry attempt | Next retry | Error |
|-----|---------------|------------|-------|
| SendContactFormNotification | 1 of 10 | in 1m | Discord webhook timeout |

### Servers Tab

Aktywne Hangfire workers:

```
┌────────────────────────────────────────┐
│ Server: DESKTOP-ABC123:12345:worker-1  │
│ Started: 2 hours ago                   │
│ Workers: 20                            │
│ Queues: default                        │
│ Processing: 1 job                      │
└────────────────────────────────────────┘
```

## Użyteczne operacje w Dashboard

### 1. Retry failed job

1. Przejdź do **Jobs** → **Failed**
2. Kliknij na failed job
3. Kliknij **"Requeue"**

### 2. Trigger recurring job manualnie

1. Przejdź do **Recurring Jobs**
2. Znajdź job (np. `deactivate-expired-clients`)
3. Kliknij **"Trigger now"**

### 3. Usuń scheduled job

1. Przejdź do **Jobs** → **Scheduled**
2. Kliknij na job
3. Kliknij **"Delete"**

### 4. Zobacz szczegóły wykonania

1. Kliknij na dowolny job
2. Zobacz:
   - Duration
   - Exception (jeśli failed)
   - State history
   - Job parameters

## Monitoring w produkcji

### Alerty gdy za dużo failed jobs

Możesz stworzyć monitoring endpoint:

```csharp
[HttpGet("api/admin/hangfire/health")]
public IActionResult HangfireHealth()
{
    var api = JobStorage.Current.GetMonitoringApi();
    var stats = api.GetStatistics();

    if (stats.Failed > 10)
    {
        return StatusCode(500, new {
            status = "unhealthy",
            failedJobs = stats.Failed
        });
    }

    return Ok(new {
        status = "healthy",
        succeeded = stats.Succeeded,
        failed = stats.Failed,
        processing = stats.Processing
    });
}
```

### Integracja z Sentry

Failed jobs są automatycznie raportowane do Sentry (jeśli skonfigurowany), dzięki czemu dostajesz alerty.

## Troubleshooting

### Problem: Dashboard pokazuje "Access Denied"

**Rozwiązanie:**
1. Sprawdź czy ustawiłeś zmienne środowiskowe:
   ```bash
   echo $HANGFIRE_DASHBOARD_USERNAME
   echo $HANGFIRE_DASHBOARD_PASSWORD
   ```

2. Sprawdź logi aplikacji:
   ```bash
   tail -f logs/wedding-photos-*.log | grep -i hangfire
   ```

3. Dashboard wymaga OBIE zmienne - brak którejkolwiek = access denied

### Problem: Nieprawidłowy login/hasło

**Rozwiązanie:**
1. Zresetuj hasło w zmiennych środowiskowych
2. Zrestartuj aplikację
3. Wyczyść cache przeglądarki (Ctrl+F5)
4. Spróbuj w trybie incognito

### Problem: Dashboard nie pokazuje się w Menu

**Rozwiązanie:**
1. Sprawdź czy `HANGFIRE_DASHBOARD_ENABLED=true`
2. Sprawdź czy `HANGFIRE_ENABLED=true`
3. Zrestartuj aplikację

### Problem: Jobs nie wykonują się

**Rozwiązanie:**
1. Sprawdź **Servers** tab - czy jest aktywny worker?
2. Sprawdź logi aplikacji
3. Sprawdź czy `HANGFIRE_ENABLED=true`

## Performance Tips

### Memory Storage vs Persistent Storage

**Domyślnie:** MemoryStorage
```csharp
.UseMemoryStorage()
```
- ✅ Szybkie
- ❌ Po restarcie - wszystkie jobs znikają

**Dla produkcji:** MongoDB Storage
```csharp
.UseMongoStorage(mongoConnectionString, new MongoStorageOptions
{
    MigrationOptions = new MongoMigrationOptions
    {
        MigrationStrategy = new MigrateMongoMigrationStrategy()
    }
})
```
- ✅ Jobs przetrwają restart
- ✅ Distributed - wiele serwerów może przetwarzać jobs
- ⚠️ Wymaga dodatkowego package: `Hangfire.Mongo`

### Zwiększenie liczby workers

W `Program.cs`:

```csharp
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 5; // Domyślnie: 20
});
```

## Przykłady użycia

### Background Job (fire-and-forget)

```csharp
BackgroundJob.Enqueue(() => Console.WriteLine("Hello from background!"));
```

W Dashboard zobaczysz:
- Job pojawi się w **Enqueued**
- Po wykonaniu → **Succeeded**
- Jeśli error → **Failed** (z możliwością retry)

### Delayed Job

```csharp
BackgroundJob.Schedule(
    () => SendReminderEmail(clientId),
    TimeSpan.FromDays(7)
);
```

W Dashboard zobaczysz w **Scheduled** → za 7 dni

### Recurring Job

```csharp
RecurringJob.AddOrUpdate(
    "send-weekly-report",
    () => SendWeeklyReport(),
    Cron.Weekly
);
```

W Dashboard zobaczysz w **Recurring Jobs**

## API do zarządzania jobs

Możesz programatycznie zarządzać jobs:

```csharp
// Get monitoring API
var monitoringApi = JobStorage.Current.GetMonitoringApi();

// Get statistics
var stats = monitoringApi.GetStatistics();
Console.WriteLine($"Succeeded: {stats.Succeeded}");
Console.WriteLine($"Failed: {stats.Failed}");

// Get failed jobs
var failedJobs = monitoringApi.FailedJobs(0, 10);

// Requeue failed job
BackgroundJob.Requeue(jobId);

// Delete job
BackgroundJob.Delete(jobId);
```

## Security Best Practices - Checklist

- [ ] Username i Password w zmiennych środowiskowych
- [ ] Silne hasło (min. 16 znaków, losowe)
- [ ] HTTPS na produkcji
- [ ] Dashboard wyłączony domyślnie (`HANGFIRE_DASHBOARD_ENABLED=false`)
- [ ] Różne credentials dla dev/staging/prod
- [ ] Regular password rotation (co 90 dni)
- [ ] IP whitelist dla dodatkowo security (opcjonalnie)
- [ ] 2FA dla production dashboard (advanced)

## Dodatkowe zabezpieczenia (advanced)

### IP Whitelist

W `HangfireAuthorizationFilter.cs` możesz dodać:

```csharp
var allowedIps = new[] { "192.168.1.100", "10.0.0.5" };
var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();

if (!allowedIps.Contains(remoteIp))
{
    return false;
}
```

### Rate Limiting

Możesz dodać rate limiting do `/hangfire` endpoint w `Program.cs`.

## Wsparcie

Dokumentacja Hangfire:
- https://docs.hangfire.io/
- https://docs.hangfire.io/en/latest/configuration/using-dashboard.html

Jeśli masz problemy:
1. Sprawdź logi: `logs/wedding-photos-*.log`
2. Sprawdź Dashboard: `/hangfire`
3. Sprawdź zmienne środowiskowe
