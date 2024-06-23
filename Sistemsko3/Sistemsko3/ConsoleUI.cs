public class ConsoleUI
{
    public void DisplayResults(List<Book> books)
    {
        if (books.Count == 0)
        {
            Console.WriteLine("Nema pronađenih knjiga za ovog autora.");
            return;
        }

        foreach (var book in books)
        {
            Console.WriteLine($"Naslov: {book.Title}");
            Console.WriteLine($"Godina izdavanja: {book.Year}");
            Console.WriteLine($"Jezici: {string.Join(", ", book.Languages)}");
            if (book.RatingCount > 0)
            {
                Console.WriteLine($"Prosečan rejting: {book.AverageRating:F2} (na osnovu {book.RatingCount} ocena)");
            }
            else
            {
                Console.WriteLine("Prosečan rejting: Nije dostupno");
            }
            Console.WriteLine();
        }
    }
}