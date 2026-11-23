#nullable enable
namespace DispensePathAuthor;

partial class ProjectSettingsDialog
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
        _grid = new DataGridView();
        _nameColumn = new DataGridViewTextBoxColumn();
        _valueColumn = new DataGridViewTextBoxColumn();
        _buttons = new FlowLayoutPanel();
        _okButton = new Button();
        _cancelButton = new Button();
        ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
        _buttons.SuspendLayout();
        SuspendLayout();
        // 
        // _grid
        // 
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = SystemColors.Window;
        _grid.ColumnHeadersHeight = 46;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.Columns.AddRange(new DataGridViewColumn[] { _nameColumn, _valueColumn });
        _grid.Dock = DockStyle.Fill;
        _grid.Location = new Point(12, 12);
        _grid.MultiSelect = false;
        _grid.Name = "_grid";
        _grid.RowHeadersVisible = false;
        _grid.RowHeadersWidth = 82;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.Size = new Size(1006, 624);
        _grid.TabIndex = 0;
        // 
        // _nameColumn
        // 
        _nameColumn.FillWeight = 55F;
        _nameColumn.HeaderText = "Name";
        _nameColumn.MinimumWidth = 10;
        _nameColumn.Name = "_nameColumn";
        _nameColumn.ReadOnly = true;
        _nameColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        // 
        // _valueColumn
        // 
        _valueColumn.FillWeight = 45F;
        _valueColumn.HeaderText = "Value";
        _valueColumn.MinimumWidth = 10;
        _valueColumn.Name = "_valueColumn";
        _valueColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        // 
        // _buttons
        // 
        _buttons.AutoSize = true;
        _buttons.Controls.Add(_okButton);
        _buttons.Controls.Add(_cancelButton);
        _buttons.Dock = DockStyle.Bottom;
        _buttons.FlowDirection = FlowDirection.RightToLeft;
        _buttons.Location = new Point(12, 636);
        _buttons.Name = "_buttons";
        _buttons.Size = new Size(1006, 54);
        _buttons.TabIndex = 1;
        _buttons.WrapContents = false;
        // 
        // _okButton
        // 
        _okButton.AutoSize = true;
        _okButton.Location = new Point(896, 12);
        _okButton.Margin = new Padding(0, 12, 0, 0);
        _okButton.MinimumSize = new Size(110, 36);
        _okButton.Name = "_okButton";
        _okButton.Size = new Size(110, 42);
        _okButton.TabIndex = 1;
        _okButton.Text = "OK";
        // 
        // _cancelButton
        // 
        _cancelButton.AutoSize = true;
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Location = new Point(778, 12);
        _cancelButton.Margin = new Padding(0, 12, 8, 0);
        _cancelButton.MinimumSize = new Size(110, 36);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(110, 42);
        _cancelButton.TabIndex = 0;
        _cancelButton.Text = "Cancel";
        // 
        // ProjectSettingsDialog
        // 
        AcceptButton = _okButton;
        AutoScaleDimensions = new SizeF(13F, 32F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(1030, 702);
        Controls.Add(_grid);
        Controls.Add(_buttons);
        Font = new Font("Segoe UI", 9F);
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new Size(480, 320);
        Name = "ProjectSettingsDialog";
        Padding = new Padding(12);
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Project settings";
        ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
        _buttons.ResumeLayout(false);
        _buttons.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private DataGridView _grid = null!;
    private DataGridViewTextBoxColumn _nameColumn = null!;
    private DataGridViewTextBoxColumn _valueColumn = null!;
    private FlowLayoutPanel _buttons = null!;
    private Button _cancelButton = null!;
    private Button _okButton = null!;
}
