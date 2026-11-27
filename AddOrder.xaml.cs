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
using Microsoft.EntityFrameworkCore;
using ShopObyv.Models;

namespace ShopObyv.View
{
    public partial class AddOrder : Window
    {
        private Zakaz? context;
        private User? us;
        private SostavZakaza? currentSostav;
        private readonly AppDbContextclear db = new AppDbContextclear();

        public AddOrder(User? user)
        {
            InitializeComponent();
            LoadComboBoxes();

            context = new Zakaz
            {
                DateOrder = DateTime.Now,
                DarteDilivery = DateTime.Now.AddDays(3),
                Statys = "Новый",
                LoginUsers = user.Login
            };

            DataContext = context;
            OrderDatePicker.SelectedDate = context.DateOrder;
            DeliveryDatePicker.SelectedDate = context.DarteDilivery;

            SaveButton.Visibility = Visibility.Visible;
            UpdateButton.Visibility = Visibility.Collapsed;
        }

        public AddOrder(SostavZakaza? sostav)
        {
            InitializeComponent();
            LoadComboBoxes();

            if (sostav != null)
            {
                currentSostav = sostav;
                this.context = sostav.IdSostavaZakazaNavigation;

                DataContext = context;

                TovarArticleTextBox.Text = sostav.ArticleZakaa;
                if (context != null)
                {
                    OrderDatePicker.SelectedDate = context.DateOrder;
                    DeliveryDatePicker.SelectedDate = context.DarteDilivery;

                    if (!string.IsNullOrEmpty(context.Statys))
                    {
                        OrderStatusComboBox.SelectedValue = context.Statys;
                    }

                    PvzComboBox.SelectedValue = context.Pvz;
                }
            }

            SaveButton.Visibility = Visibility.Collapsed;
            UpdateButton.Visibility = Visibility.Visible;
        }

        private void LoadComboBoxes()
        {
            OrderStatusComboBox.ItemsSource = db.Statys.AsNoTracking().ToList();
            OrderStatusComboBox.DisplayMemberPath = "IdStatys";
            OrderStatusComboBox.SelectedValuePath = "IdStatys";

            PvzComboBox.ItemsSource = db.AdressPvzs.AsNoTracking().ToList();
            PvzComboBox.DisplayMemberPath = "Street";
            PvzComboBox.SelectedValuePath = "IdPvz";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ButtonClickSaveOrder(object sender, RoutedEventArgs e)
        {
            if (context == null) return;

            try
            {
                if (OrderDatePicker.SelectedDate.HasValue)
                    context.DateOrder = OrderDatePicker.SelectedDate.Value;

                if (DeliveryDatePicker.SelectedDate.HasValue)
                    context.DarteDilivery = DeliveryDatePicker.SelectedDate.Value;

                if (PvzComboBox.SelectedValue != null)
                {
                    context.Pvz = (int)PvzComboBox.SelectedValue;
                }
                else
                {
                    MessageBox.Show("Выберите пункт выдачи (ПВЗ)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OrderStatusComboBox.SelectedValue != null)
                {
                    context.Statys = OrderStatusComboBox.SelectedValue.ToString()!;
                }

                string newArticle = TovarArticleTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newArticle))
                {
                    var productExists = db.Tovars.AsNoTracking().Any(t => t.Article == newArticle);
                    if (!productExists)
                    {
                        MessageBox.Show("Товар с таким артикулом не найден в базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                context.PvzNavigation = null!;
                context.StatysNavigation = null!;
                context.LoginUsersNavigation = null!;

                db.Zakazs.Add(context);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(newArticle))
                {
                    db.Database.ExecuteSqlRaw(
                        "INSERT INTO sostavZakaza (idSostavaZakaza, articleZakaa, kolvoVzakaze) VALUES ({0}, {1}, {2})",
                        context.NomZakaza, newArticle, 1
                    );
                }

                MessageBox.Show("Заказ успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Ошибка сохранения: {errorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClickUpdateOrder(object sender, RoutedEventArgs e)
        {
            if (context == null) return;

            try
            {
                var zakazToUpdate = db.Zakazs.Find(context.NomZakaza);
                if (zakazToUpdate == null)
                {
                    MessageBox.Show("Заказ не найден в базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (OrderDatePicker.SelectedDate.HasValue)
                    zakazToUpdate.DateOrder = OrderDatePicker.SelectedDate.Value;

                if (DeliveryDatePicker.SelectedDate.HasValue)
                    zakazToUpdate.DarteDilivery = DeliveryDatePicker.SelectedDate.Value;

                if (PvzComboBox.SelectedValue != null)
                {
                    zakazToUpdate.Pvz = (int)PvzComboBox.SelectedValue;
                }
                else
                {
                    MessageBox.Show("Выберите пункт выдачи (ПВЗ)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (OrderStatusComboBox.SelectedValue != null)
                {
                    zakazToUpdate.Statys = OrderStatusComboBox.SelectedValue.ToString()!;
                }

                string newArticle = TovarArticleTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(newArticle))
                {
                    var productExists = db.Tovars.AsNoTracking().Any(t => t.Article == newArticle);
                    if (!productExists)
                    {
                        MessageBox.Show("Товар с таким артикулом не найден в базе данных!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (currentSostav != null && !string.IsNullOrEmpty(newArticle))
                {
                    if (currentSostav.ArticleZakaa != newArticle)
                    {
                        db.Database.ExecuteSqlRaw(
                            "DELETE FROM sostavZakaza WHERE idSostavaZakaza = {0} AND articleZakaa = {1}",
                            zakazToUpdate.NomZakaza, currentSostav.ArticleZakaa
                        );

                        db.Database.ExecuteSqlRaw(
                            "INSERT INTO sostavZakaza (idSostavaZakaza, articleZakaa, kolvoVzakaze) VALUES ({0}, {1}, {2})",
                            zakazToUpdate.NomZakaza, newArticle, currentSostav.KolvoVzakaze > 0 ? currentSostav.KolvoVzakaze : 1
                        );
                    }
                }

                db.SaveChanges();
                MessageBox.Show("Заказ успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Ошибка обновления: {errorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClickDeleteOrder(object sender, RoutedEventArgs e)
        {
            if (context == null || context.NomZakaza == 0)
            {
                this.Close();
                return;
            }

            var result = MessageBox.Show("Вы действительно хотите удалить этот заказ?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    db.Database.ExecuteSqlRaw(
                        "DELETE FROM sostavZakaza WHERE idSostavaZakaza = {0}",
                        context.NomZakaza
                    );

                    var zakazToDelete = db.Zakazs.Find(context.NomZakaza);
                    if (zakazToDelete != null)
                    {
                        db.Zakazs.Remove(zakazToDelete);
                        db.SaveChanges();
                    }

                    MessageBox.Show("Заказ успешно удален!");
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось удалить заказ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}