# Contact Messages History - MongoDB Collection

## Przegląd

Wszystkie wiadomości z formularza kontaktowego są automatycznie zapisywane w MongoDB w kolekcji `ContactMessages`. To zapewnia:
- 📊 Pełną historię kontaktów
- 🔍 Możliwość przeszukiwania i filtrowania
- 📈 Analizę statystyk (liczba wiadomości, źródła ruchu, itp.)
- 💾 Backup na wypadek problemów z Discord webhook

## Struktura Dokumentu ContactMessage

```json
{
  "_id": "ObjectId",
  "name": "Jan Kowalski",
  "email": "jan@example.com",
  "phone": "+48123456789",
  "subject": "Pytanie o ofertę",
  "message": "Treść wiadomości...",
  "createdAt": "2025-01-14T20:30:45.123Z",
  "ipAddress": "192.168.1.100",
  "userAgent": "Mozilla/5.0...",
  "isRead": false,
  "readAt": null,
  "notificationSent": true,
  "notificationSentAt": "2025-01-14T20:30:45.456Z",
  "notes": null
}
```

## Pola

| Pole | Typ | Opis |
|------|-----|------|
| `_id` | ObjectId | Unikalny identyfikator MongoDB |
| `name` | string | Imię i nazwisko nadawcy |
| `email` | string | Adres email nadawcy |
| `phone` | string? | Numer telefonu (opcjonalny) |
| `subject` | string? | Temat wiadomości (opcjonalny) |
| `message` | string | Treść wiadomości |
| `createdAt` | DateTime | Data i czas utworzenia (UTC) |
| `ipAddress` | string? | Adres IP nadawcy |
| `userAgent` | string? | User-Agent przeglądarki |
| `isRead` | bool | Czy wiadomość została przeczytana |
| `readAt` | DateTime? | Data i czas przeczytania |
| `notificationSent` | bool | Czy wysłano powiadomienie Discord |
| `notificationSentAt` | DateTime? | Data i czas wysłania powiadomienia |
| `notes` | string? | Notatki administratora |

## MongoDB Indexes

Kolekcja ma następujące indeksy dla wydajności:

```javascript
{
  "email": 1,           // Wyszukiwanie po emailu
  "createdAt": -1,      // Sortowanie po dacie (najnowsze pierwsze)
  "isRead": 1           // Filtrowanie nieprzeczytanych wiadomości
}
```

## Przykładowe Query

### Pobierz wszystkie wiadomości (ostatnie 50)

```javascript
db.ContactMessages.find()
  .sort({ createdAt: -1 })
  .limit(50)
```

### Pobierz nieprzeczytane wiadomości

```javascript
db.ContactMessages.find({ isRead: false })
  .sort({ createdAt: -1 })
```

### Pobierz wiadomości od konkretnego emaila

```javascript
db.ContactMessages.find({ email: "jan@example.com" })
  .sort({ createdAt: -1 })
```

### Oznacz wiadomość jako przeczytaną

```javascript
db.ContactMessages.updateOne(
  { _id: ObjectId("...") },
  {
    $set: {
      isRead: true,
      readAt: new Date()
    }
  }
)
```

### Dodaj notatkę do wiadomości

```javascript
db.ContactMessages.updateOne(
  { _id: ObjectId("...") },
  { $set: { notes: "Skontaktowałem się mailem - oferta wysłana" } }
)
```

### Statystyki - liczba wiadomości dzisiaj

```javascript
db.ContactMessages.countDocuments({
  createdAt: {
    $gte: new Date(new Date().setHours(0,0,0,0))
  }
})
```

### Statystyki - wiadomości w ostatnim miesiącu

```javascript
db.ContactMessages.aggregate([
  {
    $match: {
      createdAt: {
        $gte: new Date(new Date().setMonth(new Date().getMonth() - 1))
      }
    }
  },
  {
    $group: {
      _id: { $dateToString: { format: "%Y-%m-%d", date: "$createdAt" } },
      count: { $sum: 1 }
    }
  },
  { $sort: { _id: 1 } }
])
```

### Najpopularniejsze źródła (IP addresses)

```javascript
db.ContactMessages.aggregate([
  {
    $group: {
      _id: "$ipAddress",
      count: { $sum: 1 }
    }
  },
  { $sort: { count: -1 } },
  { $limit: 10 }
])
```

## Integracja z Repository Pattern

Aplikacja używa `IContactMessageRepository` do operacji na kolekcji:

```csharp
public interface IContactMessageRepository
{
    Task<ContactMessage> CreateAsync(ContactMessage message);
    Task<ContactMessage?> GetByIdAsync(string id);
    Task<List<ContactMessage>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<List<ContactMessage>> GetUnreadAsync();
    Task<bool> MarkAsReadAsync(string id);
    Task<bool> UpdateNotificationStatusAsync(string id, bool sent);
    Task<long> GetTotalCountAsync();
    Task<long> GetUnreadCountAsync();
}
```

## Automatyczne Zapisywanie

Proces wysyłania formularza kontaktowego:

1. **Walidacja** - FluentValidation sprawdza dane
2. **Zapis do MongoDB** - `ContactMessage` jest zapisywany jako pierwszy krok
3. **Wysłanie powiadomienia Discord** - Jeśli webhook jest skonfigurowany
4. **Update statusu** - `notificationSent` jest aktualizowany
5. **Logi** - Wszystkie operacje są logowane

```csharp
// NotificationService.cs (uproszczony flow)
public async Task SendContactFormNotificationAsync(ContactFormRequest request)
{
    // 1. Zapis do bazy
    var contactMessage = new ContactMessage { ... };
    var savedMessage = await _contactMessageRepository.CreateAsync(contactMessage);

    // 2. Wysłanie powiadomienia
    if (!string.IsNullOrEmpty(_discordWebhookUrl))
    {
        await SendDiscordNotificationAsync(request);
        await _contactMessageRepository.UpdateNotificationStatusAsync(savedMessage.Id, true);
    }
}
```

## Przydatne Widoki MongoDB Compass

### View 1: Nieprzeczytane wiadomości

```javascript
{
  isRead: false
}
```

Sort: `{ createdAt: -1 }`

### View 2: Wiadomości bez powiadomienia

```javascript
{
  notificationSent: false
}
```

### View 3: Wiadomości z ostatnich 24h

```javascript
{
  createdAt: {
    $gte: new Date(Date.now() - 24*60*60*1000)
  }
}
```

## Monitoring i Alerty

### Sprawdź nieprzeczytane wiadomości

```bash
# MongoDB Shell
use WeddingPhotos
db.ContactMessages.countDocuments({ isRead: false })
```

### Alert - wiele wiadomości z tego samego IP (możliwy spam)

```javascript
db.ContactMessages.aggregate([
  {
    $match: {
      createdAt: { $gte: new Date(Date.now() - 24*60*60*1000) } // Last 24h
    }
  },
  {
    $group: {
      _id: "$ipAddress",
      count: { $sum: 1 },
      emails: { $push: "$email" }
    }
  },
  {
    $match: { count: { $gt: 3 } } // Więcej niż 3 wiadomości
  }
])
```

## Backup i Archiwizacja

### Export do JSON

```bash
mongoexport --uri="mongodb+srv://username:password@cluster.mongodb.net/WeddingPhotos" \
  --collection=ContactMessages \
  --out=contact_messages_backup.json
```

### Import z JSON

```bash
mongoimport --uri="mongodb+srv://username:password@cluster.mongodb.net/WeddingPhotos" \
  --collection=ContactMessages \
  --file=contact_messages_backup.json
```

### Archiwizacja starych wiadomości (starsze niż 1 rok)

```javascript
// Przenieś do kolekcji ArchiveContactMessages
db.ContactMessages.find({
  createdAt: { $lt: new Date(new Date().setFullYear(new Date().getFullYear() - 1)) }
}).forEach(function(doc) {
  db.ArchiveContactMessages.insertOne(doc);
  db.ContactMessages.deleteOne({ _id: doc._id });
});
```

## Privacy i RODO

⚠️ **Uwaga:** Wiadomości kontaktowe zawierają dane osobowe (imię, email, telefon, IP).

### Zgodność z RODO:

1. **Podstawa prawna:** Zgoda użytkownika (checkbox w formularzu)
2. **Cel przetwarzania:** Obsługa zapytań kontaktowych
3. **Okres przechowywania:** Maksymalnie 2 lata lub do wycofania zgody
4. **Prawo do usunięcia:** Użytkownik może żądać usunięcia swoich danych

### Usuwanie danych użytkownika

```javascript
// Usuń wszystkie wiadomości od konkretnego emaila
db.ContactMessages.deleteMany({ email: "jan@example.com" })
```

### Anonimizacja danych

```javascript
// Anonimizuj dane osobowe (zachowaj tylko statystyki)
db.ContactMessages.updateMany(
  { createdAt: { $lt: new Date(new Date().setFullYear(new Date().getFullYear() - 2)) } },
  {
    $set: {
      name: "[DELETED]",
      email: "[DELETED]",
      phone: null,
      message: "[DELETED]",
      ipAddress: null,
      userAgent: null
    }
  }
)
```

## Future Enhancements

Możliwe rozszerzenia systemu:

1. **Admin Panel** - Hangfire Dashboard lub osobny panel do przeglądania wiadomości
2. **Email Responses** - System do odpowiadania na wiadomości bezpośrednio z aplikacji
3. **CRM Integration** - Synchronizacja z systemami CRM (HubSpot, Salesforce)
4. **Analytics Dashboard** - Wizualizacja statystyk wiadomości
5. **Auto-tagging** - Automatyczne kategoryzowanie wiadomości (oferta, pytanie, skarga)
6. **Spam Detection** - ML-based spam filtering
7. **Auto-responses** - Automatyczne odpowiedzi na często zadawane pytania

## Support

Aby uzyskać dostęp do historii wiadomości:

1. **MongoDB Atlas Web UI:** https://cloud.mongodb.com/
2. **MongoDB Compass:** Desktop application
3. **API Endpoints:** (do zaimplementowania w przyszłości)
   - `GET /api/admin/contact-messages`
   - `GET /api/admin/contact-messages/unread`
   - `PUT /api/admin/contact-messages/{id}/mark-read`
