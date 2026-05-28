using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CigkofteOtomasyon.Services;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon
{
    public class AdminForm : Form
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        private DataGridView _dgvProducts = null!;
        private TextBox _txtName = null!;
        private TextBox _txtPrice = null!;
        private ComboBox _cmbCategory = null!;

        private DataGridView _dgvOrders = null!;
        private ListBox _lstOrderDetails = null!;

        public AdminForm(IProductService productService, IOrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
            InitializeUI();
            LoadProducts();
            LoadOrders();
        }

        private void InitializeUI()
        {
            Text = "🔧 Yönetici - Ürün Yönetimi";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            var lblHeader = new Label
            {
                Text = "YÖNETİCİ PANELİ",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                Location = new Point(20, 20),
                AutoSize = true
            };

            var btnGeri = new Button
            {
                Text = "🔙 Geri",
                Location = new Point(680, 20),
                Size = new Size(80, 40),
                BackColor = Color.DarkSlateGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGeri.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            var tabControl = new TabControl
            {
                Location = new Point(20, 80),
                Size = new Size(740, 460),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // ------------------------------------
            // TAB 1: ÜRÜN YÖNETİMİ
            // ------------------------------------
            var tabProducts = new TabPage("Ürün Yönetimi") { BackColor = Color.White };

            _dgvProducts = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(710, 260),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            var grpAdd = new GroupBox
            {
                Text = "Yeni Ürün Ekle",
                Location = new Point(10, 280),
                Size = new Size(710, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var lblName = new Label { Text = "Ürün Adı:", Location = new Point(20, 40), AutoSize = true };
            _txtName = new TextBox { Location = new Point(100, 38), Size = new Size(130, 25) };

            var lblPrice = new Label { Text = "Fiyat (₺):", Location = new Point(240, 40), AutoSize = true };
            _txtPrice = new TextBox { Location = new Point(310, 38), Size = new Size(80, 25) };

            var lblCategory = new Label { Text = "Kategori:", Location = new Point(410, 40), AutoSize = true };
            _cmbCategory = new ComboBox { Location = new Point(480, 38), Size = new Size(110, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbCategory.Items.AddRange(new string[] { "Yiyecekler", "İçecekler", "Soslar", "Tatlılar", "Diğer" });
            _cmbCategory.SelectedIndex = 0;

            var btnAdd = new Button
            {
                Text = "➕ Ekle",
                Location = new Point(620, 35),
                Size = new Size(70, 30),
                BackColor = Color.MediumSeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.Click += BtnAdd_Click;

            grpAdd.Controls.AddRange(new Control[] { lblName, _txtName, lblPrice, _txtPrice, lblCategory, _cmbCategory, btnAdd });
            tabProducts.Controls.AddRange(new Control[] { _dgvProducts, grpAdd });

            // ------------------------------------
            // TAB 2: SİPARİŞ GEÇMİŞİ
            // ------------------------------------
            var tabOrders = new TabPage("Sipariş Geçmişi") { BackColor = Color.White };

            _dgvOrders = new DataGridView
            {
                Location = new Point(10, 10),
                Size = new Size(400, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.WhiteSmoke
            };
            _dgvOrders.SelectionChanged += DgvOrders_SelectionChanged;

            var lblDetails = new Label { Text = "Sipariş Detayı:", Location = new Point(430, 10), AutoSize = true };

            _lstOrderDetails = new ListBox
            {
                Location = new Point(430, 40),
                Size = new Size(280, 370),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.WhiteSmoke
            };

            tabOrders.Controls.AddRange(new Control[] { _dgvOrders, lblDetails, _lstOrderDetails });

            tabControl.TabPages.Add(tabProducts);
            tabControl.TabPages.Add(tabOrders);

            Controls.AddRange(new Control[] { lblHeader, btnGeri, tabControl });
        }

        private void LoadProducts()
        {
            var products = _productService.GetAllProducts().ToList();
            _dgvProducts.DataSource = products;
        }

        private void LoadOrders()
        {
            var orders = _orderService.GetOrderHistory()
                .Select(o => new
                {
                    SiparisNo = o.OrderNumber,
                    Tarih = o.OrderDate.ToString("HH:mm:ss"),
                    Tutar = o.TotalAmount.ToString("C0")
                }).ToList();

            _dgvOrders.DataSource = orders;
        }

        private void DgvOrders_SelectionChanged(object? sender, EventArgs e)
        {
            _lstOrderDetails.Items.Clear();

            if (_dgvOrders.SelectedRows.Count > 0)
            {
                var row = _dgvOrders.SelectedRows[0];
                int orderNo = (int)row.Cells["SiparisNo"].Value;

                var order = _orderService.GetOrderHistory().FirstOrDefault(o => o.OrderNumber == orderNo);
                if (order != null)
                {
                    foreach (var item in order.Items)
                    {
                        _lstOrderDetails.Items.Add($"{item.Quantity}x {item.Product.Name} - {item.TotalPrice:C0}");
                    }
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text) || !decimal.TryParse(_txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Lütfen geçerli bir ürün adı ve fiyatı giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var product = new Product
            {
                Name = _txtName.Text.Trim(),
                Price = price,
                Category = _cmbCategory.SelectedItem?.ToString() ?? "Diğer"
            };

            _productService.AddProduct(product);
            MessageBox.Show("Ürün başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            _txtName.Clear();
            _txtPrice.Clear();
            LoadProducts();
        }
    }
}
