using System;
using System.Windows.Forms;
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

            var productService = new ProductService();
            var orderService = new OrderService();
            var selectionForm = new SelectionForm(productService, orderService);
            
            Application.Run(selectionForm);
        }
    }
}
