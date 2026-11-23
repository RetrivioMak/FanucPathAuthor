#nullable enable
namespace DispensePathAuthor.Export.FanucFeed;

partial class FanucFeedDialog
{
    private System.ComponentModel.IContainer? components = null;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FanucFeedDialog));
        _tabs = new TabControl();
        _exportPage = new TabPage();
        _exportScroll = new Panel();
        _exportLayout = new TableLayoutPanel();
        _frameGroup = new GroupBox();
        _frameLayout = new TableLayoutPanel();
        _frameNumbers = new FlowLayoutPanel();
        _userFrameLabel = new Label();
        _userFrame = new NumericUpDown();
        _userToolLabel = new Label();
        _userTool = new NumericUpDown();
        _modeAbsolute = new RadioButton();
        _absoluteHint = new Label();
        _modeFrameOffset = new RadioButton();
        _frameHint = new Label();
        _modeCalibOffset = new RadioButton();
        _calibHint = new Label();
        _fovGroup = new GroupBox();
        _fovLayout = new TableLayoutPanel();
        _fovCenterNeedle = new CheckBox();
        _fovHint = new Label();
        _angleGroup = new GroupBox();
        _angleLayout = new TableLayoutPanel();
        _rotationPanel = new FlowLayoutPanel();
        _rotationNone = new RadioButton();
        _rotation90 = new RadioButton();
        _rotation180 = new RadioButton();
        _angleHint = new Label();
        _angleHint2 = new Label();
        _needleGroup = new GroupBox();
        _needleLayout = new TableLayoutPanel();
        _useNeedleOffset = new CheckBox();
        _needleOffsetLayout = new TableLayoutPanel();
        _needleXLabel = new Label();
        _needleX = new NumericUpDown();
        _needleYLabel = new Label();
        _needleY = new NumericUpDown();
        _needleRLabel = new Label();
        _needleR = new NumericUpDown();
        _needleHint = new Label();
        _calibrationInfo = new Label();
        _beadPage = new TabPage();
        _beadLayout = new TableLayoutPanel();
        _needleDiameterLabel = new Label();
        _needleDiameter = new NumericUpDown();
        _flowLabel = new Label();
        _flow = new NumericUpDown();
        _travelLabel = new Label();
        _travel = new NumericUpDown();
        _minSpeedLabel = new Label();
        _minSpeed = new NumericUpDown();
        _maxSpeedLabel = new Label();
        _maxSpeed = new NumericUpDown();
        _preview = new Label();
        _beadHint = new Label();
        _buttonBar = new Panel();
        _cancelButton = new Button();
        _okButton = new Button();
        _tabs.SuspendLayout();
        _exportPage.SuspendLayout();
        _exportScroll.SuspendLayout();
        _exportLayout.SuspendLayout();
        _frameGroup.SuspendLayout();
        _frameLayout.SuspendLayout();
        _frameNumbers.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_userFrame).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_userTool).BeginInit();
        _fovGroup.SuspendLayout();
        _fovLayout.SuspendLayout();
        _angleGroup.SuspendLayout();
        _angleLayout.SuspendLayout();
        _rotationPanel.SuspendLayout();
        _needleGroup.SuspendLayout();
        _needleLayout.SuspendLayout();
        _needleOffsetLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_needleX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_needleY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_needleR).BeginInit();
        _beadPage.SuspendLayout();
        _beadLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_needleDiameter).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_flow).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_travel).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_minSpeed).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_maxSpeed).BeginInit();
        _buttonBar.SuspendLayout();
        SuspendLayout();
        // 
        // _tabs
        // 
        _tabs.Controls.Add(_exportPage);
        _tabs.Controls.Add(_beadPage);
        _tabs.Dock = DockStyle.Fill;
        _tabs.Location = new Point(12, 12);
        _tabs.Name = "_tabs";
        _tabs.SelectedIndex = 0;
        _tabs.Size = new Size(1054, 927);
        _tabs.TabIndex = 0;
        // 
        // _exportPage
        // 
        _exportPage.Controls.Add(_exportScroll);
        _exportPage.Location = new Point(8, 46);
        _exportPage.Name = "_exportPage";
        _exportPage.Padding = new Padding(8);
        _exportPage.Size = new Size(1038, 873);
        _exportPage.TabIndex = 0;
        _exportPage.Text = "Export";
        _exportPage.UseVisualStyleBackColor = true;
        // 
        // _exportScroll
        // 
        _exportScroll.AutoScroll = true;
        _exportScroll.Controls.Add(_exportLayout);
        _exportScroll.Dock = DockStyle.Fill;
        _exportScroll.Location = new Point(8, 8);
        _exportScroll.Name = "_exportScroll";
        _exportScroll.Size = new Size(1022, 857);
        _exportScroll.TabIndex = 0;
        // 
        // _exportLayout
        // 
        _exportLayout.AutoSize = true;
        _exportLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _exportLayout.ColumnCount = 1;
        _exportLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _exportLayout.Controls.Add(_frameGroup, 0, 0);
        _exportLayout.Controls.Add(_fovGroup, 0, 1);
        _exportLayout.Controls.Add(_angleGroup, 0, 2);
        _exportLayout.Controls.Add(_needleGroup, 0, 3);
        _exportLayout.Controls.Add(_calibrationInfo, 0, 4);
        _exportLayout.Dock = DockStyle.Top;
        _exportLayout.Location = new Point(0, 0);
        _exportLayout.Name = "_exportLayout";
        _exportLayout.RowCount = 5;
        _exportLayout.RowStyles.Add(new RowStyle());
        _exportLayout.RowStyles.Add(new RowStyle());
        _exportLayout.RowStyles.Add(new RowStyle());
        _exportLayout.RowStyles.Add(new RowStyle());
        _exportLayout.RowStyles.Add(new RowStyle());
        _exportLayout.Size = new Size(988, 1199);
        _exportLayout.TabIndex = 0;
        // 
        // _frameGroup
        // 
        _frameGroup.AutoSize = true;
        _frameGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _frameGroup.Controls.Add(_frameLayout);
        _frameGroup.Dock = DockStyle.Top;
        _frameGroup.Location = new Point(3, 3);
        _frameGroup.Margin = new Padding(3, 3, 3, 8);
        _frameGroup.Name = "_frameGroup";
        _frameGroup.Padding = new Padding(12, 8, 12, 12);
        _frameGroup.Size = new Size(982, 380);
        _frameGroup.TabIndex = 0;
        _frameGroup.TabStop = false;
        _frameGroup.Text = "UFRAME / UTOOL and offsets";
        // 
        // _frameLayout
        // 
        _frameLayout.AutoSize = true;
        _frameLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _frameLayout.ColumnCount = 1;
        _frameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _frameLayout.Controls.Add(_frameNumbers, 0, 0);
        _frameLayout.Controls.Add(_modeAbsolute, 0, 1);
        _frameLayout.Controls.Add(_absoluteHint, 0, 2);
        _frameLayout.Controls.Add(_modeFrameOffset, 0, 3);
        _frameLayout.Controls.Add(_frameHint, 0, 4);
        _frameLayout.Controls.Add(_modeCalibOffset, 0, 5);
        _frameLayout.Controls.Add(_calibHint, 0, 6);
        _frameLayout.Dock = DockStyle.Top;
        _frameLayout.Location = new Point(12, 40);
        _frameLayout.Name = "_frameLayout";
        _frameLayout.RowCount = 7;
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.Size = new Size(958, 328);
        _frameLayout.TabIndex = 0;
        // 
        // _frameNumbers
        // 
        _frameNumbers.AutoSize = true;
        _frameNumbers.Controls.Add(_userFrameLabel);
        _frameNumbers.Controls.Add(_userFrame);
        _frameNumbers.Controls.Add(_userToolLabel);
        _frameNumbers.Controls.Add(_userTool);
        _frameNumbers.Dock = DockStyle.Top;
        _frameNumbers.Location = new Point(3, 3);
        _frameNumbers.Margin = new Padding(3, 3, 3, 8);
        _frameNumbers.Name = "_frameNumbers";
        _frameNumbers.Size = new Size(952, 45);
        _frameNumbers.TabIndex = 0;
        _frameNumbers.WrapContents = false;
        // 
        // _userFrameLabel
        // 
        _userFrameLabel.AutoSize = true;
        _userFrameLabel.Location = new Point(0, 6);
        _userFrameLabel.Margin = new Padding(0, 6, 8, 0);
        _userFrameLabel.Name = "_userFrameLabel";
        _userFrameLabel.Size = new Size(130, 32);
        _userFrameLabel.TabIndex = 0;
        _userFrameLabel.Text = "User frame";
        // 
        // _userFrame
        // 
        _userFrame.Location = new Point(141, 3);
        _userFrame.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        _userFrame.Name = "_userFrame";
        _userFrame.Size = new Size(80, 39);
        _userFrame.TabIndex = 1;
        _userFrame.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // _userToolLabel
        // 
        _userToolLabel.AutoSize = true;
        _userToolLabel.Location = new Point(240, 6);
        _userToolLabel.Margin = new Padding(16, 6, 8, 0);
        _userToolLabel.Name = "_userToolLabel";
        _userToolLabel.Size = new Size(110, 32);
        _userToolLabel.TabIndex = 2;
        _userToolLabel.Text = "User tool";
        // 
        // _userTool
        // 
        _userTool.Location = new Point(361, 3);
        _userTool.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
        _userTool.Name = "_userTool";
        _userTool.Size = new Size(80, 39);
        _userTool.TabIndex = 3;
        _userTool.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // _modeAbsolute
        // 
        _modeAbsolute.AutoSize = true;
        _modeAbsolute.Dock = DockStyle.Top;
        _modeAbsolute.Location = new Point(3, 59);
        _modeAbsolute.Name = "_modeAbsolute";
        _modeAbsolute.Size = new Size(952, 36);
        _modeAbsolute.TabIndex = 1;
        _modeAbsolute.Text = "UFRAME / UTOOL absolute (no offset)";
        _modeAbsolute.UseVisualStyleBackColor = true;
        // 
        // _absoluteHint
        // 
        _absoluteHint.AutoSize = true;
        _absoluteHint.Dock = DockStyle.Top;
        _absoluteHint.ForeColor = SystemColors.GrayText;
        _absoluteHint.Location = new Point(3, 98);
        _absoluteHint.Margin = new Padding(3, 0, 3, 6);
        _absoluteHint.MaximumSize = new Size(860, 0);
        _absoluteHint.Name = "_absoluteHint";
        _absoluteHint.Size = new Size(860, 32);
        _absoluteHint.TabIndex = 2;
        _absoluteHint.Text = "Export XY = absolute UFRAME mm after FOV and needle corrections.";
        // 
        // _modeFrameOffset
        // 
        _modeFrameOffset.AutoSize = true;
        _modeFrameOffset.Dock = DockStyle.Top;
        _modeFrameOffset.Location = new Point(3, 139);
        _modeFrameOffset.Name = "_modeFrameOffset";
        _modeFrameOffset.Size = new Size(952, 36);
        _modeFrameOffset.TabIndex = 3;
        _modeFrameOffset.Text = "Offset from Frame origin (mm)";
        _modeFrameOffset.UseVisualStyleBackColor = true;
        // 
        // _frameHint
        // 
        _frameHint.AutoSize = true;
        _frameHint.Dock = DockStyle.Top;
        _frameHint.ForeColor = SystemColors.GrayText;
        _frameHint.Location = new Point(3, 178);
        _frameHint.Margin = new Padding(3, 0, 3, 6);
        _frameHint.MaximumSize = new Size(860, 0);
        _frameHint.Name = "_frameHint";
        _frameHint.Size = new Size(860, 32);
        _frameHint.TabIndex = 4;
        _frameHint.Text = "Export XY = robot mm delta from the taught Frame origin (local 0,0).";
        // 
        // _modeCalibOffset
        // 
        _modeCalibOffset.AutoSize = true;
        _modeCalibOffset.Checked = true;
        _modeCalibOffset.Dock = DockStyle.Top;
        _modeCalibOffset.Location = new Point(3, 219);
        _modeCalibOffset.Name = "_modeCalibOffset";
        _modeCalibOffset.Size = new Size(952, 36);
        _modeCalibOffset.TabIndex = 5;
        _modeCalibOffset.TabStop = true;
        _modeCalibOffset.Text = "Offset from Calibration Reference (recommended for dispense)";
        _modeCalibOffset.UseVisualStyleBackColor = true;
        // 
        // _calibHint
        // 
        _calibHint.AutoSize = true;
        _calibHint.Dock = DockStyle.Top;
        _calibHint.ForeColor = SystemColors.GrayText;
        _calibHint.Location = new Point(3, 258);
        _calibHint.Margin = new Padding(3, 0, 3, 6);
        _calibHint.MaximumSize = new Size(860, 0);
        _calibHint.Name = "_calibHint";
        _calibHint.Size = new Size(860, 64);
        _calibHint.TabIndex = 6;
        _calibHint.Text = "Export XY = robot mm delta from calibration ReferenceRobot. Dispense: CrossCalibRef + Export.";
        // 
        // _fovGroup
        // 
        _fovGroup.AutoSize = true;
        _fovGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _fovGroup.Controls.Add(_fovLayout);
        _fovGroup.Dock = DockStyle.Top;
        _fovGroup.Location = new Point(3, 394);
        _fovGroup.Margin = new Padding(3, 3, 3, 8);
        _fovGroup.Name = "_fovGroup";
        _fovGroup.Padding = new Padding(12, 8, 12, 12);
        _fovGroup.Size = new Size(982, 158);
        _fovGroup.TabIndex = 1;
        _fovGroup.TabStop = false;
        _fovGroup.Text = "FOV center correction";
        // 
        // _fovLayout
        // 
        _fovLayout.AutoSize = true;
        _fovLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _fovLayout.ColumnCount = 1;
        _fovLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _fovLayout.Controls.Add(_fovCenterNeedle, 0, 0);
        _fovLayout.Controls.Add(_fovHint, 0, 1);
        _fovLayout.Dock = DockStyle.Top;
        _fovLayout.Location = new Point(12, 40);
        _fovLayout.Name = "_fovLayout";
        _fovLayout.RowCount = 2;
        _fovLayout.RowStyles.Add(new RowStyle());
        _fovLayout.RowStyles.Add(new RowStyle());
        _fovLayout.Size = new Size(958, 106);
        _fovLayout.TabIndex = 0;
        // 
        // _fovCenterNeedle
        // 
        _fovCenterNeedle.AutoSize = true;
        _fovCenterNeedle.Dock = DockStyle.Top;
        _fovCenterNeedle.Location = new Point(3, 3);
        _fovCenterNeedle.Name = "_fovCenterNeedle";
        _fovCenterNeedle.Size = new Size(952, 36);
        _fovCenterNeedle.TabIndex = 0;
        _fovCenterNeedle.Text = "FOV center correction (needle at image center)";
        _fovCenterNeedle.UseVisualStyleBackColor = true;
        // 
        // _fovHint
        // 
        _fovHint.AutoSize = true;
        _fovHint.Dock = DockStyle.Top;
        _fovHint.ForeColor = SystemColors.GrayText;
        _fovHint.Location = new Point(3, 42);
        _fovHint.MaximumSize = new Size(860, 0);
        _fovHint.Name = "_fovHint";
        _fovHint.Size = new Size(860, 64);
        _fovHint.TabIndex = 1;
        _fovHint.Text = "Shift export so the needle at FOV center sits over the taught point (ref pixel to FOV center).";
        // 
        // _angleGroup
        // 
        _angleGroup.AutoSize = true;
        _angleGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _angleGroup.Controls.Add(_angleLayout);
        _angleGroup.Dock = DockStyle.Top;
        _angleGroup.Location = new Point(3, 563);
        _angleGroup.Margin = new Padding(3, 3, 3, 8);
        _angleGroup.Name = "_angleGroup";
        _angleGroup.Padding = new Padding(12, 8, 12, 12);
        _angleGroup.Size = new Size(982, 228);
        _angleGroup.TabIndex = 2;
        _angleGroup.TabStop = false;
        _angleGroup.Text = "Angle";
        // 
        // _angleLayout
        // 
        _angleLayout.AutoSize = true;
        _angleLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _angleLayout.ColumnCount = 1;
        _angleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _angleLayout.Controls.Add(_rotationPanel, 0, 0);
        _angleLayout.Controls.Add(_angleHint, 0, 1);
        _angleLayout.Controls.Add(_angleHint2, 0, 2);
        _angleLayout.Dock = DockStyle.Top;
        _angleLayout.Location = new Point(12, 40);
        _angleLayout.Name = "_angleLayout";
        _angleLayout.RowCount = 3;
        _angleLayout.RowStyles.Add(new RowStyle());
        _angleLayout.RowStyles.Add(new RowStyle());
        _angleLayout.RowStyles.Add(new RowStyle());
        _angleLayout.Size = new Size(958, 176);
        _angleLayout.TabIndex = 0;
        // 
        // _rotationPanel
        // 
        _rotationPanel.AutoSize = true;
        _rotationPanel.Controls.Add(_rotationNone);
        _rotationPanel.Controls.Add(_rotation90);
        _rotationPanel.Controls.Add(_rotation180);
        _rotationPanel.Dock = DockStyle.Top;
        _rotationPanel.Location = new Point(3, 3);
        _rotationPanel.Name = "_rotationPanel";
        _rotationPanel.Size = new Size(952, 42);
        _rotationPanel.TabIndex = 0;
        _rotationPanel.WrapContents = false;
        // 
        // _rotationNone
        // 
        _rotationNone.AutoSize = true;
        _rotationNone.Location = new Point(3, 3);
        _rotationNone.Margin = new Padding(3, 3, 24, 3);
        _rotationNone.Name = "_rotationNone";
        _rotationNone.Size = new Size(104, 36);
        _rotationNone.TabIndex = 0;
        _rotationNone.Text = "None";
        _rotationNone.UseVisualStyleBackColor = true;
        // 
        // _rotation90
        // 
        _rotation90.AutoSize = true;
        _rotation90.Location = new Point(134, 3);
        _rotation90.Margin = new Padding(3, 3, 24, 3);
        _rotation90.Name = "_rotation90";
        _rotation90.Size = new Size(156, 36);
        _rotation90.TabIndex = 1;
        _rotation90.Text = "90 Degree";
        _rotation90.UseVisualStyleBackColor = true;
        // 
        // _rotation180
        // 
        _rotation180.AutoSize = true;
        _rotation180.Checked = true;
        _rotation180.Location = new Point(317, 3);
        _rotation180.Name = "_rotation180";
        _rotation180.Size = new Size(169, 36);
        _rotation180.TabIndex = 2;
        _rotation180.TabStop = true;
        _rotation180.Text = "180 Degree";
        _rotation180.UseVisualStyleBackColor = true;
        // 
        // _angleHint
        // 
        _angleHint.AutoSize = true;
        _angleHint.Dock = DockStyle.Top;
        _angleHint.ForeColor = SystemColors.GrayText;
        _angleHint.Location = new Point(3, 48);
        _angleHint.MaximumSize = new Size(860, 0);
        _angleHint.Name = "_angleHint";
        _angleHint.Size = new Size(860, 64);
        _angleHint.TabIndex = 1;
        _angleHint.Text = "Rotate exported XY coordinates CCW after the coordinate mode is applied. Same rotation for absolute and offset modes.";
        // 
        // _angleHint2
        // 
        _angleHint2.AutoSize = true;
        _angleHint2.Dock = DockStyle.Top;
        _angleHint2.ForeColor = SystemColors.GrayText;
        _angleHint2.Location = new Point(3, 112);
        _angleHint2.MaximumSize = new Size(860, 0);
        _angleHint2.Name = "_angleHint2";
        _angleHint2.Size = new Size(860, 64);
        _angleHint2.TabIndex = 2;
        _angleHint2.Text = "180 deg replaces the former fixed-camera inversion. Does not change the frame on the image or export R.";
        // 
        // _needleGroup
        // 
        _needleGroup.AutoSize = true;
        _needleGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _needleGroup.Controls.Add(_needleLayout);
        _needleGroup.Dock = DockStyle.Top;
        _needleGroup.Location = new Point(3, 802);
        _needleGroup.Margin = new Padding(3, 3, 3, 8);
        _needleGroup.Name = "_needleGroup";
        _needleGroup.Padding = new Padding(12, 8, 12, 12);
        _needleGroup.Size = new Size(982, 357);
        _needleGroup.TabIndex = 3;
        _needleGroup.TabStop = false;
        _needleGroup.Text = "Needle offset";
        // 
        // _needleLayout
        // 
        _needleLayout.AutoSize = true;
        _needleLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _needleLayout.ColumnCount = 1;
        _needleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _needleLayout.Controls.Add(_useNeedleOffset, 0, 0);
        _needleLayout.Controls.Add(_needleOffsetLayout, 0, 1);
        _needleLayout.Controls.Add(_needleHint, 0, 2);
        _needleLayout.Dock = DockStyle.Top;
        _needleLayout.Location = new Point(12, 40);
        _needleLayout.Name = "_needleLayout";
        _needleLayout.RowCount = 3;
        _needleLayout.RowStyles.Add(new RowStyle());
        _needleLayout.RowStyles.Add(new RowStyle());
        _needleLayout.RowStyles.Add(new RowStyle());
        _needleLayout.Size = new Size(958, 305);
        _needleLayout.TabIndex = 0;
        // 
        // _useNeedleOffset
        // 
        _useNeedleOffset.AutoSize = true;
        _useNeedleOffset.Dock = DockStyle.Top;
        _useNeedleOffset.Location = new Point(3, 3);
        _useNeedleOffset.Name = "_useNeedleOffset";
        _useNeedleOffset.Size = new Size(952, 36);
        _useNeedleOffset.TabIndex = 0;
        _useNeedleOffset.Text = "Apply needle offset to all export coordinates";
        _useNeedleOffset.UseVisualStyleBackColor = true;
        // 
        // _needleOffsetLayout
        // 
        _needleOffsetLayout.AutoSize = true;
        _needleOffsetLayout.ColumnCount = 2;
        _needleOffsetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        _needleOffsetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _needleOffsetLayout.Controls.Add(_needleXLabel, 0, 0);
        _needleOffsetLayout.Controls.Add(_needleX, 1, 0);
        _needleOffsetLayout.Controls.Add(_needleYLabel, 0, 1);
        _needleOffsetLayout.Controls.Add(_needleY, 1, 1);
        _needleOffsetLayout.Controls.Add(_needleRLabel, 0, 2);
        _needleOffsetLayout.Controls.Add(_needleR, 1, 2);
        _needleOffsetLayout.Dock = DockStyle.Top;
        _needleOffsetLayout.Location = new Point(15, 46);
        _needleOffsetLayout.Margin = new Padding(15, 4, 3, 3);
        _needleOffsetLayout.Name = "_needleOffsetLayout";
        _needleOffsetLayout.RowCount = 3;
        _needleOffsetLayout.RowStyles.Add(new RowStyle());
        _needleOffsetLayout.RowStyles.Add(new RowStyle());
        _needleOffsetLayout.RowStyles.Add(new RowStyle());
        _needleOffsetLayout.Size = new Size(940, 192);
        _needleOffsetLayout.TabIndex = 1;
        // 
        // _needleXLabel
        // 
        _needleXLabel.AutoSize = true;
        _needleXLabel.Dock = DockStyle.Fill;
        _needleXLabel.Location = new Point(3, 0);
        _needleXLabel.Name = "_needleXLabel";
        _needleXLabel.Size = new Size(114, 64);
        _needleXLabel.TabIndex = 0;
        _needleXLabel.Text = "Needle X mm";
        _needleXLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _needleX
        // 
        _needleX.DecimalPlaces = 3;
        _needleX.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
        _needleX.Location = new Point(123, 3);
        _needleX.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        _needleX.Minimum = new decimal(new int[] { 5000, 0, 0, int.MinValue });
        _needleX.Name = "_needleX";
        _needleX.Size = new Size(120, 39);
        _needleX.TabIndex = 1;
        // 
        // _needleYLabel
        // 
        _needleYLabel.AutoSize = true;
        _needleYLabel.Dock = DockStyle.Fill;
        _needleYLabel.Location = new Point(3, 64);
        _needleYLabel.Name = "_needleYLabel";
        _needleYLabel.Size = new Size(114, 64);
        _needleYLabel.TabIndex = 2;
        _needleYLabel.Text = "Needle Y mm";
        _needleYLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _needleY
        // 
        _needleY.DecimalPlaces = 3;
        _needleY.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
        _needleY.Location = new Point(123, 67);
        _needleY.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        _needleY.Minimum = new decimal(new int[] { 5000, 0, 0, int.MinValue });
        _needleY.Name = "_needleY";
        _needleY.Size = new Size(120, 39);
        _needleY.TabIndex = 3;
        // 
        // _needleRLabel
        // 
        _needleRLabel.AutoSize = true;
        _needleRLabel.Dock = DockStyle.Fill;
        _needleRLabel.Location = new Point(3, 128);
        _needleRLabel.Name = "_needleRLabel";
        _needleRLabel.Size = new Size(114, 64);
        _needleRLabel.TabIndex = 4;
        _needleRLabel.Text = "Needle R deg";
        _needleRLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _needleR
        // 
        _needleR.DecimalPlaces = 2;
        _needleR.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        _needleR.Location = new Point(123, 131);
        _needleR.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
        _needleR.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
        _needleR.Name = "_needleR";
        _needleR.Size = new Size(120, 39);
        _needleR.TabIndex = 5;
        // 
        // _needleHint
        // 
        _needleHint.AutoSize = true;
        _needleHint.Dock = DockStyle.Top;
        _needleHint.ForeColor = SystemColors.GrayText;
        _needleHint.Location = new Point(3, 241);
        _needleHint.MaximumSize = new Size(860, 0);
        _needleHint.Name = "_needleHint";
        _needleHint.Size = new Size(860, 64);
        _needleHint.TabIndex = 2;
        _needleHint.Text = "When enabled, needle offset is included in every export value. Do not add it again on the robot.";
        // 
        // _calibrationInfo
        // 
        _calibrationInfo.AutoSize = true;
        _calibrationInfo.Dock = DockStyle.Top;
        _calibrationInfo.Location = new Point(3, 1167);
        _calibrationInfo.MaximumSize = new Size(880, 0);
        _calibrationInfo.Name = "_calibrationInfo";
        _calibrationInfo.Size = new Size(880, 32);
        _calibrationInfo.TabIndex = 4;
        _calibrationInfo.Text = "Calibration";
        // 
        // _beadPage
        // 
        _beadPage.Controls.Add(_beadLayout);
        _beadPage.Location = new Point(8, 46);
        _beadPage.Name = "_beadPage";
        _beadPage.Padding = new Padding(12);
        _beadPage.Size = new Size(920, 558);
        _beadPage.TabIndex = 1;
        _beadPage.Text = "Bead feed";
        _beadPage.UseVisualStyleBackColor = true;
        // 
        // _beadLayout
        // 
        _beadLayout.AutoSize = true;
        _beadLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _beadLayout.ColumnCount = 2;
        _beadLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        _beadLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _beadLayout.Controls.Add(_needleDiameterLabel, 0, 0);
        _beadLayout.Controls.Add(_needleDiameter, 1, 0);
        _beadLayout.Controls.Add(_flowLabel, 0, 1);
        _beadLayout.Controls.Add(_flow, 1, 1);
        _beadLayout.Controls.Add(_travelLabel, 0, 2);
        _beadLayout.Controls.Add(_travel, 1, 2);
        _beadLayout.Controls.Add(_minSpeedLabel, 0, 3);
        _beadLayout.Controls.Add(_minSpeed, 1, 3);
        _beadLayout.Controls.Add(_maxSpeedLabel, 0, 4);
        _beadLayout.Controls.Add(_maxSpeed, 1, 4);
        _beadLayout.Controls.Add(_preview, 0, 5);
        _beadLayout.Controls.Add(_beadHint, 0, 6);
        _beadLayout.Dock = DockStyle.Top;
        _beadLayout.Location = new Point(12, 12);
        _beadLayout.Name = "_beadLayout";
        _beadLayout.RowCount = 7;
        _beadLayout.RowStyles.Add(new RowStyle());
        _beadLayout.RowStyles.Add(new RowStyle());
        _beadLayout.RowStyles.Add(new RowStyle());
        _beadLayout.RowStyles.Add(new RowStyle());
        _beadLayout.RowStyles.Add(new RowStyle());
        _beadLayout.RowStyles.Add(new RowStyle());
        _beadLayout.RowStyles.Add(new RowStyle());
        _beadLayout.Size = new Size(896, 479);
        _beadLayout.TabIndex = 0;
        // 
        // _needleDiameterLabel
        // 
        _needleDiameterLabel.AutoSize = true;
        _needleDiameterLabel.Dock = DockStyle.Fill;
        _needleDiameterLabel.Location = new Point(3, 0);
        _needleDiameterLabel.Name = "_needleDiameterLabel";
        _needleDiameterLabel.Size = new Size(174, 64);
        _needleDiameterLabel.TabIndex = 0;
        _needleDiameterLabel.Text = "Needle diameter mm";
        _needleDiameterLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _needleDiameter
        // 
        _needleDiameter.DecimalPlaces = 3;
        _needleDiameter.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
        _needleDiameter.Location = new Point(183, 3);
        _needleDiameter.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        _needleDiameter.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
        _needleDiameter.Name = "_needleDiameter";
        _needleDiameter.Size = new Size(120, 39);
        _needleDiameter.TabIndex = 1;
        _needleDiameter.Value = new decimal(new int[] { 48, 0, 0, 131072 });
        // 
        // _flowLabel
        // 
        _flowLabel.AutoSize = true;
        _flowLabel.Dock = DockStyle.Fill;
        _flowLabel.Location = new Point(3, 64);
        _flowLabel.Name = "_flowLabel";
        _flowLabel.Size = new Size(174, 45);
        _flowLabel.TabIndex = 2;
        _flowLabel.Text = "Flow mm3/s";
        _flowLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _flow
        // 
        _flow.DecimalPlaces = 3;
        _flow.Increment = new decimal(new int[] { 1, 0, 0, 196608 });
        _flow.Location = new Point(183, 67);
        _flow.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
        _flow.Minimum = new decimal(new int[] { 1, 0, 0, 196608 });
        _flow.Name = "_flow";
        _flow.Size = new Size(120, 39);
        _flow.TabIndex = 3;
        _flow.Value = new decimal(new int[] { 9048, 0, 0, 196608 });
        // 
        // _travelLabel
        // 
        _travelLabel.AutoSize = true;
        _travelLabel.Dock = DockStyle.Fill;
        _travelLabel.Location = new Point(3, 109);
        _travelLabel.Name = "_travelLabel";
        _travelLabel.Size = new Size(174, 64);
        _travelLabel.TabIndex = 4;
        _travelLabel.Text = "Travel speed mm/s";
        _travelLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _travel
        // 
        _travel.DecimalPlaces = 3;
        _travel.Location = new Point(183, 112);
        _travel.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        _travel.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
        _travel.Name = "_travel";
        _travel.Size = new Size(120, 39);
        _travel.TabIndex = 5;
        _travel.Value = new decimal(new int[] { 200, 0, 0, 0 });
        // 
        // _minSpeedLabel
        // 
        _minSpeedLabel.AutoSize = true;
        _minSpeedLabel.Dock = DockStyle.Fill;
        _minSpeedLabel.Location = new Point(3, 173);
        _minSpeedLabel.Name = "_minSpeedLabel";
        _minSpeedLabel.Size = new Size(174, 64);
        _minSpeedLabel.TabIndex = 6;
        _minSpeedLabel.Text = "Minimum speed mm/s";
        _minSpeedLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _minSpeed
        // 
        _minSpeed.DecimalPlaces = 3;
        _minSpeed.Location = new Point(183, 176);
        _minSpeed.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        _minSpeed.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
        _minSpeed.Name = "_minSpeed";
        _minSpeed.Size = new Size(120, 39);
        _minSpeed.TabIndex = 7;
        _minSpeed.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // _maxSpeedLabel
        // 
        _maxSpeedLabel.AutoSize = true;
        _maxSpeedLabel.Dock = DockStyle.Fill;
        _maxSpeedLabel.Location = new Point(3, 237);
        _maxSpeedLabel.Name = "_maxSpeedLabel";
        _maxSpeedLabel.Size = new Size(174, 64);
        _maxSpeedLabel.TabIndex = 8;
        _maxSpeedLabel.Text = "Maximum speed mm/s";
        _maxSpeedLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // _maxSpeed
        // 
        _maxSpeed.DecimalPlaces = 3;
        _maxSpeed.Location = new Point(183, 240);
        _maxSpeed.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        _maxSpeed.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
        _maxSpeed.Name = "_maxSpeed";
        _maxSpeed.Size = new Size(120, 39);
        _maxSpeed.TabIndex = 9;
        _maxSpeed.Value = new decimal(new int[] { 2000, 0, 0, 0 });
        // 
        // _preview
        // 
        _preview.AutoSize = true;
        _beadLayout.SetColumnSpan(_preview, 2);
        _preview.Dock = DockStyle.Top;
        _preview.Location = new Point(3, 313);
        _preview.Margin = new Padding(3, 12, 3, 6);
        _preview.MaximumSize = new Size(860, 0);
        _preview.Name = "_preview";
        _preview.Size = new Size(860, 32);
        _preview.TabIndex = 10;
        _preview.Text = "Preview";
        // 
        // _beadHint
        // 
        _beadHint.AutoSize = true;
        _beadLayout.SetColumnSpan(_beadHint, 2);
        _beadHint.Dock = DockStyle.Top;
        _beadHint.ForeColor = SystemColors.GrayText;
        _beadHint.Location = new Point(3, 351);
        _beadHint.MaximumSize = new Size(860, 0);
        _beadHint.Name = "_beadHint";
        _beadHint.Size = new Size(860, 128);
        _beadHint.TabIndex = 11;
        _beadHint.Text = resources.GetString("_beadHint.Text");
        // 
        // _buttonBar
        // 
        _buttonBar.Controls.Add(_cancelButton);
        _buttonBar.Controls.Add(_okButton);
        _buttonBar.Dock = DockStyle.Bottom;
        _buttonBar.Location = new Point(12, 939);
        _buttonBar.Name = "_buttonBar";
        _buttonBar.Size = new Size(1054, 64);
        _buttonBar.TabIndex = 1;
        // 
        // _cancelButton
        // 
        _cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Location = new Point(920, 10);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(132, 44);
        _cancelButton.TabIndex = 1;
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;
        // 
        // _okButton
        // 
        _okButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _okButton.Location = new Point(782, 10);
        _okButton.Name = "_okButton";
        _okButton.Size = new Size(132, 44);
        _okButton.TabIndex = 0;
        _okButton.Text = "OK";
        _okButton.UseVisualStyleBackColor = true;
        // 
        // FanucFeedDialog
        // 
        AcceptButton = _okButton;
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(1078, 1015);
        Controls.Add(_tabs);
        Controls.Add(_buttonBar);
        Font = new Font("Segoe UI", 9F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(720, 480);
        Name = "FanucFeedDialog";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Fanuc feed export";
        _tabs.ResumeLayout(false);
        _exportPage.ResumeLayout(false);
        _exportScroll.ResumeLayout(false);
        _exportScroll.PerformLayout();
        _exportLayout.ResumeLayout(false);
        _exportLayout.PerformLayout();
        _frameGroup.ResumeLayout(false);
        _frameGroup.PerformLayout();
        _frameLayout.ResumeLayout(false);
        _frameLayout.PerformLayout();
        _frameNumbers.ResumeLayout(false);
        _frameNumbers.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_userFrame).EndInit();
        ((System.ComponentModel.ISupportInitialize)_userTool).EndInit();
        _fovGroup.ResumeLayout(false);
        _fovGroup.PerformLayout();
        _fovLayout.ResumeLayout(false);
        _fovLayout.PerformLayout();
        _angleGroup.ResumeLayout(false);
        _angleGroup.PerformLayout();
        _angleLayout.ResumeLayout(false);
        _angleLayout.PerformLayout();
        _rotationPanel.ResumeLayout(false);
        _rotationPanel.PerformLayout();
        _needleGroup.ResumeLayout(false);
        _needleGroup.PerformLayout();
        _needleLayout.ResumeLayout(false);
        _needleLayout.PerformLayout();
        _needleOffsetLayout.ResumeLayout(false);
        _needleOffsetLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_needleX).EndInit();
        ((System.ComponentModel.ISupportInitialize)_needleY).EndInit();
        ((System.ComponentModel.ISupportInitialize)_needleR).EndInit();
        _beadPage.ResumeLayout(false);
        _beadPage.PerformLayout();
        _beadLayout.ResumeLayout(false);
        _beadLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_needleDiameter).EndInit();
        ((System.ComponentModel.ISupportInitialize)_flow).EndInit();
        ((System.ComponentModel.ISupportInitialize)_travel).EndInit();
        ((System.ComponentModel.ISupportInitialize)_minSpeed).EndInit();
        ((System.ComponentModel.ISupportInitialize)_maxSpeed).EndInit();
        _buttonBar.ResumeLayout(false);
        ResumeLayout(false);
    }

    private TabControl _tabs = null!;
    private TabPage _exportPage = null!;
    private Panel _exportScroll = null!;
    private TableLayoutPanel _exportLayout = null!;
    private GroupBox _frameGroup = null!;
    private TableLayoutPanel _frameLayout = null!;
    private FlowLayoutPanel _frameNumbers = null!;
    private Label _userFrameLabel = null!;
    private NumericUpDown _userFrame = null!;
    private Label _userToolLabel = null!;
    private NumericUpDown _userTool = null!;
    private RadioButton _modeAbsolute = null!;
    private Label _absoluteHint = null!;
    private RadioButton _modeFrameOffset = null!;
    private Label _frameHint = null!;
    private RadioButton _modeCalibOffset = null!;
    private Label _calibHint = null!;
    private GroupBox _fovGroup = null!;
    private TableLayoutPanel _fovLayout = null!;
    private CheckBox _fovCenterNeedle = null!;
    private Label _fovHint = null!;
    private GroupBox _angleGroup = null!;
    private TableLayoutPanel _angleLayout = null!;
    private FlowLayoutPanel _rotationPanel = null!;
    private RadioButton _rotationNone = null!;
    private RadioButton _rotation90 = null!;
    private RadioButton _rotation180 = null!;
    private Label _angleHint = null!;
    private Label _angleHint2 = null!;
    private GroupBox _needleGroup = null!;
    private TableLayoutPanel _needleLayout = null!;
    private CheckBox _useNeedleOffset = null!;
    private TableLayoutPanel _needleOffsetLayout = null!;
    private Label _needleXLabel = null!;
    private NumericUpDown _needleX = null!;
    private Label _needleYLabel = null!;
    private NumericUpDown _needleY = null!;
    private Label _needleRLabel = null!;
    private NumericUpDown _needleR = null!;
    private Label _needleHint = null!;
    private Label _calibrationInfo = null!;
    private TabPage _beadPage = null!;
    private TableLayoutPanel _beadLayout = null!;
    private Label _needleDiameterLabel = null!;
    private NumericUpDown _needleDiameter = null!;
    private Label _flowLabel = null!;
    private NumericUpDown _flow = null!;
    private Label _travelLabel = null!;
    private NumericUpDown _travel = null!;
    private Label _minSpeedLabel = null!;
    private NumericUpDown _minSpeed = null!;
    private Label _maxSpeedLabel = null!;
    private NumericUpDown _maxSpeed = null!;
    private Label _preview = null!;
    private Label _beadHint = null!;
    private Panel _buttonBar = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;
}
