using System.Collections.Generic;
using System.Linq;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon.Services
{
    public class ProductService : IProductService
    {
        // In-memory product list
        private readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Çiğköfte Dürüm", Price = 45, Category = "Yiyecekler" },
            new Product { Id = 2, Name = "Acılı Dürüm", Price = 50, Category = "Yiyecekler" },
            new Product { Id = 3, Name = "Soslu Dürüm", Price = 55, Category = "Yiyecekler" },
            new Product { Id = 4, Name = "Porsiyon 200g", Price = 60, Category = "Yiyecekler" },
            new Product { Id = 5, Name = "Porsiyon 300g", Price = 85, Category = "Yiyecekler" },
            new Product { Id = 6, Name = "Ayran", Price = 15, Category = "İçecekler" },
            new Product { Id = 7, Name = "Şalgam", Price = 18, Category = "İçecekler" },
            new Product { Id = 8, Name = "Kola", Price = 25, Category = "İçecekler" },
            new Product { Id = 9, Name = "Ekstra Sos", Price = 5, Category = "Soslar" },
            new Product { Id = 10, Name = "Nar Ekşisi", Price = 8, Category = "Soslar" }
        };

        public IEnumerable<Product> GetAllProducts()
        {
            return _products;
        }

        public Product? GetProductById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public Product? GetProductByName(string name)
        {
            return _products.FirstOrDefault(p => p.Name == name);
        }

        public void AddProduct(Product product)
        {
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
        }
    }
}
