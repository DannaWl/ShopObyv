using ShopObyv.Models;
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

namespace ShopObyv.View
{
    /// <summary>
    /// Логика взаимодействия для ViewTovar.xaml
    /// </summary>
    public partial class ViewTovar : Page
    {
        private List<Tovar> allTovar;
        public ViewTovar()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            using (var db = new AppDbContextclear())
            {
                allTovar = db.Tovars.ToList();
            }
            ListBoxTovar.ItemsSource = allTovar;
        }

        public void ApplyFilter(string search, string supplier, int sortIndex)
        {
            var res = allTovar.AsEnumerable();

            if (!string.IsNullOrEmpty(supplier))
                res = res.Where(w => (w.Supplier ?? "") == supplier);

           if (!string.IsNullOrEmpty(search))
            {
                var parts = search.Split(';');
                if (!string.IsNullOrEmpty(parts[0]))
                    res = res.Where(w => (w.NameTovar ?? "").ToLower()
                                         .StartsWith(parts[0].ToLower()));
                if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                    res = res.Where(w => (w.Manufacture ?? "").ToLower()
                                         .StartsWith(parts[1].ToLower()));
            }

            if (sortIndex == 1)
                res = res.OrderBy(o => o.UnitStorage);
            else if (sortIndex == 2)
                res = res.OrderByDescending(o => o.UnitStorage);

            ListBoxTovar.ItemsSource = res.ToList();
        }

        private void ListBoxTovar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AddTovar addTovar = new AddTovar((Tovar?)ListBoxTovar.SelectedItem);
            addTovar.Show();
        }
    }
}