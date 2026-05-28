using System.Collections.Generic;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        Product? GetProductById(int id);
        Product? GetProductByName(string name);
        void AddProduct(Product product);
    }
}
