using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class TileDocumentHelper
{
    public static void EnsureLegacyMigrated(PathDocument document)
    {
        if (document.Tiles.Count > 0)
        {
            EnsureSingleAnchor(document);
            EnsureActiveTile(document);
            SyncImagePath(document);
            MigrateFrameDefinedState(document);
            return;
        }

        if (string.IsNullOrWhiteSpace(document.ImagePath))
        {
            return;
        }

        var tile = new FovTile
        {
            Name = "Anchor",
            ImagePath = document.ImagePath,
            IsAnchor = true
        };
        document.Tiles.Add(tile);
        document.ActiveTileId = tile.Id;
        document.SchemaVersion = Math.Max(document.SchemaVersion, 2);
        SyncImagePath(document);
    }

    public static FovTile? GetAnchor(PathDocument document) =>
        document.Tiles.Count > 0 ? document.Tiles[0] : null;

    public static bool IsMultiFovMode(PathDocument document) => document.Tiles.Count > 1;

    public static bool IsSingleFovMode(PathDocument document) => document.Tiles.Count <= 1;

    public static bool CanAddAnotherFovImage(PathDocument document) =>
        document.Tiles.Count == 0 || document.FrameIsDefined;

    public static FovTile? GetActive(PathDocument document)
    {
        if (document.Tiles.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(document.ActiveTileId))
        {
            FovTile? active = document.Tiles.FirstOrDefault(tile => tile.Id == document.ActiveTileId);
            if (active != null)
            {
                return active;
            }
        }

        return GetAnchor(document) ?? document.Tiles[0];
    }

    public static FovTile AddTile(PathDocument document, string imagePath, string? name = null)
    {
        var tile = new FovTile
        {
            Name = name ?? $"Tile {document.Tiles.Count + 1}",
            ImagePath = imagePath
        };

        if (document.Tiles.Count == 0)
        {
            tile.IsAnchor = true;
            tile.Name = "Anchor";
        }

        document.Tiles.Add(tile);
        document.ActiveTileId = tile.Id;
        document.SchemaVersion = Math.Max(document.SchemaVersion, 2);
        EnsureSingleAnchor(document);
        SyncImagePath(document);
        return tile;
    }

    public static void RemoveTile(PathDocument document, string tileId)
    {
        FovTile? tile = document.Tiles.FirstOrDefault(item => item.Id == tileId);
        if (tile == null)
        {
            return;
        }

        document.Tiles.Remove(tile);
        if (document.Tiles.Count == 0)
        {
            document.ActiveTileId = null;
            document.ImagePath = null;
            return;
        }

        EnsureSingleAnchor(document);
        EnsureActiveTile(document);
        SyncImagePath(document);
    }

    public static void SetActive(PathDocument document, string tileId)
    {
        if (document.Tiles.Any(tile => tile.Id == tileId))
        {
            document.ActiveTileId = tileId;
            SyncImagePath(document);
        }
    }

    public static void SyncImagePath(PathDocument document)
    {
        document.ImagePath = GetActive(document)?.ImagePath;
    }

    public static void MigrateFrameDefinedState(PathDocument document)
    {
        if (document.FrameIsDefined)
        {
            return;
        }

        if (document.Segments.Count > 0 ||
            Math.Abs(document.Frame.OriginX_px) > 1e-9 ||
            Math.Abs(document.Frame.OriginY_px) > 1e-9 ||
            Math.Abs(document.Frame.AngleDeg) > 1e-9)
        {
            document.FrameIsDefined = true;
        }
    }

    private static void EnsureSingleAnchor(PathDocument document)
    {
        for (int i = 0; i < document.Tiles.Count; i++)
        {
            FovTile tile = document.Tiles[i];
            tile.IsAnchor = i == 0;
            if (i == 0 && string.IsNullOrWhiteSpace(tile.Name))
            {
                tile.Name = "Anchor";
            }
        }
    }

    private static void EnsureActiveTile(PathDocument document)
    {
        if (document.Tiles.Count == 0)
        {
            document.ActiveTileId = null;
            return;
        }

        if (GetActive(document) == null)
        {
            document.ActiveTileId = document.Tiles[0].Id;
        }
    }
}
