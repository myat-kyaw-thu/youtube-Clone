using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Forms
{
    // ═══════════════════════════════════════════════════════════════════════════
    // CustomerDashboard — Part 1: Fields, colours, constructor, InitializeComponent
    // ═══════════════════════════════════════════════════════════════════════════
    public class CustomerDashboard : Form
    {
        // Services
        private readonly AuthService  _authService;
        private readonly DataService  _dataService;
        private readonly SearchService _searchService;

        // Layout panels
        private Panel _sidebar, _mainArea, _header, _content;

        // Nav buttons
        private Button _btnBrowse, _btnCart, _btnOrders, _btnProfile, _btnLogout;

        // Content sections
        private Panel _secBrowse, _secCart, _secOrders, _secProfile;

        // Header cart badge
        private Label _lblCartBadge;

        // Cart section controls
        private DataGridView _dgvCart;
        private Label        _lblCartTotal;

        // Cart state — stores (product, qty) pairs
        private readonly List<CartItem> _cartItems = new List<CartItem>();

        // Current customer reference
        private Customer _currentCustomer;

        // Colours
        private static readonly Color Green     = Color.FromArgb(34, 139, 34);
        private static readonly Color GreenDark = Color.FromArgb(24, 110, 24);
        private static readonly Color BgPage    = Color.FromArgb(245, 248, 246);
        private static readonly Color Surface   = Color.White;
        private static readonly Color Danger    = Color.FromArgb(192, 57, 43);

        // ── Simple cart line-item ─────────────────────────────────────────────
        private class CartItem
        {
            public Product Product { get; set; }
            public int     Qty     { get; set; }
            public decimal Subtotal => Product.Price * Qty;
        }

        public CustomerDashboard()
        {
            _authService     = new AuthService();
            _dataService     = new DataService();
            _searchService   = new SearchService();
            _currentCustomer = _authService.GetCurrentUser() as Customer;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text        = "GreenLife Organic Store";
            this.MinimumSize = new Size(900, 580);
            this.Size        = new Size(1180, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor   = BgPage;

            BuildSidebar();
            BuildMainArea();

            this.Controls.Add(_mainArea);
            this.Controls.Add(_sidebar);

            ShowSection("browse");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 2: Sidebar + Main area builders
        // ═══════════════════════════════════════════════════════════════════════

        private void BuildSidebar()
        {
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 210, BackColor = Green };

            var logo = new Label
            {
                Text      = "GreenLife",
                Font      = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Top,
                Height    = 54,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = GreenDark
            };
            var sub = new Label
            {
                Text      = "My Account",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 230, 180),
                Dock      = DockStyle.Top,
                Height    = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = GreenDark
            };

            var navFlow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                Padding       = new Padding(8, 12, 8, 0),
                BackColor     = Green
            };

            _btnBrowse  = NavBtn("🔍  Browse Products");
            _btnCart    = NavBtn("🛒  Shopping Cart");
            _btnOrders  = NavBtn("📦  My Orders");
            _btnProfile = NavBtn("👤  Profile");
            _btnLogout  = NavBtn("🚪  Logout", Danger);

            _btnBrowse.Click  += (s, e) => ShowSection("browse");
            _btnCart.Click    += (s, e) => ShowSection("cart");
            _btnOrders.Click  += (s, e) => ShowSection("orders");
            _btnProfile.Click += (s, e) => ShowSection("profile");
            _btnLogout.Click  += (s, e) => { _authService.Logout(); this.Close(); };

            navFlow.Controls.Add(_btnBrowse);
            navFlow.Controls.Add(_btnCart);
            navFlow.Controls.Add(_btnOrders);
            navFlow.Controls.Add(_btnProfile);
            navFlow.Controls.Add(new Panel { Width = 194, Height = 16, BackColor = Green });
            navFlow.Controls.Add(_btnLogout);

            _sidebar.Controls.Add(navFlow);
            _sidebar.Controls.Add(sub);
            _sidebar.Controls.Add(logo);
        }

        private void BuildMainArea()
        {
            _mainArea = new Panel { Dock = DockStyle.Fill, BackColor = BgPage };

            // Header bar
            var user = _authService.GetCurrentUser();
            _header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 60,
                BackColor = Surface,
                Padding   = new Padding(24, 0, 24, 0)
            };
            _header.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Color.FromArgb(220, 220, 220)),
                    0, _header.Height - 1, _header.Width, _header.Height - 1);

            _header.Controls.Add(new Label
            {
                Text      = "Welcome,",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize  = true,
                Location  = new Point(24, 12)
            });
            _header.Controls.Add(new Label
            {
                Text      = user?.FullName ?? "Customer",
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                AutoSize  = true,
                Location  = new Point(24, 30)
            });

            _lblCartBadge = new Label
            {
                Text      = "🛒  0 items",
                Font      = new Font("Segoe UI", 10),
                ForeColor = Green,
                AutoSize  = true,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            _header.Controls.Add(_lblCartBadge);
            _header.Resize += (s, e) =>
                _lblCartBadge.Location = new Point(_header.Width - _lblCartBadge.Width - 24, 22);

            // Content area
            _content = new Panel
            {
                Dock       = DockStyle.Fill,
                BackColor  = BgPage,
                AutoScroll = true,
                Padding    = new Padding(24, 20, 24, 20)
            };

            BuildBrowseSection();
            BuildCartSection();
            BuildOrdersSection();
            BuildProfileSection();

            _mainArea.Controls.Add(_content);
            _mainArea.Controls.Add(_header);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 3: Browse section (product grid + qty-aware Add to Cart)
        // ═══════════════════════════════════════════════════════════════════════

        private void BuildBrowseSection()
        {
            _secBrowse = Section();

            // Top bar: title | search box | category combo | search button
            var topBar = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                Height      = 52,
                ColumnCount = 4,
                RowCount    = 1,
                BackColor   = Color.Transparent,
                Padding     = new Padding(0, 6, 0, 6)
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            topBar.Controls.Add(new Label
            {
                Text      = "Browse Products",
                Font      = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin    = new Padding(0, 0, 16, 0)
            }, 0, 0);

            var txtSearch = new TextBox
            {
                Dock   = DockStyle.Fill,
                Font   = new Font("Segoe UI", 10),
                Margin = new Padding(0, 4, 8, 4)
            };

            var cmbCat = new ComboBox
            {
                Dock          = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = new Font("Segoe UI", 10),
                Margin        = new Padding(0, 4, 8, 4)
            };
            cmbCat.Items.Add("All Categories");
            foreach (var c in _dataService.GetAllCategories()) cmbCat.Items.Add(c);
            cmbCat.SelectedIndex = 0;

            var btnSearch = new Button
            {
                Text      = "Search",
                Dock      = DockStyle.Fill,
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 4, 0, 4)
            };
            btnSearch.FlatAppearance.BorderSize = 0;

            topBar.Controls.Add(txtSearch, 1, 0);
            topBar.Controls.Add(cmbCat,    2, 0);
            topBar.Controls.Add(btnSearch, 3, 0);

            // Product grid
            var dgv = MakeGrid("dgvBrowse");
            dgv.DataSource = _dataService.GetActiveProducts();

            // Bottom bar: Add to Cart button
            var cartBar = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 50,
                BackColor = Color.Transparent,
                Padding   = new Padding(0, 8, 0, 0)
            };
            var btnAdd = new Button
            {
                Text      = "🛒  Select Item & Add to Cart",
                Size      = new Size(240, 36),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Location  = new Point(0, 0)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => AddToCartWithQty(dgv);
            cartBar.Controls.Add(btnAdd);

            // Search handler
            btnSearch.Click += (s, e) =>
            {
                var results = _searchService.LinearSearchByName(txtSearch.Text);
                if (cmbCat.SelectedIndex > 0)
                    results = results.FindAll(p => p.Category == cmbCat.SelectedItem.ToString());
                dgv.DataSource = results;
            };

            // Add controls in correct dock order (Bottom first, then Fill, then Top)
            _secBrowse.Controls.Add(cartBar);
            _secBrowse.Controls.Add(dgv);
            _secBrowse.Controls.Add(topBar);
            _content.Controls.Add(_secBrowse);
        }

        // ── Qty dialog + add to cart ──────────────────────────────────────────
        private void AddToCartWithQty(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var product = dgv.SelectedRows[0].DataBoundItem as Product;
            if (product == null || product.Stock <= 0)
            {
                MessageBox.Show("This product is out of stock.", "Out of Stock",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // How many already in cart for this product?
            int alreadyInCart = 0;
            foreach (var ci in _cartItems)
                if (ci.Product.Id == product.Id) alreadyInCart += ci.Qty;

            int available = product.Stock - alreadyInCart;
            if (available <= 0)
            {
                MessageBox.Show($"You already have all available stock ({product.Stock}) in your cart.",
                    "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show qty dialog
            int qty = ShowQtyDialog(product.Name, product.Price, available);
            if (qty <= 0) return;

            // Check if already in cart — update qty
            bool found = false;
            foreach (var ci in _cartItems)
            {
                if (ci.Product.Id == product.Id)
                {
                    ci.Qty += qty;
                    found = true;
                    break;
                }
            }
            if (!found)
                _cartItems.Add(new CartItem { Product = product, Qty = qty });

            UpdateCartBadge();
            MessageBox.Show($"✓  {qty}× {product.Name} added to cart!", "Added",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int ShowQtyDialog(string productName, decimal unitPrice, int maxQty)
        {
            // Build a small inline dialog
            var dlg = new Form
            {
                Text            = "Select Quantity",
                Size            = new Size(320, 200),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                BackColor       = Color.White
            };

            dlg.Controls.Add(new Label
            {
                Text      = productName,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location  = new Point(20, 16),
                Size      = new Size(270, 22)
            });
            dlg.Controls.Add(new Label
            {
                Text      = $"${unitPrice:F2} each  |  Max available: {maxQty}",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location  = new Point(20, 40),
                Size      = new Size(270, 18)
            });
            dlg.Controls.Add(new Label
            {
                Text      = "Quantity:",
                Font      = new Font("Segoe UI", 10),
                Location  = new Point(20, 72),
                AutoSize  = true
            });

            var numQty = new NumericUpDown
            {
                Location = new Point(100, 68),
                Size     = new Size(80, 26),
                Minimum  = 1,
                Maximum  = maxQty,
                Value    = 1,
                Font     = new Font("Segoe UI", 11)
            };
            dlg.Controls.Add(numQty);

            var btnOk = new Button
            {
                Text      = "Add to Cart",
                Location  = new Point(20, 118),
                Size      = new Size(130, 36),
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text         = "Cancel",
                Location     = new Point(162, 118),
                Size         = new Size(100, 36),
                BackColor    = Color.FromArgb(150, 150, 150),
                ForeColor    = Color.White,
                FlatStyle    = FlatStyle.Flat,
                Font         = new Font("Segoe UI", 10),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            dlg.Controls.Add(btnOk);
            dlg.Controls.Add(btnCancel);
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            return dlg.ShowDialog(this) == DialogResult.OK ? (int)numQty.Value : 0;
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 4: Cart section (DataGridView shows items instantly, checkout writes JSON)
        // ═══════════════════════════════════════════════════════════════════════

        private void BuildCartSection()
        {
            _secCart = Section();

            // Title
            _secCart.Controls.Add(SectionTitle("Shopping Cart"));

            // Cart grid — shows product name, qty, unit price, subtotal
            _dgvCart = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                Name                  = "dgvCart",
                BackgroundColor       = Surface,
                BorderStyle           = BorderStyle.None,
                ReadOnly              = true,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                GridColor             = Color.FromArgb(230, 230, 230),
                Font                  = new Font("Segoe UI", 10),
                ColumnHeadersHeight   = 34,
                AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill
            };
            _dgvCart.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(245, 248, 246);
            _dgvCart.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 10, FontStyle.Bold);
            _dgvCart.ColumnHeadersDefaultCellStyle.ForeColor  = Color.FromArgb(60, 60, 60);
            _dgvCart.EnableHeadersVisualStyles = false;
            _dgvCart.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 253, 251);

            // Define columns manually so we control what shows
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName",     HeaderText = "Product",    FillWeight = 40 });
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty",      HeaderText = "Qty",        FillWeight = 10 });
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colUnit",     HeaderText = "Unit Price", FillWeight = 20 });
            _dgvCart.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSubtotal", HeaderText = "Subtotal",   FillWeight = 20 });

            // Bottom bar: total | remove | checkout
            var bottomBar = new TableLayoutPanel
            {
                Dock        = DockStyle.Bottom,
                Height      = 54,
                ColumnCount = 3,
                RowCount    = 1,
                BackColor   = Color.Transparent,
                Padding     = new Padding(0, 10, 0, 0)
            };
            bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));

            _lblCartTotal = new Label
            {
                Text      = "Total: $0.00",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Green,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnRemove = new Button
            {
                Text      = "✕  Remove Selected",
                Dock      = DockStyle.Fill,
                BackColor = Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 8, 0)
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) => RemoveFromCart();

            var btnCheckout = new Button
            {
                Text      = "Checkout →",
                Dock      = DockStyle.Fill,
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += (s, e) => Checkout();

            bottomBar.Controls.Add(_lblCartTotal, 0, 0);
            bottomBar.Controls.Add(btnRemove,     1, 0);
            bottomBar.Controls.Add(btnCheckout,   2, 0);

            // Add in correct dock order: Bottom first, then Fill, then Top
            _secCart.Controls.Add(bottomBar);
            _secCart.Controls.Add(_dgvCart);
            _content.Controls.Add(_secCart);
        }

        // ── Cart operations ───────────────────────────────────────────────────

        private void RefreshCartGrid()
        {
            _dgvCart.Rows.Clear();
            decimal total = 0;
            foreach (var ci in _cartItems)
            {
                _dgvCart.Rows.Add(
                    ci.Product.Name,
                    ci.Qty,
                    $"${ci.Product.Price:F2}",
                    $"${ci.Subtotal:F2}");
                total += ci.Subtotal;
            }
            _lblCartTotal.Text = $"Total: ${total:F2}";
        }

        private void RemoveFromCart()
        {
            if (_dgvCart.SelectedRows.Count == 0) return;
            int idx = _dgvCart.SelectedRows[0].Index;
            if (idx >= 0 && idx < _cartItems.Count)
            {
                _cartItems.RemoveAt(idx);
                RefreshCartGrid();
                UpdateCartBadge();
            }
        }

        private void UpdateCartBadge()
        {
            int total = 0;
            foreach (var ci in _cartItems) total += ci.Qty;
            _lblCartBadge.Text = $"🛒  {total} item{(total == 1 ? "" : "s")}";
        }

        // ── Checkout — writes orders.json AND reduces stock in products.json ──
        private void Checkout()
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Your cart is empty.", "Cart Empty",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = _authService.GetCurrentUser();
            var order = new Order(user.Id, user.FullName, user.Address);

            foreach (var ci in _cartItems)
            {
                order.AddItem(new OrderItem(ci.Product.Id, ci.Product.Name, ci.Qty, ci.Product.Price));

                // Deduct stock and write back to products.json
                var liveProduct = _dataService.GetProductById(ci.Product.Id);
                if (liveProduct != null)
                {
                    liveProduct.Stock = Math.Max(0, liveProduct.Stock - ci.Qty);
                    _dataService.UpdateProduct(liveProduct);
                }
            }

            // Write order to orders.json
            _dataService.AddOrder(order);

            // Update customer stats and write to users.json
            if (_currentCustomer != null)
            {
                _currentCustomer.TotalOrders++;
                _currentCustomer.TotalSpent += order.TotalAmount;
                _currentCustomer.UpdateLoyaltyTier();
                _dataService.UpdateUser(_currentCustomer);
            }

            // Update analytics.json
            _dataService.UpdateCustomerAnalytics(
                user.Id, user.FullName, order.TotalAmount,
                order.GetTotalItemCount(),
                _cartItems.Count > 0 ? _cartItems[0].Product.Category : "");

            _cartItems.Clear();
            RefreshCartGrid();
            UpdateCartBadge();

            MessageBox.Show(
                $"Order placed!\n" +
                $"Order ID: {order.Id.Substring(0, 8).ToUpper()}\n" +
                $"Total: ${order.TotalAmount:F2}\n\n" +
                $"Data saved to:\n{System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")}",
                "Order Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // Part 5: Orders section, Profile section, ShowSection, UI factories
        // ═══════════════════════════════════════════════════════════════════════

        private void BuildOrdersSection()
        {
            _secOrders = Section();
            _secOrders.Controls.Add(SectionTitle("My Orders"));
            var dgv = MakeGrid("dgvMyOrders");
            var user = _authService.GetCurrentUser();
            dgv.DataSource = _dataService.GetOrdersByCustomerId(user?.Id ?? "");
            _secOrders.Controls.Add(dgv);
            _content.Controls.Add(_secOrders);
        }

        private void BuildProfileSection()
        {
            _secProfile = Section();
            _secProfile.Controls.Add(SectionTitle("My Profile"));

            var user     = _authService.GetCurrentUser();
            var customer = user as Customer;

            var card = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 260,
                BackColor = Surface,
                Padding   = new Padding(28, 20, 28, 20)
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 220, 220)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var brush = new SolidBrush(Green))
                    e.Graphics.FillRectangle(brush, 0, card.Height - 4, card.Width, 4);
            };

            var info = new Label
            {
                Text =
                    $"Username:          {user?.Username}\n\n" +
                    $"Email:                 {user?.Email}\n\n" +
                    $"Full Name:          {user?.FullName}\n\n" +
                    $"Phone:               {user?.Phone}\n\n" +
                    $"Address:            {user?.Address}\n\n" +
                    $"Loyalty Tier:       {customer?.LoyaltyTier ?? "N/A"}     " +
                    $"Total Spent: ${customer?.TotalSpent ?? 0:N2}     " +
                    $"Orders: {customer?.TotalOrders ?? 0}",
                Font      = new Font("Segoe UI", 10.5f),
                ForeColor = Color.FromArgb(50, 50, 50),
                Dock      = DockStyle.Fill,
                AutoSize  = false
            };

            var btnEdit = new Button
            {
                Text      = "✏  Edit Profile",
                Dock      = DockStyle.Bottom,
                Height    = 38,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) =>
            {
                new ProfileManagementForm().ShowDialog();
                // Refresh profile display after edit
                RefreshProfileSection();
            };

            card.Controls.Add(info);
            card.Controls.Add(btnEdit);
            _secProfile.Controls.Add(card);
            _content.Controls.Add(_secProfile);
        }

        private void RefreshProfileSection()
        {
            // Rebuild profile section with fresh data
            _content.Controls.Remove(_secProfile);
            BuildProfileSection();
            if (_secProfile != null) _secProfile.Visible = true;
        }

        // ── Section visibility ────────────────────────────────────────────────

        private void ShowSection(string name)
        {
            _secBrowse.Visible  = false;
            _secCart.Visible    = false;
            _secOrders.Visible  = false;
            _secProfile.Visible = false;

            switch (name)
            {
                case "browse":
                    _secBrowse.Visible = true;
                    break;
                case "cart":
                    _secCart.Visible = true;
                    RefreshCartGrid();   // always refresh when switching to cart
                    break;
                case "orders":
                    _secOrders.Visible = true;
                    RefreshOrders();
                    break;
                case "profile":
                    _secProfile.Visible = true;
                    break;
            }

            // Highlight active nav button
            foreach (var b in new[] { _btnBrowse, _btnCart, _btnOrders, _btnProfile })
                b.BackColor = Green;

            Button active = null;
            switch (name)
            {
                case "browse":  active = _btnBrowse;  break;
                case "cart":    active = _btnCart;    break;
                case "orders":  active = _btnOrders;  break;
                case "profile": active = _btnProfile; break;
            }
            if (active != null) active.BackColor = GreenDark;
        }

        private void RefreshOrders()
        {
            var user = _authService.GetCurrentUser();
            foreach (Control c in _secOrders.Controls)
            {
                if (c is DataGridView dgv && dgv.Name == "dgvMyOrders")
                {
                    dgv.DataSource = _dataService.GetOrdersByCustomerId(user?.Id ?? "");
                    break;
                }
            }
        }

        // ── UI factories ──────────────────────────────────────────────────────

        private Button NavBtn(string text, Color? bg = null)
        {
            var b = new Button
            {
                Text      = text,
                Width     = 194,
                Height    = 42,
                BackColor = bg ?? Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(14, 0, 0, 0),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 2, 0, 2)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = GreenDark;
            return b;
        }

        private static Panel Section() => new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.Transparent,
            Visible   = false
        };

        private static Label SectionTitle(string text) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(40, 40, 40),
            Dock      = DockStyle.Top,
            Height    = 42,
            TextAlign = ContentAlignment.MiddleLeft
        };

        private static DataGridView MakeGrid(string name)
        {
            var dgv = new DataGridView
            {
                Name                  = name,
                Dock                  = DockStyle.Fill,
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
            dgv.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(245, 248, 246);
            dgv.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor  = Color.FromArgb(60, 60, 60);
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 253, 251);
            return dgv;
        }
    }
}
