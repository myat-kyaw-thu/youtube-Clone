using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Forms
{
    /// <summary>
    /// Order Tracking — fluid split: grid top, detail card bottom.
    /// </summary>
    public class OrderTrackingForm : Form
    {
        private readonly DataService  _dataService;
        private readonly AuthService  _authService;

        private DataGridView _dgv;
        private Panel        _detailCard;
        private Label        _lblDetail;

        private static readonly Color Green   = Color.FromArgb(34, 139, 34);
        private static readonly Color BgPage  = Color.FromArgb(245, 248, 246);
        private static readonly Color Surface = Color.White;

        public OrderTrackingForm()
        {
            _dataService = new DataService();
            _authService = new AuthService();
            InitializeComponent();
            LoadOrders();
        }

        private void InitializeComponent()
        {
            this.Text = "Order Tracking — GreenLife";
            this.MinimumSize = new Size(760, 560);
            this.Size = new Size(960, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgPage;

            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = BgPage,
                Padding = new Padding(24, 20, 24, 20)
            };
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));   // title
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 60));    // grid
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 40));    // detail

            // Title
            var titlePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            titlePanel.Controls.Add(new Label
            {
                Text = "Track Your Orders",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 40, 40),
                Dock = DockStyle.Left,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            });
            titlePanel.Controls.Add(new Label
            {
                Text = "Select a row to view full details",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.Gray,
                Dock = DockStyle.Bottom,
                Height = 18,
                TextAlign = ContentAlignment.BottomLeft
            });

            // Grid card
            var gridCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Surface,
                Margin = new Padding(0, 0, 0, 12)
            };
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
            _dgv.SelectionChanged += DgvSelectionChanged;
            gridCard.Controls.Add(_dgv);

            // Detail card
            _detailCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Surface,
                Padding = new Padding(20, 14, 20, 14)
            };
            _detailCard.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(215, 215, 215)))
                    e.Graphics.DrawRectangle(pen, 0, 0, _detailCard.Width - 1, _detailCard.Height - 1);
                using (var brush = new SolidBrush(Green))
                    e.Graphics.FillRectangle(brush, 0, 0, 4, _detailCard.Height);
            };

            _lblDetail = new Label
            {
                Text = "Select an order above to view its details.",
                Font = new Font("Segoe UI", 10.5f),
                ForeColor = Color.FromArgb(80, 80, 80),
                Dock = DockStyle.Fill,
                AutoSize = false
            };
            _detailCard.Controls.Add(_lblDetail);

            outer.Controls.Add(titlePanel, 0, 0);
            outer.Controls.Add(gridCard,   0, 1);
            outer.Controls.Add(_detailCard, 0, 2);
            this.Controls.Add(outer);
        }

        private void LoadOrders()
        {
            var user = _authService.GetCurrentUser();
            if (user != null)
                _dgv.DataSource = _dataService.GetOrdersByCustomerId(user.Id);
        }

        private void DgvSelectionChanged(object sender, EventArgs e)
        {
            if (_dgv.SelectedRows.Count == 0) return;
            var order = _dgv.SelectedRows[0].DataBoundItem as Order;
            if (order != null) ShowDetail(order);
        }

        private void ShowDetail(Order order)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Order ID:   {order.Id.Substring(0, 8).ToUpper()}");
            sb.AppendLine($"Date:         {order.OrderDate:yyyy-MM-dd  HH:mm}");
            sb.AppendLine($"Status:       {order.Status}");
            sb.AppendLine($"Total:         ${order.TotalAmount:F2}");
            sb.AppendLine($"Ship To:      {order.ShippingAddress}");
            sb.AppendLine();
            sb.AppendLine("Items:");
            foreach (var item in order.Items)
                sb.AppendLine($"   • {item.ProductName}  ×{item.Quantity}  =  ${item.Subtotal:F2}");
            if (!string.IsNullOrEmpty(order.Notes))
            { sb.AppendLine(); sb.AppendLine($"Notes: {order.Notes}"); }

            _lblDetail.Text = sb.ToString();
        }
    }
}
