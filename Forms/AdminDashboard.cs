using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Forms
{
    // ═══════════════════════════════════════════════════════════════════════════
    // AdminDashboard — Part 1: Fields, colours, constructor, InitializeComponent
    // ═══════════════════════════════════════════════════════════════════════════
    public class AdminDashboard : Form
    {
        private readonly AuthService   _authService;
        private readonly DataService   _dataService;
        private readonly ReportService _reportService;

        // Layout
        private Panel _sidebar, _mainArea, _header, _content;

        // Nav buttons
        private Button _btnDashboard, _btnProducts, _btnOrders,
                       _btnCustomers, _btnReports, _btnSuppliers, _btnLogout;

        // Content sections
        private Panel _secDashboard, _secProducts, _secOrders,
                      _secCustomers, _secReports, _secSuppliers;

        // Dashboard stat labels
        private Label _statProducts, _statOrders, _statCustomers, _statRevenue;

        // ── Product form fields (embedded inline) ─────────────────────────────
        private TextBox    _pTxtName, _pTxtPrice, _pTxtStock, _pTxtDescription,
                           _pTxtRating, _pTxtLowStock, _pTxtImagePath;
        private ComboBox   _pCmbCategory, _pCmbSupplier;
        private DateTimePicker _pDtpExpiry;
        private CheckBox   _pChkActive;
        private PictureBox _pPicPreview;
        private DataGridView _pDgv;
        private Button     _pBtnAdd, _pBtnUpdate, _pBtnDelete, _pBtnClear, _pBtnBrowseImage;
        private string     _pSelectedId = null;

        private static readonly string[] ProductCategories =
            { "Fruits", "Vegetables", "Dairy", "Pantry", "Nuts", "Beverages", "Meat", "Bakery", "Other" };

        // ── Customer management fields ────────────────────────────────────────
        private DataGridView _cDgv;
        private Label        _cLblSelected;
        private Button       _cBtnToggleActive, _cBtnResetPw;
        private Label        _cLblStatus;
        private DataGridView _oDgv;
        private ComboBox     _oCmbStatus;
        private Button       _oBtnUpdateStatus;
        private Label        _oLblSelected;

        // ── Colours ───────────────────────────────────────────────────────────
        private static readonly Color Green     = Color.FromArgb(34, 139, 34);
        private static readonly Color GreenDark = Color.FromArgb(24, 110, 24);
        private static readonly Color Danger    = Color.FromArgb(192, 57, 43);
        private static readonly Color Blue      = Color.FromArgb(52, 152, 219);
        private static readonly Color Surface   = Color.White;
        private static readonly Color BgPage    = Color.FromArgb(245, 248, 246);

        public AdminDashboard()
        {
            _authService   = new AuthService();
            _dataService   = new DataService();
            _reportService = new ReportService();
            InitializeComponent();
            LoadDashboardData();
        }

        private void InitializeComponent()
        {
            this.Text        = "GreenLife — Admin";
            this.MinimumSize = new Size(1100, 660);
            this.Size        = new Size(1380, 820);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor   = BgPage;

            BuildSidebar();
            BuildMainArea();

            this.Controls.Add(_mainArea);
            this.Controls.Add(_sidebar);

            ShowSection("dashboard");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 2: Sidebar + Main area
        // ═══════════════════════════════════════════════════════════════════════

        private void BuildSidebar()
        {
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = Green };

            var logo = new Label
            {
                Text = "GreenLife", Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White, Dock = DockStyle.Top, Height = 56,
                TextAlign = ContentAlignment.MiddleCenter, BackColor = GreenDark
            };
            var sub = new Label
            {
                Text = "Admin Panel", Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 230, 180), Dock = DockStyle.Top, Height = 26,
                TextAlign = ContentAlignment.MiddleCenter, BackColor = GreenDark
            };

            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown,
                WrapContents = false, Padding = new Padding(10, 14, 10, 0),
                BackColor = Green, AutoScroll = true
            };

            _btnDashboard = NavBtn("📊  Dashboard");
            _btnProducts  = NavBtn("📦  Products");
            _btnOrders    = NavBtn("📋  Orders");
            _btnCustomers = NavBtn("👥  Customers");
            _btnReports   = NavBtn("📈  Reports");
            _btnSuppliers = NavBtn("🏢  Suppliers");
            _btnLogout    = NavBtn("🚪  Logout", Danger);

            _btnDashboard.Click += (s, e) => ShowSection("dashboard");
            _btnProducts.Click  += (s, e) => ShowSection("products");
            _btnOrders.Click    += (s, e) => ShowSection("orders");
            _btnCustomers.Click += (s, e) => ShowSection("customers");
            _btnReports.Click   += (s, e) => ShowSection("reports");
            _btnSuppliers.Click += (s, e) => ShowSection("suppliers");
            _btnLogout.Click    += (s, e) => { _authService.Logout(); this.Close(); };

            navFlow.Controls.Add(_btnDashboard);
            navFlow.Controls.Add(_btnProducts);
            navFlow.Controls.Add(_btnOrders);
            navFlow.Controls.Add(_btnCustomers);
            navFlow.Controls.Add(_btnReports);
            navFlow.Controls.Add(_btnSuppliers);
            navFlow.Controls.Add(new Panel { Width = 200, Height = 20, BackColor = Green });
            navFlow.Controls.Add(_btnLogout);

            _sidebar.Controls.Add(navFlow);
            _sidebar.Controls.Add(sub);
            _sidebar.Controls.Add(logo);
        }

        private void BuildMainArea()
        {
            _mainArea = new Panel { Dock = DockStyle.Fill, BackColor = BgPage };

            var user = _authService.GetCurrentUser();
            _header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Surface };
            _header.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220)),
                    0, _header.Height - 1, _header.Width, _header.Height - 1);
            _header.Controls.Add(new Label
            {
                Text = "Welcome back,", Font = new Font("Segoe UI", 9), ForeColor = Color.Gray,
                AutoSize = true, Location = new Point(28, 14)
            });
            _header.Controls.Add(new Label
            {
                Text = user?.FullName ?? "Admin",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40), AutoSize = true, Location = new Point(28, 32)
            });

            _content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                Padding = new Padding(24, 20, 24, 20)
            };

            BuildDashboardSection();
            BuildProductsSection();
            BuildOrdersSection();
            BuildCustomersSection();
            BuildReportsSection();
            BuildSuppliersSection();

            _mainArea.Controls.Add(_content);
            _mainArea.Controls.Add(_header);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 3: Dashboard section + Products section (inline form + grid)
        // ═══════════════════════════════════════════════════════════════════════

        // Dashboard live-data labels (beyond the 4 stat cards)
        private Label _dashLowStock, _dashOutOfStock, _dashNearExpiry,
                      _dashPending, _dashShipped, _dashDelivered, _dashCancelled,
                      _dashRevenue30, _dashOrders30, _dashTopCat;
        private DataGridView _dashDgvRecentOrders, _dashDgvLowStock;

        private void BuildDashboardSection()
        {
            _secDashboard = ScrollSection();

            // ── Build all rows first, then add in REVERSE order (WinForms Dock=Top renders last-added first) ──

            // Row 1: Section title
            var title = SectionTitle("Dashboard Overview");

            // Row 2: 4 stat cards
            var cardRow = new TableLayoutPanel
            {
                ColumnCount = 4, RowCount = 1, Dock = DockStyle.Top, Height = 120,
                BackColor = Color.Transparent, Padding = new Padding(0, 8, 0, 8)
            };
            for (int i = 0; i < 4; i++)
                cardRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var (c1, v1) = StatCard("Total Products", "0", Color.FromArgb(52, 152, 219));
            var (c2, v2) = StatCard("Total Orders",   "0", Color.FromArgb(155, 89, 182));
            var (c3, v3) = StatCard("Customers",      "0", Color.FromArgb(46, 204, 113));
            var (c4, v4) = StatCard("Total Revenue",  "$0", Color.FromArgb(230, 160, 20));
            _statProducts = v1; _statOrders = v2; _statCustomers = v3; _statRevenue = v4;
            cardRow.Controls.Add(c1, 0, 0); cardRow.Controls.Add(c2, 1, 0);
            cardRow.Controls.Add(c3, 2, 0); cardRow.Controls.Add(c4, 3, 0);

            // Row 3: Stock Health | Order Status | Last 30 Days
            var midRow = new TableLayoutPanel
            {
                ColumnCount = 3, RowCount = 1, Dock = DockStyle.Top, Height = 190,
                BackColor = Color.Transparent, Padding = new Padding(0, 0, 0, 8)
            };
            midRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            midRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            midRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));

            // Stock health card
            var stockCard = InfoCard("Stock Health");
            var stockLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Surface, Padding = new Padding(10, 4, 10, 4) };
            stockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            stockLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            stockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            stockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            stockLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));
            stockLayout.RowCount = 3;
            _dashLowStock   = InfoVal("—"); _dashOutOfStock = InfoVal("—"); _dashNearExpiry = InfoVal("—");
            stockLayout.Controls.Add(InfoKey("Low Stock Items:"),   0, 0); stockLayout.Controls.Add(_dashLowStock,   1, 0);
            stockLayout.Controls.Add(InfoKey("Out of Stock:"),      0, 1); stockLayout.Controls.Add(_dashOutOfStock, 1, 1);
            stockLayout.Controls.Add(InfoKey("Near Expiry (30d):"), 0, 2); stockLayout.Controls.Add(_dashNearExpiry, 1, 2);
            InfoCardAddContent(stockCard, stockLayout);
            midRow.Controls.Add(stockCard, 0, 0);

            // Order status card
            var orderCard = InfoCard("Order Status");
            var orderLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Surface, Padding = new Padding(10, 4, 10, 4) };
            orderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            orderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            orderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            orderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            orderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            orderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            orderLayout.RowCount = 4;
            _dashPending = InfoVal("—"); _dashShipped = InfoVal("—");
            _dashDelivered = InfoVal("—"); _dashCancelled = InfoVal("—");
            orderLayout.Controls.Add(InfoKey("Pending:"),   0, 0); orderLayout.Controls.Add(_dashPending,   1, 0);
            orderLayout.Controls.Add(InfoKey("Shipped:"),   0, 1); orderLayout.Controls.Add(_dashShipped,   1, 1);
            orderLayout.Controls.Add(InfoKey("Delivered:"), 0, 2); orderLayout.Controls.Add(_dashDelivered, 1, 2);
            orderLayout.Controls.Add(InfoKey("Cancelled:"), 0, 3); orderLayout.Controls.Add(_dashCancelled, 1, 3);
            InfoCardAddContent(orderCard, orderLayout);
            midRow.Controls.Add(orderCard, 1, 0);

            // Last 30 days card
            var last30Card = InfoCard("Last 30 Days");
            var last30Layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = Surface, Padding = new Padding(10, 4, 10, 4) };
            last30Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            last30Layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            last30Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            last30Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 33f));
            last30Layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34f));
            last30Layout.RowCount = 3;
            _dashRevenue30 = InfoVal("—"); _dashOrders30 = InfoVal("—"); _dashTopCat = InfoVal("—");
            last30Layout.Controls.Add(InfoKey("Revenue:"),      0, 0); last30Layout.Controls.Add(_dashRevenue30, 1, 0);
            last30Layout.Controls.Add(InfoKey("Orders:"),       0, 1); last30Layout.Controls.Add(_dashOrders30,  1, 1);
            last30Layout.Controls.Add(InfoKey("Top Category:"), 0, 2); last30Layout.Controls.Add(_dashTopCat,    1, 2);
            InfoCardAddContent(last30Card, last30Layout);
            midRow.Controls.Add(last30Card, 2, 0);

            // Row 4: Recent orders grid + Low stock grid
            var bottomRow = new TableLayoutPanel
            {
                ColumnCount = 2, RowCount = 1, Dock = DockStyle.Top, Height = 260,
                BackColor = Color.Transparent
            };
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            var recentOrdersCard = InfoCard("Recent Orders (last 10)");
            _dashDgvRecentOrders = MakeGrid("dgvDashRecent");
            InfoCardAddContent(recentOrdersCard, _dashDgvRecentOrders);
            bottomRow.Controls.Add(recentOrdersCard, 0, 0);

            var lowStockCard = InfoCard("Low Stock Alert");
            _dashDgvLowStock = MakeGrid("dgvDashLowStock");
            InfoCardAddContent(lowStockCard, _dashDgvLowStock);
            bottomRow.Controls.Add(lowStockCard, 1, 0);

            // ── Add in REVERSE order: last added = rendered at top ────────────
            _secDashboard.Controls.Add(bottomRow);   // rendered 4th (bottom)
            _secDashboard.Controls.Add(midRow);      // rendered 3rd
            _secDashboard.Controls.Add(cardRow);     // rendered 2nd
            _secDashboard.Controls.Add(title);       // rendered 1st (top)
            _content.Controls.Add(_secDashboard);
        }

        private static Panel InfoCard(string title)
        {
            // Use TableLayoutPanel internally: title row (fixed) + content row (fills rest)
            // This avoids Dock=Top vs Dock=Fill conflicts entirely
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Margin = new Padding(0, 0, 8, 0) };
            outer.Paint += (s, e) =>
            {
                var p = s as Panel;
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                using (var b = new SolidBrush(Color.FromArgb(34, 139, 34)))
                    e.Graphics.FillRectangle(b, 0, 0, 4, p.Height);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Surface
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); // title
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // content

            var titleLbl = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(246, 249, 247)
            };
            titleLbl.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawLine(pen, 0, titleLbl.Height - 1, titleLbl.Width, titleLbl.Height - 1);
            };

            // Content placeholder — callers add their content into tbl row 1
            tbl.Controls.Add(titleLbl, 0, 0);
            outer.Controls.Add(tbl);

            // Store tbl reference so callers can add content to row 1
            // We use Tag to pass the inner tbl reference
            outer.Tag = tbl;
            return outer;
        }

        /// <summary>Adds a content control into the content row (row 1) of an InfoCard.</summary>
        private static void InfoCardAddContent(Panel card, Control content)
        {
            var tbl = card.Tag as TableLayoutPanel;
            if (tbl != null)
            {
                content.Dock = DockStyle.Fill;
                tbl.Controls.Add(content, 0, 1);
            }
        }

        private static Label InfoKey(string text) => new Label
        {
            Text = text, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 100, 100),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0)
        };

        private static Label InfoVal(string text) => new Label
        {
            Text = text, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(34, 139, 34), Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        private void BuildProductsSection()
        {
            _secProducts = Section();

            // Root: form card (fixed height) on top, grid fills rest
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = BgPage
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 340));  // form card
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // grid

            // ── Form card ─────────────────────────────────────────────────────
            var formCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Surface,
                Padding = new Padding(14, 8, 14, 8), Margin = new Padding(0, 0, 0, 10)
            };
            formCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, formCard.Width - 1, formCard.Height - 1);
                using (var b = new SolidBrush(Green))
                    e.Graphics.FillRectangle(b, 0, 0, 4, formCard.Height);
            };

            // ── Title + buttons (TableLayoutPanel so buttons never clip) ──────
            var titleRow = new TableLayoutPanel
            {
                Dock = DockStyle.Top, Height = 44, ColumnCount = 2, RowCount = 1, BackColor = Surface
            };
            titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            titleRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400)); // enough for 4 buttons
            titleRow.Controls.Add(new Label
            {
                Text = "Product Management", Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40), Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var btnFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                BackColor = Surface, WrapContents = false, Padding = new Padding(0, 6, 0, 0)
            };
            _pBtnAdd    = ProdBtn("+ Add",    Green);
            _pBtnUpdate = ProdBtn("✎ Update", Blue);
            _pBtnDelete = ProdBtn("✕ Delete", Danger);
            _pBtnClear  = ProdBtn("↺ Clear",  Color.FromArgb(130, 130, 130));
            _pBtnAdd.Click    += PBtnAdd_Click;
            _pBtnUpdate.Click += PBtnUpdate_Click;
            _pBtnDelete.Click += PBtnDelete_Click;
            _pBtnClear.Click  += (s, e) => PClearForm();
            btnFlow.Controls.Add(_pBtnAdd);
            btnFlow.Controls.Add(_pBtnUpdate);
            btnFlow.Controls.Add(_pBtnDelete);
            btnFlow.Controls.Add(_pBtnClear);
            titleRow.Controls.Add(btnFlow, 1, 0);

            // ── Fields: left grid + right image preview ───────────────────────
            var fieldsOuter = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Surface
            };
            fieldsOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            fieldsOuter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));

            // 4-column, 4-row field grid with EXPLICIT row heights
            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 4, BackColor = Surface,
                Padding = new Padding(0, 4, 8, 0)
            };
            for (int i = 0; i < 4; i++)
                fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));  // row 0: name/cat/price/stock
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));  // row 1: supplier/rating/lowstock/expiry
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));  // row 2: description + active
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));  // row 3: image path + browse

            // Row 0
            fields.Controls.Add(PFieldGroup("Name *",          out _pTxtName),       0, 0);
            fields.Controls.Add(PCategoryGroup(),                                     1, 0);
            fields.Controls.Add(PFieldGroup("Price ($) *",     out _pTxtPrice),      2, 0);
            fields.Controls.Add(PFieldGroup("Stock *",         out _pTxtStock),      3, 0);

            // Row 1
            fields.Controls.Add(PSupplierGroup(),                                     0, 1);
            fields.Controls.Add(PFieldGroup("Rating (0-5)",    out _pTxtRating),     1, 1);
            fields.Controls.Add(PFieldGroup("Low Stock Alert", out _pTxtLowStock),   2, 1);
            _pTxtLowStock.Text = "10";

            // Expiry date — use TableLayoutPanel so label is always above picker
            var expiryGrp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Surface,
                Padding = new Padding(4, 2, 4, 2)
            };
            expiryGrp.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            expiryGrp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            expiryGrp.Controls.Add(new Label { Text = "Expiry Date", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Fill }, 0, 0);
            _pDtpExpiry = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Font = new Font("Segoe UI", 10) };
            expiryGrp.Controls.Add(_pDtpExpiry, 0, 1);
            fields.Controls.Add(expiryGrp, 3, 1);

            // Row 2 — description spans 3 cols, active checkbox in col 3
            var descGrp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Surface,
                Padding = new Padding(4, 2, 4, 2)
            };
            descGrp.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            descGrp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            descGrp.Controls.Add(new Label { Text = "Description", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Fill }, 0, 0);
            _pTxtDescription = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            descGrp.Controls.Add(_pTxtDescription, 0, 1);
            fields.SetColumnSpan(descGrp, 3);
            fields.Controls.Add(descGrp, 0, 2);

            var activeGrp = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(4, 24, 4, 0) };
            _pChkActive = new CheckBox { Text = "Active", Font = new Font("Segoe UI", 10), Checked = true, AutoSize = true, Cursor = Cursors.Hand };
            activeGrp.Controls.Add(_pChkActive);
            fields.Controls.Add(activeGrp, 3, 2);

            // Row 3 — image path + browse button, spans 4 cols
            var imgRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Surface,
                Padding = new Padding(4, 2, 4, 2)
            };
            imgRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            imgRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

            var imgFieldGrp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Surface
            };
            imgFieldGrp.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            imgFieldGrp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            imgFieldGrp.Controls.Add(new Label { Text = "Image Path", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Fill }, 0, 0);
            _pTxtImagePath = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle, ReadOnly = true };
            imgFieldGrp.Controls.Add(_pTxtImagePath, 0, 1);

            var btnImgGrp = new Panel { Dock = DockStyle.Fill, BackColor = Surface, Padding = new Padding(0, 18, 0, 2) };
            _pBtnBrowseImage = new Button
            {
                Text = "Browse...", Dock = DockStyle.Fill, BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9), Cursor = Cursors.Hand
            };
            _pBtnBrowseImage.FlatAppearance.BorderSize = 0;
            _pBtnBrowseImage.Click += PBtnBrowseImage_Click;
            btnImgGrp.Controls.Add(_pBtnBrowseImage);

            imgRow.Controls.Add(imgFieldGrp, 0, 0);
            imgRow.Controls.Add(btnImgGrp,   1, 0);
            fields.SetColumnSpan(imgRow, 4);
            fields.Controls.Add(imgRow, 0, 3);

            // Image preview
            var previewPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 240, 240), Padding = new Padding(4) };
            previewPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                    e.Graphics.DrawRectangle(pen, 0, 0, previewPanel.Width - 1, previewPanel.Height - 1);
            };
            _pPicPreview = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(240, 240, 240) };
            previewPanel.Controls.Add(_pPicPreview);

            fieldsOuter.Controls.Add(fields,       0, 0);
            fieldsOuter.Controls.Add(previewPanel, 1, 0);

            // Add to formCard in REVERSE dock order: Fill content first, then Top title
            formCard.Controls.Add(fieldsOuter);
            formCard.Controls.Add(titleRow);

            // ── Grid card ─────────────────────────────────────────────────────
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            gridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1);
            };
            _pDgv = MakeGrid("dgvProducts");
            _pDgv.SelectionChanged += PDgvSelectionChanged;
            gridCard.Controls.Add(_pDgv);

            root.Controls.Add(formCard, 0, 0);
            root.Controls.Add(gridCard, 0, 1);
            _secProducts.Controls.Add(root);
            _content.Controls.Add(_secProducts);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 4: Orders section (grid + status update panel)
        // ═══════════════════════════════════════════════════════════════════════

        private void BuildOrdersSection()
        {
            _secOrders = Section();

            // Root split: grid top, status panel bottom (fixed height)
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = BgPage
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));

            // ── Grid card ─────────────────────────────────────────────────────
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            gridCard.Paint += (s, e) => { using (var pen = new Pen(Color.FromArgb(210, 210, 210))) e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1); };

            var titleLbl = new Label
            {
                Text = "Order Management", Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40), Dock = DockStyle.Top, Height = 44,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0)
            };

            _oDgv = MakeGrid("dgvOrders");
            _oDgv.SelectionChanged += ODgvSelectionChanged;

            gridCard.Controls.Add(_oDgv);
            gridCard.Controls.Add(titleLbl);

            // ── Status update card ────────────────────────────────────────────
            var statusCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Surface,
                Padding = new Padding(16, 10, 16, 10), Margin = new Padding(0, 8, 0, 0)
            };
            statusCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, statusCard.Width - 1, statusCard.Height - 1);
                using (var b = new SolidBrush(Blue))
                    e.Graphics.FillRectangle(b, 0, 0, 4, statusCard.Height);
            };

            var statusLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Surface
            };
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // selected label
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));  // "Status:" label
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); // combo
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); // button

            _oLblSelected = new Label
            {
                Text = "Select an order from the grid above to update its status.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(100, 100, 100),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var statusLbl = new Label
            {
                Text = "Status:", Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(60, 60, 60),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 6, 0)
            };

            _oCmbStatus = new ComboBox
            {
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5f), Margin = new Padding(0, 8, 8, 8)
            };
            foreach (var s in Order.ValidStatuses) _oCmbStatus.Items.Add(s);
            _oCmbStatus.SelectedIndex = 0;

            _oBtnUpdateStatus = new Button
            {
                Text = "✔  Update Status",
                Dock = DockStyle.Fill,
                BackColor = Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 0, 8)
            };
            _oBtnUpdateStatus.FlatAppearance.BorderSize = 0;
            _oBtnUpdateStatus.Click += OBtnUpdateStatus_Click;

            statusLayout.Controls.Add(_oLblSelected,     0, 0);
            statusLayout.Controls.Add(statusLbl,         1, 0);
            statusLayout.Controls.Add(_oCmbStatus,       2, 0);
            statusLayout.Controls.Add(_oBtnUpdateStatus, 3, 0);
            statusCard.Controls.Add(statusLayout);

            root.Controls.Add(gridCard,   0, 0);
            root.Controls.Add(statusCard, 0, 1);
            _secOrders.Controls.Add(root);
            _content.Controls.Add(_secOrders);
        }

        // ── Order selection + status update ───────────────────────────────────

        private void ODgvSelectionChanged(object sender, EventArgs e)
        {
            if (_oDgv.SelectedRows.Count == 0) return;
            var order = _oDgv.SelectedRows[0].DataBoundItem as Order;
            if (order == null) return;

            _oLblSelected.Text = $"Order: {order.Id.Substring(0, 8).ToUpper()}  |  " +
                                 $"Customer: {order.CustomerName}  |  " +
                                 $"Total: ${order.TotalAmount:F2}  |  " +
                                 $"Current Status: {order.Status}";
            _oLblSelected.ForeColor = Color.FromArgb(40, 40, 40);

            // Pre-select current status in combo
            for (int i = 0; i < _oCmbStatus.Items.Count; i++)
                if (_oCmbStatus.Items[i].ToString() == order.Status)
                { _oCmbStatus.SelectedIndex = i; break; }
        }

        private void OBtnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (_oDgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an order from the grid first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var order = _oDgv.SelectedRows[0].DataBoundItem as Order;
            if (order == null) return;

            string newStatus = _oCmbStatus.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(newStatus)) return;

            if (order.Status == newStatus)
            {
                MessageBox.Show($"Order is already '{newStatus}'.", "No Change",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            order.UpdateStatus(newStatus);
            _dataService.UpdateOrder(order);

            // Refresh grid
            _oDgv.DataSource = null;
            _oDgv.DataSource = _dataService.GetAllOrders();

            _oLblSelected.Text = $"✔  Order {order.Id.Substring(0, 8).ToUpper()} updated to '{newStatus}'";
            _oLblSelected.ForeColor = Green;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 5: Customers, Reports, Suppliers sections
        // ═══════════════════════════════════════════════════════════════════════

        private void BuildCustomersSection()
        {
            _secCustomers = Section();

            // Root: grid (fills) on top, edit panel (fixed) on bottom
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = BgPage
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));

            // ── Grid card ─────────────────────────────────────────────────────
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            gridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1);
            };

            var gridTitle = new Label
            {
                Text = "Customer Management", Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40), Dock = DockStyle.Top, Height = 44,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0)
            };

            _cDgv = MakeGrid("dgvCustomers");
            _cDgv.DataSource = _dataService.GetAllCustomers();
            _cDgv.SelectionChanged += CDgvSelectionChanged;

            // Friendly column headers + hide noisy columns
            _cDgv.DataBindingComplete += (s, e) =>
            {
                var map = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Id",                "ID"               },
                    { "Username",          "Username"         },
                    { "Email",             "Email"            },
                    { "FullName",          "Full Name"        },
                    { "Phone",             "Phone"            },
                    { "UserType",          "Type"             },
                    { "RegistrationDate",  "Registered"       },
                    { "LastLogin",         "Last Login"       },
                    { "IsActive",          "Active"           },
                    { "LoyaltyTier",       "Loyalty Tier"     },
                    { "TotalSpent",        "Total Spent ($)"  },
                    { "TotalOrders",       "Orders"           },
                    { "PreferredCategory", "Pref. Category"   },
                    { "DateOfBirth",       "Date of Birth"    }
                };
                foreach (DataGridViewColumn col in _cDgv.Columns)
                    if (map.ContainsKey(col.Name)) col.HeaderText = map[col.Name];

                foreach (var hide in new[] { "PasswordHash", "OrderIds", "Address" })
                    if (_cDgv.Columns.Contains(hide)) _cDgv.Columns[hide].Visible = false;
            };

            // Add in reverse dock order: Fill grid first, then Top title
            gridCard.Controls.Add(_cDgv);
            gridCard.Controls.Add(gridTitle);

            // ── Edit panel ────────────────────────────────────────────────────
            var editCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Surface,
                Padding = new Padding(14, 4, 14, 4), Margin = new Padding(0, 8, 0, 0)
            };
            editCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, editCard.Width - 1, editCard.Height - 1);
                using (var b = new SolidBrush(Blue))
                    e.Graphics.FillRectangle(b, 0, 0, 4, editCard.Height);
            };

            // Edit layout: info label | action buttons only
            var editLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Surface
            };
            editLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26)); // selected info
            editLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // buttons row

            _cLblSelected = new Label
            {
                Text = "Select a customer from the grid above to view their details.",
                Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(100, 100, 100),
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft
            };
            editLayout.Controls.Add(_cLblSelected, 0, 0);

            // Action buttons row (only 2 buttons: Toggle Active + Reset Password)
            var buttonsRow = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = Surface
            };
            buttonsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // spacer
            buttonsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); // toggle active (wider)
            buttonsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); // reset pw (wider)

            // Status label (shown after actions)
            _cLblStatus = new Label
            {
                Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = Green,
                Dock = DockStyle.Bottom, Height = 16, TextAlign = ContentAlignment.MiddleLeft
            };

            // Toggle active button
            _cBtnToggleActive = new Button
            {
                Text = "⊘ Deactivate", Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(230, 126, 34), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(4, 20, 4, 4), Height = 40
            };
            _cBtnToggleActive.FlatAppearance.BorderSize = 0;
            _cBtnToggleActive.Click += CBtnToggleActive_Click;

            // Reset password button
            _cBtnResetPw = new Button
            {
                Text = "🔑 Reset Password", Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(142, 68, 173), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand, Margin = new Padding(4, 20, 4, 4), Height = 40
            };
            _cBtnResetPw.FlatAppearance.BorderSize = 0;
            _cBtnResetPw.Click += CBtnResetPw_Click;

            buttonsRow.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 0); // spacer
            buttonsRow.Controls.Add(_cBtnToggleActive, 1, 0);
            buttonsRow.Controls.Add(_cBtnResetPw,      2, 0);

            editLayout.Controls.Add(buttonsRow, 0, 1);
            editCard.Controls.Add(_cLblStatus);
            editCard.Controls.Add(editLayout);

            root.Controls.Add(gridCard,  0, 0);
            root.Controls.Add(editCard,  0, 1);
            _secCustomers.Controls.Add(root);
            _content.Controls.Add(_secCustomers);
        }

        // ── Customer field helper ─────────────────────────────────────────────
        // ── Customer grid selection ───────────────────────────────────────────
        private void CDgvSelectionChanged(object sender, EventArgs e)
        {
            if (_cDgv.SelectedRows.Count == 0) return;
            var customer = _cDgv.SelectedRows[0].DataBoundItem as Customer;
            if (customer == null) return;

            _cLblSelected.Text =
                $"Selected:  {customer.FullName}  |  {customer.Username}  |  " +
                $"Tier: {customer.LoyaltyTier}  |  Spent: ${customer.TotalSpent:N2}  |  " +
                $"Orders: {customer.TotalOrders}  |  " +
                $"Status: {(customer.IsActive ? "Active" : "INACTIVE")}";
            _cLblSelected.ForeColor = customer.IsActive
                ? Color.FromArgb(40, 40, 40)
                : Color.FromArgb(192, 57, 43);

            // Update toggle button label
            _cBtnToggleActive.Text      = customer.IsActive ? "⊘ Deactivate" : "✔ Activate";
            _cBtnToggleActive.BackColor = customer.IsActive
                ? Color.FromArgb(230, 126, 34)
                : Color.FromArgb(39, 174, 96);

            _cLblStatus.Text = "";
        }

        // ── Toggle active/inactive ────────────────────────────────────────────
        private void CBtnToggleActive_Click(object sender, EventArgs e)
        {
            if (_cDgv.SelectedRows.Count == 0)
            { _cLblStatus.Text = "Select a customer first."; _cLblStatus.ForeColor = Danger; return; }

            var customer = _cDgv.SelectedRows[0].DataBoundItem as Customer;
            if (customer == null) return;

            string action = customer.IsActive ? "deactivate" : "activate";
            if (MessageBox.Show($"Are you sure you want to {action} {customer.FullName}?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            customer.IsActive = !customer.IsActive;
            _dataService.UpdateUser(customer);

            _cDgv.DataSource = null;
            _cDgv.DataSource = _dataService.GetAllCustomers();

            _cLblStatus.Text      = $"✔  {customer.FullName} is now {(customer.IsActive ? "Active" : "Inactive")}.";
            _cLblStatus.ForeColor = Green;
        }

        // ── Reset password ────────────────────────────────────────────────────
        private void CBtnResetPw_Click(object sender, EventArgs e)
        {
            if (_cDgv.SelectedRows.Count == 0)
            { _cLblStatus.Text = "Select a customer first."; _cLblStatus.ForeColor = Danger; return; }

            var customer = _cDgv.SelectedRows[0].DataBoundItem as Customer;
            if (customer == null) return;

            // Ask for new password
            string newPw = ShowInputDialog($"Enter new password for {customer.FullName}:", "Reset Password");
            if (string.IsNullOrWhiteSpace(newPw)) return;
            if (newPw.Length < 6)
            { _cLblStatus.Text = "Password must be at least 6 characters."; _cLblStatus.ForeColor = Danger; return; }

            var auth = new AuthService();
            auth.ResetPassword(customer.Id, newPw);

            _cLblStatus.Text      = $"✔  Password reset for {customer.FullName}.";
            _cLblStatus.ForeColor = Green;
        }

        private static string ShowInputDialog(string prompt, string title)
        {
            var dlg = new Form
            {
                Text = title, Size = new Size(380, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false, BackColor = Color.White
            };
            dlg.Controls.Add(new Label { Text = prompt, Location = new Point(16, 16), Size = new Size(340, 20), Font = new Font("Segoe UI", 9.5f) });
            var txt = new TextBox { Location = new Point(16, 42), Size = new Size(340, 26), Font = new Font("Segoe UI", 10.5f), UseSystemPasswordChar = true };
            dlg.Controls.Add(txt);
            var btnOk = new Button
            {
                Text = "OK", Location = new Point(16, 80), Size = new Size(100, 32),
                BackColor = Color.FromArgb(34, 139, 34), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button
            {
                Text = "Cancel", Location = new Point(126, 80), Size = new Size(100, 32),
                BackColor = Color.FromArgb(150, 150, 150), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCancel);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;
            return dlg.ShowDialog() == DialogResult.OK ? txt.Text : null;
        }

        // Report section controls
        private DataGridView _rDgvMain;
        private Label        _rLblSummary;
        private DateTimePicker _rDtpFrom, _rDtpTo;
        private ComboBox     _rCmbType;

        private void BuildReportsSection()
        {
            _secReports = Section();

            // Use TableLayoutPanel as root so the grid row gets a real Fill height
            // even inside an AutoScroll content panel
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, BackColor = BgPage
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // section title
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));   // controls bar
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));  // summary card
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // grid fills rest

            // ── Section title ─────────────────────────────────────────────────
            root.Controls.Add(new Label
            {
                Text = "Reports & Analytics",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            // ── Controls bar ──────────────────────────────────────────────────
            var bar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 1,
                BackColor = Surface, Padding = new Padding(12, 8, 12, 8)
            };
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, bar.Width - 1, bar.Height - 1);
            };

            _rCmbType = new ComboBox
            {
                Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 8, 0)
            };
            _rCmbType.Items.AddRange(new[] { "Sales Report", "Stock Report", "Customer Report", "Order Report" });
            _rCmbType.SelectedIndex = 0;

            _rDtpFrom = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 8, 0) };
            _rDtpFrom.Value = DateTime.Now.AddDays(-30);
            _rDtpTo = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 8, 0) };

            var btnGen = new Button
            {
                Text = "Generate", Dock = DockStyle.Right, Width = 130,
                BackColor = Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand
            };
            btnGen.FlatAppearance.BorderSize = 0;
            btnGen.Click += (s, e) => RunReport();

            bar.Controls.Add(_rCmbType, 0, 0);
            bar.Controls.Add(new Label { Text = "From:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(80, 80, 80) }, 1, 0);
            bar.Controls.Add(_rDtpFrom, 2, 0);
            bar.Controls.Add(new Label { Text = "To:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(80, 80, 80) }, 3, 0);
            bar.Controls.Add(_rDtpTo, 4, 0);
            bar.Controls.Add(btnGen, 5, 0);
            root.Controls.Add(bar, 0, 1);

            // ── Summary card ──────────────────────────────────────────────────
            var summaryCard = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Surface,
                Padding = new Padding(14, 8, 14, 8)
            };
            summaryCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, summaryCard.Width - 1, summaryCard.Height - 1);
                using (var b = new SolidBrush(Green))
                    e.Graphics.FillRectangle(b, 0, 0, 4, summaryCard.Height);
            };
            _rLblSummary = new Label
            {
                Text = "Select a report type and click Generate.",
                Font = new Font("Consolas", 9.5f), ForeColor = Color.FromArgb(50, 50, 50),
                Dock = DockStyle.Fill
            };
            summaryCard.Controls.Add(_rLblSummary);
            root.Controls.Add(summaryCard, 0, 2);

            // ── Data grid (fills remaining height) ───────────────────────────
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            gridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1);
            };
            _rDgvMain = MakeGrid("dgvReport");
            gridCard.Controls.Add(_rDgvMain);
            root.Controls.Add(gridCard, 0, 3);

            _secReports.Controls.Add(root);
            _content.Controls.Add(_secReports);
        }

        private void BuildSuppliersSection()
        {
            _secSuppliers = Section();

            // Root TableLayoutPanel: title row + grid fills rest
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = BgPage
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // title
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // grid

            root.Controls.Add(new Label
            {
                Text = "Supplier Management",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            gridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1);
            };

            var dgv = MakeGrid("dgvSuppliers");
            dgv.DataSource = _dataService.GetAllSuppliers();

            // Friendly column headers
            dgv.DataBindingComplete += (s, e) =>
            {
                var map = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "Id",            "ID"             },
                    { "Name",          "Supplier Name"  },
                    { "ContactPerson", "Contact Person" },
                    { "Email",         "Email"          },
                    { "Phone",         "Phone"          },
                    { "Address",       "Address"        },
                    { "Website",       "Website"        },
                    { "IsActive",      "Active"         },
                    { "DateAdded",     "Date Added"     },
                    { "Notes",         "Notes"          }
                };
                foreach (DataGridViewColumn col in dgv.Columns)
                    if (map.ContainsKey(col.Name)) col.HeaderText = map[col.Name];

                // Hide less useful columns
                foreach (var hide in new[] { "Notes", "Address" })
                    if (dgv.Columns.Contains(hide)) dgv.Columns[hide].Visible = false;
            };

            gridCard.Controls.Add(dgv);
            root.Controls.Add(gridCard, 0, 1);
            _secSuppliers.Controls.Add(root);
            _content.Controls.Add(_secSuppliers);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 6: Product form logic (inline Add/Update/Delete/Clear)
        // ═══════════════════════════════════════════════════════════════════════

        private Panel PCategoryGroup()
        {
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Surface,
                Padding = new Padding(4, 2, 4, 2)
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tbl.Controls.Add(new Label { Text = "Category *", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Fill }, 0, 0);
            _pCmbCategory = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var cat in ProductCategories) _pCmbCategory.Items.Add(cat);
            foreach (var cat in _dataService.GetAllCategories())
                if (!_pCmbCategory.Items.Contains(cat)) _pCmbCategory.Items.Add(cat);
            if (_pCmbCategory.Items.Count > 0) _pCmbCategory.SelectedIndex = 0;
            tbl.Controls.Add(_pCmbCategory, 0, 1);
            return tbl;
        }

        private Panel PSupplierGroup()
        {
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Surface,
                Padding = new Padding(4, 2, 4, 2)
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tbl.Controls.Add(new Label { Text = "Supplier", Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Fill }, 0, 0);
            _pCmbSupplier = new ComboBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDownList };
            // Populate with existing suppliers: display Name, store Id as tag via index
            _pCmbSupplier.Items.Add("-- None --");
            foreach (var sup in _dataService.GetAllSuppliers())
                _pCmbSupplier.Items.Add($"{sup.Name} ({sup.Id})");
            _pCmbSupplier.SelectedIndex = 0;
            tbl.Controls.Add(_pCmbSupplier, 0, 1);
            return tbl;
        }

        private static Panel PFieldGroup(string label, out TextBox box)
        {
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White,
                Padding = new Padding(4, 2, 4, 2)
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tbl.Controls.Add(new Label { Text = label, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(80, 80, 80), Dock = DockStyle.Fill }, 0, 0);
            var txt = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), BorderStyle = BorderStyle.FixedSingle };
            tbl.Controls.Add(txt, 0, 1);
            box = txt;
            return tbl;
        }

        private static Button ProdBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text, Size = new Size(92, 32), BackColor = bg, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void PBtnBrowseImage_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog
            {
                Title = "Select Product Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*"
            })
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _pTxtImagePath.Text = dlg.FileName;
                    try { _pPicPreview.Image = Image.FromFile(dlg.FileName); }
                    catch { _pPicPreview.Image = null; }
                }
            }
        }
        
        /// <summary>
        /// Copies the selected image to Resources/Images folder and returns the relative path
        /// </summary>
        private string CopyImageToResources(string sourceImagePath, string productId)
        {
            if (string.IsNullOrWhiteSpace(sourceImagePath) || !File.Exists(sourceImagePath))
                return null;
                
            try
            {
                // Get the file extension
                string extension = Path.GetExtension(sourceImagePath);
                
                // Get the project root directory (go up from bin/Debug to project root)
                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                // Navigate up from bin\Debug\ to project root
                projectRoot = Directory.GetParent(projectRoot).Parent.FullName;
                
                // Create the Resources/Images folder path in the actual project directory
                string imagesFolder = Path.Combine(projectRoot, "Resources", "Images");
                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);
                
                // Create new filename: product_{productId}.{extension}
                string newFileName = $"product_{productId}{extension}";
                string destinationPath = Path.Combine(imagesFolder, newFileName);
                
                // Copy the file (overwrite if exists)
                File.Copy(sourceImagePath, destinationPath, true);
                
                // Return relative path from project root
                return Path.Combine("Resources", "Images", newFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying image: {ex.Message}", "Image Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        /// <summary>
        /// Resolves a relative image path to full path (handles both relative and absolute paths)
        /// </summary>
        private string ResolveImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;
                
            // If it's already an absolute path and exists, return it
            if (Path.IsPathRooted(imagePath) && File.Exists(imagePath))
                return imagePath;
                
            // If it's a relative path, resolve it from project root
            try
            {
                // Get project root (go up from bin/Debug)
                string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                projectRoot = Directory.GetParent(projectRoot).Parent.FullName;
                
                // Combine with relative path
                string fullPath = Path.Combine(projectRoot, imagePath);
                
                if (File.Exists(fullPath))
                    return fullPath;
            }
            catch { }
            
            return null;
        }

        private void PDgvSelectionChanged(object sender, EventArgs e)
        {
            if (_pDgv.SelectedRows.Count == 0) return;
            var p = _pDgv.SelectedRows[0].DataBoundItem as Product;
            if (p == null) return;

            _pSelectedId           = p.Id;
            _pTxtName.Text         = p.Name;
            _pTxtPrice.Text        = p.Price.ToString("F2");
            _pTxtStock.Text        = p.Stock.ToString();
            _pTxtDescription.Text  = p.Description;
            // Set supplier dropdown — match by supplier ID embedded in item text
            _pCmbSupplier.SelectedIndex = 0; // default to None
            for (int i = 1; i < _pCmbSupplier.Items.Count; i++)
            {
                if (_pCmbSupplier.Items[i].ToString().Contains(p.SupplierId))
                { _pCmbSupplier.SelectedIndex = i; break; }
            }
            _pTxtRating.Text       = p.Rating.ToString("F1");
            _pTxtLowStock.Text     = p.LowStockThreshold.ToString();
            _pTxtImagePath.Text    = p.ImageUrl ?? "";
            _pDtpExpiry.Value      = p.ExpiryDate != DateTime.MinValue ? p.ExpiryDate : DateTime.Now;
            _pChkActive.Checked    = p.IsActive;

            if (_pCmbCategory.Items.Contains(p.Category))
                _pCmbCategory.SelectedItem = p.Category;
            else { _pCmbCategory.Items.Add(p.Category); _pCmbCategory.SelectedItem = p.Category; }

            // Load image preview - resolve relative path to full path
            string fullImagePath = ResolveImagePath(p.ImageUrl);
            if (fullImagePath != null)
            { try { _pPicPreview.Image = Image.FromFile(fullImagePath); } catch { _pPicPreview.Image = null; } }
            else _pPicPreview.Image = null;
        }

        private void PBtnAdd_Click(object sender, EventArgs e)
        {
            if (!PValidate()) return;
            try
            {
                _dataService.AddProduct(PBuildProduct(null));
                _pDgv.DataSource = null;
                _pDgv.DataSource = _dataService.GetAllProducts();
                PClearForm();
                MessageBox.Show("Product added.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void PBtnUpdate_Click(object sender, EventArgs e)
        {
            if (_pSelectedId == null)
            { MessageBox.Show("Select a product to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (!PValidate()) return;
            try
            {
                _dataService.UpdateProduct(PBuildProduct(_pSelectedId));
                _pDgv.DataSource = null;
                _pDgv.DataSource = _dataService.GetAllProducts();
                MessageBox.Show("Product updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void PBtnDelete_Click(object sender, EventArgs e)
        {
            if (_pSelectedId == null)
            { MessageBox.Show("Select a product to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show("Delete this product?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _dataService.DeleteProduct(_pSelectedId);
                _pDgv.DataSource = null;
                _pDgv.DataSource = _dataService.GetAllProducts();
                PClearForm();
            }
        }

        private static string ExtractSupplierId(string comboText)
        {
            // Format is "Supplier Name (sup-001)" — extract the ID inside parentheses
            if (string.IsNullOrEmpty(comboText) || comboText.StartsWith("--")) return "";
            int start = comboText.LastIndexOf('(');
            int end   = comboText.LastIndexOf(')');
            if (start >= 0 && end > start)
                return comboText.Substring(start + 1, end - start - 1).Trim();
            return comboText.Trim();
        }

        private bool PValidate()
        {
            if (string.IsNullOrWhiteSpace(_pTxtName.Text))
            { MessageBox.Show("Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (_pCmbCategory.SelectedItem == null)
            { MessageBox.Show("Select a category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!decimal.TryParse(_pTxtPrice.Text, out var pr) || pr < 0)
            { MessageBox.Show("Enter a valid price.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (!int.TryParse(_pTxtStock.Text, out var st) || st < 0)
            { MessageBox.Show("Enter a valid stock quantity.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        private Product PBuildProduct(string id)
        {
            // Preserve original DateAdded when updating an existing product
            DateTime dateAdded = DateTime.Now;
            string existingImageUrl = null;
            if (id != null)
            {
                var existing = _dataService.GetProductById(id);
                if (existing != null)
                {
                    dateAdded = existing.DateAdded;
                    existingImageUrl = existing.ImageUrl;
                }
            }
            
            // Generate product ID if new
            string productId = id ?? Guid.NewGuid().ToString();
            
            // Handle image: copy to Resources/Images if a new image was selected
            string imageUrl = existingImageUrl; // default to existing
            if (!string.IsNullOrWhiteSpace(_pTxtImagePath.Text))
            {
                // Check if this is a new external image (not already in Resources/Images)
                if (!_pTxtImagePath.Text.Contains("Resources\\Images") && 
                    !_pTxtImagePath.Text.Contains("Resources/Images"))
                {
                    // Copy the image to Resources/Images
                    string copiedPath = CopyImageToResources(_pTxtImagePath.Text, productId);
                    if (copiedPath != null)
                        imageUrl = copiedPath;
                    else
                        imageUrl = _pTxtImagePath.Text; // fallback to original path if copy failed
                }
                else
                {
                    // Already in Resources/Images, use as-is
                    imageUrl = _pTxtImagePath.Text;
                }
            }

            return new Product
            {
                Id                = productId,
                Name              = _pTxtName.Text.Trim(),
                Category          = _pCmbCategory.SelectedItem.ToString(),
                Price             = decimal.TryParse(_pTxtPrice.Text, out var pr) ? pr : 0,
                Stock             = int.TryParse(_pTxtStock.Text, out var st) ? st : 0,
                Description       = _pTxtDescription.Text.Trim(),
                SupplierId        = ExtractSupplierId(_pCmbSupplier.SelectedItem?.ToString()),
                Rating            = decimal.TryParse(_pTxtRating.Text, out var rt) ? rt : 0,
                LowStockThreshold = int.TryParse(_pTxtLowStock.Text, out var ls) ? ls : 10,
                ExpiryDate        = _pDtpExpiry.Value,
                IsActive          = _pChkActive.Checked,
                ImageUrl          = imageUrl,   // relative path in Resources/Images
                DateAdded         = dateAdded
            };
        }

        private void PClearForm()
        {
            _pSelectedId = null;
            _pTxtName.Clear(); _pTxtPrice.Clear(); _pTxtStock.Clear();
            _pTxtDescription.Clear(); _pTxtRating.Clear(); _pTxtLowStock.Text = "10";
            _pTxtImagePath.Clear(); _pPicPreview.Image = null;
            _pDtpExpiry.Value = DateTime.Now; _pChkActive.Checked = true;
            if (_pCmbCategory.Items.Count > 0) _pCmbCategory.SelectedIndex = 0;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 7: ShowSection, data loaders, reports, UI factories
        // ═══════════════════════════════════════════════════════════════════════

        private void ShowSection(string name)
        {
            _secDashboard.Visible = false; _secProducts.Visible  = false;
            _secOrders.Visible    = false; _secCustomers.Visible = false;
            _secReports.Visible   = false; _secSuppliers.Visible = false;

            switch (name)
            {
                case "dashboard": _secDashboard.Visible = true; LoadDashboardData(); break;
                case "products":
                    _secProducts.Visible = true;
                    _pDgv.DataSource = null;
                    _pDgv.DataSource = _dataService.GetAllProducts();
                    break;
                case "orders":
                    _secOrders.Visible = true;
                    _oDgv.DataSource = null;
                    _oDgv.DataSource = _dataService.GetAllOrders();
                    break;
                case "customers":
                    _secCustomers.Visible = true;
                    _cDgv.DataSource = null;
                    _cDgv.DataSource = _dataService.GetAllCustomers();
                    break;
                case "reports":   _secReports.Visible   = true; break;
                case "suppliers":
                    _secSuppliers.Visible = true;
                    var sdgv = FindGrid(_secSuppliers, "dgvSuppliers");
                    if (sdgv != null) { sdgv.DataSource = null; sdgv.DataSource = _dataService.GetAllSuppliers(); }
                    break;            }

            foreach (var btn in new[] { _btnDashboard, _btnProducts, _btnOrders,
                                        _btnCustomers, _btnReports, _btnSuppliers })
                btn.BackColor = Green;

            Button active = null;
            switch (name)
            {
                case "dashboard": active = _btnDashboard; break;
                case "products":  active = _btnProducts;  break;
                case "orders":    active = _btnOrders;    break;
                case "customers": active = _btnCustomers; break;
                case "reports":   active = _btnReports;   break;
                case "suppliers": active = _btnSuppliers; break;
            }
            if (active != null) active.BackColor = GreenDark;
        }

        private void LoadDashboardData()
        {
            // Sync all customer data from analytics.json to users.json first
            _dataService.SyncAllCustomersFromAnalytics();
            
            var products  = _dataService.GetAllProducts();
            var orders    = _dataService.GetAllOrders();
            var customers = _dataService.GetAllCustomers();
            var revenue   = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount);

            // Stat cards
            _statProducts.Text  = products.Count.ToString();
            _statOrders.Text    = orders.Count.ToString();
            _statCustomers.Text = customers.Count.ToString();
            _statRevenue.Text   = $"${revenue:N0}";

            // Stock health
            _dashLowStock.Text   = products.Count(p => p.IsLowStock()).ToString();
            _dashOutOfStock.Text = products.Count(p => p.Stock == 0).ToString();
            _dashNearExpiry.Text = products.Count(p => p.IsNearExpiry(30)).ToString();

            // Order status breakdown
            var statusCounts = _dataService.GetOrderStatusCounts();
            _dashPending.Text   = statusCounts.ContainsKey("Pending")   ? statusCounts["Pending"].ToString()   : "0";
            _dashShipped.Text   = statusCounts.ContainsKey("Shipped")   ? statusCounts["Shipped"].ToString()   : "0";
            _dashDelivered.Text = statusCounts.ContainsKey("Delivered") ? statusCounts["Delivered"].ToString() : "0";
            _dashCancelled.Text = statusCounts.ContainsKey("Cancelled") ? statusCounts["Cancelled"].ToString() : "0";

            // Last 30 days
            var from30 = DateTime.Now.AddDays(-30);
            var recent30 = orders.Where(o => o.OrderDate >= from30 && o.Status != "Cancelled").ToList();
            _dashRevenue30.Text = $"${recent30.Sum(o => o.TotalAmount):N0}";
            _dashOrders30.Text  = recent30.Count.ToString();

            // Top category from products
            var topCat = products
                .GroupBy(p => p.Category)
                .OrderByDescending(g => g.Sum(p => p.Stock))
                .FirstOrDefault();
            _dashTopCat.Text = topCat != null ? topCat.Key : "—";

            // Recent orders grid (last 10)
            var recentOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new
                {
                    ID       = o.Id.Substring(0, 8).ToUpper(),
                    Customer = o.CustomerName,
                    Date     = o.OrderDate.ToString("MM/dd HH:mm"),
                    Total    = $"${o.TotalAmount:F2}",
                    Status   = o.Status
                }).ToList();
            _dashDgvRecentOrders.DataSource = null;
            _dashDgvRecentOrders.DataSource = recentOrders;

            // Low stock grid
            var lowStockList = products
                .Where(p => p.IsLowStock())
                .OrderBy(p => p.Stock)
                .Select(p => new
                {
                    Name     = p.Name,
                    Category = p.Category,
                    Stock    = p.Stock,
                    Minimum  = p.LowStockThreshold
                }).ToList();
            _dashDgvLowStock.DataSource = null;
            _dashDgvLowStock.DataSource = lowStockList;
        }

        private DataGridView FindGrid(Panel section, string name)
        {
            foreach (Control c in section.Controls)
                if (c is DataGridView d && d.Name == name) return d;
            return null;
        }

        private void RunReport()
        {
            switch (_rCmbType.SelectedItem?.ToString())
            {
                case "Sales Report":    ShowSalesReport();    break;
                case "Stock Report":    ShowStockReport();    break;
                case "Customer Report": ShowCustomerReport(); break;
                case "Order Report":    ShowOrderReport();    break;
            }
        }

        private void ShowSalesReport()
        {
            var r = _reportService.GenerateSalesReport(_rDtpFrom.Value, _rDtpTo.Value);
            var sb = new System.Text.StringBuilder();
            foreach (var k in r.OrdersByStatus.Keys) sb.Append($"  {k}: {r.OrdersByStatus[k]}   ");
            _rLblSummary.Text =
                $"SALES REPORT  |  {_rDtpFrom.Value:yyyy-MM-dd} to {_rDtpTo.Value:yyyy-MM-dd}\r\n" +
                $"Total Orders: {r.TotalOrders}   |   Revenue: ${r.TotalRevenue:N2}   |   Avg Order: ${r.AverageOrderValue:N2}\r\n" +
                $"By Status:{sb}";

            // Clear existing data and columns
            _rDgvMain.DataSource = null;
            _rDgvMain.Columns.Clear();
            _rDgvMain.Rows.Clear();

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Order ID");
            dt.Columns.Add("Customer");
            dt.Columns.Add("Date");
            dt.Columns.Add("Total ($)");
            dt.Columns.Add("Items");
            dt.Columns.Add("Status");
            foreach (var o in r.Orders)
                dt.Rows.Add(o.Id.Substring(0, 8).ToUpper(), o.CustomerName,
                    o.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                    o.TotalAmount.ToString("F2"),
                    o.GetTotalItemCount(), o.Status);
            _rDgvMain.DataSource = dt;
        }

        private void ShowStockReport()
        {
            var r = _reportService.GenerateStockReport();
            var sb = new System.Text.StringBuilder();
            foreach (var k in r.StockByCategory.Keys) sb.Append($"  {k}: {r.StockByCategory[k]}   ");
            _rLblSummary.Text =
                $"STOCK REPORT\r\n" +
                $"Products: {r.TotalProducts}   |   Value: ${r.TotalStockValue:N2}   |   " +
                $"Low Stock: {r.LowStockProducts.Count}   |   Out of Stock: {r.OutOfStockProducts.Count}   |   Near Expiry: {r.NearExpiryProducts.Count}\r\n" +
                $"By Category:{sb}";

            var allProducts = _dataService.GetAllProducts();
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Category");
            dt.Columns.Add("Stock");
            dt.Columns.Add("Min Threshold");
            dt.Columns.Add("Price ($)");
            dt.Columns.Add("Status");
            dt.Columns.Add("Expiry");
            foreach (var p in allProducts)
            {
                string status = p.Stock == 0 ? "Out of Stock"
                              : p.IsLowStock() ? "Low Stock"
                              : p.IsNearExpiry(30) ? "Near Expiry"
                              : "OK";
                dt.Rows.Add(p.Name, p.Category, p.Stock, p.LowStockThreshold,
                    p.Price.ToString("F2"), status,
                    p.ExpiryDate != DateTime.MinValue ? p.ExpiryDate.ToString("yyyy-MM-dd") : "N/A");
            }
            _rDgvMain.DataSource = null;
            _rDgvMain.Columns.Clear();
            _rDgvMain.DataSource = dt;
        }

        private void ShowCustomerReport()
        {
            var r = _reportService.GenerateCustomerReport();
            var sb = new System.Text.StringBuilder();
            foreach (var k in r.CustomersByTier.Keys) sb.Append($"  {k}: {r.CustomersByTier[k]}   ");
            _rLblSummary.Text =
                $"CUSTOMER REPORT\r\n" +
                $"Total: {r.TotalCustomers}   |   Active: {r.ActiveCustomers}   |   Revenue: ${r.TotalRevenue:N2}   |   Avg Value: ${r.AverageCustomerValue:N2}\r\n" +
                $"By Tier:{sb}";

            // Clear existing data and columns
            _rDgvMain.DataSource = null;
            _rDgvMain.Columns.Clear();
            _rDgvMain.Rows.Clear();

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Customer");
            dt.Columns.Add("Orders");
            dt.Columns.Add("Total Spent ($)");
            dt.Columns.Add("Avg Order ($)");
            dt.Columns.Add("Fav Category");
            dt.Columns.Add("Last Purchase");
            foreach (var a in r.TopCustomers)
                dt.Rows.Add(a.CustomerName, a.TotalOrders,
                    a.TotalSpent.ToString("N2"), a.AverageOrderValue.ToString("N2"),
                    a.FavoriteCategory, a.LastPurchaseDate.ToString("yyyy-MM-dd"));
            _rDgvMain.DataSource = dt;
        }

        private void ShowOrderReport()
        {
            var r = _reportService.GenerateOrderReport();
            _rLblSummary.Text =
                $"ORDER REPORT\r\n" +
                $"Total: {r.TotalOrders}   |   Pending: {r.PendingOrders}   |   " +
                $"Shipped: {r.ShippedOrders}   |   Delivered: {r.DeliveredOrders}   |   Cancelled: {r.CancelledOrders}";

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Order ID");
            dt.Columns.Add("Customer");
            dt.Columns.Add("Date");
            dt.Columns.Add("Total ($)");
            dt.Columns.Add("Items");
            dt.Columns.Add("Status");
            dt.Columns.Add("Address");
            foreach (var o in r.RecentOrders)
                dt.Rows.Add(o.Id.Substring(0, 8).ToUpper(), o.CustomerName,
                    o.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                    o.TotalAmount.ToString("F2"),
                    o.GetTotalItemCount(), o.Status, o.ShippingAddress);
            _rDgvMain.DataSource = null;
            _rDgvMain.Columns.Clear();
            _rDgvMain.DataSource = dt;
        }

        // ── UI factories ──────────────────────────────────────────────────────

        private Button NavBtn(string text, Color? bg = null)
        {
            var b = new Button
            {
                Text = text, Width = 200, Height = 44, BackColor = bg ?? Green,
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f), TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0), Cursor = Cursors.Hand, Margin = new Padding(0, 2, 0, 2)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = bg.HasValue
                ? Color.FromArgb(Math.Max(0, bg.Value.R - 20), Math.Max(0, bg.Value.G - 20), Math.Max(0, bg.Value.B - 20))
                : GreenDark;
            return b;
        }

        private static Panel Section() => new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Visible = false
        };

        // Dashboard section needs AutoScroll since it uses Dock=Top rows
        private static Panel ScrollSection() => new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Visible = false,
            AutoScroll = true
        };

        private static Label SectionTitle(string text) => new Label
        {
            Text = text, Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.FromArgb(40, 40, 40), Dock = DockStyle.Top,
            Height = 44, TextAlign = ContentAlignment.MiddleLeft
        };

        private static Button ActionButton(string text, Color bg)
        {
            var b = new Button
            {
                Text = text, Size = new Size(170, 36), BackColor = bg, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 10, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static DataGridView MakeGrid(string name)
        {
            var dgv = new DataGridView
            {
                Name = name, Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Surface, BorderStyle = BorderStyle.None,
                ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                RowHeadersVisible = false, GridColor = Color.FromArgb(230, 230, 230),
                Font = new Font("Segoe UI", 9.5f), ColumnHeadersHeight = 36
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 246);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 60);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 253, 251);
            return dgv;
        }

        private (Panel card, Label valueLabel) StatCard(string title, string value, Color accent)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill, BackColor = Surface,
                Margin = new Padding(0, 0, 12, 0), Padding = new Padding(18, 14, 18, 0)
            };
            card.Paint += (s, e) =>
            {
                using (var brush = new SolidBrush(accent))
                    e.Graphics.FillRectangle(brush, 0, card.Height - 4, card.Width, 4);
                using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            var lTitle = new Label { Text = title, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, AutoSize = true, Location = new Point(18, 14) };
            var lValue = new Label { Text = value, Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = accent, AutoSize = true, Location = new Point(16, 38) };
            card.Controls.Add(lTitle);
            card.Controls.Add(lValue);
            return (card, lValue);
        }
    }
}

// commit 17: docs: document AdminDashboard

// commit 25: refactor: simplify AdminDashboard logic

// commit 33: fix: address problem in AdminDashboard

// commit 41: refactor: optimize AdminDashboard

// commit 49: refactor: optimize AdminDashboard
