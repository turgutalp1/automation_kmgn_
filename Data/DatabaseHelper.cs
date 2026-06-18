using System;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;

namespace CigkofteOtomasyon.Data
{
    public static class DatabaseHelper
    {
        private static readonly string DbFileName = "CigkofteDB.accdb";
        private static string? _provider;

        public static string DbPath => Path.Combine(Application.StartupPath, DbFileName);

        /// <summary>
        /// Sistemde yüklü ACE.OLEDB provider'ını algılar (16.0 veya 12.0).
        /// </summary>
        public static string Provider
        {
            get
            {
                if (_provider != null) return _provider;

                // Önce 16.0, sonra 12.0 dene
                string[] candidates = { "Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0" };
                foreach (var candidate in candidates)
                {
                    try
                    {
                        var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(candidate + "\\Clsid");
                        if (key != null)
                        {
                            key.Close();
                            _provider = candidate;
                            return _provider;
                        }
                    }
                    catch
                    {
                        // Sonraki provider'ı dene
                    }
                }

                // Varsayılan olarak 12.0 kullan
                _provider = "Microsoft.ACE.OLEDB.12.0";
                return _provider;
            }
        }

        public static string ConnectionString =>
            $"Provider={Provider};Data Source={DbPath};Persist Security Info=False;";

        /// <summary>
        /// Veritabanını ve tabloları oluşturur, varsayılan ürünleri ekler.
        /// </summary>
        public static void Initialize()
        {
            if (!File.Exists(DbPath))
            {
                CreateDatabase();
            }

            CreateTables();
            SeedDefaultProducts();
        }

        private static void CreateDatabase()
        {
            // Her iki provider ile de deneme yap
            string[] providers = { "Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0" };
            Exception? lastException = null;

            foreach (var provider in providers)
            {
                try
                {
                    string connStr = $"Provider={provider};Data Source={DbPath};";

                    var catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
                    if (catalogType != null)
                    {
                        dynamic catalog = Activator.CreateInstance(catalogType)!;
                        try
                        {
                            catalog.Create(connStr);
                            _provider = provider; // Çalışan provider'ı kaydet
                            return;
                        }
                        finally
                        {
                            // Bağlantıyı kapat
                            try
                            {
                                var conn = catalog.ActiveConnection;
                                if (conn != null)
                                {
                                    conn.Close();
                                    System.Runtime.InteropServices.Marshal.ReleaseComObject(conn);
                                }
                            }
                            catch { }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(catalog);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    // Bu provider ile olmadı, sonrakini dene
                    // Yarım kalan dosyayı sil
                    try { if (File.Exists(DbPath)) File.Delete(DbPath); } catch { }
                }
            }

            // Hiçbir provider çalışmadıysa, boş veritabanı şablonunu kullan
            try
            {
                CreateEmptyAccessDatabase();
            }
            catch (Exception ex2)
            {
                throw new InvalidOperationException(
                    $"Veritabanı oluşturulamadı. Son hata: {lastException?.Message ?? ex2.Message}\n\n" +
                    "Microsoft Access Database Engine 2016 Redistributable yüklemeniz gerekebilir:\n" +
                    "https://www.microsoft.com/en-us/download/details.aspx?id=54920",
                    lastException ?? ex2);
            }
        }

        /// <summary>
        /// ADOX olmadan boş bir Access veritabanı oluşturur.
        /// Minimal .accdb dosyası binary olarak yazılır.
        /// </summary>
        private static void CreateEmptyAccessDatabase()
        {
            // Boş Access veritabanını DAO ile oluştur
            var dbeType = Type.GetTypeFromProgID("DAO.DBEngine.120");
            if (dbeType == null)
            {
                dbeType = Type.GetTypeFromProgID("DAO.DBEngine.36");
            }

            if (dbeType != null)
            {
                dynamic dbe = Activator.CreateInstance(dbeType)!;
                try
                {
                    // dbVersion120 = 128 (Access 2007+ format)
                    dynamic db = dbe.CreateDatabase(DbPath, ";LANGID=0x0409;CP=1252;COUNTRY=0", 128);
                    db.Close();
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(db);
                    return;
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(dbe);
                }
            }

            throw new InvalidOperationException(
                "Ne ADOX ne de DAO kullanılarak veritabanı oluşturulabildi. " +
                "Lütfen Microsoft Access Database Engine yükleyin.");
        }

        private static void CreateTables()
        {
            using var conn = new OleDbConnection(ConnectionString);
            conn.Open();

            // Products tablosu
            ExecuteIfTableNotExists(conn, "Products",
                @"CREATE TABLE Products (
                    Id AUTOINCREMENT PRIMARY KEY,
                    [Name] TEXT(255) NOT NULL,
                    Price CURRENCY NOT NULL,
                    Category TEXT(100) NOT NULL
                )");

            // Orders tablosu
            ExecuteIfTableNotExists(conn, "Orders",
                @"CREATE TABLE Orders (
                    Id AUTOINCREMENT PRIMARY KEY,
                    OrderNumber INTEGER NOT NULL,
                    OrderDate DATETIME NOT NULL,
                    TotalAmount CURRENCY NOT NULL
                )");

            // OrderItems tablosu
            ExecuteIfTableNotExists(conn, "OrderItems",
                @"CREATE TABLE OrderItems (
                    Id AUTOINCREMENT PRIMARY KEY,
                    OrderId INTEGER NOT NULL,
                    ProductName TEXT(255) NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice CURRENCY NOT NULL,
                    TotalPrice CURRENCY NOT NULL
                )");
        }

        private static void ExecuteIfTableNotExists(OleDbConnection conn, string tableName, string createSql)
        {
            // Access'te tablo var mı kontrol et
            var schema = conn.GetSchema("Tables");
            foreach (System.Data.DataRow row in schema.Rows)
            {
                if (row["TABLE_NAME"].ToString()!.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return; // Tablo zaten var
                }
            }

            using var cmd = new OleDbCommand(createSql, conn);
            cmd.ExecuteNonQuery();
        }

        private static void SeedDefaultProducts()
        {
            using var conn = new OleDbConnection(ConnectionString);
            conn.Open();

            // Ürün var mı kontrol et
            using var countCmd = new OleDbCommand("SELECT COUNT(*) FROM Products", conn);
            int count = (int)countCmd.ExecuteScalar();

            if (count > 0)
                return; // Zaten ürünler mevcut, seed yapma

            var defaultProducts = new[]
            {
                ("Çiğköfte Dürüm", 45m, "Yiyecekler"),
                ("Acılı Dürüm", 50m, "Yiyecekler"),
                ("Soslu Dürüm", 55m, "Yiyecekler"),
                ("Porsiyon 200g", 60m, "Yiyecekler"),
                ("Porsiyon 300g", 85m, "Yiyecekler"),
                ("Ayran", 15m, "İçecekler"),
                ("Şalgam", 18m, "İçecekler"),
                ("Kola", 25m, "İçecekler"),
                ("Ekstra Sos", 5m, "Soslar"),
                ("Nar Ekşisi", 8m, "Soslar")
            };

            foreach (var (name, price, category) in defaultProducts)
            {
                using var cmd = new OleDbCommand(
                    "INSERT INTO Products ([Name], Price, Category) VALUES (?, ?, ?)", conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Price", price);
                cmd.Parameters.AddWithValue("@Category", category);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
