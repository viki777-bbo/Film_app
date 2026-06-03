using System.Text.Json.Serialization;

namespace MovieApp
{
    // Trida reprezentujici jeden film v seznamu vysledku
    public class Movie
    {
        [JsonPropertyName("Title")]
        public string Title { get; set; }

        [JsonPropertyName("Year")]
        public string Year { get; set; }

        [JsonPropertyName("imdbID")]
        public string ImdbId { get; set; }
    }

    // Trida pro zpracovani celkove odpovedi vyhledavani z API
    public class SearchResponse
    {
        [JsonPropertyName("Search")]
        public List<Movie> Search { get; set; }

        [JsonPropertyName("Response")]
        public string Response { get; set; }

        [JsonPropertyName("Error")]
        public string Error { get; set; }
    }

    // Trida pro detailni informace o vybranem filmu
    public class MovieDetails
    {
        [JsonPropertyName("Title")]
        public string Title { get; set; }

        [JsonPropertyName("Year")]
        public string Year { get; set; }

        [JsonPropertyName("Genre")]
        public string Genre { get; set; }

        [JsonPropertyName("Director")]
        public string Director { get; set; }

        [JsonPropertyName("Plot")]
        public string Plot { get; set; }

        [JsonPropertyName("Response")]
        public string Response { get; set; }
    }
}