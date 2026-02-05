# Discord Webhook Setup for Contact Form

## Czym jest Discord Webhook?

Discord webhook pozwala na otrzymywanie natychmiastowych powiadomień o nowych wiadomościach z formularza kontaktowego bezpośrednio na Discordzie (na komputerze i telefonie).

## Krok 1: Utwórz Serwer Discord (jeśli nie masz)

1. Otwórz Discord (desktop app lub przeglądarka)
2. Kliknij **+** (Dodaj serwer)
3. Wybierz **"Utwórz własny"**
4. Nazwij serwer np. **"Wedding Photos Notifications"**

## Krok 2: Utwórz Kanał dla Powiadomień

1. Na swoim serwerze Discord, kliknij **+** obok "KANAŁY TEKSTOWE"
2. Nazwij kanał np. **"formularz-kontaktowy"** lub **"notifications"**
3. Kliknij **Utwórz kanał**

## Krok 3: Utwórz Webhook

1. Kliknij prawym przyciskiem myszy na kanał
2. Wybierz **"Edytuj kanał"**
3. W menu po lewej wybierz **"Integracje"**
4. Kliknij **"Webhooks"**
5. Kliknij **"Nowy webhook"**
6. (Opcjonalnie) Zmień nazwę na **"Wedding Photos Contact Form"**
7. (Opcjonalnie) Zmień avatar
8. Kliknij **"Kopiuj URL webhooka"** - zapisz go w bezpiecznym miejscu!

Przykładowy URL wygląda tak:
```
https://discord.com/api/webhooks/1234567890123456789/abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789
```

## Krok 4: Skonfiguruj Aplikację

### Dla Local Development:

Dodaj do swojego pliku `.env`:
```bash
DISCORD_WEBHOOK_URL=https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN
```

Lub ustaw zmienną środowiskową:

**Windows PowerShell:**
```powershell
$env:DISCORD_WEBHOOK_URL="https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
```

**Linux/macOS:**
```bash
export DISCORD_WEBHOOK_URL="https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN"
```

### Dla Production (Docker):

Dodaj do `docker-compose.yml` lub przekaż jako zmienną środowiskową:
```yaml
environment:
  - DISCORD_WEBHOOK_URL=https://discord.com/api/webhooks/YOUR_WEBHOOK_ID/YOUR_WEBHOOK_TOKEN
```

Lub przy uruchamianiu Docker:
```bash
docker run -e DISCORD_WEBHOOK_URL="https://discord.com/api/webhooks/..." wedding-photos-api
```

## Krok 5: Testuj Webhook

### Opcja A: Test przez API endpoint

Wyślij POST request do `/api/contact`:

```bash
curl -X POST http://localhost:5000/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jan Kowalski",
    "email": "jan@example.com",
    "phone": "+48123456789",
    "subject": "Test wiadomości",
    "message": "To jest testowa wiadomość z formularza kontaktowego."
  }'
```

### Opcja B: Użyj REST Client w VS Code

Stwórz plik `test-contact.http`:
```http
### Test contact form
POST http://localhost:5000/api/contact
Content-Type: application/json

{
  "name": "Jan Kowalski",
  "email": "jan@example.com",
  "phone": "+48123456789",
  "subject": "Test wiadomości",
  "message": "To jest testowa wiadomość z formularza kontaktowego."
}
```

## Jak Wygląda Powiadomienie?

Po wysłaniu formularza, na Discord dostaniesz wiadomość:

```
📬 **Nowa wiadomość z formularza kontaktowego!**

Nowa wiadomość kontaktowa

👤 Imię i nazwisko: Jan Kowalski
📧 Email: jan@example.com
📱 Telefon: +48123456789

💬 Wiadomość:
To jest testowa wiadomość z formularza kontaktowego.

Wedding Photos - Contact Form
Timestamp: 2025-01-14 20:30:45 UTC
```

## Bezpieczeństwo Webhook URL

⚠️ **WAŻNE:** Webhook URL jest tajny - każdy kto go posiada może wysyłać wiadomości na Twój kanał!

### Zasady bezpieczeństwa:

1. **NIE commituj webhook URL do Git**
   - ✅ Dodane do `.gitignore`: `.env`, `appsettings.Development.json`

2. **Używaj zmiennych środowiskowych**
   - ✅ Implementacja w kodzie używa `Environment.GetEnvironmentVariable()`

3. **Jeśli webhook wycieknie:**
   - Usuń stary webhook w ustawieniach kanału Discord
   - Stwórz nowy webhook z nowym URL
   - Zaktualizuj zmienną środowiskową

4. **Rate limiting:**
   - ✅ API endpoint: maksymalnie 5 wiadomości na godzinę na IP
   - Discord webhook: Discord limituje do ~30 req/s

## Dodatkowe Funkcje

### Oznaczenia @mention w Discord

Możesz zmodyfikować kod, aby oznaczać Cię przy ważnych wiadomościach:

```csharp
var payload = new
{
    content = "<@YOUR_DISCORD_USER_ID> 📬 **Nowa wiadomość kontaktowa!**",
    // ...
};
```

Aby znaleźć swoje Discord User ID:
1. Settings → Advanced → Enable "Developer Mode"
2. Kliknij prawym na swój nick → Copy User ID

### Filtry według tematu

Możesz dodać różne kolory/ikony zależnie od tematu wiadomości:

```csharp
var color = request.Subject?.ToLower() switch
{
    var s when s.Contains("pilne") => 15158332, // Red
    var s when s.Contains("oferta") => 3447003, // Blue
    _ => 5814783 // Default blue
};
```

### Powiadomienia dźwiękowe

Aby otrzymać powiadomienie push na telefonie, Discord musi:
1. Mieć włączone powiadomienia dla tego kanału
2. Serwer musi być na Twojej liście aktywnych

Możesz również użyć `@everyone` lub `@here` w content (jeśli masz uprawnienia).

## Monitorowanie

### Sprawdź status webhooka:

```bash
curl http://localhost:5000/api/contact/health
```

Odpowiedź:
```json
{
  "status": "healthy",
  "discordWebhook": "configured",
  "timestamp": "2025-01-14T20:30:45Z"
}
```

### Logi aplikacji:

Sprawdź logi w `logs/wedding-photos-YYYY-MM-DD.log`:
```
[INF] Contact form queued for Jan Kowalski (jan@example.com)
[INF] Contact form notification sent successfully for Jan Kowalski (jan@example.com)
```

## Troubleshooting

### Problem: Nie otrzymuję powiadomień

**1. Sprawdź czy webhook URL jest ustawiony:**
```bash
echo $DISCORD_WEBHOOK_URL
```

**2. Sprawdź logi aplikacji:**
```bash
tail -f logs/wedding-photos-*.log | grep -i "contact"
```

**3. Sprawdź czy Hangfire działa:**
- Otwórz `/hangfire` (jeśli włączony)
- Sprawdź czy job się wykonał

**4. Testuj webhook bezpośrednio:**
```bash
curl -X POST "YOUR_WEBHOOK_URL" \
  -H "Content-Type: application/json" \
  -d '{"content": "Test message"}'
```

### Problem: Error 429 (Too Many Requests)

Discord limituje webhooks do ~30 req/s. Jeśli otrzymujesz 429:
- Hangfire automatycznie spróbuje ponownie
- Sprawdź czy nie wysyłasz zbyt wielu testów

### Problem: Webhook zwraca 404

Webhook został usunięty lub URL jest nieprawidłowy:
1. Sprawdź czy webhook istnieje w ustawieniach kanału Discord
2. Stwórz nowy webhook
3. Zaktualizuj `DISCORD_WEBHOOK_URL`

## Alternatywy

Jeśli wolisz inne platformy:

- **Slack:** Podobny webhook system
- **Microsoft Teams:** Incoming Webhooks
- **Telegram:** Bot API
- **Email:** SendGrid/Mailgun (wymaga dodatkowej implementacji)

Kod jest napisany tak, żeby łatwo dodać inne sposoby powiadomień w `NotificationService`.

## Przykładowe Użycie na Frontend

```typescript
// React/Next.js example
const handleSubmit = async (e: React.FormEvent) => {
  e.preventDefault();

  try {
    const response = await fetch('https://api.yourweddingphotos.pl/api/contact', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        name: formData.name,
        email: formData.email,
        phone: formData.phone,
        subject: formData.subject,
        message: formData.message,
      }),
    });

    const data = await response.json();

    if (response.ok) {
      alert(data.message); // "Dziękujemy za wiadomość!..."
    } else {
      alert(data.error);
    }
  } catch (error) {
    alert('Wystąpił błąd. Spróbuj ponownie później.');
  }
};
```

## Wsparcie

Jeśli masz problemy z konfiguracją Discord webhook:
1. Sprawdź logi: `logs/wedding-photos-*.log`
2. Testuj endpoint: `/api/contact/health`
3. Sprawdź dokumentację Discord: https://discord.com/developers/docs/resources/webhook
