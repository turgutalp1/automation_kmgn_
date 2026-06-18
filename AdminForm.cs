using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CigkofteOtomasyon.Services;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon
{
    public partial class AdminForm : Form
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;

        // Default parameterless constructor required by Visual Studio Designer
        public AdminForm()
        {
            InitializeComponent();
        }

        public AdminForm(IProductService productService, IOrderService orderService) : this()
        {
            _productService = productService;
            _orderService = orderService;
            LoadProducts();
            LoadOrders();
        }

        private void LoadProducts()
        {
            var products = _productService.GetAllProducts().ToList();
            dgvProducts.DataSource = products;
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

            dgvOrders.DataSource = orders;
        }

        private void DgvOrders_SelectionChanged(object? sender, EventArgs e)
        {
            lstOrderDetails.Items.Clear();

            if (dgvOrders.SelectedRows.Count > 0)
            {
                var row = dgvOrders.SelectedRows[0];
                int orderNo = (int)row.Cells["SiparisNo"].Value;

                var order = _orderService.GetOrderHistory().FirstOrDefault(o => o.OrderNumber == orderNo);
                if (order != null)
                {
                    foreach (var item in order.Items)
                    {
                        lstOrderDetails.Items.Add($"{item.Quantity}x {item.Product.Name} - {item.TotalPrice:C0}");
                    }
                }
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || !decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Lütfen geçerli bir ürün adı ve fiyatı giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var product = new Product
            {
                Name = txtName.Text.Trim(),
                Price = price,
                Category = cmbCategory.SelectedItem?.ToString() ?? "Diğer"
            };

            _productService.AddProduct(product);
            MessageBox.Show("Ürün başarıyla eklendi!", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            txtName.Clear();
            txtPrice.Clear();
            LoadProducts();
        }

        private void BtnGeri_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel; 
            this.Close();
        }
    }
}
