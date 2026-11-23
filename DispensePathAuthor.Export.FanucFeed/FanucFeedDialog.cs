using DispensePathAuthor.Contracts;

namespace DispensePathAuthor.Export.FanucFeed;

public sealed partial class FanucFeedDialog : Form
{
    public FanucExportSettings? Settings { get; private set; }

    public BeadFeedSettings? Process { get; private set; }

    public FanucFeedDialog(FanucExportSettings current, FanucCalibrationSnapshot calibration, bool teachWorkspace)
    {
        InitializeComponent();
        _useNeedleOffset.CheckedChanged += (_, _) => SetNeedleEditorsEnabled(_useNeedleOffset.Checked);
        _needleDiameter.ValueChanged += (_, _) => RefreshPreview();
        _flow.ValueChanged += (_, _) => RefreshPreview();
        _okButton.Click += OnOkClick;

        bool hasCalibration = calibration.HasTransform;
        _modeAbsolute.Enabled = hasCalibration;
        _modeFrameOffset.Enabled = hasCalibration;
        _modeCalibOffset.Enabled = hasCalibration;
        _fovCenterNeedle.Enabled = hasCalibration &&
                                    calibration.FrameWidth > 0 &&
                                    calibration.FrameHeight > 0;

        switch (current.CoordinateMode)
        {
            case FanucCoordinateMode.UframeAbsolute:
                _modeAbsolute.Checked = true;
                break;
            case FanucCoordinateMode.OffsetFromFrameOrigin:
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
        SetLocalRotation(current.LocalRotation);
        _useNeedleOffset.Checked = current.UseNeedleOffset;
        _needleX.Value = Clamp(_needleX, (decimal)current.NeedleOffsetX_mm);
        _needleY.Value = Clamp(_needleY, (decimal)current.NeedleOffsetY_mm);
        _needleR.Value = Clamp(_needleR, (decimal)current.NeedleOffsetR_deg);
        _userFrame.Value = current.UserFrame ?? calibration.DefaultUserFrame;
        _userTool.Value = current.UserTool ?? calibration.DefaultUserTool;
        SetNeedleEditorsEnabled(current.UseNeedleOffset);
        UpdateCalibrationInfo(calibration);

        if (teachWorkspace)
        {
            _modeAbsolute.Enabled = false;
            _modeFrameOffset.Enabled = false;
            _modeCalibOffset.Enabled = false;
            _fovCenterNeedle.Enabled = false;
            _rotationNone.Enabled = false;
            _rotation90.Enabled = false;
            _rotation180.Enabled = false;
            _calibrationInfo.Text =
                "Teach project: XY is exported as absolute robot-frame millimeters. Camera rotation and FOV correction stay off. Needle offset is still applied when enabled.";
        }

        ApplyBead(BeadFeedSettingsStore.Load());
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        FitGroup(_frameGroup);
        FitGroup(_fovGroup);
        FitGroup(_angleGroup);
        FitGroup(_needleGroup);
        _exportLayout.PerformLayout();
    }

    private static void FitGroup(GroupBox group)
    {
        if (group.Controls.Count == 0)
        {
            return;
        }

        Control content = group.Controls[0];
        group.PerformLayout();
        Rectangle area = group.DisplayRectangle;
        content.Location = new Point(area.X, area.Y);
        content.Width = Math.Max(0, area.Width);
        content.PerformLayout();
        int contentHeight = content.GetPreferredSize(new Size(content.Width, 0)).Height;
        content.Height = contentHeight;
        group.AutoSize = false;
        group.Height = area.Y + contentHeight + group.Padding.Bottom;
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        Process = null;
        double min = (double)_minSpeed.Value;
        double max = (double)_maxSpeed.Value;
        if (max < min)
        {
            _tabs.SelectedTab = _beadPage;
            MessageBox.Show(
                this,
                "Maximum speed must be at least the minimum speed.",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        FanucCoordinateMode mode = _modeAbsolute.Checked
            ? FanucCoordinateMode.UframeAbsolute
            : _modeFrameOffset.Checked
                ? FanucCoordinateMode.OffsetFromFrameOrigin
                : FanucCoordinateMode.OffsetFromCalibrationRef;

        Settings = new FanucExportSettings
        {
            CoordinateMode = mode,
            UseFovCenterNeedleOffset = _fovCenterNeedle.Checked,
            LocalRotation = ReadLocalRotation(),
            UseNeedleOffset = _useNeedleOffset.Checked,
            NeedleOffsetX_mm = (double)_needleX.Value,
            NeedleOffsetY_mm = (double)_needleY.Value,
            NeedleOffsetR_deg = (double)_needleR.Value,
            UserFrame = (int)_userFrame.Value,
            UserTool = (int)_userTool.Value
        };
        Process = new BeadFeedSettings
        {
            NeedleDiameter_mm = (double)_needleDiameter.Value,
            Flow_mm3_s = (double)_flow.Value,
            TravelSpeed_mm_s = (double)_travel.Value,
            MinSpeed_mm_s = min,
            MaxSpeed_mm_s = max
        };
        BeadFeedSettingsStore.Save(Process);
        DialogResult = DialogResult.OK;
    }

    private void UpdateCalibrationInfo(FanucCalibrationSnapshot calibration)
    {
        if (!calibration.HasTransform)
        {
            _calibrationInfo.Text =
                "Calibration not loaded. Load RobotCvTransformCalibration.json or CompactMarker/<PoseId>/transform.json.";
            return;
        }

        string kindLabel = calibration.IsCompactMarker
            ? $"Compact Marker [{calibration.PoseId ?? "?"}]"
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

    private void SetLocalRotation(FanucLocalRotation rotation)
    {
        switch (rotation)
        {
            case FanucLocalRotation.Degree90:
                _rotation90.Checked = true;
                break;
            case FanucLocalRotation.Degree180:
                _rotation180.Checked = true;
                break;
            default:
                _rotationNone.Checked = true;
                break;
        }
    }

    private FanucLocalRotation ReadLocalRotation()
    {
        if (_rotation90.Checked)
        {
            return FanucLocalRotation.Degree90;
        }

        if (_rotation180.Checked)
        {
            return FanucLocalRotation.Degree180;
        }

        return FanucLocalRotation.None;
    }

    private void SetNeedleEditorsEnabled(bool enabled)
    {
        _needleX.Enabled = enabled;
        _needleY.Enabled = enabled;
        _needleR.Enabled = enabled;
    }

    private void ApplyBead(BeadFeedSettings settings)
    {
        _needleDiameter.Value = Clamp(_needleDiameter, (decimal)settings.NeedleDiameter_mm);
        _flow.Value = Clamp(_flow, (decimal)settings.Flow_mm3_s);
        _travel.Value = Clamp(_travel, (decimal)settings.TravelSpeed_mm_s);
        _minSpeed.Value = Clamp(_minSpeed, (decimal)settings.MinSpeed_mm_s);
        _maxSpeed.Value = Clamp(_maxSpeed, (decimal)settings.MaxSpeed_mm_s);
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        double needle = (double)_needleDiameter.Value;
        double flow = (double)_flow.Value;
        double speed = BeadFeed.BeadSpeed(needle, flow);
        _preview.Text = double.IsFinite(speed)
            ? $"Needle-width bead ({needle:0.###} mm) runs at {speed:0.###} mm/s."
            : "Enter a needle diameter and a flow above zero.";
    }

    private static decimal Clamp(NumericUpDown control, decimal value) =>
        Math.Max(control.Minimum, Math.Min(control.Maximum, value));
}
