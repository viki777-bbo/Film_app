namespace Film_app
{
    // Rozhrani pro praci s filmovou databazi
    public interface IMovieService
    {
        Task<SearchResponse> SearchMoviesAsync(string query);
        Task<MovieDetails> GetMovieDetailsAsync(string imdbId);
    }
}