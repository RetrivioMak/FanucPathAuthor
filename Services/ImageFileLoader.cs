namespace DispensePathAuthor.Services;

public static class ImageFileLoader
{
    public static Image Open(string path)
    {
        using var stream = new MemoryStream(File.ReadAllBytes(path));
        using Image source = Image.FromStream(stream);
        return new Bitmap(source);
    }
}
