using System;

class MainClass
{
    public static void Main(string[] args)
    {
        var rootFolder = Directory.GetCurrentDirectory();
        var server = new HttpServer(rootFolder);
        server.Start(5050);

        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        server.Stop();
    }
}