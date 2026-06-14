namespace Film_app
{
    class Program
    {
        // Inicializace služeb a vnitřní paměti programu
        private static readonly IMovieService MovieService = new OmdbService();
        private static readonly List<string> SearchHistory = new List<string>();
        private static MovieDetails lastViewedMovie = null;

        static async Task Main()
        {
            Console.WriteLine("=== Vitajte v aplikaci Filmy OMDb ===");
            PrintHelp();

            bool running = true;
            while (running)
            {
                Console.Write("\nZadej prikaz > ");
                string command = Console.ReadLine();

                if (command == null) continue;
                command = command.Trim().ToLower(); // Odstraní mezery a převede na malá písmena

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

        private static async Task HandleSearchAsync()
        {
            Console.Write("Zadej nazev filmu pro vyhledani: ");
            string query = Console.ReadLine();

            // Kontrola, zda uživatel nezadal prázdný text nebo jen 1 znak
            if (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("[CHYBA] Nazev filmu nesmi byt prazdny.");
                return;
            }

            if (query.Length < 2)
            {
                Console.WriteLine("[CHYBA] Zadej alespon 2 znaky.");
                return;
            }

            Console.Write("Zadej maximalni pocet zobrazenych vysledku (cislo): ");
            string limitInput = Console.ReadLine();

            // Převod textu na číslo. Pokud selže, nastaví se výchozí limit 5
            if (!int.TryParse(limitInput, out int limit) || limit <= 0)
            {
                Console.WriteLine("[INFO] Neplatne cislo. Pouzije se vychozi limit 5 filmu.");
                limit = 5;
            }

            SearchHistory.Add(query);

            Console.WriteLine("Vyhledavam...");
            SearchResponse response = await MovieService.SearchMoviesAsync(query);

            // Kontrola, zda API vůbec něco vrátilo
            if (response == null || response.Response == "False")
            {
                Console.WriteLine("[INFO] Zadny film nebyl nalezen. Zkontrolujte zadany nazev.");
                return;
            }

            Console.WriteLine($"\nVysledky vyhledavani (zobrazeno max {limit}):");
            int count = 0;
            foreach (Movie movie in response.Search)
            {
                if (count >= limit) break; // Zastaví výpis, pokud jsme dosáhli limitu
                Console.WriteLine($"- [{movie.ImdbId}] {movie.Title} ({movie.Year})");
                count++;
            }
        }

        private static async Task HandleDetailsAsync()
        {
            Console.Write("Zadej IMDb ID filmu (napr. tt0120737): ");
            string id = Console.ReadLine();

            // Validace ID formatu (nesmí být prázdné a musí začínat na 'tt')
            if (string.IsNullOrWhiteSpace(id))
            {
                Console.WriteLine("[CHYBA] ID nesmi byt prazdne.");
                return;
            }

            if (!id.StartsWith("tt"))
            {
                Console.WriteLine("[CHYBA] IMDb ID musi zacinat na 'tt' (napr. tt0120737).");
                return;
            }

            Console.WriteLine("Nacitam detaily...");
            MovieDetails details = await MovieService.GetMovieDetailsAsync(id);

            if (details == null || details.Response == "False")
            {
                Console.WriteLine("[CHYBA] Film s timto ID nebyl nalezen.");
                return;
            }

            // Uloží film do paměti, aby se dal později exportovat přes příkaz 'save'
            lastViewedMovie = details;

            Console.WriteLine($"\nDETAIL FILMU: {details.Title} ({details.Year})");
            Console.WriteLine($"Zanr: {details.Genre}");
            Console.WriteLine($"Rezirer: {details.Director}");
            Console.WriteLine($"Popis: {details.Plot}");
            Console.WriteLine("[INFO] Tento detail muzete nyni ulozit prikazem 'save'.");
        }

        private static void SaveLastMovieToFile()
        {
            // Kontrola, zda uživatel předtím vůbec zobrazil nějaký detail filmu
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

                // Zápis textu do souboru (pokud soubor existuje, přepíše ho)
                File.WriteAllText(fileName, text);
                Console.WriteLine($"[USPCH] Data byla uspesne ulozena do souboru: {Path.GetFullPath(fileName)}");
            }
            catch
            {
                // Ochrana před pádem aplikace, pokud selže zápis na disk (např. kvůli právům)
                Console.WriteLine("[CHYBA] Nepodarilo se zapsat data do souboru.");
            }
        }

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
