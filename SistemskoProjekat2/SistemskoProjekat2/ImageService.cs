using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

public static class ImageConverter
{
    public static async Task<byte[]> KonvertujJpgUPngAsync(string putanjaDoSlike)
    {
        if (!File.Exists(putanjaDoSlike))
        {
            throw new FileNotFoundException($"Datoteka '{putanjaDoSlike}' nije pronađena.");
        }

        using (var slika = Image.FromFile(putanjaDoSlike))
        using (var memorijskiTok = new MemoryStream())
        {
            await Task.Run(() => slika.Save(memorijskiTok, ImageFormat.Png));
            return memorijskiTok.ToArray();
        }
    }
}