using System.Text.Json;

namespace Film_app
{
    public class OmdbService : IMovieService
    {
        // Sdílená instance pro síťová spojení. Používá se jedna statická pro celou aplikaci, 
        // aby nedocházelo k přetěžování systému a vyčerpání volných síťových portů.
        private static readonly HttpClient Client = new HttpClient();

        private readonly string apiKey = "f5b1565b";
        private readonly string baseUrl = "https://omdbapi.com";

        // Lokální mezipaměť (cache). Ukládá již stažené výsledky vyhledávání, 
        // abychom při opakovaném dotazu nemuseli znovu volat API přes internet.
        private readonly Dictionary<string, SearchResponse> cache = new();

        public async Task<SearchResponse> SearchMoviesAsync(string query)
        {
            try
            {
                // Kontrola mezipaměti: Pokud jsme tento dotaz už jednou hledali, 
                // ihned vrátíme uložená data a nezatěžujeme síť.
                if (cache.ContainsKey(query))
                {
                    Console.WriteLine("[CACHE] Pouzivam ulozena data");
                    return cache[query];
                }

                // Kódování vyhledávaného textu. Převede speciální znaky a mezery (např. v "batman begins") 
                // do formátu, který je bezpečný pro validní URL adresu.
                string url = $"{baseUrl}?apikey={apiKey}&s={Uri.EscapeDataString(query)}";

                // Odeslání asynchronního požadavku na API a čekání na odpověď serveru.
                HttpResponseMessage response = await Client.GetAsync(url);
                // Kontrola úspěchu: Pokud server vrátí chybu (např. neexistující stránku), vyvolá se výjimka.
                response.EnsureSuccessStatusCode();

                // Načtení obsahu odpovědi ze serveru do textového řetězce ve formátu JSON.
                string jsonString = await response.Content.ReadAsStringAsync();

                // Převod textu ve formátu JSON na strukturovaný C# objekt (deserializace).
                var result = JsonSerializer.Deserialize<SearchResponse>(jsonString);

                // Uložení získaného výsledku do mezipaměti pro budoucí použití.
                cache[query] = result;

                return result;
            }
            catch (HttpRequestException)
            {
                // Zachycení chyb specifických pro síťovou komunikaci (výpadek internetu, nedostupnost API).
                Console.WriteLine("[CHYBA] Internet nebo API nedostupne.");
                return null;
            }
            catch
            {
                // Zachycení jakýchkoliv ostatních nečekaných chyb (např. chybný formát přijatých dat).
                Console.WriteLine("[CHYBA] Neocekavana chyba.");
                return null;
            }
        }

        public async Task<MovieDetails> GetMovieDetailsAsync(string imdbId)
        {
            try
            {
                string url = $"{baseUrl}?apikey={apiKey}&i={imdbId}";

                HttpResponseMessage response = await Client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<MovieDetails>(jsonString);
            }
            catch
            {
                // Ošetření chyb při načítání detailu filmu, které zabrání pádu celé aplikace.
                Console.WriteLine("[CHYBA] Nepodarilo se nacist detail.");
                return null;
            }
        }
    }
}
