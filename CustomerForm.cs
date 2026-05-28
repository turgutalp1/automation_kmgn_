using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CigkofteOtomasyon.Services;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon
{
    public partial class CustomerForm : Form
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        // Default parameterless constructor required by Visual Studio Designer
        public CustomerForm()
        {
            InitializeComponent();
        }

        public CustomerForm(IProductService productService, IOrderService orderService) : this()
        {
            _productService = productService;
            _orderService = orderService;

            LoadDynamicResources();
            LoadCategories();
        }

        private void LoadDynamicResources()
        {
            string bgPath = Path.Combine(Application.StartupPath, "Resources", "bg.png");
            if (File.Exists(bgPath))
            {
                BackgroundImage = Image.FromFile(bgPath);
                BackgroundImageLayout = ImageLayout.Stretch;
            }

            string logoPath = Path.Combine(Application.StartupPath, "Resources", "logo.png");
            if (File.Exists(logoPath))
            {
                picLogo.Image = Image.FromFile(logoPath);
            }
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
                pnlCategories.Controls.Add(btn);
            }

            if (categories.Any())
            {
                LoadProductsByCategory(categories.First());
            }
        }

        private void LoadProductsByCategory(string category)
        {
            pnlProducts.Controls.Clear();
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
                pnlProducts.Controls.Add(btn);
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
            if (lstSepet.SelectedIndex >= 0)
            {
                int selectedIndex = lstSepet.SelectedIndex;
                _orderService.IncreaseQuantity(selectedIndex);
                UpdateCartUI();
                lstSepet.SelectedIndex = selectedIndex; // keep selection
            }
        }

        private void BtnMinus_Click(object? sender, EventArgs e)
        {
            if (lstSepet.SelectedIndex >= 0)
            {
                int selectedIndex = lstSepet.SelectedIndex;
                _orderService.DecreaseQuantity(selectedIndex);
                UpdateCartUI();
                if (selectedIndex < lstSepet.Items.Count)
                    lstSepet.SelectedIndex = selectedIndex;
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (lstSepet.SelectedIndex >= 0)
            {
                _orderService.RemoveFromCart(lstSepet.SelectedIndex);
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

        private void BtnGeri_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; 
            this.Close();
        }

        private void UpdateCartUI()
        {
            lstSepet.Items.Clear();
            var cartItems = _orderService.GetCartItems();
            
            foreach (var item in cartItems)
            {
                lstSepet.Items.Add($"{item.Product.Name} x{item.Quantity} = {item.TotalPrice:C0}");
            }
            
            lblToplam.Text = $"Toplam: {_orderService.GetCartTotal():C0}";
        }
    }
}
