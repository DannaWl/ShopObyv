using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using ShopObyv.Models;

namespace ShopObyv.View
{
    /// <summary>
    /// Логика взаимодействия для AddTovar.xaml
    /// </summary>
    public partial class AddTovar : Window
    {
        private Tovar? context;
        private readonly AppDbContextclear
            db = new AppDbContextclear();

        public AddTovar()
        {
            InitializeComponent();

            context = new Tovar();
            DataContext = context;
        }

        public AddTovar(Tovar? tovar)
        {
            InitializeComponent();

            this.context = tovar;
            DataContext = context;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   
        }

        private void ButtonClickSaveTovar(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(context.NameTovar) || context.Price <= 0)
            //{
            //    MessageBox.Show("Введите название товара и цену больше нуля!");
            //    return;
            //}

            context.Article = new Random().Next(1000000, 9999999).ToString();

            if (!string.IsNullOrWhiteSpace(context.Image))
            {
                string name = Path.GetFileName(context.Image);
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "image");
                Directory.CreateDirectory(folder); 

                string newPath = Path.Combine(folder, name);
                try
                {
                    File.Copy(context.Image, newPath, true);
                    context.Image = $@"\image\{name}";
                }
                catch { context.Image = @"\image\picture.png"; }
            }
            else
            {
                context.Image = @"\image\picture.png";
            }

            try
            {
                db.Tovars.Add(context);
                db.SaveChanges();
                MessageBox.Show("Сохранено!");
                this.Close(); 
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Ошибка сохранения: {errorMessage}");
            }
        }


        private void ButtonClickRemoveTovar(object sender, RoutedEventArgs e)
        {
            if (context != null)
            {
                // Проверяем прямо в базе данных, используется ли артикул товара в заказах
                bool isUsedInOrders = db.SostavZakazas.Any(s => s.ArticleZakaa == context.Article);

                if (isUsedInOrders)
                {
                    MessageBox.Show("Этот товар содержится в заказе, его нельзя удалить!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Если заказов нет — смело удаляем
                db.Tovars.Remove(context);
                db.SaveChanges();
                MessageBox.Show("Товар успешно удален!");
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CategoryComboBox.ItemsSource = db.Categories.Select(s => s.IdCategory).ToList();
            ManufactureComboBox.ItemsSource = db.Manufactres.Select(s => s.IdManufacture).ToList();
            SupplierComboBox.ItemsSource = db.Suppliers.Select(s => s.IdSupplier).ToList();
            UnitComboBox.ItemsSource = db.Units.Select(s => s.IdUnit).ToList();
        }
    }
}
