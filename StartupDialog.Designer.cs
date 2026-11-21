#nullable enable
namespace DispensePathAuthor;

partial class StartupDialog
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
        _heading = new Label();
        _note = new Label();
        _choices = new TableLayoutPanel();
        _cvRow = new Panel();
        _cvLayout = new TableLayoutPanel();
        _cvIcon = new PictureBox();
        _cvText = new FlowLayoutPanel();
        _cvTitle = new Label();
        _cvDetail = new Label();
        _teachRow = new Panel();
        _teachLayout = new TableLayoutPanel();
        _teachIcon = new PictureBox();
        _teachText = new FlowLayoutPanel();
        _teachTitle = new Label();
        _teachDetail = new Label();
        _openRow = new Panel();
        _openLayout = new TableLayoutPanel();
        _openIcon = new PictureBox();
        _openText = new FlowLayoutPanel();
        _openTitle = new Label();
        _openDetail = new Label();
        _buttonRow = new FlowLayoutPanel();
        _cancelButton = new Button();
        _layout.SuspendLayout();
        _choices.SuspendLayout();
        _cvRow.SuspendLayout();
        _cvLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_cvIcon).BeginInit();
        _cvText.SuspendLayout();
        _teachRow.SuspendLayout();
        _teachLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_teachIcon).BeginInit();
        _teachText.SuspendLayout();
        _openRow.SuspendLayout();
        _openLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_openIcon).BeginInit();
        _openText.SuspendLayout();
        _buttonRow.SuspendLayout();
        SuspendLayout();
        //
        // _layout
        //
        _layout.AutoSize = true;
        _layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layout.ColumnCount = 1;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _layout.Controls.Add(_heading, 0, 0);
        _layout.Controls.Add(_note, 0, 1);
        _layout.Controls.Add(_choices, 0, 2);
        _layout.Controls.Add(_buttonRow, 0, 3);
        _layout.Dock = DockStyle.Top;
        _layout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _layout.Name = "startupLayout";
        _layout.RowCount = 4;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //
        // _heading
        //
        _heading.AutoSize = true;
        _heading.Dock = DockStyle.Fill;
        _heading.Font = new Font(Font, FontStyle.Bold);
        _heading.Margin = new Padding(0, 0, 0, 4);
        _heading.Name = "headingLabel";
        _heading.Text = "Choose a project";
        //
        // _note
        //
        _note.AutoSize = true;
        _note.Dock = DockStyle.Fill;
        _note.ForeColor = Color.DimGray;
        _note.Margin = new Padding(0, 0, 0, 12);
        _note.MaximumSize = new Size(360, 0);
        _note.Name = "noteLabel";
        _note.Text = "The project type stays fixed for the life of the project.";
        //
        // _choices
        //
        _choices.AutoSize = true;
        _choices.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _choices.ColumnCount = 1;
        _choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _choices.Controls.Add(_cvRow, 0, 0);
        _choices.Controls.Add(_teachRow, 0, 1);
        _choices.Controls.Add(_openRow, 0, 2);
        _choices.Dock = DockStyle.Fill;
        _choices.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        _choices.Margin = new Padding(0);
        _choices.Name = "choiceList";
        _choices.RowCount = 3;
        _choices.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _choices.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _choices.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        //
        // _cvRow
        //
        _cvRow.AutoSize = true;
        _cvRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _cvRow.BackColor = Color.White;
        _cvRow.Controls.Add(_cvLayout);
        _cvRow.Padding = new Padding(1);
        _cvRow.Dock = DockStyle.Fill;
        _cvRow.MinimumSize = new Size(0, 68);
        _cvRow.Margin = new Padding(0, 0, 0, 8);
        _cvRow.Name = "cvChoice";
        _cvRow.TabStop = true;
        _cvLayout.ColumnCount = 2;
        _cvLayout.AutoSize = true;
        _cvLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _cvLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
        _cvLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _cvLayout.Controls.Add(_cvIcon, 0, 0);
        _cvLayout.Controls.Add(_cvText, 1, 0);
        _cvLayout.Dock = DockStyle.Top;
        _cvLayout.Margin = new Padding(0);
        _cvLayout.Name = "cvChoiceLayout";
        _cvLayout.RowCount = 1;
        _cvLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _cvIcon.Anchor = AnchorStyles.None;
        _cvIcon.Margin = new Padding(12, 0, 0, 0);
        _cvIcon.Name = "cvIcon";
        _cvIcon.Size = new Size(32, 32);
        _cvIcon.SizeMode = PictureBoxSizeMode.CenterImage;
        _cvIcon.TabStop = false;
        _cvText.Anchor = AnchorStyles.Left;
        _cvText.AutoSize = true;
        _cvText.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _cvText.Controls.Add(_cvTitle);
        _cvText.Controls.Add(_cvDetail);
        _cvText.FlowDirection = FlowDirection.TopDown;
        _cvText.Margin = new Padding(4, 0, 12, 0);
        _cvText.Name = "cvText";
        _cvText.WrapContents = false;
        _cvTitle.AutoSize = true;
        _cvTitle.Font = new Font(Font, FontStyle.Bold);
        _cvTitle.Margin = new Padding(0);
        _cvTitle.Name = "cvTitle";
        _cvTitle.Text = "New CV project";
        _cvDetail.AutoSize = true;
        _cvDetail.ForeColor = Color.DimGray;
        _cvDetail.Margin = new Padding(0, 2, 0, 0);
        _cvDetail.Name = "cvDetail";
        _cvDetail.Text = "Image and sensor frame.";
        //
        // _teachRow
        //
        _teachRow.AutoSize = true;
        _teachRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _teachRow.BackColor = Color.White;
        _teachRow.Controls.Add(_teachLayout);
        _teachRow.Padding = new Padding(1);
        _teachRow.Dock = DockStyle.Fill;
        _teachRow.MinimumSize = new Size(0, 68);
        _teachRow.Margin = new Padding(0, 0, 0, 8);
        _teachRow.Name = "teachChoice";
        _teachRow.TabStop = true;
        _teachLayout.ColumnCount = 2;
        _teachLayout.AutoSize = true;
        _teachLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _teachLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
        _teachLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _teachLayout.Controls.Add(_teachIcon, 0, 0);
        _teachLayout.Controls.Add(_teachText, 1, 0);
        _teachLayout.Dock = DockStyle.Top;
        _teachLayout.Margin = new Padding(0);
        _teachLayout.Name = "teachChoiceLayout";
        _teachLayout.RowCount = 1;
        _teachLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _teachIcon.Anchor = AnchorStyles.None;
        _teachIcon.Margin = new Padding(12, 0, 0, 0);
        _teachIcon.Name = "teachIcon";
        _teachIcon.Size = new Size(32, 32);
        _teachIcon.SizeMode = PictureBoxSizeMode.CenterImage;
        _teachIcon.TabStop = false;
        _teachText.Anchor = AnchorStyles.Left;
        _teachText.AutoSize = true;
        _teachText.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _teachText.Controls.Add(_teachTitle);
        _teachText.Controls.Add(_teachDetail);
        _teachText.FlowDirection = FlowDirection.TopDown;
        _teachText.Margin = new Padding(4, 0, 12, 0);
        _teachText.Name = "teachText";
        _teachText.WrapContents = false;
        _teachTitle.AutoSize = true;
        _teachTitle.Font = new Font(Font, FontStyle.Bold);
        _teachTitle.Margin = new Padding(0);
        _teachTitle.Name = "teachTitle";
        _teachTitle.Text = "New Teach project";
        _teachDetail.AutoSize = true;
        _teachDetail.ForeColor = Color.DimGray;
        _teachDetail.Margin = new Padding(0, 2, 0, 0);
        _teachDetail.Name = "teachDetail";
        _teachDetail.Text = "Points and a mm grid.";
        //
        // _openRow
        //
        _openRow.AutoSize = true;
        _openRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _openRow.BackColor = Color.White;
        _openRow.Controls.Add(_openLayout);
        _openRow.Padding = new Padding(1);
        _openRow.Dock = DockStyle.Fill;
        _openRow.MinimumSize = new Size(0, 68);
        _openRow.Margin = new Padding(0, 0, 0, 8);
        _openRow.Name = "openChoice";
        _openRow.TabStop = true;
        _openLayout.ColumnCount = 2;
        _openLayout.AutoSize = true;
        _openLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _openLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
        _openLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _openLayout.Controls.Add(_openIcon, 0, 0);
        _openLayout.Controls.Add(_openText, 1, 0);
        _openLayout.Dock = DockStyle.Top;
        _openLayout.Margin = new Padding(0);
        _openLayout.Name = "openChoiceLayout";
        _openLayout.RowCount = 1;
        _openLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _openIcon.Anchor = AnchorStyles.None;
        _openIcon.Margin = new Padding(12, 0, 0, 0);
        _openIcon.Name = "openIcon";
        _openIcon.Size = new Size(32, 32);
        _openIcon.SizeMode = PictureBoxSizeMode.CenterImage;
        _openIcon.TabStop = false;
        _openText.Anchor = AnchorStyles.Left;
        _openText.AutoSize = true;
        _openText.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _openText.Controls.Add(_openTitle);
        _openText.Controls.Add(_openDetail);
        _openText.FlowDirection = FlowDirection.TopDown;
        _openText.Margin = new Padding(4, 0, 12, 0);
        _openText.Name = "openText";
        _openText.WrapContents = false;
        _openTitle.AutoSize = true;
        _openTitle.Font = new Font(Font, FontStyle.Bold);
        _openTitle.Margin = new Padding(0);
        _openTitle.Name = "openTitle";
        _openTitle.Text = "Open project...";
        _openDetail.AutoSize = true;
        _openDetail.ForeColor = Color.DimGray;
        _openDetail.Margin = new Padding(0, 2, 0, 0);
        _openDetail.Name = "openDetail";
        _openDetail.Text = "Open a saved file.";
        //
        // _buttonRow
        //
        _buttonRow.AutoSize = true;
        _buttonRow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _buttonRow.Controls.Add(_cancelButton);
        _buttonRow.Dock = DockStyle.Fill;
        _buttonRow.FlowDirection = FlowDirection.RightToLeft;
        _buttonRow.Margin = new Padding(0, 12, 0, 0);
        _buttonRow.Name = "buttonRow";
        _buttonRow.WrapContents = false;
        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Margin = new Padding(0);
        _cancelButton.MinimumSize = new Size(96, 28);
        _cancelButton.Name = "cancelButton";
        _cancelButton.Text = "Cancel";
        //
        // StartupDialog
        //
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        CancelButton = _cancelButton;
        ClientSize = new Size(480, 420);
        Controls.Add(_layout);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(420, 0);
        Name = "StartupDialog";
        Padding = new Padding(20);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Dispense Path Author";
        _layout.ResumeLayout(false);
        _layout.PerformLayout();
        _choices.ResumeLayout(false);
        _cvRow.ResumeLayout(false);
        _cvLayout.ResumeLayout(false);
        _cvLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_cvIcon).EndInit();
        _cvText.ResumeLayout(false);
        _cvText.PerformLayout();
        _teachRow.ResumeLayout(false);
        _teachLayout.ResumeLayout(false);
        _teachLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_teachIcon).EndInit();
        _teachText.ResumeLayout(false);
        _teachText.PerformLayout();
        _openRow.ResumeLayout(false);
        _openLayout.ResumeLayout(false);
        _openLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_openIcon).EndInit();
        _openText.ResumeLayout(false);
        _openText.PerformLayout();
        _buttonRow.ResumeLayout(false);
        _buttonRow.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private TableLayoutPanel _layout = null!;
    private Label _heading = null!;
    private Label _note = null!;
    private TableLayoutPanel _choices = null!;
    private Panel _cvRow = null!;
    private TableLayoutPanel _cvLayout = null!;
    private PictureBox _cvIcon = null!;
    private FlowLayoutPanel _cvText = null!;
    private Label _cvTitle = null!;
    private Label _cvDetail = null!;
    private Panel _teachRow = null!;
    private TableLayoutPanel _teachLayout = null!;
    private PictureBox _teachIcon = null!;
    private FlowLayoutPanel _teachText = null!;
    private Label _teachTitle = null!;
    private Label _teachDetail = null!;
    private Panel _openRow = null!;
    private TableLayoutPanel _openLayout = null!;
    private PictureBox _openIcon = null!;
    private FlowLayoutPanel _openText = null!;
    private Label _openTitle = null!;
    private Label _openDetail = null!;
    private FlowLayoutPanel _buttonRow = null!;
    private Button _cancelButton = null!;
}
