using System;
using System.Drawing;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;

namespace GreenLifeOrganicStore.Forms
{
    /// <summary>
    /// Login form — centered card, explicit layout so nothing overlaps or overflows.
    /// </summary>
    public class LoginForm : Form
    {
        private readonly AuthService _authService;

        private TextBox  _txtUsername;
        private TextBox  _txtPassword;
        private CheckBox _chkShowPassword;
        private Label    _lblError;
        private Button   _btnLogin;
        private Button   _btnRegister;

        // Card dimensions
        private const int CardW  = 420;
        private const int CardH  = 530;
        private const int Pad    = 36;   // left/right padding inside card
        private const int FieldW = CardW - Pad * 2;  // 348

        private static readonly Color Green      = Color.FromArgb(34, 139, 34);
        private static readonly Color GreenLight = Color.FromArgb(60, 179, 113);
        private static readonly Color BgPage     = Color.FromArgb(236, 245, 238);

        public LoginForm()
        {
            _authService = new AuthService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "GreenLife Organic Store";
            this.MinimumSize     = new Size(500, 620);
            this.Size            = new Size(540, 660);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox     = false;
            this.BackColor       = BgPage;
            this.Resize         += (s, e) => PositionCard();

            // ── Card (white rounded-feel panel) ───────────────────────────────
            var card = new Panel
            {
                Size      = new Size(CardW, CardH),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                // thin border
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                // bottom accent
                using (var b = new SolidBrush(Green))
                    e.Graphics.FillRectangle(b, 0, card.Height - 4, card.Width, 4);
            };
            this.Controls.Add(card);

            // ── Green header (top 130 px of card) ────────────────────────────
            var header = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(CardW, 130),
                BackColor = Green
            };
            var lblTitle = new Label
            {
                Text      = "GreenLife",
                Font      = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true
            };
            var lblSub = new Label
            {
                Text      = "Organic Store",
                Font      = new Font("Segoe UI", 14),
                ForeColor = Color.FromArgb(180, 230, 180),
                AutoSize  = true
            };
            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSub);
            header.Resize += (s, e) =>
            {
                lblTitle.Location = new Point((header.Width - lblTitle.Width) / 2, 24);
                lblSub.Location   = new Point((header.Width - lblSub.Width)   / 2, 68);
            };
            card.Controls.Add(header);

            // ── Fields — placed below header (y starts at 150) ───────────────
            int y = 150;

            // Username label
            card.Controls.Add(new Label
            {
                Text      = "Username",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(70, 70, 70),
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 18)
            });
            y += 20;

            _txtUsername = new TextBox
            {
                Location    = new Point(Pad, y),
                Size        = new Size(FieldW, 28),
                Font        = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            card.Controls.Add(_txtUsername);
            y += 36;

            // Password label
            card.Controls.Add(new Label
            {
                Text      = "Password",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(70, 70, 70),
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 18)
            });
            y += 20;

            _txtPassword = new TextBox
            {
                Location              = new Point(Pad, y),
                Size                  = new Size(FieldW, 28),
                Font                  = new Font("Segoe UI", 11),
                BorderStyle           = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };
            card.Controls.Add(_txtPassword);
            y += 36;

            // Show password checkbox
            _chkShowPassword = new CheckBox
            {
                Text      = "Show Password",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(90, 90, 90),
                Location  = new Point(Pad, y),
                AutoSize  = true,
                Cursor    = Cursors.Hand
            };
            _chkShowPassword.CheckedChanged += (s, e) =>
                _txtPassword.UseSystemPasswordChar = !_chkShowPassword.Checked;
            card.Controls.Add(_chkShowPassword);

            // Error label (right-aligned on same row)
            _lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(192, 57, 43),
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 20),
                TextAlign = ContentAlignment.MiddleRight
            };
            card.Controls.Add(_lblError);
            y += 32;

            // Login button
            _btnLogin = new Button
            {
                Text      = "Login",
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 44),
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _btnLogin.FlatAppearance.BorderSize = 0;
            _btnLogin.Click += BtnLogin_Click;
            card.Controls.Add(_btnLogin);
            y += 52;

            // Register button
            _btnRegister = new Button
            {
                Text      = "Register as New Customer",
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 38),
                BackColor = GreenLight,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            _btnRegister.FlatAppearance.BorderSize = 0;
            _btnRegister.Click += (s, e) => new CustomerRegistrationForm().ShowDialog();
            card.Controls.Add(_btnRegister);

            // ── Demo hint below card ──────────────────────────────────────────
            var lblDemo = new Label
            {
                Text      = "Demo: admin / password  |  johndoe / password",
                Font      = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize  = true
            };
            this.Controls.Add(lblDemo);

            this.AcceptButton = _btnLogin;
            this.Load += (s, e) =>
            {
                PositionCard();
                // trigger header label centering
                header.Width = header.Width;
                // position demo label
                lblDemo.Location = new Point(
                    (this.ClientSize.Width - lblDemo.Width) / 2,
                    card.Bottom + 10);
            };

            // Keep demo label positioned on resize
            this.Resize += (s, e) =>
            {
                lblDemo.Location = new Point(
                    (this.ClientSize.Width - lblDemo.Width) / 2,
                    card.Bottom + 10);
            };
        }

        private void PositionCard()
        {
            // Find the card (first Panel child)
            Panel card = null;
            foreach (Control c in this.Controls)
                if (c is Panel p) { card = p; break; }
            if (card == null) return;

            card.Location = new Point(
                (this.ClientSize.Width  - card.Width)  / 2,
                Math.Max(16, (this.ClientSize.Height - card.Height - 40) / 2));
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                _lblError.Text = "";
                string username = _txtUsername.Text.Trim();
                string password = _txtPassword.Text;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    _lblError.Text = "Please enter username and password";
                    return;
                }

                var user = _authService.Login(username, password);
                if (user == null)
                {
                    _lblError.Text = "Invalid username or password";
                    return;
                }

                _authService.SetCurrentUser(user);
                this.Hide();

                if (user.UserType == "Admin")
                    new AdminDashboard().ShowDialog();
                else
                    new CustomerDashboard().ShowDialog();

                // Dashboard closed (user logged out) — show login again with cleared fields
                _authService.Logout();
                _txtUsername.Clear();
                _txtPassword.Clear();
                _lblError.Text = "";
                this.Show();
            }
            catch (Exception ex)
            {
                _lblError.Text = "Login failed: " + ex.Message;
            }
        }
    }
}
