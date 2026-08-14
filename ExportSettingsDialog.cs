using FanucPathAuthor.Models;
using FanucPathAuthor.Services;

namespace FanucPathAuthor;

public sealed class ExportSettingsDialog : Form
{
    private const int DialogPadding = 12;
    private const int ContentWidth = 1100;
    private const int FormClientWidth = ContentWidth + DialogPadding * 2;
    private const int ButtonWidth = 132;
    private const int ButtonHeight = 44;
    private const int ButtonBarVerticalPadding = 10;

    private readonly ProcessDefaults _sourceDefaults;
    private readonly RadioButton _modeAbsolute;
    private readonly RadioButton _modeFrameOffset;
    private readonly RadioButton _modeCalibOffset;
    private readonly CheckBox _fovCenterNeedle;
    private readonly CheckBox _fixedCameraInversion;
    private readonly CheckBox _useNeedleOffset;
    private readonly NumericUpDown _needleX;
    private readonly NumericUpDown _needleY;
    private readonly NumericUpDown _needleR;
    private readonly Label _calibrationInfo;
    private TableLayoutPanel? _root;
    private GroupBox? _coordinateGroup;

    public ProcessDefaults Result { get; private set; } = new();

    public ExportSettingsDialog(ProcessDefaults current, RobotCalibrationFile? calibration)
    {
        _sourceDefaults = current;

        _modeAbsolute = new RadioButton
        {
            Text = "UFRAME / UTOOL absolute (no offset)",
            AutoSize = true,
            MaximumSize = new Size(ContentWidth - 8, 0)
        };

        _modeFrameOffset = new RadioButton
        {
            Text = "Offset from Frame origin (sensor px)",
            AutoSize = true,
            MaximumSize = new Size(ContentWidth - 8, 0)
        };

        _modeCalibOffset = new RadioButton
        {
            Text = "Offset from Calibration Reference (recommended for dispense)",
            AutoSize = true,
            MaximumSize = new Size(ContentWidth - 8, 0)
        };

        _fovCenterNeedle = new CheckBox
        {
            Text = "FOV center correction (needle at image center)",
            AutoSize = true,
            MaximumSize = new Size(ContentWidth - 8, 0),
            Margin = new Padding(0, 4, 0, 0)
        };

        _fixedCameraInversion = new CheckBox
        {
            Text = "Fixed camera inversion (+180 deg local to sensor)",
            AutoSize = true,
            MaximumSize = new Size(ContentWidth - 8, 0),
            Margin = new Padding(0, 4, 0, 0)
        };

        _useNeedleOffset = new CheckBox
        {
            Text = "Apply needle offset to all export coordinates",
            AutoSize = true,
            MaximumSize = new Size(ContentWidth - 8, 0),
            Margin = new Padding(0, 4, 0, 0)
        };

        _needleX = CreateNumeric(-5000, 5000, 0.001m, 3);
        _needleY = CreateNumeric(-5000, 5000, 0.001m, 3);
        _needleR = CreateNumeric(-360, 360, 0.1m, 2);

        _calibrationInfo = HintLabel(string.Empty, ContentWidth - 20);

        InitializeFormShell();
        BuildLayout();

        bool hasCalibration = calibration?.PixelToRobot != null;
        _modeAbsolute.Enabled = hasCalibration;
        _modeFrameOffset.Enabled = hasCalibration;
        _modeCalibOffset.Enabled = hasCalibration;
        _fovCenterNeedle.Enabled = hasCalibration &&
                                    calibration!.FrameWidth > 0 &&
                                    calibration.FrameHeight > 0;

        switch (current.CoordinateMode)
        {
            case ExportCoordinateMode.UframeAbsolute:
                _modeAbsolute.Checked = true;
                break;
            case ExportCoordinateMode.OffsetFromFrameOrigin:
                _modeFrameOffset.Checked = true;
                break;
            default:
                _modeCalibOffset.Checked = hasCalibration;
                break;
        }

        if (!hasCalibration)
        {
            _modeAbsolute.Checked = true;
        }

        _fovCenterNeedle.Checked = current.UseFovCenterNeedleOffset;
        _fixedCameraInversion.Checked = current.UseFixedCameraInversion;
        _useNeedleOffset.Checked = current.UseNeedleOffset;
        _needleX.Value = Clamp(_needleX, (decimal)current.NeedleOffsetX_mm);
        _needleY.Value = Clamp(_needleY, (decimal)current.NeedleOffsetY_mm);
        _needleR.Value = Clamp(_needleR, (decimal)current.NeedleOffsetR_deg);
        SetNeedleEditorsEnabled(current.UseNeedleOffset);

        _useNeedleOffset.CheckedChanged += (_, _) =>
            SetNeedleEditorsEnabled(_useNeedleOffset.Checked);

        UpdateCalibrationInfo(calibration);
        Shown += OnShownAdjustSize;
    }

    private void InitializeFormShell()
    {
        Text = "Export Settings";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        AutoSize = false;
        AutoSizeMode = AutoSizeMode.GrowOnly;
        ClientSize = new Size(FormClientWidth, 520);
        MinimumSize = new Size(FormClientWidth, 200);
        MaximumSize = new Size(FormClientWidth, 1400);
    }

    private void BuildLayout()
    {
        _root = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(DialogPadding),
            Dock = DockStyle.Top,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
        };
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _coordinateGroup = BuildCoordinateGroup();
        _root.Controls.Add(_coordinateGroup, 0, 0);
        _root.Controls.Add(BuildBottomPanel(), 0, 1);

        Controls.Add(_root);
    }

    private GroupBox BuildCoordinateGroup()
    {
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 14,
            Dock = DockStyle.Top,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (int i = 0; i < 14; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        int row = 0;
        layout.Controls.Add(_modeAbsolute, 0, row++);
        layout.Controls.Add(HintLabel(
            "Export XY = absolute UFRAME mm after FOV and needle corrections.",
            ContentWidth - 32), 0, row++);
        layout.Controls.Add(_modeFrameOffset, 0, row++);
        layout.Controls.Add(HintLabel(
            "Export XY = robot mm delta from the taught Frame origin (local 0,0).",
            ContentWidth - 32), 0, row++);
        layout.Controls.Add(_modeCalibOffset, 0, row++);
        layout.Controls.Add(HintLabel(
            "Export XY = robot mm delta from calibration ReferenceRobot. Dispense: CrossCalibRef + Export.",
            ContentWidth - 32), 0, row++);
        layout.Controls.Add(_fovCenterNeedle, 0, row++);
        layout.Controls.Add(HintLabel(
            "Shift export so the needle at FOV center sits over the taught point (ref pixel to FOV center).",
            ContentWidth - 32), 0, row++);
        layout.Controls.Add(_fixedCameraInversion, 0, row++);
        layout.Controls.Add(HintLabel(
            "Use when the camera and needle are fixed and the robot moves the part under them.",
            ContentWidth - 32), 0, row++);
        layout.Controls.Add(_useNeedleOffset, 0, row++);
        layout.Controls.Add(BuildNeedleOffsetPanel(), 0, row++);
        layout.Controls.Add(HintLabel(
            "When enabled, needle offset is included in every export value. Do not add it again on the robot.",
            ContentWidth - 32), 0, row++);
        layout.Controls.Add(_calibrationInfo, 0, row);

        return WrapGroupBox("Waypoint coordinates (XY)", layout);
    }

    private Control BuildNeedleOffsetPanel()
    {
        var layout = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 3,
            Margin = new Padding(24, 0, 0, 0)
        };
        layout.Controls.Add(new Label { Text = "Needle X mm", AutoSize = true }, 0, 0);
        layout.Controls.Add(_needleX, 1, 0);
        layout.Controls.Add(new Label { Text = "Needle Y mm", AutoSize = true }, 0, 1);
        layout.Controls.Add(_needleY, 1, 1);
        layout.Controls.Add(new Label { Text = "Needle R deg", AutoSize = true }, 0, 2);
        layout.Controls.Add(_needleR, 1, 2);
        return layout;
    }

    private Control BuildBottomPanel()
    {
        var buttonBar = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Dock = DockStyle.Right,
            Padding = new Padding(0, ButtonBarVerticalPadding, 0, ButtonBarVerticalPadding)
        };

        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Size = new Size(ButtonWidth, ButtonHeight),
            Margin = new Padding(10, 0, 0, 0),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular)
        };

        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Size = new Size(ButtonWidth, ButtonHeight),
            Margin = new Padding(10, 0, 0, 0),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular)
        };

        buttonBar.Controls.Add(cancel);
        buttonBar.Controls.Add(ok);

        AcceptButton = ok;
        CancelButton = cancel;
        ok.Click += OnOkClick;

        var wrapper = new Panel
        {
            Dock = DockStyle.Top,
            Height = ButtonHeight + ButtonBarVerticalPadding * 2 + 8,
            Margin = new Padding(0, 8, 0, 0)
        };
        wrapper.Controls.Add(buttonBar);
        return wrapper;
    }

    private GroupBox WrapGroupBox(string title, Control content)
    {
        var group = new GroupBox
        {
            Text = title,
            Padding = new Padding(10, 8, 10, 12),
            Margin = new Padding(0, 0, 0, 8),
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top
        };

        content.Dock = DockStyle.Top;
        group.Controls.Add(content);
        return group;
    }

    private void OnShownAdjustSize(object? sender, EventArgs e)
    {
        Shown -= OnShownAdjustSize;

        int innerWidth = ClientSize.Width - _root!.Padding.Horizontal;

        if (_coordinateGroup != null)
        {
            _coordinateGroup.Width = innerWidth;
            FitGroupBox(_coordinateGroup, innerWidth);
        }

        _root.Width = ClientSize.Width;
        _root.PerformLayout();
        Size preferred = _root.GetPreferredSize(new Size(ClientSize.Width, 0));
        ClientSize = new Size(FormClientWidth, preferred.Height + DialogPadding);
    }

    private static void FitGroupBox(GroupBox group, int innerWidth)
    {
        if (group.Controls.Count == 0)
        {
            return;
        }

        group.Width = innerWidth;
        Control content = group.Controls[0];

        group.PerformLayout();
        Rectangle area = group.DisplayRectangle;
        content.Location = new Point(area.X, area.Y);
        content.Width = area.Width;
        content.PerformLayout();

        int contentHeight = content.GetPreferredSize(new Size(content.Width, 0)).Height;
        content.Height = contentHeight;

        group.Height = area.Y + contentHeight + group.Padding.Bottom;
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        ExportCoordinateMode mode = _modeAbsolute.Checked
            ? ExportCoordinateMode.UframeAbsolute
            : _modeFrameOffset.Checked
                ? ExportCoordinateMode.OffsetFromFrameOrigin
                : ExportCoordinateMode.OffsetFromCalibrationRef;

        Result = new ProcessDefaults
        {
            Z_mm = _sourceDefaults.Z_mm,
            W_deg = _sourceDefaults.W_deg,
            P_deg = _sourceDefaults.P_deg,
            R_deg = _sourceDefaults.R_deg,
            Speed_mm_s = _sourceDefaults.Speed_mm_s,
            GlueOn = _sourceDefaults.GlueOn,
            CoordinateMode = mode,
            UseFovCenterNeedleOffset = _fovCenterNeedle.Checked,
            UseFixedCameraInversion = _fixedCameraInversion.Checked,
            UseNeedleOffset = _useNeedleOffset.Checked,
            NeedleOffsetX_mm = (double)_needleX.Value,
            NeedleOffsetY_mm = (double)_needleY.Value,
            NeedleOffsetR_deg = (double)_needleR.Value
        };
    }

    private void UpdateCalibrationInfo(RobotCalibrationFile? calibration)
    {
        if (calibration?.PixelToRobot == null)
        {
            _calibrationInfo.Text =
                "Calibration not loaded. Load RobotCvTransformCalibration.json or CompactMarker/<PoseId>/transform.json.";
            return;
        }

        string kindLabel = calibration.IsCompactMarker
            ? $"Compact Marker [{calibration.CalibrationPoseId ?? "?"}]"
            : "Robot-CV transform";

        string fixedPose = calibration.IsCompactMarker
            ? $" Fixed Z/W/P/R from profile: {calibration.FixedRobotZ_mm ?? 0:0.###} / " +
              $"{calibration.FixedRobotW_deg ?? 0:0.#} / {calibration.FixedRobotP_deg ?? 0:0.#} / {calibration.FixedRobotR_deg ?? 0:0.#}."
            : string.Empty;

        _calibrationInfo.Text =
            $"{kindLabel}: ref pixel ({calibration.ReferencePixelX:0.##}, {calibration.ReferencePixelY:0.##}) -> " +
            $"ReferenceRobot ({calibration.ReferenceRobotX_mm:0.###}, {calibration.ReferenceRobotY_mm:0.###}) mm. " +
            $"FOV {calibration.FrameWidth}x{calibration.FrameHeight} px, center ({calibration.FrameWidth / 2.0:0.##}, {calibration.FrameHeight / 2.0:0.##})." +
            fixedPose;
    }

    private void SetNeedleEditorsEnabled(bool enabled)
    {
        _needleX.Enabled = enabled;
        _needleY.Enabled = enabled;
        _needleR.Enabled = enabled;
    }

    private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal increment, int decimals) =>
        new()
        {
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimals,
            Increment = increment,
            Width = 120
        };

    private static decimal Clamp(NumericUpDown control, decimal value) =>
        Math.Max(control.Minimum, Math.Min(control.Maximum, value));

    private static Label HintLabel(string text, int width) =>
        new()
        {
            Text = text,
            ForeColor = SystemColors.GrayText,
            AutoSize = true,
            MaximumSize = new Size(width, 0),
            Margin = new Padding(12, 0, 0, 8),
            UseMnemonic = false
        };
}
