using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

public class OpenLibraryService
{
    private readonly HttpClient _client = new HttpClient();

    public IObservable<List<Book>> SearchAuthor(string author)
    {
        return Observable.FromAsync(() => SearchAuthorAsync(author));
    }

    private async Task<List<Book>> SearchAuthorAsync(string author)
    {
        var searchUrl = $"https://openlibrary.org/search.json?author={Uri.EscapeDataString(author)}";
        var searchResponse = await _client.GetStringAsync(searchUrl);
        var searchJson = JObject.Parse(searchResponse);

        var books = new List<Book>();
        var docs = searchJson["docs"].Take(10);

        var tasks = docs.Select(async doc =>
        {
            var book = new Book
            {
                Title = doc["title"]?.ToString(),
                Year = doc["first_publish_year"]?.ToObject<int>() ?? 0,
                Languages = doc["language"]?.ToObject<List<string>>() ?? new List<string>(),
                AverageRating = 0,
                RatingCount = 0
            };

            var worksKey = doc["key"]?.ToString();
            if (!string.IsNullOrEmpty(worksKey))
            {
                await FetchBookRatings(book, worksKey);
            }

            return book;
        }).ToList();

        books = (await Task.WhenAll(tasks)).ToList();
        return books;
    }

    private async Task FetchBookRatings(Book book, string worksKey)
    {
        try
        {
            var ratingsUrl = $"https://openlibrary.org{worksKey}/ratings.json";
            var ratingsResponse = await _client.GetStringAsync(ratingsUrl);
            var ratingsJson = JObject.Parse(ratingsResponse);

            book.AverageRating = ratingsJson["summary"]?["average"]?.ToObject<double>() ?? 0;
            book.RatingCount = ratingsJson["summary"]?["count"]?.ToObject<int>() ?? 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Greška pri dobijanju rejtinga za knjigu: {ex.Message}");
        }
    }
}