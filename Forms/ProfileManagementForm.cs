using System;
using System.Drawing;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Forms
{
    /// <summary>
    /// Profile Management — clean two-column layout.
    /// Left column: read-only account info + editable details.
    /// Right column: change password.
    /// No dock tricks — explicit TableLayoutPanel rows throughout.
    /// </summary>
    public class ProfileManagementForm : Form
    {
        private readonly AuthService _authService;
        private readonly DataService _dataService;

        // Read-only labels
        private Label _valId, _valUsername, _valType, _valReg,
                      _valLogin, _valTier, _valSpent, _valOrders;

        // Editable fields
        private TextBox        _txtEmail, _txtFullName, _txtPhone,
                               _txtAddress, _txtPrefCat;
        private DateTimePicker _dtpDob;

        // Password fields
        private TextBox _txtCurPw, _txtNewPw, _txtConfPw;

        // Status + buttons
        private Label  _lblStatus;
        private Button _btnSave, _btnCancel;

        // Colours
        private static readonly Color Green    = Color.FromArgb(34, 139, 34);
        private static readonly Color GreenDark = Color.FromArgb(24, 110, 24);
        private static readonly Color BgPage   = Color.FromArgb(236, 245, 238);
        private static readonly Color Surface  = Color.White;
        private static readonly Color ReadBg   = Color.FromArgb(246, 249, 247);
        private static readonly Color Border   = Color.FromArgb(210, 210, 210);

        public ProfileManagementForm()
        {
            _authService = new AuthService();
            _dataService = new DataService();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text            = "My Profile — GreenLife";
            this.Size            = new Size(860, 680);
            this.MinimumSize     = new Size(760, 600);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox     = false;
            this.BackColor       = BgPage;

            // ── Master layout: header | body | footer ─────────────────────────
            var master = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                RowCount    = 3,
                ColumnCount = 1,
                BackColor   = BgPage,
                Padding     = new Padding(0)
            };
            master.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // header
            master.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // body
            master.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));   // footer buttons
            this.Controls.Add(master);

            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Fill, BackColor = Green };
            header.Controls.Add(new Label
            {
                Text      = "My Profile",
                Font      = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });
            master.Controls.Add(header, 0, 0);

            // ── Body: left column | right column ──────────────────────────────
            var body = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = BgPage,
                Padding     = new Padding(20, 16, 20, 8)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            master.Controls.Add(body, 0, 1);

            // ── LEFT: Account info card + Edit details card ───────────────────
            var leftScroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = BgPage,
                Padding    = new Padding(0, 0, 10, 0)
            };

            var leftStack = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                AutoSize    = true,
                ColumnCount = 1,
                BackColor   = BgPage
            };

            // Account info card
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftStack.RowCount = 1;
            leftStack.Controls.Add(BuildAccountInfoCard(), 0, 0);

            // Spacer
            leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 12));
            leftStack.RowCount++;
            leftStack.Controls.Add(new Panel { Height = 12, BackColor = BgPage }, 0, 1);

            // Edit details card
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftStack.RowCount++;
            leftStack.Controls.Add(BuildEditDetailsCard(), 0, 2);

            leftScroll.Controls.Add(leftStack);
            body.Controls.Add(leftScroll, 0, 0);

            // ── RIGHT: Change password card ───────────────────────────────────
            var rightScroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = BgPage,
                Padding    = new Padding(10, 0, 0, 0)
            };
            rightScroll.Controls.Add(BuildPasswordCard());
            body.Controls.Add(rightScroll, 1, 0);

            // ── Footer: status label + Save + Cancel ──────────────────────────
            var footer = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 1,
                BackColor   = Surface,
                Padding     = new Padding(20, 8, 20, 8)
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            footer.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                    e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
            };

            _lblStatus = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _btnSave = new Button
            {
                Text      = "Save Changes",
                Dock      = DockStyle.Fill,
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 8, 0)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text      = "Cancel",
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(140, 140, 140),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => this.Close();

            footer.Controls.Add(_lblStatus, 0, 0);
            footer.Controls.Add(_btnSave,   1, 0);
            footer.Controls.Add(_btnCancel, 2, 0);
            master.Controls.Add(footer, 0, 2);

            this.AcceptButton = _btnSave;
        }

        // ── Account info card (read-only grid) ────────────────────────────────

        private Panel BuildAccountInfoCard()
        {
            var card = Card("Account Information");

            var grid = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                AutoSize    = true,
                ColumnCount = 4,
                BackColor   = Surface,
                Padding     = new Padding(12, 6, 12, 12)
            };
            // 4 columns: label | value | label | value
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Row 0
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            grid.Controls.Add(RoLabel("User ID:"),       0, 0);
            _valId       = RoValue(); grid.Controls.Add(_valId,       1, 0);
            grid.Controls.Add(RoLabel("Username:"),      2, 0);
            _valUsername = RoValue(); grid.Controls.Add(_valUsername, 3, 0);
            // Row 1
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            grid.Controls.Add(RoLabel("Account Type:"),  0, 1);
            _valType     = RoValue(); grid.Controls.Add(_valType,     1, 1);
            grid.Controls.Add(RoLabel("Registered:"),    2, 1);
            _valReg      = RoValue(); grid.Controls.Add(_valReg,      3, 1);
            // Row 2
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            grid.Controls.Add(RoLabel("Last Login:"),    0, 2);
            _valLogin    = RoValue(); grid.Controls.Add(_valLogin,    1, 2);
            grid.Controls.Add(RoLabel("Loyalty Tier:"),  2, 2);
            _valTier     = RoValue(); grid.Controls.Add(_valTier,     3, 2);
            // Row 3
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            grid.Controls.Add(RoLabel("Total Spent:"),   0, 3);
            _valSpent    = RoValue(); grid.Controls.Add(_valSpent,    1, 3);
            grid.Controls.Add(RoLabel("Total Orders:"),  2, 3);
            _valOrders   = RoValue(); grid.Controls.Add(_valOrders,   3, 3);

            grid.RowCount = 4;
            card.Controls.Add(grid);
            return card;
        }

        // ── Edit details card ─────────────────────────────────────────────────

        private Panel BuildEditDetailsCard()
        {
            var card = Card("Edit Your Details");

            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                AutoSize    = true,
                ColumnCount = 2,
                BackColor   = Surface,
                Padding     = new Padding(12, 6, 12, 12)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Row 0: Email | Full Name
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.Controls.Add(FieldGroup("Email",     out _txtEmail),    0, 0);
            layout.Controls.Add(FieldGroup("Full Name", out _txtFullName), 1, 0);
            // Row 1: Phone | Address
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.Controls.Add(FieldGroup("Phone",   out _txtPhone),   0, 1);
            layout.Controls.Add(FieldGroup("Address", out _txtAddress), 1, 1);
            // Row 2: Preferred Category | Date of Birth
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.Controls.Add(FieldGroup("Preferred Category", out _txtPrefCat), 0, 2);
            layout.Controls.Add(DobGroup(), 1, 2);

            layout.RowCount = 3;
            card.Controls.Add(layout);
            return card;
        }

        // ── Password card ─────────────────────────────────────────────────────

        private Panel BuildPasswordCard()
        {
            var card = Card("Change Password");
            card.Dock = DockStyle.Top;

            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                AutoSize    = true,
                ColumnCount = 1,
                BackColor   = Surface,
                Padding     = new Padding(12, 6, 12, 12)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.Controls.Add(FieldGroup("Current Password", out _txtCurPw, pw: true), 0, 0);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.Controls.Add(FieldGroup("New Password",     out _txtNewPw, pw: true), 0, 1);
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
            layout.Controls.Add(FieldGroup("Confirm Password", out _txtConfPw, pw: true), 0, 2);

            var hint = new Label
            {
                Text      = "Leave all three blank to keep your current password.",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(130, 130, 130),
                Dock      = DockStyle.Top,
                Height    = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(0, 4, 0, 0)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.Controls.Add(hint, 0, 3);
            layout.RowCount = 4;

            card.Controls.Add(layout);
            return card;
        }

        // ── UI helpers ────────────────────────────────────────────────────────

        /// <summary>Creates a white card panel with a green left accent bar and a bold section title.</summary>
        private static Panel Card(string title)
        {
            var card = new Panel
            {
                Dock      = DockStyle.Top,
                AutoSize  = true,
                BackColor = Surface,
                Padding   = new Padding(0),
                Margin    = new Padding(0, 0, 0, 0)
            };
            card.Paint += (s, e) =>
            {
                var p = s as Panel;
                using (var pen = new Pen(Border))
                    e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
                using (var b = new SolidBrush(Color.FromArgb(34, 139, 34)))
                    e.Graphics.FillRectangle(b, 0, 0, 4, p.Height);
            };

            var titleLbl = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 139, 34),
                Dock      = DockStyle.Top,
                Height    = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(14, 0, 0, 0),
                BackColor = Color.FromArgb(246, 249, 247)
            };
            titleLbl.Paint += (s, e) =>
            {
                using (var pen = new Pen(Border))
                    e.Graphics.DrawLine(pen, 0, titleLbl.Height - 1, titleLbl.Width, titleLbl.Height - 1);
            };
            card.Controls.Add(titleLbl);
            return card;
        }

        private static Label RoLabel(string text) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(110, 110, 110),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Padding   = new Padding(0, 0, 8, 0)
        };

        private static Label RoValue() => new Label
        {
            Text      = "—",
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 30, 30),
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = ReadBg,
            Padding   = new Padding(6, 0, 0, 0)
        };

        private static Panel FieldGroup(string label, out TextBox box, bool pw = false)
        {
            var p = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Surface,
                Padding   = new Padding(4, 2, 4, 2)
            };
            p.Controls.Add(new Label
            {
                Text      = label,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(90, 90, 90),
                Dock      = DockStyle.Top,
                Height    = 18
            });
            var txt = new TextBox
            {
                Dock                  = DockStyle.Top,
                Font                  = new Font("Segoe UI", 10.5f),
                BorderStyle           = BorderStyle.FixedSingle,
                UseSystemPasswordChar = pw
            };
            p.Controls.Add(txt);
            box = txt;
            return p;
        }

        private Panel DobGroup()
        {
            var p = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Surface,
                Padding   = new Padding(4, 2, 4, 2)
            };
            p.Controls.Add(new Label
            {
                Text      = "Date of Birth",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(90, 90, 90),
                Dock      = DockStyle.Top,
                Height    = 18
            });
            _dtpDob = new DateTimePicker
            {
                Dock   = DockStyle.Top,
                Font   = new Font("Segoe UI", 10.5f),
                Format = DateTimePickerFormat.Short
            };
            p.Controls.Add(_dtpDob);
            return p;
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadData()
        {
            var user     = _authService.GetCurrentUser();
            if (user == null) return;
            var customer = user as Customer;

            // Read-only
            _valId.Text     = user.Id;
            _valUsername.Text = user.Username;
            _valType.Text   = user.UserType;
            _valReg.Text    = user.RegistrationDate.ToString("yyyy-MM-dd");
            _valLogin.Text  = user.LastLogin.ToString("yyyy-MM-dd  HH:mm");
            _valTier.Text   = customer?.LoyaltyTier ?? "N/A";
            _valSpent.Text  = $"${customer?.TotalSpent ?? 0:N2}";
            _valOrders.Text = (customer?.TotalOrders ?? 0).ToString();

            // Editable
            _txtEmail.Text    = user.Email    ?? "";
            _txtFullName.Text = user.FullName ?? "";
            _txtPhone.Text    = user.Phone    ?? "";
            _txtAddress.Text  = user.Address  ?? "";
            _txtPrefCat.Text  = customer?.PreferredCategory ?? "";
            _dtpDob.Value     = (customer != null && customer.DateOfBirth > DateTime.MinValue)
                                ? customer.DateOfBirth
                                : new DateTime(1990, 1, 1);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                _lblStatus.Text      = "";
                _lblStatus.ForeColor = Color.FromArgb(100, 100, 100);

                var user = _authService.GetCurrentUser();
                if (user == null) return;

                user.Email    = _txtEmail.Text.Trim();
                user.FullName = _txtFullName.Text.Trim();
                user.Phone    = _txtPhone.Text.Trim();
                user.Address  = _txtAddress.Text.Trim();

                var customer = user as Customer;
                if (customer != null)
                {
                    customer.PreferredCategory = _txtPrefCat.Text.Trim();
                    customer.DateOfBirth       = _dtpDob.Value;
                }

                // Password change (only if current password field is filled)
                if (!string.IsNullOrEmpty(_txtCurPw.Text))
                {
                    if (string.IsNullOrEmpty(_txtNewPw.Text))
                    { _lblStatus.Text = "Enter a new password."; _lblStatus.ForeColor = Color.FromArgb(192, 57, 43); return; }
                    if (_txtNewPw.Text != _txtConfPw.Text)
                    { _lblStatus.Text = "New passwords do not match."; _lblStatus.ForeColor = Color.FromArgb(192, 57, 43); return; }
                    if (!_authService.ChangePassword(user.Id, _txtCurPw.Text, _txtNewPw.Text))
                    { _lblStatus.Text = "Current password is incorrect."; _lblStatus.ForeColor = Color.FromArgb(192, 57, 43); return; }
                }

                _dataService.UpdateUser(user);
                _lblStatus.Text      = "✔  Profile saved successfully.";
                _lblStatus.ForeColor = Color.FromArgb(34, 139, 34);

                // Clear password fields after save
                _txtCurPw.Clear(); _txtNewPw.Clear(); _txtConfPw.Clear();
            }
            catch (Exception ex)
            {
                _lblStatus.Text      = ex.Message;
                _lblStatus.ForeColor = Color.FromArgb(192, 57, 43);
            }
        }
    }
}
