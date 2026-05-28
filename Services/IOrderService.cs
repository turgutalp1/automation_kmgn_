using System.Collections.Generic;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon.Services
{
    public interface IOrderService
    {
        IReadOnlyList<CartItem> GetCartItems();
        decimal GetCartTotal();
        void AddToCart(Product product);
        void RemoveFromCart(int index);
        void IncreaseQuantity(int index);
        void DecreaseQuantity(int index);
        void ClearCart();
        Order? ConfirmOrder();
        IReadOnlyList<Order> GetOrderHistory();
    }
}
