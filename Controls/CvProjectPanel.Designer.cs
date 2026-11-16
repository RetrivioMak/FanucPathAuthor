#nullable enable
namespace DispensePathAuthor.Controls;

partial class CvProjectPanel
{
    private System.ComponentModel.IContainer? components;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _layout = new TableLayoutPanel();
        _frameGroup = new GroupBox();
        _frameLayout = new TableLayoutPanel();
        _originXLabel = new Label();
        _originX = new NumericUpDown();
        _originYLabel = new Label();
        _originY = new NumericUpDown();
        _angleLabel = new Label();
        _angle = new NumericUpDown();
        _invertY = new CheckBox();
        _invertHint = new Label();
        _applyModePanel = new FlowLayoutPanel();
        _moveShapesRadio = new RadioButton();
        _frameOnlyRadio = new RadioButton();
        _frameButtons = new FlowLayoutPanel();
        _editFrameButton = new Button();
        _applyFrameButton = new Button();
        _cancelFrameButton = new Button();
        _tileList = new ListView();
        _tileBottom = new Panel();
        _tileButtons = new FlowLayoutPanel();
        _openImageButton = new Button();
        _addFovButton = new Button();
        _removeTileButton = new Button();
        _focusTileButton = new Button();
        _poseGroup = new GroupBox();
        _poseLayout = new TableLayoutPanel();
        _captureXLabel = new Label();
        _captureX = new NumericUpDown();
        _captureYLabel = new Label();
        _captureY = new NumericUpDown();
        _captureRLabel = new Label();
        _captureR = new NumericUpDown();
        _resetCaptureButton = new Button();
        _infoPanel = new FlowLayoutPanel();
        _modeLabel = new Label();
        _captureHint = new Label();
        _offsetLabel = new Label();
        _worldLabel = new Label();
        _calibrationLabel = new Label();
        SuspendLayout();
        _layout.SuspendLayout();
        _frameGroup.SuspendLayout();
        _frameLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_originX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_originY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_angle).BeginInit();
        _applyModePanel.SuspendLayout();
        _frameButtons.SuspendLayout();
        _tileBottom.SuspendLayout();
        _tileButtons.SuspendLayout();
        _poseGroup.SuspendLayout();
        _poseLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_captureX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_captureY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_captureR).BeginInit();
        _infoPanel.SuspendLayout();

        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_frameGroup, 0, 0);
        _layout.Controls.Add(_tileList, 0, 1);
        _layout.Controls.Add(_tileBottom, 0, 2);
        _layout.Controls.Add(_calibrationLabel, 0, 3);
        _layout.Dock = DockStyle.Fill;
        _layout.Name = "cvLayout";
        _layout.RowCount = 4;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        ConfigureInteger(_originX, 0, 10000, 1);
        ConfigureInteger(_originY, 0, 10000, 1);
        ConfigureInteger(_angle, -180, 180, 1);
        _originX.Name = "originX";
        _originY.Name = "originY";
        _angle.Name = "angle";

        _originXLabel.AutoSize = true;
        _originXLabel.Text = "Origin X";
        _originYLabel.AutoSize = true;
        _originYLabel.Text = "Origin Y";
        _angleLabel.AutoSize = true;
        _angleLabel.Text = "Angle deg";
        _invertY.AutoSize = true;
        _invertY.Margin = new Padding(0, 8, 0, 0);
        _invertY.Name = "invertFrameY";
        _invertY.Text = "Invert frame Y (Fanuc right-hand)";
        _invertHint.AutoSize = true;
        _invertHint.MaximumSize = new Size(600, 0);
        _invertHint.Text = "Export XY becomes millimeters along this frame. +Y is counterclockwise from +X. Set export rotation to None.";
        _moveShapesRadio.AutoSize = true;
        _moveShapesRadio.Checked = true;
        _moveShapesRadio.Enabled = false;
        _moveShapesRadio.Name = "moveShapesRadio";
        _moveShapesRadio.Text = "Move shapes with frame (keep sensor px)";
        _frameOnlyRadio.AutoSize = true;
        _frameOnlyRadio.Enabled = false;
        _frameOnlyRadio.Name = "frameOnlyRadio";
        _frameOnlyRadio.Text = "Update frame only (keep local px)";
        _applyModePanel.AutoSize = true;
        _applyModePanel.Controls.Add(_moveShapesRadio);
        _applyModePanel.Controls.Add(_frameOnlyRadio);
        _applyModePanel.FlowDirection = FlowDirection.TopDown;
        _applyModePanel.Margin = new Padding(0, 8, 0, 0);
        _applyModePanel.Name = "applyModePanel";
        _applyModePanel.WrapContents = false;
        _editFrameButton.AutoSize = true;
        _editFrameButton.Name = "editFrameButton";
        _editFrameButton.Text = "Edit Frame";
        _applyFrameButton.AutoSize = true;
        _applyFrameButton.Enabled = false;
        _applyFrameButton.Name = "applyFrameButton";
        _applyFrameButton.Text = "Apply";
        _cancelFrameButton.AutoSize = true;
        _cancelFrameButton.Enabled = false;
        _cancelFrameButton.Name = "cancelFrameButton";
        _cancelFrameButton.Text = "Cancel";
        _frameButtons.AutoSize = true;
        _frameButtons.Controls.Add(_editFrameButton);
        _frameButtons.Controls.Add(_applyFrameButton);
        _frameButtons.Controls.Add(_cancelFrameButton);
        _frameButtons.Margin = new Padding(0, 8, 0, 0);
        _frameButtons.Name = "frameButtons";
        _frameButtons.WrapContents = false;

        _frameLayout.AutoSize = true;
        _frameLayout.ColumnCount = 2;
        _frameLayout.Controls.Add(_originXLabel, 0, 0);
        _frameLayout.Controls.Add(_originX, 1, 0);
        _frameLayout.Controls.Add(_originYLabel, 0, 1);
        _frameLayout.Controls.Add(_originY, 1, 1);
        _frameLayout.Controls.Add(_angleLabel, 0, 2);
        _frameLayout.Controls.Add(_angle, 1, 2);
        _frameLayout.Controls.Add(_invertY, 0, 3);
        _frameLayout.Controls.Add(_invertHint, 0, 4);
        _frameLayout.Controls.Add(_applyModePanel, 0, 5);
        _frameLayout.Controls.Add(_frameButtons, 0, 6);
        _frameLayout.Dock = DockStyle.Fill;
        _frameLayout.Name = "frameLayout";
        _frameLayout.SetColumnSpan(_invertY, 2);
        _frameLayout.SetColumnSpan(_invertHint, 2);
        _frameLayout.SetColumnSpan(_applyModePanel, 2);
        _frameLayout.SetColumnSpan(_frameButtons, 2);

        _frameGroup.AutoSize = true;
        _frameGroup.Controls.Add(_frameLayout);
        _frameGroup.Dock = DockStyle.Top;
        _frameGroup.Name = "frameGroup";
        _frameGroup.Padding = new Padding(8);
        _frameGroup.Text = "Frame (sensor px)";

        _tileList.Columns.Add("Name", 100);
        _tileList.Columns.Add("Capture X", 70);
        _tileList.Columns.Add("Capture Y", 70);
        _tileList.Columns.Add("R", 45);
        _tileList.Columns.Add("Active", 55);
        _tileList.Dock = DockStyle.Fill;
        _tileList.FullRowSelect = true;
        _tileList.HideSelection = false;
        _tileList.Name = "tileList";
        _tileList.View = View.Details;

        ConfigureDecimal(_captureX, -5000, 5000, 0.1m, 2);
        ConfigureDecimal(_captureY, -5000, 5000, 0.1m, 2);
        ConfigureDecimal(_captureR, -360, 360, 0.1m, 2);
        _captureX.Name = "captureX";
        _captureY.Name = "captureY";
        _captureR.Name = "captureR";
        _captureXLabel.AutoSize = true;
        _captureXLabel.Text = "Capture X mm";
        _captureYLabel.AutoSize = true;
        _captureYLabel.Text = "Capture Y mm";
        _captureRLabel.AutoSize = true;
        _captureRLabel.Text = "Capture R deg";
        _poseLayout.AutoSize = true;
        _poseLayout.ColumnCount = 2;
        _poseLayout.Controls.Add(_captureXLabel, 0, 0);
        _poseLayout.Controls.Add(_captureX, 1, 0);
        _poseLayout.Controls.Add(_captureYLabel, 0, 1);
        _poseLayout.Controls.Add(_captureY, 1, 1);
        _poseLayout.Controls.Add(_captureRLabel, 0, 2);
        _poseLayout.Controls.Add(_captureR, 1, 2);
        _poseLayout.Dock = DockStyle.Top;
        _poseLayout.Name = "poseLayout";
        _poseLayout.Padding = new Padding(0, 0, 0, 6);
        _resetCaptureButton.AutoSize = true;
        _resetCaptureButton.Dock = DockStyle.Top;
        _resetCaptureButton.Margin = new Padding(0, 4, 0, 0);
        _resetCaptureButton.Name = "resetCaptureButton";
        _resetCaptureButton.Text = "Use calib. reference";
        _poseGroup.AutoSize = true;
        _poseGroup.Controls.Add(_resetCaptureButton);
        _poseGroup.Controls.Add(_poseLayout);
        _poseGroup.Dock = DockStyle.Top;
        _poseGroup.Name = "poseGroup";
        _poseGroup.Padding = new Padding(8, 4, 8, 8);
        _poseGroup.Text = "Capture pose (robot ref. pose)";

        _openImageButton.AutoSize = true;
        _openImageButton.Name = "openImageButton";
        _openImageButton.Text = "Open image...";
        _addFovButton.AutoSize = true;
        _addFovButton.Name = "addFovButton";
        _addFovButton.Text = "Add FOV image...";
        _removeTileButton.AutoSize = true;
        _removeTileButton.Name = "removeTileButton";
        _removeTileButton.Text = "Remove";
        _focusTileButton.AutoSize = true;
        _focusTileButton.Name = "focusTileButton";
        _focusTileButton.Text = "Focus";
        _tileButtons.AutoSize = true;
        _tileButtons.Controls.Add(_openImageButton);
        _tileButtons.Controls.Add(_addFovButton);
        _tileButtons.Controls.Add(_removeTileButton);
        _tileButtons.Controls.Add(_focusTileButton);
        _tileButtons.Dock = DockStyle.Top;
        _tileButtons.Name = "tileButtons";
        _tileButtons.Padding = new Padding(0, 0, 0, 6);
        _tileButtons.WrapContents = true;

        _modeLabel.AutoSize = true;
        _modeLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _modeLabel.Name = "modeLabel";
        _modeLabel.Text = "Mode: Single FOV";
        _captureHint.AutoSize = true;
        _captureHint.ForeColor = Color.DimGray;
        _captureHint.MaximumSize = new Size(430, 0);
        _captureHint.Name = "captureHint";
        _captureHint.Text = "Open the anchor FOV image, define Frame, then teach the path in sensor space.";
        _offsetLabel.AutoSize = true;
        _offsetLabel.Name = "offsetLabel";
        _offsetLabel.Text = "Canvas offset: ¡ª";
        _worldLabel.AutoSize = true;
        _worldLabel.Name = "worldLabel";
        _worldLabel.Text = "Tile (0,0) world: ¡ª";
        _infoPanel.AutoSize = true;
        _infoPanel.Controls.Add(_modeLabel);
        _infoPanel.Controls.Add(_captureHint);
        _infoPanel.Controls.Add(_offsetLabel);
        _infoPanel.Controls.Add(_worldLabel);
        _infoPanel.Dock = DockStyle.Top;
        _infoPanel.FlowDirection = FlowDirection.TopDown;
        _infoPanel.Name = "tileInfo";
        _infoPanel.WrapContents = false;

        _tileBottom.AutoSize = true;
        _tileBottom.Controls.Add(_infoPanel);
        _tileBottom.Controls.Add(_poseGroup);
        _tileBottom.Controls.Add(_tileButtons);
        _tileBottom.Dock = DockStyle.Bottom;
        _tileBottom.Name = "tileBottom";
        _tileBottom.Padding = new Padding(0, 6, 0, 0);

        _calibrationLabel.AutoSize = true;
        _calibrationLabel.Dock = DockStyle.Top;
        _calibrationLabel.Name = "calibrationLabel";
        _calibrationLabel.Text = "Calibration: not loaded";

        Controls.Add(_layout);
        Name = "CvProjectPanel";
        Padding = new Padding(8);

        _infoPanel.ResumeLayout(false);
        _infoPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_captureR).EndInit();
        ((System.ComponentModel.ISupportInitialize)_captureY).EndInit();
        ((System.ComponentModel.ISupportInitialize)_captureX).EndInit();
        _poseLayout.ResumeLayout(false);
        _poseLayout.PerformLayout();
        _poseGroup.ResumeLayout(false);
        _poseGroup.PerformLayout();
        _tileButtons.ResumeLayout(false);
        _tileButtons.PerformLayout();
        _tileBottom.ResumeLayout(false);
        _tileBottom.PerformLayout();
        _frameButtons.ResumeLayout(false);
        _frameButtons.PerformLayout();
        _applyModePanel.ResumeLayout(false);
        _applyModePanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_angle).EndInit();
        ((System.ComponentModel.ISupportInitialize)_originY).EndInit();
        ((System.ComponentModel.ISupportInitialize)_originX).EndInit();
        _frameLayout.ResumeLayout(false);
        _frameLayout.PerformLayout();
        _frameGroup.ResumeLayout(false);
        _frameGroup.PerformLayout();
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        ResumeLayout(false);
    }

    private static void ConfigureInteger(NumericUpDown control, int minimum, int maximum, int increment)
    {
        control.DecimalPlaces = 0;
        control.Increment = increment;
        control.Maximum = maximum;
        control.Minimum = minimum;
        control.Width = 120;
    }

    private static void ConfigureDecimal(NumericUpDown control, decimal minimum, decimal maximum, decimal increment, int decimals)
    {
        control.DecimalPlaces = decimals;
        control.Increment = increment;
        control.Maximum = maximum;
        control.Minimum = minimum;
        control.Width = 120;
    }

    private TableLayoutPanel _layout = null!;
    private GroupBox _frameGroup = null!;
    private TableLayoutPanel _frameLayout = null!;
    private Label _originXLabel = null!;
    private NumericUpDown _originX = null!;
    private Label _originYLabel = null!;
    private NumericUpDown _originY = null!;
    private Label _angleLabel = null!;
    private NumericUpDown _angle = null!;
    private CheckBox _invertY = null!;
    private Label _invertHint = null!;
    private FlowLayoutPanel _applyModePanel = null!;
    private RadioButton _moveShapesRadio = null!;
    private RadioButton _frameOnlyRadio = null!;
    private FlowLayoutPanel _frameButtons = null!;
    private Button _editFrameButton = null!;
    private Button _applyFrameButton = null!;
    private Button _cancelFrameButton = null!;
    private ListView _tileList = null!;
    private Panel _tileBottom = null!;
    private FlowLayoutPanel _tileButtons = null!;
    private Button _openImageButton = null!;
    private Button _addFovButton = null!;
    private Button _removeTileButton = null!;
    private Button _focusTileButton = null!;
    private GroupBox _poseGroup = null!;
    private TableLayoutPanel _poseLayout = null!;
    private Label _captureXLabel = null!;
    private NumericUpDown _captureX = null!;
    private Label _captureYLabel = null!;
    private NumericUpDown _captureY = null!;
    private Label _captureRLabel = null!;
    private NumericUpDown _captureR = null!;
    private Button _resetCaptureButton = null!;
    private FlowLayoutPanel _infoPanel = null!;
    private Label _modeLabel = null!;
    private Label _captureHint = null!;
    private Label _offsetLabel = null!;
    private Label _worldLabel = null!;
    private Label _calibrationLabel = null!;
}
