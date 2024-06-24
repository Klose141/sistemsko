using System;
using System.Reactive.Linq;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        var service = new OpenLibraryService();
        var ui = new ConsoleUI();

        var inputObservable = Observable.Create<string>(observer =>
        {
            while (true)
            {
                Console.WriteLine("\nUnesite ime autora (ili 'exit' za izlaz):");
                var input = Console.ReadLine();
                if (input?.ToLower() == "exit")
                {
                    observer.OnCompleted();
                    break;
                }
                if (!string.IsNullOrWhiteSpace(input))
                {
                    observer.OnNext(input);
                }
            }
            return System.Reactive.Disposables.Disposable.Empty;
        });

        inputObservable
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
