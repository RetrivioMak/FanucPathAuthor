namespace DispensePathAuthor.Models;

/// <summary>
/// Fixed path-object type.
/// Dot stores one point, Line stores start and end, Arc stores start, mid, and end.
/// </summary>
public enum SegmentKind
{
    Dot,
    Line,
    Arc
}

public enum WorkspaceKind
{
    Unset,
    Cv,
    Teach
}

public enum LengthUnit
{
    Micrometer,
    Millimeter,
    Centimeter,
    Meter
}

public sealed class FrameDefinition
{
    public double OriginX_px { get; set; }
    public double OriginY_px { get; set; }
    public double AngleDeg { get; set; }

    /// <summary>
    /// Open eVision +Y points down the image. When set, +Y is counterclockwise from +X
    /// so the frame matches a right-handed Fanuc user frame.
    /// </summary>
    public bool InvertY { get; set; }

    public FrameDefinition Clone() =>
        new()
        {
            OriginX_px = OriginX_px,
            OriginY_px = OriginY_px,
            AngleDeg = AngleDeg,
            InvertY = InvertY
        };
}

public enum ExportCoordinateMode
{
    /// <summary>Absolute UFRAME / UTOOL mm after FOV and needle corrections.</summary>
    UframeAbsolute,

    /// <summary>Robot mm delta from the taught Frame origin (local 0,0).</summary>
    OffsetFromFrameOrigin,

    /// <summary>Robot mm delta from calibration ReferenceRobot (recommended for dispense).</summary>
    OffsetFromCalibrationRef
}

public enum ExportLocalRotation
{
    None,
    Degree90,
    Degree180
}

public sealed class ProcessDefaults
{
    public const double DefaultMinWidthMm = 0.1;
    public const double DefaultReferenceMmPerPx = 0.1;

    public static readonly double[] DefaultWidthsMm = { 0.2, 0.3, 0.5, 0.8, 1, 1.5, 2, 3, 5 };

    public static readonly double[] DefaultZDeltasMm = { -1, -0.5, 0, 0.5, 1 };

    public double Z_mm { get; set; } = 0;
    public double W_deg { get; set; } = 0;
    public double P_deg { get; set; } = 0;
    public double R_deg { get; set; } = 0;
    public bool GlueOn { get; set; } = true;

    /// <summary>Smallest stroke offered for new lines and arcs.</summary>
    public double MinWidth_mm { get; set; } = DefaultMinWidthMm;

    /// <summary>CV image scale used until a calibration file is loaded. 0.1 means 1 px = 0.1 mm.</summary>
    public double ReferenceMmPerPx { get; set; } = DefaultReferenceMmPerPx;

    public List<double> Widths_mm { get; set; } = new(DefaultWidthsMm);

    /// <summary>Segment Z offsets offered in the Z lists. Zero is always kept.</summary>
    public List<double> ZDeltas_mm { get; set; } = new(DefaultZDeltasMm);
}

public sealed class PathSegment
{
    public SegmentKind Kind { get; set; }
    public bool GlueOn { get; set; } = true;

    /// <summary>Bead width in millimeters. Dots do not use it. Zero means the project minimum.</summary>
    public double Width_mm { get; set; }

    /// <summary>Offset added to project Z. Zero exports the project Z unchanged.</summary>
    public double ZDelta_mm { get; set; }

    // Teach, and CV from schema 6: millimeters along the project frame.
    // Older CV documents: frame-local pixels, migrated when calibration is known.
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
    public double X3 { get; set; }
    public double Y3 { get; set; }

    public string DisplayName =>
        Kind switch
        {
            SegmentKind.Dot => $"Dot ({X1:0.###}, {Y1:0.###})",
            SegmentKind.Line => $"Line ({X1:0.###},{Y1:0.###})->({X2:0.###},{Y2:0.###})",
            SegmentKind.Arc => $"Arc ({X1:0.###},{Y1:0.###})->({X3:0.###},{Y3:0.###})",
            _ => Kind.ToString()
        };
}

public sealed class ReferencePoint
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public double X_mm { get; set; }
    public double Y_mm { get; set; }

    public ReferencePoint Clone() =>
        new()
        {
            Id = Id,
            Name = Name,
            X_mm = X_mm,
            Y_mm = Y_mm
        };
}

public sealed class TeachGridSettings
{
    public static readonly double[] DefaultStepsMm = { 0.1, 0.2, 0.5, 1, 2, 5, 10 };

    /// <summary>Draw the grid and allow snap to land on its intersections.</summary>
    public bool Enabled { get; set; } = true;
    public double Step_mm { get; set; } = 0.2;
    public List<double> Steps_mm { get; set; } = new(DefaultStepsMm);
    public double OriginX_mm { get; set; }
    public double OriginY_mm { get; set; }
    public bool IsAligned { get; set; }

    public TeachGridSettings Clone() =>
        new()
        {
            Enabled = Enabled,
            Step_mm = Step_mm,
            Steps_mm = new List<double>(Steps_mm),
            OriginX_mm = OriginX_mm,
            OriginY_mm = OriginY_mm,
            IsAligned = IsAligned
        };
}

public sealed class PathDocument
{
    public int SchemaVersion { get; set; } = 4;
    public WorkspaceKind Workspace { get; set; } = WorkspaceKind.Unset;
    public string? ImagePath { get; set; }
    public string? CalibrationPath { get; set; }
    public string? ActiveTileId { get; set; }
    public FrameDefinition Frame { get; set; } = new();
    public bool FrameIsDefined { get; set; }
    public ProcessDefaults Defaults { get; set; } = new();
    public ExporterSettings Exporters { get; set; } = new();
    public TeachGridSettings Grid { get; set; } = new();
    public List<ReferencePoint> ReferencePoints { get; set; } = new();
    public List<FovTile> Tiles { get; set; } = new();
    public List<PathSegment> Segments { get; set; } = new();
}

public sealed class FovTile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Tile 1";
    public string? ImagePath { get; set; }
    public double RobotX_mm { get; set; }
    public double RobotY_mm { get; set; }
    public double RobotR_deg { get; set; }
    public bool IsAnchor { get; set; }
}
