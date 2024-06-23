using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class HttpServer
{
    private readonly string _rootFolder;
    private bool _isRunning;
    private readonly ConcurrentDictionary<string, byte[]> _imageCache = new ConcurrentDictionary<string, byte[]>();

    public HttpServer(string rootFolder)
    {
        _rootFolder = rootFolder;
        _isRunning = false;
    }

    public void Start(int port)
    {
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://localhost:{port}/");
        httpListener.Start();

        Console.WriteLine($"Server sluša na http://localhost:{port}/");
        _isRunning = true;

        Task.Run(async () =>
        {
            while (_isRunning)
            {
                var context = await httpListener.GetContextAsync();
                _ = ObradiZahtevAsync(context);
            }
        });
    }

    public void Stop()
    {
        _isRunning = false;
    }

    private async Task ObradiZahtevAsync(HttpListenerContext context)
    {
        var zahtev = context.Request;
        var odgovor = context.Response; 

        try
        {
            if (zahtev.HttpMethod != "GET")
            {
                odgovor.StatusCode = 405; 
                odgovor.ContentType = "text/plain";
                var message = Encoding.UTF8.GetBytes("Dozvoljene su samo GET zahtevi.");
                await odgovor.OutputStream.WriteAsync(message, 0, message.Length);
                return;
            }

            var nazivFajla = Path.GetFileName(zahtev.Url.LocalPath.TrimStart('/'));
            if (string.IsNullOrEmpty(nazivFajla))
            {
                odgovor.StatusCode = 400; 
                odgovor.ContentType = "text/plain";
                var message = Encoding.UTF8.GetBytes("Naziv fajla nedostaje u zahtevu.");
                await odgovor.OutputStream.WriteAsync(message, 0, message.Length);
                return;
            }

            var putanjaDoSlike = Path.Combine(_rootFolder, nazivFajla);

            if (!_imageCache.TryGetValue(putanjaDoSlike, out var konvertovanaSlika))
            {
                Console.WriteLine($"GET request received for image: {nazivFajla}");
                if (!File.Exists(putanjaDoSlike))
                {
                    odgovor.StatusCode = 404;
                    odgovor.ContentType = "text/plain";
                    var message = Encoding.UTF8.GetBytes($"Datoteka '{nazivFajla}' nije pronađena.");
                    await odgovor.OutputStream.WriteAsync(message, 0, message.Length);
                    return;
                }

                konvertovanaSlika = await ImageConverter.KonvertujJpgUPngAsync(putanjaDoSlike);
                _imageCache[putanjaDoSlike] = konvertovanaSlika;
            }
            
            
            odgovor.StatusCode = 200;
            odgovor.ContentType = "image/png";
            odgovor.ContentLength64 = konvertovanaSlika.Length;
            await odgovor.OutputStream.WriteAsync(konvertovanaSlika, 0, konvertovanaSlika.Length);
        }
        catch (Exception ex)
        {
            odgovor.StatusCode = 500; 
            odgovor.ContentType = "text/plain";
            var message = Encoding.UTF8.GetBytes($"Greška: {ex.Message}");
            await odgovor.OutputStream.WriteAsync(message, 0, message.Length);
        }
        finally
        {
            odgovor.Close();
        }
    }
}
