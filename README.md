# FanucPathAuthor (MVP)

Standalone C# WinForms tool for authoring glue trajectories from a CCD3 still image.

## Features

- Open a CCD3 image (2448��2048 or any size)
- Define a **Frame** in sensor pixels (origin + angle)
- Draw path segments: **Dot**, **Line**, **Arc**
- Store geometry in **frame-local pixels**
- Save / load path JSON
- Load `RobotCvTransformCalibration.json` from Nilgiris
- Preview Fanuc XY waypoints
- Export CSV and a simple Fanuc `.LS` program

## Build

Requires .NET 8 SDK on Windows x64:

```powershell
cd FanucPathAuthor
dotnet build -c Release
```

Run:

```powershell
dotnet run -c Release
```

Or publish a self-contained exe:

```powershell
dotnet publish -c Release
```

Output: `bin\x64\Release\net8.0-windows\win-x64\publish\FanucPathAuthor.exe`

## Workflow

1. **File �� Open image** �� load CCD3 snapshot
2. Select **Frame** tool and click the reference origin on the image  
   (or enter origin X/Y and angle manually)
3. Select **Dot / Line / Arc** and click on the image:
   - Dot: 1 click
   - Line: 2 clicks (start, end)
   - Arc: 3 clicks (start, mid, end)
4. **File �� Load calibration JSON** �� `RobotCvTransformCalibration.json`
5. Check **Robot preview** panel for Fanuc XY
6. **File -> Save path JSON**
7. **File -> Export settings...** (Z/W/P/R/speed) — also shown before CSV / `.LS` export
8. **File -> Export CSV** or **Export Fanuc .LS**

## Path JSON format

```json
{
  "schemaVersion": 1,
  "imagePath": "C:\\path\\to\\image.png",
  "frame": { "originX_px": 1229, "originY_px": 1017, "angleDeg": 0 },
  "defaults": { "z_mm": 0, "w_deg": 0, "p_deg": 0, "r_deg": 0, "speed_mm_s": 50, "glueOn": true },
  "segments": [
    { "kind": "dot", "x1": 10, "y1": 20, "glueOn": true, "speed_mm_s": 50 },
    { "kind": "line", "x1": 0, "y1": 0, "x2": 100, "y2": 0, "glueOn": true, "speed_mm_s": 50 },
    { "kind": "arc", "x1": 0, "y1": 0, "x2": 50, "y2": 50, "x3": 100, "y3": 0, "glueOn": true, "speed_mm_s": 50 }
  ]
}
```

All segment coordinates are **frame-local pixels**.

## Controls

| Action | Input |
|--------|--------|
| Zoom | Mouse wheel |
| Pan | Ctrl + left drag (or middle drag) |
| Delete segment | Delete key |
| Undo / Redo | Ctrl+Z / Ctrl+Y |
| Cancel in-progress shape | Esc |
| Fit image | View -> Fit image |

## Step 2 additions

- **Undo / Redo** for segments and frame edits
- **Live rubber-band preview** while drawing Line / Arc / Rect
- **Rectangle tool** (2 clicks -> 4 Line segments)
- **Cursor coordinates** in status bar (sensor px + frame-local px)
- **Save path JSON as...**
- Unit tests in `FanucPathAuthor.Tests`

Run tests:

```powershell
dotnet test FanucPathAuthor.Tests
```

### Build troubleshooting

If you see `CS0579` duplicate assembly attributes or `Xunit` errors in the **main** project:

1. Pull the latest `FanucPathAuthor.csproj` (test sources must be excluded from the WinExe).
2. Delete `obj` and `bin` folders, then rebuild:
   ```powershell
   dotnet clean
   dotnet build
   ```
3. If `Properties\AssemblyInfo.cs` exists, delete it (SDK generates assembly info automatically).
4. Build the **solution** (`FanucPathAuthor.sln`), not only the test `.csproj`, when running tests.

## Notes

- No Euresys / Open eVision license required in this tool
- Calibration JSON is read-only input from Nilgiris
- Z / W / P / speed are set in **Export settings** (saved in path JSON `defaults`)
- Runtime Frame offset on the robot is **not** part of this MVP

## Post-MVP (planned)

- Multi-tile continuous trajectory (Approach A + B)
- Helpers: rectangle, polygon, polyline
- Hatch / fill generator
