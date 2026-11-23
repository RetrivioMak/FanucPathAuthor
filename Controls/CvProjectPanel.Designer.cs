#nullable enable
namespace DispensePathAuthor.Controls;

partial class CvProjectPanel
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
        _tileBottom = new TableLayoutPanel();
        _tileButtons = new FlowLayoutPanel();
        _openImageButton = new Button();
        _addFovButton = new Button();
        _removeTileButton = new Button();
        _focusTileButton = new Button();
        _poseGroup = new GroupBox();
        _poseStack = new FlowLayoutPanel();
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
        _poseStack.SuspendLayout();
        _poseLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_captureX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_captureY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_captureR).BeginInit();
        _infoPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _layout
        // 
        _layout.AutoSize = true;
        _layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_frameGroup, 0, 0);
        _layout.Controls.Add(_tileList, 0, 1);
        _layout.Controls.Add(_tileBottom, 0, 2);
        _layout.Controls.Add(_calibrationLabel, 0, 3);
        _layout.Dock = DockStyle.Top;
        _layout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _layout.Location = new Point(8, 8);
        _layout.Margin = new Padding(0);
        _layout.Name = "_layout";
        _layout.RowCount = 4;
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        _layout.RowStyles.Add(new RowStyle());
        _layout.RowStyles.Add(new RowStyle());
        _layout.Size = new Size(605, 1090);
        _layout.TabIndex = 0;
        // 
        // _frameGroup
        // 
        _frameGroup.AutoSize = true;
        _frameGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _frameGroup.Controls.Add(_frameLayout);
        _frameGroup.Dock = DockStyle.Fill;
        _frameGroup.Location = new Point(0, 0);
        _frameGroup.Margin = new Padding(0, 0, 0, 8);
        _frameGroup.Name = "_frameGroup";
        _frameGroup.Padding = new Padding(8);
        _frameGroup.Size = new Size(605, 453);
        _frameGroup.TabIndex = 0;
        _frameGroup.TabStop = false;
        _frameGroup.Text = "Frame (sensor px)";
        // 
        // _frameLayout
        // 
        _frameLayout.AutoSize = true;
        _frameLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _frameLayout.ColumnCount = 2;
        _frameLayout.ColumnStyles.Add(new ColumnStyle());
        _frameLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
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
        _frameLayout.Dock = DockStyle.Top;
        _frameLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _frameLayout.Location = new Point(8, 40);
        _frameLayout.Margin = new Padding(0);
        _frameLayout.Name = "_frameLayout";
        _frameLayout.RowCount = 7;
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.RowStyles.Add(new RowStyle());
        _frameLayout.Size = new Size(589, 405);
        _frameLayout.TabIndex = 0;
        // 
        // _originXLabel
        // 
        _originXLabel.Anchor = AnchorStyles.Left;
        _originXLabel.AutoSize = true;
        _originXLabel.Location = new Point(0, 5);
        _originXLabel.Margin = new Padding(0, 4, 12, 4);
        _originXLabel.Name = "_originXLabel";
        _originXLabel.Size = new Size(101, 32);
        _originXLabel.TabIndex = 0;
        _originXLabel.Text = "Origin X";
        // 
        // _originX
        // 
        _originX.Anchor = AnchorStyles.Left;
        _originX.Location = new Point(136, 2);
        _originX.Margin = new Padding(0, 2, 0, 2);
        _originX.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        _originX.Name = "_originX";
        _originX.Size = new Size(120, 39);
        _originX.TabIndex = 1;
        // 
        // _originYLabel
        // 
        _originYLabel.Anchor = AnchorStyles.Left;
        _originYLabel.AutoSize = true;
        _originYLabel.Location = new Point(0, 48);
        _originYLabel.Margin = new Padding(0, 4, 12, 4);
        _originYLabel.Name = "_originYLabel";
        _originYLabel.Size = new Size(100, 32);
        _originYLabel.TabIndex = 2;
        _originYLabel.Text = "Origin Y";
        // 
        // _originY
        // 
        _originY.Anchor = AnchorStyles.Left;
        _originY.Location = new Point(136, 45);
        _originY.Margin = new Padding(0, 2, 0, 2);
        _originY.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
        _originY.Name = "_originY";
        _originY.Size = new Size(120, 39);
        _originY.TabIndex = 3;
        // 
        // _angleLabel
        // 
        _angleLabel.Anchor = AnchorStyles.Left;
        _angleLabel.AutoSize = true;
        _angleLabel.Location = new Point(0, 91);
        _angleLabel.Margin = new Padding(0, 4, 12, 4);
        _angleLabel.Name = "_angleLabel";
        _angleLabel.Size = new Size(124, 32);
        _angleLabel.TabIndex = 4;
        _angleLabel.Text = "Angle deg";
        // 
        // _angle
        // 
        _angle.Anchor = AnchorStyles.Left;
        _angle.Location = new Point(136, 88);
        _angle.Margin = new Padding(0, 2, 0, 2);
        _angle.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
        _angle.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
        _angle.Name = "_angle";
        _angle.Size = new Size(120, 39);
        _angle.TabIndex = 5;
        // 
        // _invertY
        // 
        _invertY.Anchor = AnchorStyles.Left;
        _invertY.AutoSize = true;
        _frameLayout.SetColumnSpan(_invertY, 2);
        _invertY.Location = new Point(0, 137);
        _invertY.Margin = new Padding(0, 8, 0, 0);
        _invertY.Name = "_invertY";
        _invertY.Size = new Size(400, 36);
        _invertY.TabIndex = 6;
        _invertY.Text = "Invert frame Y (Fanuc right-hand)";
        // 
        // _invertHint
        // 
        _invertHint.AutoSize = true;
        _frameLayout.SetColumnSpan(_invertHint, 2);
        _invertHint.Location = new Point(0, 173);
        _invertHint.Margin = new Padding(0, 0, 0, 4);
        _invertHint.MaximumSize = new Size(560, 0);
        _invertHint.Name = "_invertHint";
        _invertHint.Size = new Size(536, 96);
        _invertHint.TabIndex = 7;
        _invertHint.Text = "Export XY becomes millimeters along this frame. +Y is counterclockwise from +X. Set export rotation to None.";
        // 
        // _applyModePanel
        // 
        _applyModePanel.AutoSize = true;
        _applyModePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _frameLayout.SetColumnSpan(_applyModePanel, 2);
        _applyModePanel.Controls.Add(_moveShapesRadio);
        _applyModePanel.Controls.Add(_frameOnlyRadio);
        _applyModePanel.Dock = DockStyle.Fill;
        _applyModePanel.FlowDirection = FlowDirection.TopDown;
        _applyModePanel.Location = new Point(0, 281);
        _applyModePanel.Margin = new Padding(0, 8, 0, 0);
        _applyModePanel.Name = "_applyModePanel";
        _applyModePanel.Size = new Size(589, 74);
        _applyModePanel.TabIndex = 8;
        _applyModePanel.WrapContents = false;
        // 
        // _moveShapesRadio
        // 
        _moveShapesRadio.AutoSize = true;
        _moveShapesRadio.Checked = true;
        _moveShapesRadio.Location = new Point(0, 0);
        _moveShapesRadio.Margin = new Padding(0, 0, 0, 2);
        _moveShapesRadio.Name = "_moveShapesRadio";
        _moveShapesRadio.Size = new Size(488, 36);
        _moveShapesRadio.TabIndex = 0;
        _moveShapesRadio.TabStop = true;
        _moveShapesRadio.Text = "Move shapes with frame (keep image position)";
        // 
        // _frameOnlyRadio
        // 
        _frameOnlyRadio.AutoSize = true;
        _frameOnlyRadio.Location = new Point(0, 38);
        _frameOnlyRadio.Margin = new Padding(0);
        _frameOnlyRadio.Name = "_frameOnlyRadio";
        _frameOnlyRadio.Size = new Size(405, 36);
        _frameOnlyRadio.TabIndex = 1;
        _frameOnlyRadio.Text = "Update frame only (keep local mm)";
        // 
        // _frameButtons
        // 
        _frameButtons.AutoSize = true;
        _frameButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _frameLayout.SetColumnSpan(_frameButtons, 2);
        _frameButtons.Controls.Add(_editFrameButton);
        _frameButtons.Controls.Add(_applyFrameButton);
        _frameButtons.Controls.Add(_cancelFrameButton);
        _frameButtons.Dock = DockStyle.Fill;
        _frameButtons.Location = new Point(0, 363);
        _frameButtons.Margin = new Padding(0, 8, 0, 0);
        _frameButtons.Name = "_frameButtons";
        _frameButtons.Size = new Size(589, 42);
        _frameButtons.TabIndex = 9;
        // 
        // _editFrameButton
        // 
        _editFrameButton.AutoSize = true;
        _editFrameButton.Location = new Point(0, 0);
        _editFrameButton.Margin = new Padding(0, 0, 6, 0);
        _editFrameButton.Name = "_editFrameButton";
        _editFrameButton.Size = new Size(137, 42);
        _editFrameButton.TabIndex = 0;
        _editFrameButton.Text = "Edit Frame";
        // 
        // _applyFrameButton
        // 
        _applyFrameButton.AutoSize = true;
        _applyFrameButton.Location = new Point(143, 0);
        _applyFrameButton.Margin = new Padding(0, 0, 6, 0);
        _applyFrameButton.Name = "_applyFrameButton";
        _applyFrameButton.Size = new Size(85, 42);
        _applyFrameButton.TabIndex = 1;
        _applyFrameButton.Text = "Apply";
        // 
        // _cancelFrameButton
        // 
        _cancelFrameButton.AutoSize = true;
        _cancelFrameButton.Location = new Point(234, 0);
        _cancelFrameButton.Margin = new Padding(0);
        _cancelFrameButton.Name = "_cancelFrameButton";
        _cancelFrameButton.Size = new Size(95, 42);
        _cancelFrameButton.TabIndex = 2;
        _cancelFrameButton.Text = "Cancel";
        // 
        // _tileList
        // 
        _tileList.Columns.Add("Name", 100);
        _tileList.Columns.Add("Capture X", 70);
        _tileList.Columns.Add("Capture Y", 70);
        _tileList.Columns.Add("R", 45);
        _tileList.Columns.Add("Active", 55);
        _tileList.Dock = DockStyle.Fill;
        _tileList.FullRowSelect = true;
        _tileList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _tileList.Location = new Point(0, 461);
        _tileList.Margin = new Padding(0, 0, 0, 8);
        _tileList.MinimumSize = new Size(0, 96);
        _tileList.Name = "_tileList";
        _tileList.Size = new Size(605, 96);
        _tileList.TabIndex = 1;
        _tileList.UseCompatibleStateImageBehavior = false;
        _tileList.View = View.Details;
        // 
        // _tileBottom
        // 
        _tileBottom.AutoSize = true;
        _tileBottom.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _tileBottom.ColumnCount = 1;
        _tileBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _tileBottom.Controls.Add(_tileButtons, 0, 0);
        _tileBottom.Controls.Add(_poseGroup, 0, 1);
        _tileBottom.Controls.Add(_infoPanel, 0, 2);
        _tileBottom.Dock = DockStyle.Fill;
        _tileBottom.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _tileBottom.Location = new Point(0, 557);
        _tileBottom.Margin = new Padding(0);
        _tileBottom.Name = "_tileBottom";
        _tileBottom.RowCount = 3;
        _tileBottom.RowStyles.Add(new RowStyle());
        _tileBottom.RowStyles.Add(new RowStyle());
        _tileBottom.RowStyles.Add(new RowStyle());
        _tileBottom.Size = new Size(605, 493);
        _tileBottom.TabIndex = 2;
        // 
        // _tileButtons
        // 
        _tileButtons.AutoSize = true;
        _tileButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _tileButtons.Controls.Add(_openImageButton);
        _tileButtons.Controls.Add(_addFovButton);
        _tileButtons.Controls.Add(_removeTileButton);
        _tileButtons.Controls.Add(_focusTileButton);
        _tileButtons.Dock = DockStyle.Fill;
        _tileButtons.Location = new Point(0, 0);
        _tileButtons.Margin = new Padding(0, 0, 0, 6);
        _tileButtons.Name = "_tileButtons";
        _tileButtons.Size = new Size(605, 46);
        _tileButtons.TabIndex = 0;
        // 
        // _openImageButton
        // 
        _openImageButton.AutoSize = true;
        _openImageButton.Location = new Point(0, 0);
        _openImageButton.Margin = new Padding(0, 0, 6, 4);
        _openImageButton.Name = "_openImageButton";
        _openImageButton.Size = new Size(171, 42);
        _openImageButton.TabIndex = 0;
        _openImageButton.Text = "Open image...";
        // 
        // _addFovButton
        // 
        _addFovButton.AutoSize = true;
        _addFovButton.Location = new Point(177, 0);
        _addFovButton.Margin = new Padding(0, 0, 6, 4);
        _addFovButton.Name = "_addFovButton";
        _addFovButton.Size = new Size(206, 42);
        _addFovButton.TabIndex = 1;
        _addFovButton.Text = "Add FOV image...";
        // 
        // _removeTileButton
        // 
        _removeTileButton.AutoSize = true;
        _removeTileButton.Location = new Point(389, 0);
        _removeTileButton.Margin = new Padding(0, 0, 6, 4);
        _removeTileButton.Name = "_removeTileButton";
        _removeTileButton.Size = new Size(110, 42);
        _removeTileButton.TabIndex = 2;
        _removeTileButton.Text = "Remove";
        // 
        // _focusTileButton
        // 
        _focusTileButton.AutoSize = true;
        _focusTileButton.Location = new Point(505, 0);
        _focusTileButton.Margin = new Padding(0, 0, 6, 4);
        _focusTileButton.Name = "_focusTileButton";
        _focusTileButton.Size = new Size(85, 42);
        _focusTileButton.TabIndex = 3;
        _focusTileButton.Text = "Focus";
        // 
        // _poseGroup
        // 
        _poseGroup.AutoSize = true;
        _poseGroup.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _poseGroup.Controls.Add(_poseStack);
        _poseGroup.Dock = DockStyle.Fill;
        _poseGroup.Location = new Point(0, 52);
        _poseGroup.Margin = new Padding(0, 0, 0, 8);
        _poseGroup.Name = "_poseGroup";
        _poseGroup.Padding = new Padding(8, 4, 8, 8);
        _poseGroup.Size = new Size(605, 219);
        _poseGroup.TabIndex = 1;
        _poseGroup.TabStop = false;
        _poseGroup.Text = "Capture pose (robot ref. pose)";
        // 
        // _poseStack
        // 
        _poseStack.AutoSize = true;
        _poseStack.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _poseStack.Controls.Add(_poseLayout);
        _poseStack.Controls.Add(_resetCaptureButton);
        _poseStack.Dock = DockStyle.Top;
        _poseStack.FlowDirection = FlowDirection.TopDown;
        _poseStack.Location = new Point(8, 36);
        _poseStack.Margin = new Padding(0);
        _poseStack.Name = "_poseStack";
        _poseStack.Size = new Size(589, 175);
        _poseStack.TabIndex = 0;
        _poseStack.WrapContents = false;
        // 
        // _poseLayout
        // 
        _poseLayout.AutoSize = true;
        _poseLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _poseLayout.ColumnCount = 2;
        _poseLayout.ColumnStyles.Add(new ColumnStyle());
        _poseLayout.ColumnStyles.Add(new ColumnStyle());
        _poseLayout.Controls.Add(_captureXLabel, 0, 0);
        _poseLayout.Controls.Add(_captureX, 1, 0);
        _poseLayout.Controls.Add(_captureYLabel, 0, 1);
        _poseLayout.Controls.Add(_captureY, 1, 1);
        _poseLayout.Controls.Add(_captureRLabel, 0, 2);
        _poseLayout.Controls.Add(_captureR, 1, 2);
        _poseLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _poseLayout.Location = new Point(0, 0);
        _poseLayout.Margin = new Padding(0);
        _poseLayout.Name = "_poseLayout";
        _poseLayout.RowCount = 3;
        _poseLayout.RowStyles.Add(new RowStyle());
        _poseLayout.RowStyles.Add(new RowStyle());
        _poseLayout.RowStyles.Add(new RowStyle());
        _poseLayout.Size = new Size(300, 129);
        _poseLayout.TabIndex = 0;
        // 
        // _captureXLabel
        // 
        _captureXLabel.Anchor = AnchorStyles.Left;
        _captureXLabel.AutoSize = true;
        _captureXLabel.Location = new Point(0, 5);
        _captureXLabel.Margin = new Padding(0, 4, 12, 4);
        _captureXLabel.Name = "_captureXLabel";
        _captureXLabel.Size = new Size(168, 32);
        _captureXLabel.TabIndex = 0;
        _captureXLabel.Text = "Capture X mm";
        // 
        // _captureX
        // 
        _captureX.Anchor = AnchorStyles.Left;
        _captureX.DecimalPlaces = 2;
        _captureX.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        _captureX.Location = new Point(180, 2);
        _captureX.Margin = new Padding(0, 2, 0, 2);
        _captureX.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        _captureX.Minimum = new decimal(new int[] { 5000, 0, 0, int.MinValue });
        _captureX.Name = "_captureX";
        _captureX.Size = new Size(120, 39);
        _captureX.TabIndex = 1;
        // 
        // _captureYLabel
        // 
        _captureYLabel.Anchor = AnchorStyles.Left;
        _captureYLabel.AutoSize = true;
        _captureYLabel.Location = new Point(0, 48);
        _captureYLabel.Margin = new Padding(0, 4, 12, 4);
        _captureYLabel.Name = "_captureYLabel";
        _captureYLabel.Size = new Size(167, 32);
        _captureYLabel.TabIndex = 2;
        _captureYLabel.Text = "Capture Y mm";
        // 
        // _captureY
        // 
        _captureY.Anchor = AnchorStyles.Left;
        _captureY.DecimalPlaces = 2;
        _captureY.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        _captureY.Location = new Point(180, 45);
        _captureY.Margin = new Padding(0, 2, 0, 2);
        _captureY.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
        _captureY.Minimum = new decimal(new int[] { 5000, 0, 0, int.MinValue });
        _captureY.Name = "_captureY";
        _captureY.Size = new Size(120, 39);
        _captureY.TabIndex = 3;
        // 
        // _captureRLabel
        // 
        _captureRLabel.Anchor = AnchorStyles.Left;
        _captureRLabel.AutoSize = true;
        _captureRLabel.Location = new Point(0, 91);
        _captureRLabel.Margin = new Padding(0, 4, 12, 4);
        _captureRLabel.Name = "_captureRLabel";
        _captureRLabel.Size = new Size(167, 32);
        _captureRLabel.TabIndex = 4;
        _captureRLabel.Text = "Capture R deg";
        // 
        // _captureR
        // 
        _captureR.Anchor = AnchorStyles.Left;
        _captureR.DecimalPlaces = 2;
        _captureR.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        _captureR.Location = new Point(180, 88);
        _captureR.Margin = new Padding(0, 2, 0, 2);
        _captureR.Maximum = new decimal(new int[] { 360, 0, 0, 0 });
        _captureR.Minimum = new decimal(new int[] { 360, 0, 0, int.MinValue });
        _captureR.Name = "_captureR";
        _captureR.Size = new Size(120, 39);
        _captureR.TabIndex = 5;
        // 
        // _resetCaptureButton
        // 
        _resetCaptureButton.AutoSize = true;
        _resetCaptureButton.Location = new Point(0, 133);
        _resetCaptureButton.Margin = new Padding(0, 4, 0, 0);
        _resetCaptureButton.Name = "_resetCaptureButton";
        _resetCaptureButton.Size = new Size(232, 42);
        _resetCaptureButton.TabIndex = 1;
        _resetCaptureButton.Text = "Use calib. reference";
        // 
        // _infoPanel
        // 
        _infoPanel.AutoSize = true;
        _infoPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _infoPanel.Controls.Add(_modeLabel);
        _infoPanel.Controls.Add(_captureHint);
        _infoPanel.Controls.Add(_offsetLabel);
        _infoPanel.Controls.Add(_worldLabel);
        _infoPanel.Dock = DockStyle.Fill;
        _infoPanel.FlowDirection = FlowDirection.TopDown;
        _infoPanel.Location = new Point(0, 279);
        _infoPanel.Margin = new Padding(0);
        _infoPanel.Name = "_infoPanel";
        _infoPanel.Size = new Size(605, 214);
        _infoPanel.TabIndex = 2;
        _infoPanel.WrapContents = false;
        // 
        // _modeLabel
        // 
        _modeLabel.AutoSize = true;
        _modeLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _modeLabel.Location = new Point(0, 0);
        _modeLabel.Margin = new Padding(0, 0, 0, 2);
        _modeLabel.Name = "_modeLabel";
        _modeLabel.Size = new Size(217, 32);
        _modeLabel.TabIndex = 0;
        _modeLabel.Text = "Mode: Single FOV";
        // 
        // _captureHint
        // 
        _captureHint.AutoSize = true;
        _captureHint.ForeColor = Color.DimGray;
        _captureHint.Location = new Point(0, 34);
        _captureHint.Margin = new Padding(0, 0, 0, 4);
        _captureHint.MaximumSize = new Size(560, 0);
        _captureHint.Name = "_captureHint";
        _captureHint.Size = new Size(540, 64);
        _captureHint.TabIndex = 1;
        _captureHint.Text = "Open the anchor FOV image, define Frame, then teach the path in sensor space.";
        // 
        // _offsetLabel
        // 
        _offsetLabel.AutoSize = true;
        _offsetLabel.Location = new Point(0, 102);
        _offsetLabel.Margin = new Padding(0, 0, 0, 2);
        _offsetLabel.Name = "_offsetLabel";
        _offsetLabel.Size = new Size(193, 32);
        _offsetLabel.TabIndex = 2;
        _offsetLabel.Text = "Canvas offset: —";
        // 
        // _worldLabel
        // 
        _worldLabel.AutoSize = true;
        _worldLabel.Location = new Point(0, 136);
        _worldLabel.Margin = new Padding(0);
        _worldLabel.Name = "_worldLabel";
        _worldLabel.Size = new Size(206, 32);
        _worldLabel.TabIndex = 3;
        _worldLabel.Text = "Tile (0,0) world: —";
        // 
        // _calibrationLabel
        // 
        _calibrationLabel.AutoSize = true;
        _calibrationLabel.Dock = DockStyle.Fill;
        _calibrationLabel.Location = new Point(0, 1058);
        _calibrationLabel.Margin = new Padding(0, 8, 0, 0);
        _calibrationLabel.Name = "_calibrationLabel";
        _calibrationLabel.Size = new Size(605, 32);
        _calibrationLabel.TabIndex = 3;
        _calibrationLabel.Text = "Calibration: not loaded";
        // 
        // CvProjectPanel
        // 
        AutoScroll = true;
        Controls.Add(_layout);
        Name = "CvProjectPanel";
        Padding = new Padding(8);
        Size = new Size(621, 1138);
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        _frameGroup.ResumeLayout(false);
        _frameGroup.PerformLayout();
        _frameLayout.ResumeLayout(false);
        _frameLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_originX).EndInit();
        ((System.ComponentModel.ISupportInitialize)_originY).EndInit();
        ((System.ComponentModel.ISupportInitialize)_angle).EndInit();
        _applyModePanel.ResumeLayout(false);
        _applyModePanel.PerformLayout();
        _frameButtons.ResumeLayout(false);
        _frameButtons.PerformLayout();
        _tileBottom.ResumeLayout(false);
        _tileBottom.PerformLayout();
        _tileButtons.ResumeLayout(false);
        _tileButtons.PerformLayout();
        _poseGroup.ResumeLayout(false);
        _poseGroup.PerformLayout();
        _poseStack.ResumeLayout(false);
        _poseStack.PerformLayout();
        _poseLayout.ResumeLayout(false);
        _poseLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_captureX).EndInit();
        ((System.ComponentModel.ISupportInitialize)_captureY).EndInit();
        ((System.ComponentModel.ISupportInitialize)_captureR).EndInit();
        _infoPanel.ResumeLayout(false);
        _infoPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
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
    private TableLayoutPanel _tileBottom = null!;
    private FlowLayoutPanel _tileButtons = null!;
    private Button _openImageButton = null!;
    private Button _addFovButton = null!;
    private Button _removeTileButton = null!;
    private Button _focusTileButton = null!;
    private GroupBox _poseGroup = null!;
    private FlowLayoutPanel _poseStack = null!;
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
