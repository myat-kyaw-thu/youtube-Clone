using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Forms
{
    /// <summary>
    /// Product Management — Add / Edit / Delete with image upload and category dropdown.
    /// </summary>
    public class ProductManagementForm : Form
    {
        private readonly DataService _dataService;

        // Form fields
        private TextBox    _txtName, _txtPrice, _txtStock,
                           _txtDescription, _txtSupplierId, _txtRating, _txtLowStock, _txtImagePath;
        private ComboBox   _cmbCategory;
        private DateTimePicker _dtpExpiry;
        private CheckBox   _chkActive;
        private Button     _btnBrowseImage;
        private PictureBox _picPreview;

        // Grid + action buttons
        private DataGridView _dgv;
        private Button _btnAdd, _btnUpdate, _btnDelete, _btnClear;

        // Currently selected product Id (for update/delete)
        private string _selectedId = null;

        private static readonly Color Green   = Color.FromArgb(34, 139, 34);
        private static readonly Color BgPage  = Color.FromArgb(245, 248, 246);
        private static readonly Color Surface = Color.White;
        private static readonly Color Danger  = Color.FromArgb(192, 57, 43);
        private static readonly Color Blue    = Color.FromArgb(52, 152, 219);

        // Known categories — loaded from existing products + defaults
        private static readonly string[] DefaultCategories =
            { "Fruits", "Vegetables", "Dairy", "Pantry", "Nuts", "Beverages", "Meat", "Bakery", "Other" };

        public ProductManagementForm()
        {
            _dataService = new DataService();
            InitializeComponent();
            LoadCategoryDropdown();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text        = "Product Management — GreenLife";
            this.MinimumSize = new Size(960, 660);
            this.Size        = new Size(1100, 780);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor   = BgPage;

            // ── Root split: form card top (fixed height), grid fills rest ─────
            var root = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 2,
                ColumnCount = 1,
                BackColor   = BgPage,
                Padding     = new Padding(18)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 320));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ── Form card ─────────────────────────────────────────────────────
            var formCard = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Surface,
                Padding   = new Padding(16, 10, 16, 10),
                Margin    = new Padding(0, 0, 0, 10)
            };
            formCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, formCard.Width - 1, formCard.Height - 1);
                using (var b = new SolidBrush(Green))
                    e.Graphics.FillRectangle(b, 0, 0, 4, formCard.Height);
            };

            // Title + action buttons row
            var titleRow = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                Height      = 42,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Surface
            };
            titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            titleRow.Controls.Add(new Label
            {
                Text      = "Product Management",
                Font      = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var btnFlow = new FlowLayoutPanel
            {
                Dock            = DockStyle.Fill,
                FlowDirection   = FlowDirection.LeftToRight,
                BackColor       = Surface,
                WrapContents    = false,
                Padding         = new Padding(0, 4, 0, 0)
            };
            _btnAdd    = ActionBtn("+ Add",    Green);
            _btnUpdate = ActionBtn("✎ Update", Blue);
            _btnDelete = ActionBtn("✕ Delete", Danger);
            _btnClear  = ActionBtn("↺ Clear",  Color.FromArgb(130, 130, 130));
            _btnAdd.Click    += BtnAdd_Click;
            _btnUpdate.Click += BtnUpdate_Click;
            _btnDelete.Click += BtnDelete_Click;
            _btnClear.Click  += (s, e) => ClearForm();
            btnFlow.Controls.Add(_btnAdd);
            btnFlow.Controls.Add(_btnUpdate);
            btnFlow.Controls.Add(_btnDelete);
            btnFlow.Controls.Add(_btnClear);
            titleRow.Controls.Add(btnFlow, 1, 0);

            // ── Fields area: left columns + right image preview ───────────────
            var fieldsOuter = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Surface
            };
            fieldsOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            fieldsOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));

            // Left: 4-column field grid
            var fields = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 4,
                RowCount    = 4,
                BackColor   = Surface,
                Padding     = new Padding(0, 4, 8, 0)
            };
            for (int i = 0; i < 4; i++)
                fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int i = 0; i < 4; i++)
                fields.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

            // Row 0
            fields.Controls.Add(FieldGroup("Name *",        out _txtName),       0, 0);
            fields.Controls.Add(CategoryGroup(),                                  1, 0);
            fields.Controls.Add(FieldGroup("Price ($) *",   out _txtPrice),      2, 0);
            fields.Controls.Add(FieldGroup("Stock *",       out _txtStock),      3, 0);
            // Row 1
            fields.Controls.Add(FieldGroup("Supplier ID",   out _txtSupplierId), 0, 1);
            fields.Controls.Add(FieldGroup("Rating (0-5)",  out _txtRating),     1, 1);
            fields.Controls.Add(FieldGroup("Low Stock Alert", out _txtLowStock), 2, 1);
            _txtLowStock.Text = "10";
            // Expiry date
            var expiryGrp = ControlGroup("Expiry Date", out Panel expiryPanel);
            _dtpExpiry = new DateTimePicker
            {
                Dock   = DockStyle.Top,
                Format = DateTimePickerFormat.Short,
                Font   = new Font("Segoe UI", 10)
            };
            expiryPanel.Controls.Add(_dtpExpiry);
            fields.Controls.Add(expiryGrp, 3, 1);

            // Row 2 — description spans 3, active checkbox col 3
            var descGrp = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(4, 0, 4, 0) };
            descGrp.Controls.Add(new Label { Text = "Description", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Top, Height = 18 });
            _txtDescription = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            descGrp.Controls.Add(_txtDescription);
            fields.SetColumnSpan(descGrp, 3);
            fields.Controls.Add(descGrp, 0, 2);

            var activeGrp = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(4, 22, 4, 0) };
            _chkActive = new CheckBox { Text = "Active", Font = new Font("Segoe UI", 10), Checked = true, AutoSize = true, Cursor = Cursors.Hand };
            activeGrp.Controls.Add(_chkActive);
            fields.Controls.Add(activeGrp, 3, 2);

            // Row 3 — image path + browse button spans 4
            var imgRow = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Surface,
                Padding     = new Padding(4, 0, 4, 0)
            };
            imgRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            imgRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            var imgFieldGrp = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            imgFieldGrp.Controls.Add(new Label { Text = "Image Path", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Top, Height = 18 });
            _txtImagePath = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true };
            imgFieldGrp.Controls.Add(_txtImagePath);

            var btnGrp = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(0, 18, 0, 0) };
            _btnBrowseImage = new Button
            {
                Text      = "📁 Browse Image",
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9),
                Cursor    = Cursors.Hand
            };
            _btnBrowseImage.FlatAppearance.BorderSize = 0;
            _btnBrowseImage.Click += BtnBrowseImage_Click;
            btnGrp.Controls.Add(_btnBrowseImage);

            imgRow.Controls.Add(imgFieldGrp, 0, 0);
            imgRow.Controls.Add(btnGrp, 1, 0);
            fields.SetColumnSpan(imgRow, 4);
            fields.Controls.Add(imgRow, 0, 3);

            // Right: image preview
            var previewPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding   = new Padding(4)
            };
            previewPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                    e.Graphics.DrawRectangle(pen, 0, 0, previewPanel.Width - 1, previewPanel.Height - 1);
            };
            _picPreview = new PictureBox
            {
                Dock      = DockStyle.Fill,
                SizeMode  = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            previewPanel.Controls.Add(_picPreview);

            fieldsOuter.Controls.Add(fields, 0, 0);
            fieldsOuter.Controls.Add(previewPanel, 1, 0);

            formCard.Controls.Add(fieldsOuter);
            formCard.Controls.Add(titleRow);

            // ── Grid card ─────────────────────────────────────────────────────
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            gridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1);
            };

            _dgv = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                Name                  = "dgvProducts",
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor       = Surface,
                BorderStyle           = BorderStyle.None,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                GridColor             = Color.FromArgb(230, 230, 230),
                Font                  = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight   = 34
            };
            _dgv.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(245, 248, 246);
            _dgv.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor  = Color.FromArgb(60, 60, 60);
            _dgv.EnableHeadersVisualStyles = false;
            _dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 253, 251);
            _dgv.SelectionChanged += DgvSelectionChanged;
            gridCard.Controls.Add(_dgv);

            root.Controls.Add(formCard, 0, 0);
            root.Controls.Add(gridCard, 0, 1);
            this.Controls.Add(root);
        }

        // ── Category dropdown ─────────────────────────────────────────────────

        private Panel CategoryGroup()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(4, 0, 4, 0) };
            p.Controls.Add(new Label { Text = "Category *", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Top, Height = 18 });
            _cmbCategory = new ComboBox
            {
                Dock          = DockStyle.Top,
                Font          = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            p.Controls.Add(_cmbCategory);
            return p;
        }

        private void LoadCategoryDropdown()
        {
            _cmbCategory.Items.Clear();
            // Add defaults
            foreach (var cat in DefaultCategories)
                _cmbCategory.Items.Add(cat);
            // Add any extra categories from existing products
            foreach (var cat in _dataService.GetAllCategories())
                if (!_cmbCategory.Items.Contains(cat))
                    _cmbCategory.Items.Add(cat);
            if (_cmbCategory.Items.Count > 0)
                _cmbCategory.SelectedIndex = 0;
        }

        // ── Image browse ──────────────────────────────────────────────────────

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Title  = "Select Product Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _txtImagePath.Text = dlg.FileName;
                    try
                    {
                        _picPreview.Image = Image.FromFile(dlg.FileName);
                    }
                    catch
                    {
                        _picPreview.Image = null;
                    }
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Panel FieldGroup(string label, out TextBox box)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(4, 0, 4, 0) };
            p.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Top, Height = 18 });
            var txt = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(txt);
            box = txt;
            return p;
        }

        private static Panel ControlGroup(string label, out Panel inner)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(4, 0, 4, 0) };
            p.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Top, Height = 18 });
            inner = p;
            return p;
        }

        private static Button ActionBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text      = text,
                Size      = new Size(92, 32),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadProducts() => _dgv.DataSource = _dataService.GetAllProducts();

        private void DgvSelectionChanged(object sender, EventArgs e)
        {
            if (_dgv.SelectedRows.Count == 0) return;
            var p = _dgv.SelectedRows[0].DataBoundItem as Product;
            if (p == null) return;

            _selectedId          = p.Id;
            _txtName.Text        = p.Name;
            _txtPrice.Text       = p.Price.ToString("F2");
            _txtStock.Text       = p.Stock.ToString();
            _txtDescription.Text = p.Description;
            _txtSupplierId.Text  = p.SupplierId;
            _txtRating.Text      = p.Rating.ToString("F1");
            _txtLowStock.Text    = p.LowStockThreshold.ToString();
            _txtImagePath.Text   = p.ImageUrl ?? "";
            _dtpExpiry.Value     = p.ExpiryDate != DateTime.MinValue ? p.ExpiryDate : DateTime.Now;
            _chkActive.Checked   = p.IsActive;

            // Set category dropdown
            if (_cmbCategory.Items.Contains(p.Category))
                _cmbCategory.SelectedItem = p.Category;
            else
            {
                _cmbCategory.Items.Add(p.Category);
                _cmbCategory.SelectedItem = p.Category;
            }

            // Load image preview
            if (!string.IsNullOrEmpty(p.ImageUrl) && File.Exists(p.ImageUrl))
            {
                try { _picPreview.Image = Image.FromFile(p.ImageUrl); }
                catch { _picPreview.Image = null; }
            }
            else
            {
                _picPreview.Image = null;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
            try
            {
                _dataService.AddProduct(BuildProduct(null));
                LoadProducts();
                ClearForm();
                MessageBox.Show("Product added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedId == null)
            { MessageBox.Show("Select a product from the list to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!ValidateForm()) return;
            try
            {
                var p = BuildProduct(_selectedId);
                _dataService.UpdateProduct(p);
                LoadProducts();
                MessageBox.Show("Product updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedId == null)
            { MessageBox.Show("Select a product from the list to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show("Delete this product?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _dataService.DeleteProduct(_selectedId);
                LoadProducts();
                ClearForm();
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            { MessageBox.Show("Product name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (_cmbCategory.SelectedItem == null)
            { MessageBox.Show("Please select a category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!decimal.TryParse(_txtPrice.Text, out var pr) || pr < 0)
            { MessageBox.Show("Enter a valid price (e.g. 3.99).", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!int.TryParse(_txtStock.Text, out var st) || st < 0)
            { MessageBox.Show("Enter a valid stock quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        private Product BuildProduct(string existingId)
        {
            return new Product
            {
                Id                = existingId ?? Guid.NewGuid().ToString(),
                Name              = _txtName.Text.Trim(),
                Category          = _cmbCategory.SelectedItem.ToString(),
                Price             = decimal.TryParse(_txtPrice.Text, out var pr) ? pr : 0,
                Stock             = int.TryParse(_txtStock.Text, out var st) ? st : 0,
                Description       = _txtDescription.Text.Trim(),
                SupplierId        = _txtSupplierId.Text.Trim(),
                Rating            = decimal.TryParse(_txtRating.Text, out var rt) ? rt : 0,
                LowStockThreshold = int.TryParse(_txtLowStock.Text, out var ls) ? ls : 10,
                ExpiryDate        = _dtpExpiry.Value,
                IsActive          = _chkActive.Checked,
                ImageUrl          = _txtImagePath.Text.Trim(),
                DateAdded         = DateTime.Now
            };
        }

        private void ClearForm()
        {
            _selectedId = null;
            _txtName.Clear(); _txtPrice.Clear(); _txtStock.Clear();
            _txtDescription.Clear(); _txtSupplierId.Clear();
            _txtRating.Clear(); _txtLowStock.Text = "10";
            _txtImagePath.Clear(); _picPreview.Image = null;
            _dtpExpiry.Value = DateTime.Now;
            _chkActive.Checked = true;
            if (_cmbCategory.Items.Count > 0) _cmbCategory.SelectedIndex = 0;
        }
    }
}
