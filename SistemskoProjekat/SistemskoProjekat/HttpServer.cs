using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

public class HttpServer
{
    private readonly string _rootFolder;
    private bool _isRunning;
    private readonly object fileLock = new object();

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

        while (_isRunning)
        {
            var context = httpListener.GetContext();
            ThreadPool.QueueUserWorkItem(new WaitCallback(ObradiZahtev), context);
        }
    }

    public void Stop()
    {
        _isRunning = false;
    }

    private void ObradiZahtev(object state)
    {
        var context = (HttpListenerContext)state;
        var zahtev = context.Request;
        var odgovor = context.Response;

        try
        {
            if (zahtev.HttpMethod != "GET")
            {
                odgovor.StatusCode = 405; // Metoda nije dozvoljena
                odgovor.ContentType = "text/plain";
                odgovor.OutputStream.Write(Encoding.UTF8.GetBytes("Dozvoljene su samo GET zahtevi."));
                return;
            }

            var nazivFajla = Path.GetFileName(zahtev.Url.LocalPath.TrimStart('/'));
            if (string.IsNullOrEmpty(nazivFajla))
            {
                odgovor.StatusCode = 400; // Loš zahtev
                odgovor.ContentType = "text/plain";
                odgovor.OutputStream.Write(Encoding.UTF8.GetBytes("Naziv fajla nedostaje u zahtevu."));
                return;
            }

            var putanjaDoSlike = Path.Combine(_rootFolder, nazivFajla);

            lock (fileLock)
            {
                Console.WriteLine($"GET request received for image: {nazivFajla}");
                if (!File.Exists(putanjaDoSlike))
                {
                    odgovor.StatusCode = 404; // Nije pronađeno
                    odgovor.ContentType = "text/plain";
                    odgovor.OutputStream.Write(Encoding.UTF8.GetBytes($"Datoteka '{nazivFajla}' nije pronađena."));
                    return;
                }
            }
            var konvertovanaSlika = ImageConverter.KonvertujJpgUPng(putanjaDoSlike);

            // Snimi konvertovanu sliku na disk
            var putanjaDoKonvertovaneSlike = Path.ChangeExtension(putanjaDoSlike, "png");
            File.WriteAllBytes(putanjaDoKonvertovaneSlike, konvertovanaSlika);

            odgovor.StatusCode = 200; // OK
            odgovor.ContentType = "image/png";
            odgovor.ContentLength64 = konvertovanaSlika.Length;
            odgovor.OutputStream.Write(konvertovanaSlika, 0, konvertovanaSlika.Length);
        }
        catch (Exception ex)
        {
            odgovor.StatusCode = 500; // Interna greška servera
            odgovor.ContentType = "text/plain";
            odgovor.OutputStream.Write(Encoding.UTF8.GetBytes($"Greška: {ex.Message}"));
        }
        finally
        {
            odgovor.Close();
        }
    }
}