using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CigkofteOtomasyon.Services;

namespace CigkofteOtomasyon
{
    public class SelectionForm : Form
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        public SelectionForm(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Giriş Ekranı";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;

            // Logo
            string logoPath = Path.Combine(Application.StartupPath, "Resources", "logo.png");
            if (File.Exists(logoPath))
            {
                var picLogo = new PictureBox
                {
                    Image = Image.FromFile(logoPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(200, 120),
                    Location = new Point(140, 20)
                };
                Controls.Add(picLogo);
            }

            var lblTitle = new Label
            {
                Text = "Çiğköfte Sipariş Sistemine Hoşgeldiniz",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(60, 160),
                AutoSize = true
            };
            Controls.Add(lblTitle);

            var btnCustomer = new Button
            {
                Text = "🛒 Müşteri Girişi\n(Sipariş Ver)",
                Location = new Point(80, 220),
                Size = new Size(150, 80),
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnCustomer.Click += BtnCustomer_Click;

            var btnAdmin = new Button
            {
                Text = "🔧 Yönetici Girişi\n(Ürün Yönet)",
                Location = new Point(250, 220),
                Size = new Size(150, 80),
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnAdmin.Click += BtnAdmin_Click;

            Controls.AddRange(new Control[] { btnCustomer, btnAdmin });
        }

        private void BtnCustomer_Click(object? sender, EventArgs e)
        {
            this.Hide();
            using var form = new CustomerForm(_productService, _orderService);
            form.ShowDialog();
            this.Show();
        }

        private void InitializeComponent()
        {

        }

        private void BtnAdmin_Click(object? sender, EventArgs e)
        {
            using var passwordForm = new Form
            {
                Text = "Yönetici Girişi",
                Size = new Size(300, 150),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            var lbl = new Label { Text = "Şifre:", Location = new Point(20, 30), AutoSize = true, Font = new Font("Segoe UI", 10) };
            var txtPassword = new TextBox { Location = new Point(80, 28), Size = new Size(150, 25), PasswordChar = '*' };
            var btnLogin = new Button
            {
                Text = "Giriş",
                Location = new Point(150, 60),
                Size = new Size(80, 30),
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnLogin.Click += (s, ev) =>
            {
                if (txtPassword.Text == "yeşil123")
                {
                    passwordForm.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Hatalı şifre girdiniz!", "Giriş Başarısız", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            passwordForm.AcceptButton = btnLogin;
            passwordForm.Controls.AddRange(new Control[] { lbl, txtPassword, btnLogin });

            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                this.Hide();
                using var form = new AdminForm(_productService, _orderService);
                form.ShowDialog();
                this.Show();
            }
        }
    }
}
