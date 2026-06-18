using System;
using System.Collections.Generic;
using System.Data.OleDb;
using CigkofteOtomasyon.Data;
using CigkofteOtomasyon.Models;

namespace CigkofteOtomasyon.Services
{
    public class ProductService : IProductService
    {
        public IEnumerable<Product> GetAllProducts()
        {
            var products = new List<Product>();

            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            using var cmd = new OleDbCommand("SELECT Id, [Name], Price, Category FROM Products ORDER BY Id", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Category = reader.GetString(3)
                });
            }

            return products;
        }

        public Product? GetProductById(int id)
        {
            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            using var cmd = new OleDbCommand("SELECT Id, [Name], Price, Category FROM Products WHERE Id = ?", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Category = reader.GetString(3)
                };
            }

            return null;
        }

        public Product? GetProductByName(string name)
        {
            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            using var cmd = new OleDbCommand("SELECT Id, [Name], Price, Category FROM Products WHERE [Name] = ?", conn);
            cmd.Parameters.AddWithValue("@Name", name);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Category = reader.GetString(3)
                };
            }

            return null;
        }

        public void AddProduct(Product product)
        {
            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            using var cmd = new OleDbCommand(
                "INSERT INTO Products ([Name], Price, Category) VALUES (?, ?, ?)", conn);
            cmd.Parameters.Add(new OleDbParameter("@Name", OleDbType.VarWChar, 255)).Value = product.Name;
            cmd.Parameters.Add(new OleDbParameter("@Price", OleDbType.Currency)).Value = product.Price;
            cmd.Parameters.Add(new OleDbParameter("@Category", OleDbType.VarWChar, 100)).Value = product.Category;
            cmd.ExecuteNonQuery();

            // Yeni eklenen ürünün Id'sini al
            using var idCmd = new OleDbCommand("SELECT @@IDENTITY", conn);
            product.Id = Convert.ToInt32(idCmd.ExecuteScalar());
        }

        public void DeleteProduct(int id)
        {
            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            using var cmd = new OleDbCommand("DELETE FROM Products WHERE Id = ?", conn);
            cmd.Parameters.Add(new OleDbParameter("@Id", OleDbType.Integer)).Value = id;
            cmd.ExecuteNonQuery();
        }

        public void UpdateProduct(Product product)
        {
            using var conn = new OleDbConnection(DatabaseHelper.ConnectionString);
            conn.Open();

            using var cmd = new OleDbCommand(
                "UPDATE Products SET [Name] = ?, Price = ?, Category = ? WHERE Id = ?", conn);
            cmd.Parameters.Add(new OleDbParameter("@Name", OleDbType.VarWChar, 255)).Value = product.Name;
            cmd.Parameters.Add(new OleDbParameter("@Price", OleDbType.Currency)).Value = product.Price;
            cmd.Parameters.Add(new OleDbParameter("@Category", OleDbType.VarWChar, 100)).Value = product.Category;
            cmd.Parameters.Add(new OleDbParameter("@Id", OleDbType.Integer)).Value = product.Id;
            cmd.ExecuteNonQuery();
        }
    }
}
