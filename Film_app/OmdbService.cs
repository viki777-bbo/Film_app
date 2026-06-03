using System.Text.Json;

namespace Film_app
{
    // Implementace sluzby komunikujici s OMDb API
    public class OmdbService : IMovieService
    {
        // Jedina sdilena instance HttpClient pro celou aplikaci podle zadani
        private static readonly HttpClient Client = new HttpClient();
        
        // Privatni promenne skryte pred vnejscim svetem (zapouzdrini)
        private readonly string apiKey = "f5b1565b";
        private readonly string baseUrl = "https://omdbapi.com";

        // Asynchronni metoda pro vyhledavani seznamu filmu
        public async Task<SearchResponse> SearchMoviesAsync(string query)
        {
            try
            {
                string url = $"{baseUrl}?apikey={apiKey}&s={query}";
                
                // Odeslani pozadavku a kontrola, zda server neodpovedel chybou
                HttpResponseMessage response = await Client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Precteni textu z odpovedi
                string jsonString = await response.Content.ReadAsStringAsync();
                
                // Deserializace textu do objektu tridy SearchResponse
                return JsonSerializer.Deserialize<SearchResponse>(jsonString);
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("[CHYBA] Nepodarilo se pripojit k internetu.");
                return null;
            }
            catch (Exception)
            {
                Console.WriteLine("[CHYBA] Nastala neocekavana chyba pri zpracovani dat.");
                return null;
            }
        }

        // Asynchronni metoda pro ziskani detailu o jednom filmu
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
            catch (Exception)
            {
                Console.WriteLine("[CHYBA] Nepodarilo se stahnout detaily filmu.");
                return null;
            }
        }
    }
}