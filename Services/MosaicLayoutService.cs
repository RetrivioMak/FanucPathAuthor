using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public sealed class MosaicTilePlacement
{
    public required FovTile Tile { get; init; }
    public required Image Image { get; init; }
    public double CanvasOriginX { get; init; }
    public double CanvasOriginY { get; init; }
}

public sealed class MosaicLayout
{
    public required IReadOnlyList<MosaicTilePlacement> Tiles { get; init; }
    public RectangleF Bounds { get; init; }
    public bool IsMosaic => Tiles.Count > 1;
}

public static class MosaicLayoutService
{
    private const float BoundsPaddingPx = 40f;

    public static MosaicLayout? TryBuild(PathDocument document, RobotCalibrationFile calibration)
    {
        if (calibration.PixelToRobot == null || document.Tiles.Count == 0)
        {
            return null;
        }

        FovTile? anchorTile = TileDocumentHelper.GetAnchor(document);
        if (anchorTile == null)
        {
            return null;
        }

        var placements = new List<MosaicTilePlacement>();
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (FovTile tile in document.Tiles)
        {
            if (string.IsNullOrWhiteSpace(tile.ImagePath) || !File.Exists(tile.ImagePath))
            {
                continue;
            }

            Image image = Image.FromFile(tile.ImagePath);
            (double originX, double originY) = TileTransformService.GetTileCanvasOffset(tile, anchorTile, calibration);

            float left = (float)originX;
            float top = (float)originY;
            float right = left + image.Width;
            float bottom = top + image.Height;
            minX = Math.Min(minX, left);
            minY = Math.Min(minY, top);
            maxX = Math.Max(maxX, right);
            maxY = Math.Max(maxY, bottom);

            placements.Add(new MosaicTilePlacement
            {
                Tile = tile,
                Image = image,
                CanvasOriginX = originX,
                CanvasOriginY = originY
            });
        }

        if (placements.Count == 0)
        {
            foreach (MosaicTilePlacement placement in placements)
            {
                placement.Image.Dispose();
            }

            return null;
        }

        var bounds = RectangleF.FromLTRB(
            minX - BoundsPaddingPx,
            minY - BoundsPaddingPx,
            maxX + BoundsPaddingPx,
            maxY + BoundsPaddingPx);

        return new MosaicLayout
        {
            Tiles = placements,
            Bounds = bounds
        };
    }
}
