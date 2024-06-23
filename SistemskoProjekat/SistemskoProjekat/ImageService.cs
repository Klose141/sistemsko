using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public static class ImageConverter
{
    public static byte[] KonvertujJpgUPng(string putanjaDoSlike)
    {
        if (!File.Exists(putanjaDoSlike))
        {
            throw new FileNotFoundException($"Datoteka '{putanjaDoSlike}' nije pronađena.");
        }

        using (var slika = Image.FromFile(putanjaDoSlike))
        using (var memorijskiTok = new MemoryStream())
        {
            slika.Save(memorijskiTok, ImageFormat.Png);
            return memorijskiTok.ToArray();
        }
    }
}