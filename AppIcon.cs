namespace DispensePathAuthor;

internal static class AppIcon
{
    public static Icon Load()
    {
        using Stream? resource = typeof(AppIcon).Assembly.GetManifestResourceStream("DispensePathAuthor.AppIcon.ico");
        if (resource is null)
        {
            throw new InvalidOperationException("Application icon resource is missing.");
        }

        var copy = new MemoryStream();
        resource.CopyTo(copy);
        copy.Position = 0;
        return new Icon(copy);
    }
}
