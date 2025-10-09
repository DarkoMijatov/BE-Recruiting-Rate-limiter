## Opis zahteva

Neophodno je kreirati **.NET Core** biblioteku `RateLimiter` koja implementira osnovne funkcionalnosti filtriranja pristupa endpoint-ima servisa na osnovu konfigurabilnih limita. Biblioteku struktuirati na način da se može postaviti na random **Nuget** repozitorijum, odnosno neophodno je da bude **u potpunosti self-contained**.

## Funkcionalnosti

`RateLimiter` biblioteka u funkcionalnom pattern-u treba da predstavlja **middleware**, koji u middleware pipeline-u postoji pre *request-specific* middleware biblioteka koje zahtevaju poslovnu obradu request header-a (Auth, CORS itd.).

Osnovni kriterijum na osnovu koga će se vršiti ograničavanje zahteva je **dolazna IP adresa**, odnosno, svi limiti za pristup će se primenjivati na osnovu IP adrese sa koje dolazi zahtev.

Biblioteka treba da omogući:

#### Podrazumevani limit za sve endpoint-e

Potrebno je implementirati podrazumevani limit za pristup svim endpoint-ima servisa, i to:

*   Limit na uzastopni broj zahteva sa iste IP adrese u podrazumevanom vremenskom okviru - `DefaultRequestLimitCount`,
*   Podrazumevani vremenski okvir na broj zahteva sa iste IP adrese (u milisekundama)- `DefaultRequestLimitMs`,
*   U slučaju prekoračenja limita, korisniku je potrebno vratiti grešku `429 - Too Many Requests`,
*   **Nije potrebno** implementirati standardizaciju za rate-limit header polja (`Retry-After`, `X-Limit-` itd.)

**Primer:**

Za vrednosti `DefaultRequestLimitCount = 5` i `DefaultRequestLimitMs = 1000`, korisniku sa jedne IP adrese dozvoljeno je da u roku od **1 sekunde** pošalje **5 upita** ka endpoint-ima servisa.

#### **Extra credits** - **nije neophodno za review biblioteke** - Limit za konkretan endpoint

Potrebno je implementirati limit za pristup **konkretnom endpoint-u** servisa, i to:

*   Limit na uzastopni broj zahteva sa iste IP adrese ka konkretnom endpoint-u u odgovarajućem vremenskom okviru - `RequestLimitCount`,
*   Vremenski okvir na broj zahteva sa iste IP adrese ka konkretnom endpoint-u (u milisekundama)- `RequestLimitMs`,
*   U slučaju prekoračenja limita, korisniku je potrebno vratiti grešku `429 - Too Many Requests`,
*   **Nije potrebno** implementirati standardizaciju za rate-limit header polja (`Retry-After`, `X-Limit-` itd.)

#### Konfiguracija servisa

| Parametar                         | Opis                                                                  | Vrednost  |
| --------------------------------- | --------------------------------------------------------------------- | --------- |
| RequestLimiterEnabled             | Uključuje rate limiter funkcionalnosti                                | `boolean` |
| DefaultRequestLimitMs             | Podrazumevani vremenski okvir na broj zahteva za sve endpoint-e       | `integer` |
| DefaultRequestLimitCount          | Limit na uzastopni broj zahteva u vremenskom okviru za sve endpoint-e | `integer` |
|                                   |                                                                       |           |
| EndpointLimits*                   | Lista limita za konkretne endpoint-e                                  |           |
| EndpointLimits/Endpoint*          | Putanja konkretnog endpoint-a                                         | `string`  |
| EndpointLimits/RequestLimitMs*    | Podrazumevani vremenski okvir na broj zahteva za endpoint             | `integer` |
| EndpointLimits/RequestLimitCount* | Limit na uzastopni broj zahteva u vremenskom okviru za endpoint       | `integer` |

*Ukoliko se implementira _extra-credits_ zadatak

**Primer konfiguracije:**

```json
"RateLimiter": {
  "RequestLimiterEnabled": true,
  "DefaultRequestLimitMs": 1000,
  "DefaultRequestLimitCount": 10,
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

## Prerequisites za projekat

*   [.NET Core 9](https://dotnet.microsoft.com/download/dotnet/9.0),
*   Adekvatan **README.md** sa opisom konfiguracije i načina inicijalizacije biblioteke u projekat

## Napomene

*   Zanemariti production-grade optimizacije, projekat će biti korišćen isključivo za potrebe valuacije kandidata,
*   Оčekivano vreme za završetak zadatka - 4 radna sata
