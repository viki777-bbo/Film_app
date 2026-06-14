# Film Aplikace podle OMDb Api

Jednoduchá konzolová aplikace v jazyce C#, která umožňuje vyhledávat filmy pomocí OMDb API.

--

##  O projektu

Aplikace slouží k práci se:

* API (OMDb)
* async / await
* JSON daty
* základy OOP
* konzolovým vstupem (REPL)

---

## Funkce

* Vyhledávání filmů podle názvu
* Zobrazení detailu filmu
* Historie vyhledávání
* Uložení filmu do souboru
* Cachování (opakované dotazy jsou rychlejší)
* Ošetření chyb (špatný vstup, API, internet)

---

## Příkazy

* Příkaz    Popis                          

| `search`  | Vyhledá filmy podle názvu      |
| `details` | Zobrazí detail podle IMDb ID   |
| `history` | Zobrazí historii hledání       |
| `save`    | Uloží poslední film do souboru |
| `help`    | Vypíše nápovědu                |
| `exit`    | Ukončí aplikaci                |

---

## Spuštění

```bash
dotnet run
```

---

## Ukázka použití

```
search
batman
5

details
tt0372784

save

history

exit
```

---

## Struktura projektu

* `Program.cs` - hlavní logika aplikace
* `IMovieService.cs` - rozhraní
* `OmdbService.cs` - práce s API
* `Models` - datové třídy

---

## Technologie

* C#
* .NET
* HttpClient
* System.Text.Json

---

## Výstup

* soubor `film_detail.txt`

---

## Použití AI

* Při tvorbě projektu jsem využil AI (ChatGPT/Gemini) jako **dočasný** pomocný nástroj především pro:

* Vysvětlení principů (HttpClient, async/await, JSON)
* AI jsem použil minimálně díky použití funkce v Rideru červené nebo oranžové žárovky

Příklady promptů
* „Kde mám typo v Klíč Api když vidím že je zcela správně?“
* „Proč je čtení tak pomalé z mého API OMDb?“
* „jak přidat cache do aplikace? Vypis mi weby kde se to vysvetluje a posli mi odkazy, nepis žádnou odpověď nebo pomoc ty sám“

## Výstup

* AI mi pomohla:

* pochopit práci s API
* správně použít async/await

## Poznámky

* Je potřeba internet
* API má omezení (Limitace free verze)

* 
