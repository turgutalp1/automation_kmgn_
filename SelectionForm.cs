using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CigkofteOtomasyon.Services;

namespace CigkofteOtomasyon
{
    public partial class SelectionForm : Form
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        // Default parameterless constructor required by Visual Studio Designer
        public SelectionForm()
        {
            InitializeComponent();
        }

        public SelectionForm(IProductService productService, IOrderService orderService) : this()
        {
            _productService = productService;
            _orderService = orderService;
            LoadDynamicResources();
        }

        private void LoadDynamicResources()
        {
            string logoPath = Path.Combine(Application.StartupPath, "Resources", "logo.png");
            if (File.Exists(logoPath))
            {
                picLogo.Image = Image.FromFile(logoPath);
            }
        }

        private void BtnCustomer_Click(object? sender, EventArgs e)
        {
            if (_productService == null || _orderService == null) return;
            this.Hide();
            using var form = new CustomerForm(_productService, _orderService);
            form.ShowDialog();
            this.Show();
        }

        private void BtnAdmin_Click(object? sender, EventArgs e)
        {
            if (_productService == null || _orderService == null) return;
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
