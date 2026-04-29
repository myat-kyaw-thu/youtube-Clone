using System;
using System.Drawing;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Forms
{
    /// <summary>
    /// Reports Form — fluid layout: controls bar top, summary card, data grid fills remainder.
    /// </summary>
    public class ReportsForm : Form
    {
        private readonly ReportService _reportService;
        private readonly DataService   _dataService;

        private ComboBox       _cmbType;
        private DateTimePicker _dtpFrom, _dtpTo;
        private Button         _btnGenerate;
        private TextBox        _txtSummary;
        private DataGridView   _dgv;

        private static readonly Color Green   = Color.FromArgb(34, 139, 34);
        private static readonly Color BgPage  = Color.FromArgb(245, 248, 246);
        private static readonly Color Surface = Color.White;

        public ReportsForm()
        {
            _reportService = new ReportService();
            _dataService   = new DataService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Reports — GreenLife";
            this.MinimumSize = new Size(800, 580);
            this.Size = new Size(1020, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgPage;

            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = BgPage,
                Padding = new Padding(24, 20, 24, 20)
            };
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // title
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // controls bar
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));  // summary card
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // data grid

            // ── Title ─────────────────────────────────────────────────────────
            outer.Controls.Add(new Label
            {
                Text = "Generate Reports",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            // ── Controls bar ──────────────────────────────────────────────────
            var bar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                BackColor = Surface,
                Padding = new Padding(12, 8, 12, 8),
                Margin = new Padding(0, 0, 0, 10)
            };
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160)); // combo
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));  // "From"
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // from picker
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));  // "To"
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140)); // to picker
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // generate btn
            bar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, bar.Width - 1, bar.Height - 1);
            };

            _cmbType = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 0, 8, 0)
            };
            _cmbType.Items.AddRange(new[] { "Sales Report", "Stock Report", "Customer Report", "Order Report" });
            _cmbType.SelectedIndex = 0;

            _dtpFrom = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 8, 0) };
            _dtpFrom.Value = DateTime.Now.AddDays(-30);
            _dtpTo   = new DateTimePicker { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10), Margin = new Padding(0, 0, 8, 0) };

            _btnGenerate = new Button
            {
                Text = "Generate →",
                Dock = DockStyle.Right,
                Width = 130,
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnGenerate.FlatAppearance.BorderSize = 0;
            _btnGenerate.Click += BtnGenerate_Click;

            bar.Controls.Add(_cmbType,   0, 0);
            bar.Controls.Add(BarLabel("From"), 1, 0);
            bar.Controls.Add(_dtpFrom,   2, 0);
            bar.Controls.Add(BarLabel("To"),   3, 0);
            bar.Controls.Add(_dtpTo,     4, 0);
            bar.Controls.Add(_btnGenerate, 5, 0);
            outer.Controls.Add(bar, 0, 1);

            // ── Summary card ──────────────────────────────────────────────────
            var summaryCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Surface,
                Padding = new Padding(14, 10, 14, 10),
                Margin = new Padding(0, 0, 0, 10)
            };
            summaryCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, summaryCard.Width - 1, summaryCard.Height - 1);
                using (var brush = new SolidBrush(Green))
                    e.Graphics.FillRectangle(brush, 0, 0, 4, summaryCard.Height);
            };

            _txtSummary = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Consolas", 9.5f),
                BackColor = Surface,
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                Text = "Select a report type and click Generate."
            };
            summaryCard.Controls.Add(_txtSummary);
            outer.Controls.Add(summaryCard, 0, 2);

            // ── Data grid ─────────────────────────────────────────────────────
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = Surface };
            gridCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, gridCard.Width - 1, gridCard.Height - 1);
            };

            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Surface,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(230, 230, 230),
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 34
            };
            _dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 246);
            _dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 60);
            _dgv.EnableHeadersVisualStyles = false;
            _dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 253, 251);
            gridCard.Controls.Add(_dgv);
            outer.Controls.Add(gridCard, 0, 3);

            this.Controls.Add(outer);
        }

        private static Label BarLabel(string text) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = Color.FromArgb(80, 80, 80),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Margin = new Padding(0, 0, 4, 0)
        };

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                switch (_cmbType.SelectedItem?.ToString())
                {
                    case "Sales Report":    GenSales();    break;
                    case "Stock Report":    GenStock();    break;
                    case "Customer Report": GenCustomer(); break;
                    case "Order Report":    GenOrders();   break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Report Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenSales()
        {
            var r = _reportService.GenerateSalesReport(_dtpFrom.Value, _dtpTo.Value);
            var statusLines = new System.Text.StringBuilder();
            foreach (var k in r.OrdersByStatus.Keys)
                statusLines.AppendLine($"  {k}: {r.OrdersByStatus[k]}");
            _txtSummary.Text =
                $"SALES REPORT  |  {_dtpFrom.Value:yyyy-MM-dd} → {_dtpTo.Value:yyyy-MM-dd}\r\n" +
                $"──────────────────────────────────────────────────\r\n" +
                $"Total Orders:          {r.TotalOrders}\r\n" +
                $"Total Revenue:        ${r.TotalRevenue:N2}\r\n" +
                $"Avg Order Value:    ${r.AverageOrderValue:N2}\r\n\r\n" +
                "Orders by Status:\r\n" + statusLines.ToString();
            _dgv.DataSource = r.Orders;
        }

        private void GenStock()
        {
            var r = _reportService.GenerateStockReport();
            var catLines = new System.Text.StringBuilder();
            foreach (var k in r.StockByCategory.Keys)
                catLines.AppendLine($"  {k}: {r.StockByCategory[k]} units");
            _txtSummary.Text =
                $"STOCK REPORT\r\n──────────────────────────────────────────────────\r\n" +
                $"Total Products:       {r.TotalProducts}\r\n" +
                $"Total Stock Value:  ${r.TotalStockValue:N2}\r\n" +
                $"Low Stock Items:    {r.LowStockProducts.Count}\r\n" +
                $"Out of Stock:          {r.OutOfStockProducts.Count}\r\n" +
                $"Near Expiry (30d):  {r.NearExpiryProducts.Count}\r\n\r\n" +
                "Stock by Category:\r\n" + catLines.ToString();
            _dgv.DataSource = r.LowStockProducts;
        }

        private void GenCustomer()
        {
            var r = _reportService.GenerateCustomerReport();
            var tierLines = new System.Text.StringBuilder();
            foreach (var k in r.CustomersByTier.Keys)
                tierLines.AppendLine($"  {k}: {r.CustomersByTier[k]}");
            _txtSummary.Text =
                $"CUSTOMER REPORT\r\n──────────────────────────────────────────────────\r\n" +
                $"Total Customers:     {r.TotalCustomers}\r\n" +
                $"Active:                    {r.ActiveCustomers}\r\n" +
                $"Total Revenue:        ${r.TotalRevenue:N2}\r\n" +
                $"Avg Customer Value: ${r.AverageCustomerValue:N2}\r\n\r\n" +
                "By Loyalty Tier:\r\n" + tierLines.ToString();
            _dgv.DataSource = r.TopCustomers;
        }

        private void GenOrders()
        {
            var r = _reportService.GenerateOrderReport();
            _txtSummary.Text =
                $"ORDER REPORT\r\n{"─",50}\r\n" +
                $"Total Orders:    {r.TotalOrders}\r\n" +
                $"Pending:           {r.PendingOrders}\r\n" +
                $"Shipped:           {r.ShippedOrders}\r\n" +
                $"Delivered:         {r.DeliveredOrders}\r\n" +
                $"Cancelled:        {r.CancelledOrders}";
            _dgv.DataSource = r.RecentOrders;
        }
    }
}
