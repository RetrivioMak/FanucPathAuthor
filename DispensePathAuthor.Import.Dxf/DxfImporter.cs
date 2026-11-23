using DispensePathAuthor.Contracts;
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Units;
using DxfPoint = netDxf.Entities.Point;

namespace DispensePathAuthor.Import.Dxf;

/// <summary>
/// The only project that references the DXF library.
/// </summary>
public sealed class DxfImporter : IGeometryImporter
{
    public const string FileFilter = "DXF drawing (*.dxf)|*.dxf|All files (*.*)|*.*";

    public string Id => "dxf";

    public string MenuLabel => "DXF";

    public GeometryImportResult? Prompt(IWin32Window owner)
    {
        using var dialog = new DxfImportDialog(this);
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.Result : null;
    }

    public GeometryImportSurvey Survey(string sourcePath)
    {
        DxfDocument document = Read(sourcePath);
        var layers = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        int modelEntities = 0;
        foreach (EntityObject entity in ModelEntities(document))
        {
            modelEntities++;
            layers.Add(LayerName(entity));
        }

        (DrawingLengthUnit unit, string? unitNote) = DescribeUnits(document.DrawingVariables.InsUnits);
        string? note = modelEntities == 0
            ? "This drawing has no model-space entities."
            : unitNote;
        return new GeometryImportSurvey
        {
            Layers = layers.ToArray(),
            SuggestedUnit = unit,
            Note = note
        };
    }

    public GeometryImportResult Import(string sourcePath, GeometryImportOptions options)
    {
        DxfDocument document = Read(sourcePath);
        double scale = DrawingScale.ToMillimeters(options.Unit);
        var segments = new List<ImportedSegment>();
        var skipped = new Dictionary<string, int>(StringComparer.Ordinal);
        int tilted = 0;
        int smoothed = 0;
        int degenerate = 0;
        int paper = document.Layouts.Sum(layout => layout.IsPaperSpace ? layout.AssociatedBlock.Entities.Count() : 0);

        foreach (EntityObject entity in ModelEntities(document))
        {
            if (!options.Layers.Any(layer => string.Equals(layer, LayerName(entity), StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            switch (entity)
            {
                case Line line:
                    AddLine(line, scale, options, segments, ref tilted, ref degenerate);
                    break;
                case Arc arc:
                    AddArc(arc, scale, options, segments, ref tilted, ref degenerate);
                    break;
                case Circle circle:
                    AddCircle(circle, scale, options, segments, ref tilted, ref degenerate);
                    break;
                case DxfPoint point:
                    AddPoint(point, scale, options, segments, ref tilted);
                    break;
                case Polyline2D polyline:
                    AddPolyline(polyline, scale, options, segments, ref tilted, ref smoothed, ref degenerate);
                    break;
                default:
                    string label = SkipLabel(entity.Type);
                    skipped[label] = skipped.GetValueOrDefault(label) + 1;
                    break;
            }
        }

        return new GeometryImportResult
        {
            Segments = segments,
            Warnings = DescribeWarnings(skipped, tilted, smoothed, degenerate, paper)
        };
    }

    private static void AddLine(
        Line line,
        double scale,
        GeometryImportOptions options,
        List<ImportedSegment> segments,
        ref int tilted,
        ref int degenerate)
    {
        if (!IsHorizontal(line.Normal) || Math.Abs(line.StartPoint.Z - line.EndPoint.Z) > 1e-4)
        {
            tilted++;
            return;
        }

        (double x1, double y1) = Place(line.StartPoint.X, line.StartPoint.Y, scale, options);
        (double x2, double y2) = Place(line.EndPoint.X, line.EndPoint.Y, scale, options);
        if (Distance(x1, y1, x2, y2) < 1e-9)
        {
            degenerate++;
            return;
        }

        segments.Add(DxfCurve.Line(x1, y1, x2, y2));
    }

    private static void AddArc(
        Arc arc,
        double scale,
        GeometryImportOptions options,
        List<ImportedSegment> segments,
        ref int tilted,
        ref int degenerate)
    {
        if (!IsHorizontal(arc.Normal) || arc.Radius <= 1e-9)
        {
            if (arc.Radius <= 1e-9 && IsHorizontal(arc.Normal))
            {
                degenerate++;
            }
            else
            {
                tilted++;
            }

            return;
        }

        if (!TryArcPoints(arc.Center, arc.Normal, arc.Radius, arc.StartAngle, arc.EndAngle, out Vector3 start, out Vector3 mid, out Vector3 end))
        {
            degenerate++;
            return;
        }

        segments.Add(ArcSegment(start, mid, end, scale, options));
    }

    private static void AddCircle(
        Circle circle,
        double scale,
        GeometryImportOptions options,
        List<ImportedSegment> segments,
        ref int tilted,
        ref int degenerate)
    {
        if (!IsHorizontal(circle.Normal))
        {
            tilted++;
            return;
        }

        if (!TryArcPoints(circle.Center, circle.Normal, circle.Radius, 0, 180, out Vector3 start, out Vector3 mid, out Vector3 end) ||
            !TryArcPoints(circle.Center, circle.Normal, circle.Radius, 180, 360, out Vector3 start2, out Vector3 mid2, out Vector3 end2))
        {
            degenerate++;
            return;
        }

        segments.Add(ArcSegment(start, mid, end, scale, options));
        segments.Add(ArcSegment(start2, mid2, end2, scale, options));
    }

    private static void AddPoint(
        DxfPoint point,
        double scale,
        GeometryImportOptions options,
        List<ImportedSegment> segments,
        ref int tilted)
    {
        if (!IsHorizontal(point.Normal))
        {
            tilted++;
            return;
        }

        (double x, double y) = Place(point.Position.X, point.Position.Y, scale, options);
        segments.Add(DxfCurve.Dot(x, y));
    }

    private static void AddPolyline(
        Polyline2D polyline,
        double scale,
        GeometryImportOptions options,
        List<ImportedSegment> segments,
        ref int tilted,
        ref int smoothed,
        ref int degenerate)
    {
        if (polyline.SmoothType != PolylineSmoothType.NoSmooth)
        {
            smoothed++;
            return;
        }

        if (!IsHorizontal(polyline.Normal))
        {
            tilted++;
            return;
        }

        IReadOnlyList<Polyline2DVertex> vertices = polyline.Vertexes;
        if (vertices.Count < 2)
        {
            degenerate++;
            return;
        }

        int count = polyline.IsClosed ? vertices.Count : vertices.Count - 1;
        for (int i = 0; i < count; i++)
        {
            Polyline2DVertex from = vertices[i];
            Polyline2DVertex to = vertices[(i + 1) % vertices.Count];
            Vector3 start = ToWorld(polyline, from.Position);
            Vector3 end = ToWorld(polyline, to.Position);
            if (Math.Abs(from.Bulge) < 1e-12)
            {
                (double x1, double y1) = Place(start.X, start.Y, scale, options);
                (double x2, double y2) = Place(end.X, end.Y, scale, options);
                if (Distance(x1, y1, x2, y2) < 1e-9)
                {
                    degenerate++;
                    continue;
                }

                segments.Add(DxfCurve.Line(x1, y1, x2, y2));
                continue;
            }

            if (!DxfCurve.TryBulgeMidpoint(from.Position.X, from.Position.Y, to.Position.X, to.Position.Y, from.Bulge, out double midX, out double midY))
            {
                degenerate++;
                continue;
            }

            Vector3 mid = ToWorld(polyline, new Vector2(midX, midY));
            segments.Add(ArcSegment(start, mid, end, scale, options));
        }
    }

    private static ImportedSegment ArcSegment(
        Vector3 start,
        Vector3 mid,
        Vector3 end,
        double scale,
        GeometryImportOptions options)
    {
        (double x1, double y1) = Place(start.X, start.Y, scale, options);
        (double x2, double y2) = Place(mid.X, mid.Y, scale, options);
        (double x3, double y3) = Place(end.X, end.Y, scale, options);
        return new ImportedSegment
        {
            Kind = ImportedSegmentKind.Arc,
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            X3 = x3,
            Y3 = y3
        };
    }

    private static bool TryArcPoints(
        Vector3 center,
        Vector3 normal,
        double radius,
        double startDeg,
        double endDeg,
        out Vector3 start,
        out Vector3 mid,
        out Vector3 end)
    {
        double sweep = endDeg - startDeg;
        sweep %= 360;
        if (sweep < 0)
        {
            sweep += 360;
        }

        if (sweep < 1e-4)
        {
            start = mid = end = Vector3.Zero;
            return false;
        }

        start = OnEntityCircle(center, normal, radius, startDeg);
        mid = OnEntityCircle(center, normal, radius, startDeg + sweep / 2);
        end = OnEntityCircle(center, normal, radius, startDeg + sweep);
        return true;
    }

    private static Vector3 OnEntityCircle(Vector3 center, Vector3 normal, double radius, double angleDeg)
    {
        double radians = angleDeg * Math.PI / 180.0;
        Vector3 offset = MathHelper.Transform(
            new Vector2(radius * Math.Cos(radians), radius * Math.Sin(radians)),
            normal,
            0);
        return center + offset;
    }

    private static Vector3 ToWorld(Polyline2D polyline, Vector2 position) =>
        MathHelper.Transform(position, polyline.Normal, polyline.Elevation);

    private static (double X, double Y) Place(double x, double y, double scale, GeometryImportOptions options) =>
        (x * scale + options.OriginX_mm, y * scale + options.OriginY_mm);

    private static bool IsHorizontal(Vector3 normal) => Math.Abs(Math.Abs(normal.Z) - 1) <= 1e-6;

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static string LayerName(EntityObject entity) =>
        string.IsNullOrWhiteSpace(entity.Layer?.Name) ? "0" : entity.Layer.Name;

    private static IEnumerable<EntityObject> ModelEntities(DxfDocument document) =>
        document.Layouts
            .Where(static layout => !layout.IsPaperSpace)
            .SelectMany(static layout => layout.AssociatedBlock.Entities);

    private static string SkipLabel(EntityType type) => type switch
    {
        EntityType.Text or EntityType.MText => "text",
        EntityType.Dimension => "dimension",
        EntityType.Hatch => "hatch",
        EntityType.Spline => "spline",
        EntityType.Insert => "block insert",
        EntityType.Ellipse => "ellipse",
        EntityType.Leader => "leader",
        EntityType.Polyline3D => "3D polyline",
        _ => type.ToString()
    };

    private static IReadOnlyList<string> DescribeWarnings(
        Dictionary<string, int> skipped,
        int tilted,
        int smoothed,
        int degenerate,
        int paper)
    {
        var lines = new List<string>();
        foreach (string label in new[] { "text", "dimension", "hatch" })
        {
            AddCount(lines, skipped.GetValueOrDefault(label), label);
            skipped.Remove(label);
        }

        foreach ((string label, int count) in skipped.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            AddCount(lines, count, label);
        }

        AddCount(lines, tilted, "out-of-plane");
        AddCount(lines, smoothed, "smoothed polyline");
        AddCount(lines, degenerate, "degenerate");
        AddCount(lines, paper, "paper-space");
        return lines;
    }

    private static void AddCount(List<string> lines, int count, string label)
    {
        if (count <= 0)
        {
            return;
        }

        string noun = count == 1 ? "entity" : "entities";
        lines.Add($"Skipped {count} {label} {noun}.");
    }

    private static (DrawingLengthUnit Unit, string? Note) DescribeUnits(DrawingUnits units) => units switch
    {
        DrawingUnits.Millimeters => (DrawingLengthUnit.Millimeter, null),
        DrawingUnits.Centimeters => (DrawingLengthUnit.Centimeter, null),
        DrawingUnits.Meters => (DrawingLengthUnit.Meter, null),
        DrawingUnits.Microns => (DrawingLengthUnit.Micrometer, null),
        DrawingUnits.Inches or DrawingUnits.USSurveyInches => (DrawingLengthUnit.Inch, null),
        DrawingUnits.Feet or DrawingUnits.USSurveyFeet => (DrawingLengthUnit.Foot, null),
        DrawingUnits.Unitless => (DrawingLengthUnit.Millimeter, "The DXF header does not specify units. Millimeters are selected."),
        _ => (DrawingLengthUnit.Millimeter, $"The DXF header unit is {units}. Millimeters are selected.")
    };

    private static DxfDocument Read(string sourcePath)
    {
        DxfVersion version = DxfDocument.CheckDxfFileVersion(sourcePath, out _);
        if (version < DxfVersion.AutoCad2000)
        {
            throw new InvalidDataException("DXF files older than AutoCAD 2000 cannot be imported.");
        }

        return DxfDocument.Load(sourcePath)
            ?? throw new InvalidDataException("Could not read this DXF file.");
    }
}
