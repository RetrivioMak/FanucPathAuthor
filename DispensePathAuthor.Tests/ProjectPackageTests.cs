using System.IO.Compression;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using DispensePathAuthor.Models;
using DispensePathAuthor.Services;
using Xunit;

namespace DispensePathAuthor.Tests;

public class ProjectPackageTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dpath-{Guid.NewGuid():N}");
    private readonly List<string> _caches = new();

    public ProjectPackageTests() => Directory.CreateDirectory(_directory);

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        foreach (string cache in _caches)
        {
            if (Directory.Exists(cache))
            {
                Directory.Delete(cache, recursive: true);
            }
        }
    }

    [Fact]
    public void LoadFile_LooseJson_OpensWithoutAPackageCache()
    {
        string path = Path.Combine(_directory, "legacy.json");
        File.WriteAllText(path, """
            {
              "schemaVersion": 3,
              "segments": []
            }
            """);

        LoadedProject loaded = ProjectPackage.LoadFile(path);

        Assert.Null(loaded.CacheDirectory);
        Assert.Equal(WorkspaceKind.Cv, loaded.Document.Workspace);
        Assert.Equal(4, loaded.Document.SchemaVersion);
    }

    [Fact]
    public void Save_TeachWithoutImages_KeepsSchemaAndOmitsMedia()
    {
        var document = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            FrameIsDefined = true
        };
        document.ReferencePoints.Add(new ReferencePoint { Name = "Left", X_mm = 12.5, Y_mm = 4 });
        string packagePath = Path.Combine(_directory, "teach.dpath");

        SavedPackage saved = ProjectPackage.Save(document, packagePath, cacheDirectory: null);
        Remember(saved.CacheDirectory);

        Assert.Equal(4, saved.Document.SchemaVersion);
        Assert.Equal(new[] { ProjectPackage.ProjectEntryName }, EntryNames(packagePath));

        LoadedProject loaded = ProjectPackage.Load(packagePath);
        Remember(loaded.CacheDirectory);
        Assert.Equal("Left", loaded.Document.ReferencePoints[0].Name);
        Assert.Equal(12.5, loaded.Document.ReferencePoints[0].X_mm, 3);
    }

    [Fact]
    public void Save_CopiesPngAndCalibrationIntoThePackage()
    {
        string imagePath = Path.Combine(_directory, "anchor.png");
        using (var bitmap = new Bitmap(2, 2))
        {
            bitmap.SetPixel(0, 0, Color.Red);
            bitmap.Save(imagePath, ImageFormat.Png);
        }

        string calibrationPath = Path.Combine(_directory, "RobotCvTransformCalibration.json");
        File.WriteAllText(calibrationPath, """{"probe":true}""");
        byte[] png = File.ReadAllBytes(imagePath);

        var document = new PathDocument { Workspace = WorkspaceKind.Cv };
        document.Tiles.Add(new FovTile
        {
            Name = "Anchor",
            ImagePath = imagePath,
            IsAnchor = true
        });
        document.ActiveTileId = document.Tiles[0].Id;
        document.CalibrationPath = calibrationPath;
        string packagePath = Path.Combine(_directory, "cv.dpath");

        SavedPackage saved = ProjectPackage.Save(document, packagePath, cacheDirectory: null);
        Remember(saved.CacheDirectory);

        string mediaKey = ProjectPackage.MediaKey(document.Tiles[0].Id);
        Assert.Equal(ProjectPackage.MediaSchemaVersion, saved.Document.SchemaVersion);
        Assert.Equal(mediaKey, saved.Document.Tiles[0].ImagePath);
        Assert.Equal(ProjectPackage.CalibrationEntryName, saved.Document.CalibrationPath);
        Assert.Equal(
            new[] { ProjectPackage.CalibrationEntryName, mediaKey, ProjectPackage.ProjectEntryName }.OrderBy(static name => name, StringComparer.Ordinal),
            EntryNames(packagePath));
        Assert.Equal(png, EntryBytes(packagePath, mediaKey));
        Assert.Equal(File.ReadAllBytes(calibrationPath), EntryBytes(packagePath, ProjectPackage.CalibrationEntryName));
        Assert.DoesNotContain(_directory, Encoding.UTF8.GetString(EntryBytes(packagePath, ProjectPackage.ProjectEntryName)), StringComparison.OrdinalIgnoreCase);

        LoadedProject loaded = ProjectPackage.Load(packagePath);
        Remember(loaded.CacheDirectory);
        Assert.Equal(mediaKey, loaded.Document.Tiles[0].ImagePath);
        string? resolved = ProjectPackage.Resolve(loaded.CacheDirectory, loaded.Document.Tiles[0].ImagePath);
        Assert.NotNull(resolved);
        Assert.Equal(png, File.ReadAllBytes(resolved));
    }

    [Fact]
    public void Save_BmpImage_IsStoredAsPng()
    {
        string imagePath = Path.Combine(_directory, "anchor.bmp");
        using (var bitmap = new Bitmap(2, 2))
        {
            bitmap.Save(imagePath, ImageFormat.Bmp);
        }

        var document = new PathDocument { Workspace = WorkspaceKind.Cv };
        document.Tiles.Add(new FovTile { Name = "Anchor", ImagePath = imagePath, IsAnchor = true });
        string packagePath = Path.Combine(_directory, "bmp.dpath");

        SavedPackage saved = ProjectPackage.Save(document, packagePath, cacheDirectory: null);
        Remember(saved.CacheDirectory);

        byte[] stored = EntryBytes(packagePath, saved.Document.Tiles[0].ImagePath!);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, stored[..4]);
    }

    [Fact]
    public void Save_MissingImage_DoesNotReplaceAnExistingPackage()
    {
        string packagePath = Path.Combine(_directory, "keep.dpath");
        var teach = new PathDocument
        {
            Workspace = WorkspaceKind.Teach,
            FrameIsDefined = true
        };
        teach.ReferencePoints.Add(new ReferencePoint { Name = "A", X_mm = 1, Y_mm = 2 });
        SavedPackage saved = ProjectPackage.Save(teach, packagePath, cacheDirectory: null);
        Remember(saved.CacheDirectory);

        var broken = new PathDocument { Workspace = WorkspaceKind.Cv };
        broken.Tiles.Add(new FovTile
        {
            Name = "Anchor",
            ImagePath = Path.Combine(_directory, "missing.png"),
            IsAnchor = true
        });

        Assert.Throws<InvalidOperationException>(() => ProjectPackage.Save(broken, packagePath, cacheDirectory: null));

        LoadedProject loaded = ProjectPackage.Load(packagePath);
        Remember(loaded.CacheDirectory);
        Assert.Equal("A", loaded.Document.ReferencePoints[0].Name);
    }

    private void Remember(string? cacheDirectory)
    {
        if (!string.IsNullOrWhiteSpace(cacheDirectory))
        {
            _caches.Add(cacheDirectory);
        }
    }

    private static string[] EntryNames(string packagePath)
    {
        using var stream = File.OpenRead(packagePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        return archive.Entries.Select(static entry => entry.FullName.Replace('\\', '/')).OrderBy(static name => name, StringComparer.Ordinal).ToArray();
    }

    private static byte[] EntryBytes(string packagePath, string name)
    {
        using var stream = File.OpenRead(packagePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        ZipArchiveEntry? entry = archive.GetEntry(name);
        Assert.NotNull(entry);
        using var entryStream = entry.Open();
        using var copy = new MemoryStream();
        entryStream.CopyTo(copy);
        return copy.ToArray();
    }
}
