using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using DispensePathAuthor.Models;

namespace DispensePathAuthor.Services;

public readonly record struct LoadedProject(PathDocument Document, string? CacheDirectory);

public readonly record struct SavedPackage(PathDocument Document, string CacheDirectory);

public static class ProjectPackage
{
    public const string Extension = ".dpath";
    public const string ProjectEntryName = "project.json";
    public const string CalibrationEntryName = "calibration.json";
    public const int MediaSchemaVersion = 5;
    public const string OpenFilter = "Dispense Path (*.dpath)|*.dpath|Path JSON (*.json)|*.json|All files (*.*)|*.*";
    public const string SaveFilter = "Dispense Path (*.dpath)|*.dpath";

    public static bool HasPackageExtension(string path) =>
        string.Equals(Path.GetExtension(path), Extension, StringComparison.OrdinalIgnoreCase);

    public static string MediaKey(string tileId) => $"media/{tileId}.png";

    public static LoadedProject LoadFile(string path)
    {
        if (HasPackageExtension(path))
        {
            return Load(path);
        }

        return new LoadedProject(PathSerializer.Load(path), null);
    }

    public static LoadedProject Load(string packagePath)
    {
        string cacheDirectory = CacheDirectoryFor(packagePath);
        using var archiveStream = File.Open(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
        ZipArchiveEntry? projectEntry = archive.GetEntry(ProjectEntryName);
        if (projectEntry == null)
        {
            throw new InvalidDataException("This project file has no project.json.");
        }

        string json;
        using (var reader = new StreamReader(projectEntry.Open(), Encoding.UTF8))
        {
            json = reader.ReadToEnd();
        }

        PathDocument document = PathSerializer.LoadFromJson(json);
        PublishCache(cacheDirectory, archive);
        return new LoadedProject(document, cacheDirectory);
    }

    public static SavedPackage Save(PathDocument document, string packagePath, string? cacheDirectory)
    {
        PathDocument packaged = DocumentClone.Clone(document);
        var media = new List<(string Key, byte[] Bytes)>();
        foreach (FovTile tile in packaged.Tiles)
        {
            if (string.IsNullOrWhiteSpace(tile.ImagePath))
            {
                continue;
            }

            string? source = Resolve(cacheDirectory, tile.ImagePath);
            if (source == null)
            {
                throw new InvalidOperationException($"Could not read the image for {tile.Name}: {tile.ImagePath}");
            }

            media.Add((MediaKey(tile.Id), ReadPngBytes(source)));
            tile.ImagePath = MediaKey(tile.Id);
        }

        TileDocumentHelper.SyncImagePath(packaged);

        byte[]? calibration = null;
        if (!string.IsNullOrWhiteSpace(packaged.CalibrationPath))
        {
            string? source = Resolve(cacheDirectory, packaged.CalibrationPath);
            if (source == null)
            {
                throw new InvalidOperationException($"Could not read calibration: {packaged.CalibrationPath}");
            }

            calibration = File.ReadAllBytes(source);
            packaged.CalibrationPath = CalibrationEntryName;
        }

        if (media.Count > 0 || calibration != null)
        {
            packaged.SchemaVersion = Math.Max(packaged.SchemaVersion, MediaSchemaVersion);
        }

        string? directory = Path.GetDirectoryName(Path.GetFullPath(packagePath));
        if (string.IsNullOrEmpty(directory))
        {
            throw new InvalidOperationException("Choose a folder for the project.");
        }

        Directory.CreateDirectory(directory);
        string temporary = Path.Combine(
            directory,
            "." + Path.GetFileName(packagePath) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            WriteArchive(temporary, packaged, media, calibration);
            if (File.Exists(packagePath))
            {
                File.Replace(temporary, packagePath, destinationBackupFileName: null);
            }
            else
            {
                File.Move(temporary, packagePath);
            }
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }

        string savedCache = CacheDirectoryFor(packagePath);
        using (var written = File.Open(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var archive = new ZipArchive(written, ZipArchiveMode.Read))
        {
            PublishCache(savedCache, archive);
        }

        return new SavedPackage(packaged, savedCache);
    }

    public static string? Resolve(string? cacheDirectory, string? storedPath)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return null;
        }

        if (Path.IsPathRooted(storedPath))
        {
            return File.Exists(storedPath) ? storedPath : null;
        }

        if (File.Exists(storedPath))
        {
            return Path.GetFullPath(storedPath);
        }

        if (string.IsNullOrWhiteSpace(cacheDirectory))
        {
            return null;
        }

        string root = Path.GetFullPath(cacheDirectory);
        string relative = storedPath.Replace('\\', '/').Replace('/', Path.DirectorySeparatorChar);
        string combined = Path.GetFullPath(Path.Combine(root, relative));
        string rootPrefix = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;
        if (!combined.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return File.Exists(combined) ? combined : null;
    }

    private static void WriteArchive(
        string temporaryPath,
        PathDocument document,
        IReadOnlyList<(string Key, byte[] Bytes)> media,
        byte[]? calibration)
    {
        using var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        WriteEntry(archive, ProjectEntryName, Encoding.UTF8.GetBytes(PathSerializer.ToJson(document)));
        if (calibration != null)
        {
            WriteEntry(archive, CalibrationEntryName, calibration);
        }

        foreach ((string key, byte[] bytes) in media.OrderBy(static item => item.Key, StringComparer.Ordinal))
        {
            WriteEntry(archive, key, bytes);
        }
    }

    private static void WriteEntry(ZipArchive archive, string name, byte[] bytes)
    {
        ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using Stream entryStream = entry.Open();
        entryStream.Write(bytes);
    }

    private static byte[] ReadPngBytes(string sourcePath)
    {
        byte[] bytes = File.ReadAllBytes(sourcePath);
        if (bytes.Length >= 8 &&
            bytes[0] == 0x89 &&
            bytes[1] == 0x50 &&
            bytes[2] == 0x4E &&
            bytes[3] == 0x47)
        {
            return bytes;
        }

        using Image image = ImageFileLoader.Open(sourcePath);
        using var png = new MemoryStream();
        image.Save(png, System.Drawing.Imaging.ImageFormat.Png);
        return png.ToArray();
    }

    private static void PublishCache(string cacheDirectory, ZipArchive archive)
    {
        string staging = cacheDirectory + "-staging";
        if (Directory.Exists(staging))
        {
            Directory.Delete(staging, recursive: true);
        }

        Directory.CreateDirectory(staging);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string name = entry.FullName.Replace('\\', '/');
            if (!IsStoredFile(name))
            {
                continue;
            }

            string destination = Path.Combine(
                staging,
                name.Replace('/', Path.DirectorySeparatorChar));
            string? parent = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(parent))
            {
                Directory.CreateDirectory(parent);
            }

            entry.ExtractToFile(destination, overwrite: true);
        }

        if (Directory.Exists(cacheDirectory))
        {
            Directory.Delete(cacheDirectory, recursive: true);
        }

        Directory.Move(staging, cacheDirectory);
    }

    private static bool IsStoredFile(string entryName) =>
        entryName == CalibrationEntryName ||
        (entryName.StartsWith("media/", StringComparison.Ordinal) &&
         !entryName.EndsWith("/", StringComparison.Ordinal) &&
         !entryName["media/".Length..].Contains('/') &&
         !entryName.Contains("..", StringComparison.Ordinal));

    private static string CacheDirectoryFor(string packagePath)
    {
        string fullPath = Path.GetFullPath(packagePath).ToUpperInvariant();
        string name = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fullPath)))[..16];
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(root))
        {
            root = Path.GetTempPath();
        }

        return Path.Combine(root, "DispensePathAuthor", "packages", name);
    }
}
