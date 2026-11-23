#nullable enable
namespace DispensePathAuthor.Controls;

partial class SegmentListPanel
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
        DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
        _segmentList = new ListView();
        _moveUpButton = new Button();
        _deleteButton = new Button();
        _moveDownButton = new Button();
        _gapSummaryLabel = new Label();
        _toolbar = new FlowLayoutPanel();
        _propertiesHost = new Panel();
        _propertyGrid = new DataGridView();
        nameColumn = new DataGridViewTextBoxColumn();
        valueColumn = new DataGridViewTextBoxColumn();
        _propertiesCaption = new Label();
        _toolbar.SuspendLayout();
        _propertiesHost.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_propertyGrid).BeginInit();
        SuspendLayout();
        // 
        // _segmentList
        // 
        _segmentList.Dock = DockStyle.Fill;
        _segmentList.FullRowSelect = true;
        _segmentList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _segmentList.Location = new Point(0, 0);
        _segmentList.MultiSelect = false;
        _segmentList.Name = "_segmentList";
        _segmentList.Size = new Size(528, 143);
        _segmentList.TabIndex = 0;
        _segmentList.UseCompatibleStateImageBehavior = false;
        _segmentList.View = View.Details;
        // 
        // _moveUpButton
        // 
        _moveUpButton.AutoSize = true;
        _moveUpButton.Enabled = false;
        _moveUpButton.Location = new Point(3, 9);
        _moveUpButton.Name = "_moveUpButton";
        _moveUpButton.Size = new Size(120, 42);
        _moveUpButton.TabIndex = 0;
        _moveUpButton.Text = "Move up";
        // 
        // _deleteButton
        // 
        _deleteButton.AutoSize = true;
        _deleteButton.Enabled = false;
        _deleteButton.Location = new Point(286, 9);
        _deleteButton.Name = "_deleteButton";
        _deleteButton.Size = new Size(110, 42);
        _deleteButton.TabIndex = 1;
        _deleteButton.Text = "Delete";
        // 
        // _moveDownButton
        // 
        _moveDownButton.AutoSize = true;
        _moveDownButton.Enabled = false;
        _moveDownButton.Location = new Point(129, 9);
        _moveDownButton.Name = "_moveDownButton";
        _moveDownButton.Size = new Size(151, 42);
        _moveDownButton.TabIndex = 2;
        _moveDownButton.Text = "Move down";
        // 
        // _gapSummaryLabel
        // 
        _gapSummaryLabel.AutoSize = true;
        _gapSummaryLabel.ForeColor = Color.DimGray;
        _gapSummaryLabel.Location = new Point(3, 54);
        _gapSummaryLabel.Name = "_gapSummaryLabel";
        _gapSummaryLabel.Padding = new Padding(8, 6, 0, 0);
        _gapSummaryLabel.Size = new Size(163, 38);
        _gapSummaryLabel.TabIndex = 2;
        _gapSummaryLabel.Text = "Sequence: —";
        // 
        // _toolbar
        // 
        _toolbar.AutoSize = true;
        _toolbar.Controls.Add(_moveUpButton);
        _toolbar.Controls.Add(_moveDownButton);
        _toolbar.Controls.Add(_deleteButton);
        _toolbar.Controls.Add(_gapSummaryLabel);
        _toolbar.Dock = DockStyle.Bottom;
        _toolbar.Location = new Point(0, 143);
        _toolbar.Name = "_toolbar";
        _toolbar.Padding = new Padding(0, 6, 0, 5);
        _toolbar.Size = new Size(528, 97);
        _toolbar.TabIndex = 1;
        // 
        // _propertiesHost
        // 
        _propertiesHost.Controls.Add(_propertyGrid);
        _propertiesHost.Controls.Add(_propertiesCaption);
        _propertiesHost.Dock = DockStyle.Bottom;
        _propertiesHost.Location = new Point(0, 240);
        _propertiesHost.Name = "_propertiesHost";
        _propertiesHost.Size = new Size(528, 320);
        _propertiesHost.TabIndex = 2;
        // 
        // _propertyGrid
        // 
        _propertyGrid.AllowUserToAddRows = false;
        _propertyGrid.AllowUserToDeleteRows = false;
        _propertyGrid.AllowUserToResizeRows = false;
        _propertyGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _propertyGrid.BackgroundColor = SystemColors.Window;
        _propertyGrid.ColumnHeadersHeight = 50;
        _propertyGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _propertyGrid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, valueColumn });
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = SystemColors.Window;
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
        dataGridViewCellStyle1.ForeColor = SystemColors.ControlText;
        dataGridViewCellStyle1.Padding = new Padding(6, 4, 4, 4);
        dataGridViewCellStyle1.SelectionBackColor = SystemColors.Window;
        dataGridViewCellStyle1.SelectionForeColor = SystemColors.WindowText;
        dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
        _propertyGrid.DefaultCellStyle = dataGridViewCellStyle1;
        _propertyGrid.Dock = DockStyle.Fill;
        _propertyGrid.EditMode = DataGridViewEditMode.EditOnEnter;
        _propertyGrid.Location = new Point(0, 43);
        _propertyGrid.MultiSelect = false;
        _propertyGrid.Name = "_propertyGrid";
        _propertyGrid.RowHeadersVisible = false;
        _propertyGrid.RowHeadersWidth = 82;
        _propertyGrid.RowTemplate.Height = 42;
        _propertyGrid.ScrollBars = ScrollBars.Vertical;
        _propertyGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _propertyGrid.Size = new Size(528, 277);
        _propertyGrid.StandardTab = true;
        _propertyGrid.TabIndex = 2;
        // 
        // nameColumn
        // 
        nameColumn.FillWeight = 58F;
        nameColumn.HeaderText = "Name";
        nameColumn.MinimumWidth = 10;
        nameColumn.Name = "nameColumn";
        nameColumn.ReadOnly = true;
        nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        // 
        // valueColumn
        // 
        valueColumn.FillWeight = 42F;
        valueColumn.HeaderText = "Value";
        valueColumn.MinimumWidth = 10;
        valueColumn.Name = "valueColumn";
        valueColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        // 
        // _propertiesCaption
        // 
        _propertiesCaption.Dock = DockStyle.Top;
        _propertiesCaption.ForeColor = Color.FromArgb(30, 70, 140);
        _propertiesCaption.Location = new Point(0, 0);
        _propertiesCaption.Name = "_propertiesCaption";
        _propertiesCaption.Padding = new Padding(6, 0, 0, 0);
        _propertiesCaption.Size = new Size(528, 43);
        _propertiesCaption.TabIndex = 1;
        _propertiesCaption.Text = "Properties";
        _propertiesCaption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // SegmentListPanel
        // 
        Controls.Add(_segmentList);
        Controls.Add(_toolbar);
        Controls.Add(_propertiesHost);
        Name = "SegmentListPanel";
        Size = new Size(528, 560);
        _toolbar.ResumeLayout(false);
        _toolbar.PerformLayout();
        _propertiesHost.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_propertyGrid).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private ListView _segmentList = null!;
    private Button _moveUpButton = null!;
    private Button _deleteButton = null!;
    private Button _moveDownButton = null!;
    private Label _gapSummaryLabel = null!;
    private FlowLayoutPanel _toolbar = null!;
    private Panel _propertiesHost = null!;
    private Label _propertiesCaption = null!;
    private DataGridView _propertyGrid = null!;
    private DataGridViewTextBoxColumn nameColumn = null!;
    private DataGridViewTextBoxColumn valueColumn = null!;
}
