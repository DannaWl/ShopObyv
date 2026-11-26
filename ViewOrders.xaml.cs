using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ShopObyv.Models;
using Microsoft.EntityFrameworkCore;

namespace ShopObyv.View
{
    /// <summary>
    /// Логика взаимодействия для ViewOrders.xaml
    /// </summary>
    public partial class ViewOrders : Page
    {
        AppDbContextclear db;

        public ViewOrders()
        {
            InitializeComponent();
            LoadData();
        }
           private void LoadData()
        {
            using (var db = new AppDbContextclear())
            {
                OrderTovars.ItemsSource = db.SostavZakazas
                      .Include(s => s.IdSostavaZakazaNavigation)
                          .ThenInclude(z => z.PvzNavigation)
                      .Include(s => s.ArticleZakaaNavigation)
                      .ToList();
            }
        }
        private void ViewOrdersDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (OrderTovars.SelectedItem is SostavZakaza selectedSostav)
            {
                // Передаем весь объект SostavZakaza, чтобы считывать и артикул, и заказ
                AddOrder addOrder = new AddOrder(selectedSostav);

                if (addOrder.ShowDialog() == true)
                {
                    LoadData();
                }
            }
        }
    }
}