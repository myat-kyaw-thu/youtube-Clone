using System;
using System.Drawing;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;

namespace GreenLifeOrganicStore.Forms
{
    /// <summary>
    /// Customer Registration — Same pattern as LoginForm for reliability
    /// </summary>
    public class CustomerRegistrationForm : Form
    {
        private readonly AuthService _authService;

        private TextBox _txtUsername, _txtPassword, _txtConfirmPassword,
                        _txtEmail, _txtFullName, _txtPhone, _txtAddress;
        private Label   _lblError;
        private Button  _btnRegister, _btnCancel;

        // Card dimensions
        private const int CardW  = 440;
        private const int CardH  = 640;
        private const int Pad    = 36;
        private const int FieldW = CardW - Pad * 2;  // 368

        private static readonly Color Green    = Color.FromArgb(34, 139, 34);
        private static readonly Color GreenDark = Color.FromArgb(24, 110, 24);
        private static readonly Color BgPage   = Color.FromArgb(236, 245, 238);

        public CustomerRegistrationForm()
        {
            _authService = new AuthService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Register — GreenLife Organic Store";
            this.MinimumSize     = new Size(500, 720);
            this.Size            = new Size(520, 760);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox     = false;
            this.BackColor       = BgPage;
            this.Resize         += (s, e) => PositionCard();

            // ── Card ──────────────────────────────────────────────────────────
            var card = new Panel
            {
                Size      = new Size(CardW, CardH),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(200, 200, 200)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using (var b = new SolidBrush(Green))
                    e.Graphics.FillRectangle(b, 0, card.Height - 4, card.Width, 4);
            };
            this.Controls.Add(card);

            // ── Green header ──────────────────────────────────────────────────
            var header = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(CardW, 90),
                BackColor = Green
            };
            var lblTitle = new Label
            {
                Text      = "Create Account",
                Font      = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true
            };
            header.Controls.Add(lblTitle);
            header.Resize += (s, e) =>
                lblTitle.Location = new Point((header.Width - lblTitle.Width) / 2, 28);
            card.Controls.Add(header);

            // ── Fields — start below header ───────────────────────────────────
            int y = 110;

            // Username
            AddField(card, "Username", ref _txtUsername, ref y, false);
            
            // Password
            AddField(card, "Password", ref _txtPassword, ref y, true);
            
            // Confirm Password
            AddField(card, "Confirm Password", ref _txtConfirmPassword, ref y, true);
            
            // Email
            AddField(card, "Email", ref _txtEmail, ref y, false);
            
            // Full Name
            AddField(card, "Full Name", ref _txtFullName, ref y, false);
            
            // Phone
            AddField(card, "Phone", ref _txtPhone, ref y, false);
            
            // Address
            AddField(card, "Address", ref _txtAddress, ref y, false);

            // Error label
            _lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(192, 57, 43),
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(_lblError);
            y += 28;

            // Register button
            _btnRegister = new Button
            {
                Text      = "Register",
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 44),
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            _btnRegister.FlatAppearance.BorderSize = 0;
            _btnRegister.Click += BtnRegister_Click;
            card.Controls.Add(_btnRegister);
            y += 50;

            // Cancel button
            _btnCancel = new Button
            {
                Text      = "Cancel",
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 38),
                BackColor = Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => this.Close();
            card.Controls.Add(_btnCancel);

            this.AcceptButton = _btnRegister;
            this.Load += (s, e) =>
            {
                PositionCard();
                header.Width = header.Width;  // trigger label centering
            };
        }

        private void AddField(Panel parent, string labelText, ref TextBox textBox, ref int y, bool isPassword)
        {
            parent.Controls.Add(new Label
            {
                Text      = labelText,
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(70, 70, 70),
                Location  = new Point(Pad, y),
                Size      = new Size(FieldW, 18)
            });
            y += 20;

            textBox = new TextBox
            {
                Location    = new Point(Pad, y),
                Size        = new Size(FieldW, 28),
                Font        = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle
            };
            if (isPassword)
                textBox.UseSystemPasswordChar = true;
            
            parent.Controls.Add(textBox);
            y += 36;
        }

        private void PositionCard()
        {
            Panel card = null;
            foreach (Control c in this.Controls)
                if (c is Panel p) { card = p; break; }
            if (card == null) return;

            card.Location = new Point(
                (this.ClientSize.Width  - card.Width)  / 2,
                Math.Max(16, (this.ClientSize.Height - card.Height - 20) / 2));
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                _lblError.Text = "";

                if (string.IsNullOrWhiteSpace(_txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(_txtPassword.Text)  ||
                    string.IsNullOrWhiteSpace(_txtEmail.Text)     ||
                    string.IsNullOrWhiteSpace(_txtFullName.Text))
                {
                    _lblError.Text = "Please fill in all required fields.";
                    return;
                }
                if (!_authService.IsValidUsername(_txtUsername.Text))
                {
                    _lblError.Text = "Username: 3–50 chars, letters/numbers/underscore only.";
                    return;
                }
                if (!_authService.IsValidPassword(_txtPassword.Text))
                {
                    _lblError.Text = "Password must be at least 6 characters.";
                    return;
                }
                if (_txtPassword.Text != _txtConfirmPassword.Text)
                {
                    _lblError.Text = "Passwords do not match.";
                    return;
                }
                if (!_authService.IsValidEmail(_txtEmail.Text))
                {
                    _lblError.Text = "Please enter a valid email address.";
                    return;
                }

                _authService.RegisterCustomer(
                    _txtUsername.Text, _txtPassword.Text,
                    _txtEmail.Text, _txtFullName.Text,
                    _txtPhone.Text, _txtAddress.Text);

                MessageBox.Show("Registration successful! You can now log in.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                _lblError.Text = ex.Message;
            }
        }
    }
}

// commit 16: docs: add comments to CustomerRegistrationForm

// commit 24: docs: add comments to CustomerRegistrationForm

// commit 32: style: format CustomerRegistrationForm

// commit 40: perf: improve CustomerRegistrationForm performance
