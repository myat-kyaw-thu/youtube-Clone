using System;
using System.Drawing;
using System.Windows.Forms;
using GreenLifeOrganicStore.Services;

namespace GreenLifeOrganicStore.Forms
{
    /// <summary>
    /// Customer Registration — centered card, fluid, no fixed clipping.
    /// </summary>
    public class CustomerRegistrationForm : Form
    {
        private readonly AuthService _authService;

        private TextBox _txtUsername, _txtPassword, _txtConfirmPassword,
                        _txtEmail, _txtFullName, _txtPhone, _txtAddress;
        private Label   _lblError;
        private Button  _btnRegister, _btnCancel;

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
            this.Text = "Register — GreenLife Organic Store";
            this.MinimumSize = new Size(480, 640);
            this.Size = new Size(500, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.BackColor = BgPage;
            this.Resize += (s, e) => CenterCard();

            // ── Card ──────────────────────────────────────────────────────────
            var card = new Panel
            {
                Size = new Size(420, 600),
                BackColor = Color.White
            };
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(210, 210, 210)))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };
            this.Controls.Add(card);

            // ── Header band ───────────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = Green
            };
            var lblTitle = new Label
            {
                Text = "Create Account",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblTitle);
            card.Controls.Add(header);

            // ── Scrollable body ───────────────────────────────────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                Padding = new Padding(32, 16, 32, 16)
            };

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            // Field rows
            string[] labels = { "Username", "Password", "Confirm Password",
                                 "Email", "Full Name", "Phone", "Address" };
            var boxes = new TextBox[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                body.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));  // label
                body.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));  // input
                body.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));  // gap

                var lbl = new Label
                {
                    Text = labels[i],
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = Color.FromArgb(70, 70, 70),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.BottomLeft
                };
                var txt = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10.5f),
                    BorderStyle = BorderStyle.FixedSingle
                };
                if (labels[i].Contains("Password"))
                    txt.UseSystemPasswordChar = true;

                boxes[i] = txt;
                int row = i * 3;
                body.RowCount = row + 3;
                body.Controls.Add(lbl, 0, row);
                body.Controls.Add(txt, 0, row + 1);
                body.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Color.White }, 0, row + 2);
            }

            _txtUsername        = boxes[0];
            _txtPassword        = boxes[1];
            _txtConfirmPassword = boxes[2];
            _txtEmail           = boxes[3];
            _txtFullName        = boxes[4];
            _txtPhone           = boxes[5];
            _txtAddress         = boxes[6];

            // Error label
            _lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(192, 57, 43),
                Dock = DockStyle.Fill,
                AutoSize = false,
                Height = 20
            };
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
            body.RowCount++;
            body.Controls.Add(_lblError, 0, labels.Length * 3);

            // Buttons
            _btnRegister = new Button
            {
                Text = "Register",
                Dock = DockStyle.Fill,
                Height = 42,
                BackColor = Green,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _btnRegister.FlatAppearance.BorderSize = 0;
            _btnRegister.Click += BtnRegister_Click;

            _btnCancel = new Button
            {
                Text = "Cancel",
                Dock = DockStyle.Fill,
                Height = 38,
                BackColor = Color.FromArgb(150, 150, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            _btnCancel.FlatAppearance.BorderSize = 0;
            _btnCancel.Click += (s, e) => this.Close();

            int btnRow = labels.Length * 3 + 1;
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            body.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            body.RowCount += 2;
            body.Controls.Add(_btnRegister, 0, btnRow);
            body.Controls.Add(_btnCancel,   0, btnRow + 1);

            scroll.Controls.Add(body);
            card.Controls.Add(scroll);

            this.Load += (s, e) => CenterCard();
            this.AcceptButton = _btnRegister;
        }

        private Panel _card => this.Controls.Count > 0 ? this.Controls[0] as Panel : null;

        private void CenterCard()
        {
            var card = _card;
            if (card == null) return;
            card.Location = new Point(
                Math.Max(0, (this.ClientSize.Width  - card.Width)  / 2),
                Math.Max(0, (this.ClientSize.Height - card.Height) / 2));
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
