# RateLimiter

Samostalna **middleware biblioteka za ograničavanje broja zahteva po IP adresi** u .NET 9, napravljena tako da se lako može ponovo iskoristiti ili objaviti na bilo kom **NuGet** repozitorijumu.

Middleware ograničava broj zahteva na osnovu **IP adrese klijenta** i vraća status `429 Too Many Requests` kada klijent premaši dozvoljenu brzinu slanja zahteva.  
Po želji se mogu podesiti i posebna ograničenja po endpoint-u.

---

## Karakteristike

- **Globalno ograničenje po IP-u** — konfiguriše se broj zahteva i vremenski okvir  
- **Opcionalna ograničenja po endpoint-u** („dodatni krediti“)  
- Vraća **HTTP 429** kada se prekorači limit  
- Nema zavisnosti — potpuno **samostalna**  
- Jednostavna integracija putem **extension metoda**  
- Kompatibilna sa **.NET 9**  

---

## Konfiguracija

U `appsettings` dodati sledeću sekciju (primer):

```json
"RateLimiter": {
  "RequestLimiterEnabled": true,
  "DefaultRequestLimitMs": 1000,
  "DefaultRequestLimitCount": 10,
  "RespectXForwardedFor": false,
  "EndpointLimits": [
    {
      "Endpoint": "/api/products/books",
      "RequestLimitMs": 1000,
      "RequestLimitCount": 1
    },
    {
      "Endpoint": "/api/products/pencils",
      "RequestLimitMs": 500,
      "RequestLimitCount": 2
    }
  ]
}
```

### Parametri konfiguracije

| Parametar | Opis | Tip |
|------------|------|------|
| `RequestLimiterEnabled` | Uključuje/isključuje limiter globalno | `bool` |
| `DefaultRequestLimitMs` | Vremenski okvir u milisekundama | `int` |
| `DefaultRequestLimitCount` | Broj dozvoljenih zahteva u okviru tog perioda | `int` |
| `RespectXForwardedFor` | Da li da koristi `X-Forwarded-For` heder za IP adresu klijenta | `bool` |
| `XForwardedForHeaderName` | Naziv hedera za prosleđenu IP adresu (podrazumevano `"X-Forwarded-For"`) | `string` |
| `EndpointLimits` | Lista opcionalnih ograničenja po endpoint-u | `array` |
| `EndpointLimits[].Endpoint` | Putanja endpoint-a (tačno poklapanje) | `string` |
| `EndpointLimits[].RequestLimitMs` | Vremenski okvir za taj endpoint | `int` |
| `EndpointLimits[].RequestLimitCount` | Maksimalan broj zahteva po IP-u u okviru tog perioda | `int` |

---

## Instalacija

Dodati biblioteku u projekat (preko NuGet-a ili lokalne reference):

```bash
dotnet add package RateLimiter
```

Ako se koristi lokalno, referencirati `.csproj` direktno:

```xml
<ProjectReference Include="..\RateLimiter\RateLimiter.csproj" />
```

---

##  Primer upotrebe

### **Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Register RateLimiter services
builder.Services.AddRateLimiter(builder.Configuration);

// 2️⃣ Build application
var app = builder.Build();

// 3️⃣ Use RateLimiter early in the pipeline
app.UseRateLimiterMiddleware();

// Other middlewares
// app.UseCors();
// app.UseAuthentication();
// app.UseAuthorization();

// Endpoints examples
app.MapGet("/api/products/books", () => Results.Ok("📚 Books endpoint"));
app.MapGet("/api/products/pencils", () => Results.Ok("✏️ Pencils endpoint"));
app.MapGet("/ping", () => Results.Ok("pong 🏓"));

app.Run();
```

---

## Ponašanje

### Primer:
- `DefaultRequestLimitCount = 5`
- `DefaultRequestLimitMs = 1000`

 Jedna IP adresa može da pošalje **do 5 zahteva u sekundi** ka bilo kom endpoint-u.  
Šesti zahtev u istom intervalu vratiće:

```http
HTTP/1.1 429 Too Many Requests
```

Dodatni hederi (npr. `Retry-After`) **nisu uključeni**, u skladu sa zahtevima zadatka.

---

## Tehnički pregled implementacije

- **Middleware-first** pristup — registruje se pre poslovne logike  
- **Ključ za limitiranje:** `IP|Scope` (scope = globalni ili putanja endpoint-a)  
- **Thread-safe in-memory skladište:** koristi `ConcurrentDictionary<string, ConcurrentQueue<long>>`  
- **Sliding window** pristup za praćenje zahteva  
- **Bez eksternih zavisnosti** — nema Redis-a, baza podataka, ni dodatnih paketa  
- Koristi samo: `Microsoft.AspNetCore.*` i `Microsoft.Extensions.*`

---

## Testiranje

Za lokalno testiranje možeš koristiti alate poput Postman-a:

Kada se premaši dozvoljeni broj zahteva, dobijaju se odgovori `429 Too Many Requests`.

---

## Struktura projekta (primer)

```
YourApp/
 ├─ RateLimiter/                     # Biblioteka (ovaj paket)
 │   ├─ Middleware/
 │   ├─ Core/
 │   ├─ Options/
 │   ├─ Extensions/
 │   └─ RateLimiter.csproj
 └─ README.md
```

---

## Napomene i ograničenja

- Implementacija je **samo u memoriji** — limiter funkcioniše **po instanci aplikacije** (nije distribuiran).  
  Za produkciju je preporučeno koristiti distribuirano skladište (Redis, SQL i sl.).
- Poređenje putanja je **tačno** i **ne razlikuje mala/velika slova**.  
  Po potrebi se može proširiti na prefix ili regex poklapanje.
- Hederi za rate-limit (npr. `Retry-After`, `X-RateLimit-Limit`) nisu uključeni, u skladu sa zahtevima zadatka.
