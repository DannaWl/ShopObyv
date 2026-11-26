using ShopObyv.Models;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// Логика взаимодействия для Autorization.xaml
    /// </summary>
    public partial class Autorization : Window
    {
        public Autorization()
        {
            InitializeComponent();
            //AppDbContext db = new AppDbContext();
            //MessageBox.Show(db.Пользовательs.ToList().LastOrDefault().Фио);
        }

        private void Vhod(object sender, RoutedEventArgs e)
        {
            AppDbContextclear
                db = new AppDbContextclear();

            var users = db.Users.ToList();
            if (users != null)
            {
                var user = users.ToList().
                    Where(w => w.Login == login.Text
                    && w.Password == pass1.Password).
                    LastOrDefault();
                if (user != null)
                {
                    switch (user.Role)
                    {
                        case ("Администратор"):
                            Admin admin = new Admin(user);
                            admin.Show();
                            this.Close();
                            break;

                        case ("Менеджер"):
                            Manager
                                manager = new Manager();
                            manager.Show();
                            this.Close();
                            break;

                    }

                }
                else
                    MessageBox.Show("The login is incorrect.");
            }
            else
                MessageBox.Show("DB is broken");
        }



        private void VhodGhost(object sender, RoutedEventArgs e)
        {
            Gost
                gost = new Gost();
            gost.Show();
            this.Close();
        }

    }
}
