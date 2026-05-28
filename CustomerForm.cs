using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CigkofteOtomasyon.Services;
using CigkofteOtomasyon.Models;
using System.Collections.Generic;

namespace CigkofteOtomasyon
{
    public class CustomerForm : Form
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        private ListBox _lstSepet = null!;
        private Label _lblToplam = null!;
        private FlowLayoutPanel _pnlCategories = null!;
        private FlowLayoutPanel _pnlProducts = null!;

        public CustomerForm(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;

            InitializeUI();
            LoadCategories();
        }

        private void InitializeUI()
        {
            Text = "🌯 Müşteri - Çiğköfte Sipariş Ekranı";
            Size = new Size(900, 700);
            StartPosition = FormStartPosition.CenterScreen;

            // Load background if exists
            string bgPath = Path.Combine(Application.StartupPath, "Resources", "bg.png");
            if (File.Exists(bgPath))
            {
                BackgroundImage = Image.FromFile(bgPath);
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                BackColor = Color.FromArgb(40, 40, 40);
            }

            var pnlMain = new Panel
            {
                Size = new Size(860, 640),
                Location = new Point(10, 10),
                BackColor = Color.FromArgb(160, 0, 0, 0) // Koyu ve yarı saydam arka plan
            };

            // Logo
            string logoPath = Path.Combine(Application.StartupPath, "Resources", "logo.png");
            if (File.Exists(logoPath))
            {
                var picLogo = new PictureBox
                {
                    Image = Image.FromFile(logoPath),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Size = new Size(150, 100),
                    Location = new Point(20, 10),
                    BackColor = Color.Transparent
                };
                pnlMain.Controls.Add(picLogo);
            }

            var lblHeader = new Label
            {
                Text = "SİPARİŞ EKRANI",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Orange,
                Location = new Point(180, 40),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlMain.Controls.Add(lblHeader);

            _pnlCategories = new FlowLayoutPanel
            {
                Location = new Point(20, 120),
                Size = new Size(500, 100), // Yükseklik artırıldı ki fazla kategori alta geçebilsin
                BackColor = Color.Transparent
            };

            _pnlProducts = new FlowLayoutPanel
            {
                Location = new Point(20, 220), // Aşağı kaydırıldı
                Size = new Size(500, 400), // Yükseklik dengelendi
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            _lstSepet = new ListBox
            {
                Location = new Point(540, 120),
                Size = new Size(300, 270),
                Font = new Font("Segoe UI", 11),
                BackColor = Color.WhiteSmoke
            };
            // Double click still removes, but we also have a button now
            _lstSepet.DoubleClick += LstSepet_DoubleClick;

            var btnPlus = new Button
            {
                Text = "➕",
                Location = new Point(540, 400),
                Size = new Size(60, 40),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnPlus.FlatAppearance.BorderSize = 0;
            btnPlus.Click += BtnPlus_Click;

            var btnMinus = new Button
            {
                Text = "➖",
                Location = new Point(610, 400),
                Size = new Size(60, 40),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnMinus.FlatAppearance.BorderSize = 0;
            btnMinus.Click += BtnMinus_Click;

            var btnDelete = new Button
            {
                Text = "🗑 Seçili Ürünü Sil",
                Location = new Point(680, 400),
                Size = new Size(160, 40),
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Click += BtnDelete_Click;

            _lblToplam = new Label
            {
                Location = new Point(540, 450),
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Text = "Toplam: ₺0"
            };

            var btnOnayla = new Button
            {
                Text = "✓ Siparişi Onayla",
                Location = new Point(540, 500),
                Size = new Size(300, 50),
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };
            btnOnayla.FlatAppearance.BorderSize = 0;
            btnOnayla.Click += BtnOnayla_Click;

            var btnCancel = new Button
            {
                Text = "❌ Siparişi İptal Et",
                Location = new Point(540, 560),
                Size = new Size(300, 40),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += BtnTemizle_Click;

            var btnGeri = new Button
            {
                Text = "🔙 Geri",
                Location = new Point(780, 20),
                Size = new Size(60, 40),
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGeri.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            pnlMain.Controls.AddRange(new Control[] { _pnlCategories, _pnlProducts, _lstSepet, btnPlus, btnMinus, btnDelete, _lblToplam, btnOnayla, btnCancel, btnGeri });
            Controls.Add(pnlMain);
        }

        private void LoadCategories()
        {
            var products = _productService.GetAllProducts();
            var categories = products.Select(p => p.Category).Distinct().ToList();

            foreach (var category in categories)
            {
                var btn = new Button
                {
                    Text = category,
                    Size = new Size(100, 40),
                    Margin = new Padding(5),
                    BackColor = Color.DarkOrange,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => LoadProductsByCategory(category);
                _pnlCategories.Controls.Add(btn);
            }

            if (categories.Any())
            {
                LoadProductsByCategory(categories.First());
            }
        }

        private void LoadProductsByCategory(string category)
        {
            _pnlProducts.Controls.Clear();
            var products = _productService.GetAllProducts().Where(p => p.Category == category);

            foreach (var product in products)
            {
                var btn = new Button
                {
                    Text = $"{product.Name}\n{product.Price:C0}",
                    Size = new Size(140, 90),
                    Margin = new Padding(8),
                    BackColor = Color.OrangeRed,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Tag = product
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += ProductButton_Click;
                _pnlProducts.Controls.Add(btn);
            }
        }

        private void ProductButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product product)
            {
                _orderService.AddToCart(product);
                UpdateCartUI();
            }
        }

        private void BtnPlus_Click(object? sender, EventArgs e)
        {
            if (_lstSepet.SelectedIndex >= 0)
            {
                int selectedIndex = _lstSepet.SelectedIndex;
                _orderService.IncreaseQuantity(selectedIndex);
                UpdateCartUI();
                _lstSepet.SelectedIndex = selectedIndex; // keep selection
            }
        }

        private void BtnMinus_Click(object? sender, EventArgs e)
        {
            if (_lstSepet.SelectedIndex >= 0)
            {
                int selectedIndex = _lstSepet.SelectedIndex;
                _orderService.DecreaseQuantity(selectedIndex);
                UpdateCartUI();
                if (selectedIndex < _lstSepet.Items.Count)
                    _lstSepet.SelectedIndex = selectedIndex;
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_lstSepet.SelectedIndex >= 0)
            {
                _orderService.RemoveFromCart(_lstSepet.SelectedIndex);
                UpdateCartUI();
            }
        }

        private void LstSepet_DoubleClick(object? sender, EventArgs e)
        {
            BtnDelete_Click(sender, e);
        }

        private void BtnTemizle_Click(object? sender, EventArgs e)
        {
            if (_orderService.GetCartItems().Any())
            {
                var result = MessageBox.Show("Siparişi tamamen iptal etmek istediğinize emin misiniz?", "İptal Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    _orderService.ClearCart();
                    UpdateCartUI();
                }
            }
        }

        private void BtnOnayla_Click(object? sender, EventArgs e)
        {
            var order = _orderService.ConfirmOrder();
            if (order == null)
            {
                MessageBox.Show("Sepet boş!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show($"✅ Sipariş #{order.OrderNumber} alındı!\nToplam: {order.TotalAmount:C0}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateCartUI();
        }

        private void UpdateCartUI()
        {
            _lstSepet.Items.Clear();
            var cartItems = _orderService.GetCartItems();
            
            foreach (var item in cartItems)
            {
                _lstSepet.Items.Add($"{item.Product.Name} x{item.Quantity} = {item.TotalPrice:C0}");
            }
            
            _lblToplam.Text = $"Toplam: {_orderService.GetCartTotal():C0}";
        }
    }
}
