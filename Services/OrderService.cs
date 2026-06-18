using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using CigkofteOtomasyon.Data;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon.Services
{
    public class OrderService : IOrderService
    {
        // Sepet hâlâ bellekte — geçici bir yapıdır
        private readonly List<CartItem> _cart = new();

        public IReadOnlyList<CartItem> GetCartItems()
        {
            return _cart.AsReadOnly();
        }

        public decimal GetCartTotal()
        {
            return _cart.Sum(x => x.TotalPrice);
        }

        public void AddToCart(Product product)
        {
            var existingItem = _cart.FirstOrDefault(x => x.Product.Id == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _cart.Add(new CartItem { Product = product, Quantity = 1 });
            }
        }

        public void RemoveFromCart(int index)
        {
            if (index >= 0 && index < _cart.Count)
            {
                _cart.RemoveAt(index);
            }
        }

        public void IncreaseQuantity(int index)
        {
            if (index >= 0 && index < _cart.Count)
            {
                _cart[index].Quantity++;
            }
        }

        public void DecreaseQuantity(int index)
        {
            if (index >= 0 && index < _cart.Count)
            {
                _cart[index].Quantity--;
                if (_cart[index].Quantity <= 0)
                {
                    _cart.RemoveAt(index);
                }
            }
        }

        public void ClearCart()
        {
            _cart.Clear();
        }

        public Order? ConfirmOrder()
        {
            if (!_cart.Any())
                return null;

            var order = new Order
            {
                OrderNumber = new Random().Next(1000, 9999),
                Items = _cart.ToList(),
                OrderDate = DateTime.Now
            };

            // Siparişi veritabanına kaydet
            SaveOrderToDatabase(order);

            _cart.Clear();
            return order;
        }

        public IReadOnlyList<Order> GetOrderHistory()
        {
            return LoadOrdersFromDatabase().AsReadOnly();
        }

        /// <summary>
        /// Siparişi Access veritabanına kaydeder (Orders + OrderItems).
        /// </summary>
        private void SaveOrderToDatabase(Order order)
        {
            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            // Orders tablosuna ekle
            using var orderCmd = new OleDbCommand(
                "INSERT INTO Orders (OrderNumber, OrderDate, TotalAmount) VALUES (?, ?, ?)", conn);
            orderCmd.Parameters.Add(new OleDbParameter("@OrderNumber", OleDbType.Integer)).Value = order.OrderNumber;
            orderCmd.Parameters.Add(new OleDbParameter("@OrderDate", OleDbType.Date)).Value = order.OrderDate;
            orderCmd.Parameters.Add(new OleDbParameter("@TotalAmount", OleDbType.Currency)).Value = order.TotalAmount;
            orderCmd.ExecuteNonQuery();

            // Yeni eklenen siparişin Id'sini al
            using var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn);
            int orderId = Convert.ToInt32(idCmd.ExecuteScalar());

            // OrderItems tablosuna her bir ürünü ekle
            foreach (var item in order.Items)
            {
                using var itemCmd = new OleDbCommand(
                    "INSERT INTO OrderItems (OrderId, ProductName, Quantity, UnitPrice, TotalPrice) " +
                    "VALUES (?, ?, ?, ?, ?)", conn);
                itemCmd.Parameters.Add(new OleDbParameter("@OrderId", OleDbType.Integer)).Value = orderId;
                itemCmd.Parameters.Add(new OleDbParameter("@ProductName", OleDbType.VarWChar, 255)).Value = item.Product.Name;
                itemCmd.Parameters.Add(new OleDbParameter("@Quantity", OleDbType.Integer)).Value = item.Quantity;
                itemCmd.Parameters.Add(new OleDbParameter("@UnitPrice", OleDbType.Currency)).Value = item.Product.Price;
                itemCmd.Parameters.Add(new OleDbParameter("@TotalPrice", OleDbType.Currency)).Value = item.TotalPrice;
                itemCmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Sipariş geçmişini Access veritabanından okur.
        /// </summary>
        private List<Order> LoadOrdersFromDatabase()
        {
            var orders = new List<Order>();

            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            // Tüm siparişleri oku
            using var orderCmd = new OleDbCommand(
                "SELECT Id, OrderNumber, OrderDate, TotalAmount FROM Orders ORDER BY OrderDate DESC", conn);
            using var orderReader = orderCmd.ExecuteReader();

            var orderList = new List<(int Id, int OrderNumber, DateTime OrderDate, decimal TotalAmount)>();
            while (orderReader.Read())
            {
                orderList.Add((
                    orderReader.GetInt32(0),
                    orderReader.GetInt32(1),
                    orderReader.GetDateTime(2),
                    orderReader.GetDecimal(3)
                ));
            }
            orderReader.Close();

            // Her sipariş için ürünleri oku
            foreach (var (id, orderNumber, orderDate, totalAmount) in orderList)
            {
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    OrderDate = orderDate,
                    Items = new List<CartItem>()
                };

                using var itemCmd = new OleDbCommand(
                    "SELECT ProductName, Quantity, UnitPrice, TotalPrice FROM OrderItems WHERE OrderId = ?", conn);
                itemCmd.Parameters.AddWithValue("@OrderId", id);

                using var itemReader = itemCmd.ExecuteReader();
                while (itemReader.Read())
                {
                    order.Items.Add(new CartItem
                    {
                        Product = new Product
                        {
                            Name = itemReader.GetString(0),
                            Price = itemReader.GetDecimal(2)
                        },
                        Quantity = itemReader.GetInt32(1)
                    });
                }

                orders.Add(order);
            }

            return orders;
        }
    }
}
