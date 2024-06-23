using System;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reactive;

class Program
{
    static void Main(string[] args)
    {
        var service = new OpenLibraryService();
        var ui = new ConsoleUI();


        Observable.Repeat(Unit.Default)
            .Select(_ =>
            {
                Console.WriteLine("\nUnesite ime autora (ili 'exit' za izlaz):");
                return Console.ReadLine();
            })
            .TakeWhile(input => input?.ToLower() != "exit")
            .Where(input => !string.IsNullOrWhiteSpace(input))
            .Select(author =>
            {
                var stopwatch = Stopwatch.StartNew();
                return new { Author = author, Stopwatch = stopwatch };
            })
            .SelectMany(data => service.SearchAuthor(data.Author)
                .Select(books => new { Books = books, data.Stopwatch }))
            .Subscribe(
                result =>
                {
                    result.Stopwatch.Stop();
                    ui.DisplayResults(result.Books);
                    Console.WriteLine($"Pretraga je trajala: {result.Stopwatch.ElapsedMilliseconds} ms");
                },
                error => Console.WriteLine($"Došlo je do greške: {error.Message}"),
                () => Console.WriteLine("Program je završen.")
            );

        Console.ReadLine();
    }
}