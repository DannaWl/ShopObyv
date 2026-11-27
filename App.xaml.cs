using QuestPDF.Infrastructure;
using ShopObyv.Models;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ShopObyv
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаём БД, если её ещё нет
            using (var context = new AppDbContextclear())
            {
                context.Database.EnsureCreated();
            }
        }
    }

}
