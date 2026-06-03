
{
    static async Task Main()
    {
        var client = new HttpClient();
        Console.Write("Zadej film: ");
        string input = Console.ReadLine();
        string url = $"https://www.omdbapi.com/?apikey=f5b1565b&s={input}";
        var response = await client.GetAsync(url);
        string data = await response.Content.ReadAsStringAsync();
        Console.WriteLine(data);
        
        
    }
}

class Program
{
    
}
    
class SearchResponse
{
    public List<Movie> Search { get; set; }
}

class Movie
{
    public string Title { get; set; }
    public string Year { get; set; }
}