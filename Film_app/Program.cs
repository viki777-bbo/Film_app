
namespace Film_app
{
    class Program
    {
        // Vytvoreni sluzby pres rozhrani (vyuziti abstrakce a polymorfismu)
        private static readonly IMovieService MovieService = new OmdbService();
        
        // Bonus 1: Uchovani stavu mezi prikazy (historie vyhledavani)
        private static readonly List<string> SearchHistory = new List<string>();
        
        // Bonus 2: Uchovani stavu (naposledy zobrazeny zaznam pro pozdejsi ulozeni)
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

                // Zakladni validace vstupu, aby program nespadl pri prazdnem radku
                if (command == null) continue;
                command = command.Trim().ToLower();

                // REPL rozhrani - vetveni prikazu podle zadani
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

            // Validace textoveho vstupu
            if (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("[CHYBA] Nazev filmu nesmi byt prazdny.");
                return;
            }

            // Validace uzivatelskeho vstupu pomoci TryParse (povinna podminka ze zadani)
            Console.Write("Zadej maximalni pocet zobrazenych vysledku (cislo): ");
            string limitInput = Console.ReadLine();
            
            if (!int.TryParse(limitInput, out int limit) || limit <= 0)
            {
                Console.WriteLine("[INFO] Neplatne cislo. Pouzije se vychozi limit 5 filmu.");
                limit = 5;
            }

            // Ulozeni dotazu do historie hledani (Bonus - uchovani stavu)
            SearchHistory.Add(query);

            Console.WriteLine("Vyhledavam...");
            SearchResponse response = await MovieService.SearchMoviesAsync(query);

            // Osetreni pripadu, kdy API nic nenaslo nebo selhalo pripojeni
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

            // Ulozeni do staticke promenne (Bonus - uchovani stavu pro prikaz 'save')
            lastViewedMovie = details;

            // Vypis stazenych dat
            Console.WriteLine($"\nDETAIL FILMU: {details.Title} ({details.Year})");
            Console.WriteLine($"Zanr: {details.Genre}");
            Console.WriteLine($"Rezirer: {details.Director}");
            Console.WriteLine($"Popis: {details.Plot}");
            Console.WriteLine("[INFO] Tento detail muzete nyni ulozit prikazem 'save'.");
        }

        // Bonus: Ulozeni vystupu do souboru pomoci System.IO
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
                
                // Vytvoreni obsahu textoveho souboru
                string text = $"Nazev: {lastViewedMovie.Title}\n" +
                              $"Rok: {lastViewedMovie.Year}\n" +
                              $"Zanr: {lastViewedMovie.Genre}\n" +
                              $"Rezirer: {lastViewedMovie.Director}\n" +
                              $"Popis: {lastViewedMovie.Plot}\n" +
                              $"Ulozeno dne: {DateTime.Now}\n";

                // Zapis do souboru (pokuf existuje, prepise ho)
                File.WriteAllText(fileName, text);
                Console.WriteLine($"[USPCH] Data byla uspesne ulozena do souboru: {Path.GetFullPath(fileName)}");
            }
            catch (Exception)
            {
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
