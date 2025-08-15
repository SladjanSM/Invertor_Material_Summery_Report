using Inventor;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SM.MaterialSummary
{
    public class MaterialSummaryForm : Form
    {
        private readonly Inventor.Application _inv;
        private DataGridView _grid;
        private TextBox _txtProject;
        private CheckBox _chkIncludeGeneric;
        private Button _btnAddAsm;
        private Button _btnRun;
        private Button _btnBrowseOut;
        private TextBox _txtOutFolder;

        private CheckBox _chkUseWorkspace;
        private CheckBox _chkRememberLast;
        private CheckBox _chkSplitPerAsm;
        private CheckBox _chkAutoIncrement;

        public MaterialSummaryForm(Inventor.Application invApp)
        {
            _inv = invApp;
            Text = "SM Furniture AI – Material Summary";
            Width = 1100;
            Height = 680;
            MinimumSize = new Size(900, 600);
            BuildUi();
        }

        private void BuildUi()
        {
            var lblProject = new Label { Text = "Ime projekta:", Left = 12, Top = 14, Width = 100 };
            _txtProject = new TextBox { Left = 120, Top = 10, Width = 300 };

            _chkIncludeGeneric = new CheckBox { Left = 450, Top = 12, Width = 220, Text = "Include Generic materials" };
            _chkIncludeGeneric.Checked = true;

            var lblOut = new Label { Text = "Output folder:", Left = 12, Top = 44, Width = 100 };
            _txtOutFolder = new TextBox { Left = 120, Top = 40, Width = 600 };
            _btnBrowseOut = new Button { Left = 730, Top = 39, Width = 90, Text = "Browse..." };
            _btnBrowseOut.Click += (s, e) => BrowseOutFolder();

            _chkUseWorkspace   = new CheckBox { Left = 830, Top = 42, Width = 230, Text = "Use project workspace (default)" };
            _chkUseWorkspace.Checked = true;
            _chkRememberLast   = new CheckBox { Left = 12, Top = 68, Width = 180, Text = "Remember last folder" };
            _chkSplitPerAsm    = new CheckBox { Left = 200, Top = 68, Width = 180, Text = "Split per assembly PDF" };
            _chkAutoIncrement  = new CheckBox { Left = 390, Top = 68, Width = 180, Text = "Auto-increment names" };
            _chkAutoIncrement.Checked = true;

            _grid = new DataGridView
            {
                Left = 12,
                Top = 100,
                Width = ClientSize.Width - 24,
                Height = ClientSize.Height - 180,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            BuildGridColumns();
            _grid.CellContentClick += Grid_CellContentClick;
            _grid.CellValidating += Grid_CellValidating;

            _btnAddAsm = new Button { Left = 12, Top = ClientSize.Height - 68, Width = 130, Height = 34, Anchor = AnchorStyles.Left | AnchorStyles.Bottom, Text = "Add Assembly" };
            _btnAddAsm.Click += (s, e) => AddAssemblies();

            _btnRun = new Button { Left = ClientSize.Width - 160, Top = ClientSize.Height - 68, Width = 148, Height = 34, Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Text = "Run Analysis" };
            _btnRun.Click += (s, e) => RunAnalysis();

            Controls.Add(lblProject);
            Controls.Add(_txtProject);
            Controls.Add(_chkIncludeGeneric);
            Controls.Add(lblOut);
            Controls.Add(_txtOutFolder);
            Controls.Add(_btnBrowseOut);
            Controls.Add(_chkUseWorkspace);
            Controls.Add(_chkRememberLast);
            Controls.Add(_chkSplitPerAsm);
            Controls.Add(_chkAutoIncrement);
            Controls.Add(_grid);
            Controls.Add(_btnAddAsm);
            Controls.Add(_btnRun);

            Resize += (s, e) =>
            {
                _grid.Width = ClientSize.Width - 24;
                _grid.Height = ClientSize.Height - 180;
                _btnRun.Left = ClientSize.Width - 160;
                _btnAddAsm.Top = ClientSize.Height - 68;
                _btnRun.Top = ClientSize.Height - 68;
            };
        }

        private void BuildGridColumns()
        {
            _grid.Columns.Clear();

            var colX = new DataGridViewButtonColumn
            {
                Name = "X",
                HeaderText = "X",
                Text = "X",
                UseColumnTextForButtonValue = true,
                Width = 36
            };
            _grid.Columns.Add(colX);

            var colName = new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Naziv asemblija",
                Width = 220,
                ReadOnly = false
            };
            _grid.Columns.Add(colName);

            var colVer = new DataGridViewTextBoxColumn
            {
                Name = "Version",
                HeaderText = "Verzija asemblija",
                Width = 120,
                ReadOnly = true
            };
            _grid.Columns.Add(colVer);

            var colQty = new DataGridViewTextBoxColumn
            {
                Name = "Qty",
                HeaderText = "Količina",
                Width = 80
            };
            _grid.Columns.Add(colQty);

            var colPath = new DataGridViewTextBoxColumn
            {
                Name = "Path",
                HeaderText = "Path",
                Width = 420,
                ReadOnly = false
            };
            _grid.Columns.Add(colPath);

            var colStatus = new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                Width = 160,
                ReadOnly = true
            };
            _grid.Columns.Add(colStatus);
        }

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (_grid.Columns[e.ColumnIndex].Name == "X")
            {
                _grid.Rows.RemoveAt(e.RowIndex);
            }
        }

        private void Grid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var col = _grid.Columns[e.ColumnIndex].Name;
            if (col == "Qty")
            {
                if (!int.TryParse(Convert.ToString(e.FormattedValue), out int v) || v < 0)
                {
                    e.Cancel = true;
                    MessageBox.Show("Količina mora biti nenegativan ceo broj (0, 1, 2, ...).");
                }
            }
        }

        private void AddAssemblies()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Add Assembly files";
                ofd.Filter = "Inventor Assembly (*.iam)|*.iam";
                ofd.Multiselect = true;
                if (ofd.ShowDialog(this) != DialogResult.OK) return;

                foreach (var path in ofd.FileNames)
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    int row = _grid.Rows.Add();
                    var r = _grid.Rows[row];

                    r.Cells["Name"].Value = name;
                    r.Cells["Version"].Value = "";
                    r.Cells["Qty"].Value = "1";
                    r.Cells["Path"].Value = path;
                    r.Cells["Status"].Value = "Pending";
                }
            }
        }

        private void BrowseOutFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select output folder for reports";
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    _txtOutFolder.Text = fbd.SelectedPath;
                    _chkUseWorkspace.Checked = false;
                }
            }
        }

        private void RunAnalysis()
        {
            MessageBox.Show("RunAnalysis() stub – FAZA 2 će dodati logiku.");
        }
    }
}