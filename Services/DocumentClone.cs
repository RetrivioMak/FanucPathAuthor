using FanucPathAuthor.Models;

namespace FanucPathAuthor.Services;

public static class DocumentClone
{
    public static PathDocument Clone(PathDocument source) =>
        new()
        {
            SchemaVersion = source.SchemaVersion,
            ImagePath = source.ImagePath,
            CalibrationPath = source.CalibrationPath,
            ActiveTileId = source.ActiveTileId,
            Frame = source.Frame.Clone(),
            FrameIsDefined = source.FrameIsDefined,
            Defaults = ProcessDefaultsCopy.Clone(source.Defaults),
            Tiles = source.Tiles.Select(CloneTile).ToList(),
            Segments = source.Segments.Select(CloneSegment).ToList()
        };

    public static FovTile CloneTile(FovTile tile) =>
        new()
        {
            Id = tile.Id,
            Name = tile.Name,
            ImagePath = tile.ImagePath,
            RobotX_mm = tile.RobotX_mm,
            RobotY_mm = tile.RobotY_mm,
            RobotR_deg = tile.RobotR_deg,
            IsAnchor = tile.IsAnchor
        };

    public static PathSegment CloneSegment(PathSegment segment) =>
        new()
        {
            Kind = segment.Kind,
            GlueOn = segment.GlueOn,
            Speed_mm_s = segment.Speed_mm_s,
            X1 = segment.X1,
            Y1 = segment.Y1,
            X2 = segment.X2,
            Y2 = segment.Y2,
            X3 = segment.X3,
            Y3 = segment.Y3
        };
}

public sealed class UndoRedoStack<T> where T : class
{
    private readonly Stack<T> _undo = new();
    private readonly Stack<T> _redo = new();
    private readonly Func<T, T> _clone;

    public UndoRedoStack(Func<T, T> clone) => _clone = clone;

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void PushSnapshot(T current)
    {
        _undo.Push(_clone(current));
        _redo.Clear();
    }

    public T? Undo(T current)
    {
        if (!CanUndo)
        {
            return null;
        }

        _redo.Push(_clone(current));
        return _undo.Pop();
    }

    public T? Redo(T current)
    {
        if (!CanRedo)
        {
            return null;
        }

        _undo.Push(_clone(current));
        return _redo.Pop();
    }

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }
}
