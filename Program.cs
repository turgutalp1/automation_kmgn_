using System;
using System.Windows.Forms;
using CigkofteOtomasyon.Data;
using CigkofteOtomasyon.Services;

namespace CigkofteOtomasyon
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Veritabanını başlat (yoksa oluştur, tabloları ve varsayılan ürünleri ekle)
            try
            {
                DatabaseHelper.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Veritabanı başlatılırken hata oluştu:\n\n{ex.Message}\n\n" +
                    "Microsoft Access Database Engine yüklü olduğundan emin olun.",
                    "Veritabanı Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var productService = new ProductService();
            var orderService = new OrderService();
            var selectionForm = new SelectionForm(productService, orderService);
            
            Application.Run(selectionForm);
        }
    }
}
