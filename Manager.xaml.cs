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
using System.Windows.Shapes;

namespace ShopObyv.View
{
    /// <summary>
    /// Логика взаимодействия для Manager.xaml
    /// </summary>
    public partial class Manager : Window
    {
        private ViewTovar viewTovar;
        private User? _currentUser;
        private AppDbContextclear db;

        public Manager()
        {
            InitializeComponent();

            db = new AppDbContextclear();

            viewTovar = new ViewTovar();
            FrameManager.Content = viewTovar;
            SortComboBox.Items.Add("Без сортировки");
            SortComboBox.Items.Add("по возрастанию");
            SortComboBox.Items.Add("по убыванию");
            SortComboBox.SelectedIndex = 0;

            FilterComboBox.Items.Add("Все поставщики");
            foreach (var s in db.Suppliers.ToList())
                FilterComboBox.Items.Add(s.IdSupplier);
            FilterComboBox.SelectedIndex = 0;
        }

        public Manager(User? currentUser = null) : this()
        {
            _currentUser = currentUser;
            if (!string.IsNullOrWhiteSpace(currentUser?.Fcs))
                CurrentUserLabel.Content = currentUser.Fcs;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Autorization autorization = new Autorization();
            autorization.Show();
            this.Close();
        }

        private void Search_Changed(object sender, RoutedEventArgs e)
        {
            string search = SearchTextBox.Text;
            string? supplier = FilterComboBox.SelectedIndex > 0
                ? FilterComboBox.SelectedItem?.ToString()
                : null;
            int sortIndex = SortComboBox.SelectedIndex;

            viewTovar.ApplyFilter(search, supplier, sortIndex);
        }

        private void ButtonClickZakaz(object sender, RoutedEventArgs e)
        {
            FrameManager.Navigate(new ViewOrders());
        }

        private void ButtonClickTovar(object sender, RoutedEventArgs e)
        {
            viewTovar = new ViewTovar();
            FrameManager.Navigate(viewTovar);
        }
    }
}
