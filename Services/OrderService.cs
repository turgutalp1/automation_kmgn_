using System;
using System.Collections.Generic;
using System.Linq;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon.Services
{
    public class OrderService : IOrderService
    {
        private readonly List<CartItem> _cart = new();
        private readonly List<Order> _orderHistory = new();
        private readonly Random _random = new();

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
                Items = _cart.ToList(), // Copy items
                OrderDate = DateTime.Now
            };

            _orderHistory.Add(order);
            _cart.Clear();
            return order;
        }

        public IReadOnlyList<Order> GetOrderHistory()
        {
            return _orderHistory.AsReadOnly();
        }
    }
}
