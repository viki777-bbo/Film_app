namespace Film_app
{
    class Program
    {
        // Abstrakce přes rozhraní – konkrétní implementaci lze snadno vyměnit (např. za jiné API)
        private static readonly IMovieService MovieService = new OmdbService();

        // Uchovává seznam všech výrazů, které uživatel hledal v aktuálním běhu programu
        private static readonly List<string> SearchHistory = new List<string>();

        // Poslední zobrazený film – sdílený stav mezi příkazy 'details' a 'save'
        private static MovieDetails lastViewedMovie = null;

        // Vstupní bod programu – spouští smyčku REPL (čti → vyhodnoť → vypiš → opakuj)
        static async Task Main()
        {
            Console.WriteLine("=== Vitajte v aplikaci Filmy OMDb ===");
            PrintHelp();

            bool running = true;
            while (running)
            {
                Console.Write("\nZadej prikaz > ");
                string command = Console.ReadLine();

                // Obrana proti null (Ctrl+D / EOF na Linuxu) a normalizace vstupu
                if (command == null) continue;
                command = command.Trim().ToLower();

                // Směrování příkazů – každý case volá vlastní async/sync metodu
                switch (command)
                {
                    case "search":
                        await HandleSearchAsync();
                        break;
                    case "details":
                        await HandleDetailsAsync();
                        break;
                    case "history":
                        PrintHistory();
                        break;
                    case "save":
                        SaveLastMovieToFile();
                        break;
                    case "help":
                        PrintHelp();
                        break;
                    case "exit":
                        running = false;
                        Console.WriteLine("Aplikace se ukoncuje. Nashledanou.");
                        break;
                    default:
                        Console.WriteLine("[CHYBA] Neznamy prikaz. Napis 'help' pro seznam prikazu.");
                        break;
                }
            }
        }

        // Vyhledá filmy podle názvu a vypíše max. N výsledků.
        // Dotaz se uloží do historie; limit se ověří přes TryParse.
        private static async Task HandleSearchAsync()
        {
            Console.Write("Zadej nazev filmu pro vyhledani: ");
            string query = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("[CHYBA] Nazev filmu nesmi byt prazdny.");
                return;
            }

            Console.Write("Zadej maximalni pocet zobrazenych vysledku (cislo): ");
            string limitInput = Console.ReadLine();

            // TryParse – bezpečná konverze; při neplatném vstupu použijeme výchozí hodnotu
            if (!int.TryParse(limitInput, out int limit) || limit <= 0)
            {
                Console.WriteLine("[INFO] Neplatne cislo. Pouzije se vychozi limit 5 filmu.");
                limit = 5;
            }

            // Uložení do historie před samotným voláním API
            SearchHistory.Add(query);

            Console.WriteLine("Vyhledavam...");
            SearchResponse response = await MovieService.SearchMoviesAsync(query);

            // API může vrátit null (síťová chyba) nebo Response == "False" (nenalezeno)
            if (response == null || response.Response == "False")
            {
                Console.WriteLine("[INFO] Zadny film nebyl nenalezen. Zkontrolujte zadany nazev.");
                return;
            }

            Console.WriteLine($"\nVysledky vyhledavani (zobrazeno max {limit}):");
            int count = 0;
            foreach (Movie movie in response.Search)
            {
                if (count >= limit) break;
                Console.WriteLine($"- [{movie.ImdbId}] {movie.Title} ({movie.Year})");
                count++;
            }
        }

        // Načte podrobnosti o filmu podle IMDb ID (formát tt1234567).
        // Výsledek uloží do lastViewedMovie, aby ho šlo příkazem 'save' zapsat na disk.
        private static async Task HandleDetailsAsync()
        {
            Console.Write("Zadej IMDb ID filmu (napr. tt0120737): ");
            string id = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("[CHYBA] ID nesmi byt prazdne.");
                return;
            }

            Console.WriteLine("Nacitam detaily...");
            MovieDetails details = await MovieService.GetMovieDetailsAsync(id);

            if (details == null || details.Response == "False")
            {
                Console.WriteLine("[CHYBA] Film s timto ID nebyl nalezen.");
                return;
            }

            // Sdílený stav – přepíše předchozí film; 'save' vždy uloží ten poslední
            lastViewedMovie = details;

            Console.WriteLine($"\nDETAIL FILMU: {details.Title} ({details.Year})");
            Console.WriteLine($"Zanr: {details.Genre}");
            Console.WriteLine($"Rezirer: {details.Director}");
            Console.WriteLine($"Popis: {details.Plot}");
            Console.WriteLine("[INFO] Tento detail muzete nyni ulozit prikazem 'save'.");
        }

        // Zapíše detail posledního zobrazeného filmu do souboru film_detail.txt
        // ve složce, odkud je program spuštěn. Existující soubor přepíše.
        private static void SaveLastMovieToFile()
        {
            if (lastViewedMovie == null)
            {
                Console.WriteLine("[CHYBA] Nejprve musite vyhledat detail nejakeho filmu pomoci 'details'.");
                return;
            }

            try
            {
                string fileName = "film_detail.txt";

                string text = $"Nazev: {lastViewedMovie.Title}\n" +
                              $"Rok: {lastViewedMovie.Year}\n" +
                              $"Zanr: {lastViewedMovie.Genre}\n" +
                              $"Rezirer: {lastViewedMovie.Director}\n" +
                              $"Popis: {lastViewedMovie.Plot}\n" +
                              $"Ulozeno dne: {DateTime.Now}\n";

                // WriteAllText vytvoří soubor pokud neexistuje, jinak ho přepíše
                File.WriteAllText(fileName, text);
                Console.WriteLine($"[USPCH] Data byla uspesne ulozena do souboru: {Path.GetFullPath(fileName)}");
            }
            catch (Exception)
            {
                // Může selhat např. při nedostatku práv nebo plném disku
                Console.WriteLine("[CHYBA] Nepodarilo se zapsat data do souboru.");
            }
        }

        // Vypíše všechny výrazy hledané v aktuálním běhu programu (reset při restartu)
        private static void PrintHistory()
        {
            if (SearchHistory.Count == 0)
            {
                Console.WriteLine("[INFO] Historie vyhledavani je prazdna.");
                return;
            }

            Console.WriteLine("\nHistorie vasich vyhledavani:");
            foreach (string item in SearchHistory)
            {
                Console.WriteLine($"- {item}");
            }
        }

        // Vypíše přehled všech dostupných příkazů
        private static void PrintHelp()
        {
            Console.WriteLine("\nSeznam dostupnych prikazu:");
            Console.WriteLine("  search   - Vyhledat filmy podle nazvu (s volbou limitu)");
            Console.WriteLine("  details  - Zobrazit podrobnosti o filmu pomoci IMDb ID");
            Console.WriteLine("  history  - Zobrazit historii vasich hledani");
            Console.WriteLine("  save     - Ulozit naposledy zobrazeny detail filmu do souboru");
            Console.WriteLine("  help     - Zobrazit tento seznam prikazu");
            Console.WriteLine("  exit     - Ukoncit program");
        }
    }
}