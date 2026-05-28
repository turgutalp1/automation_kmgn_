using System;
using System.Collections.Generic;
using System.Linq;

namespace CigkofteOtomasyon.Models
{
    public class Order
    {
        public int OrderNumber { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(x => x.TotalPrice);
    }
}
