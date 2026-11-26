using Microsoft.EntityFrameworkCore;
using ShopObyv.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShopObyv.View
{
    public partial class ViewAnalytics : Page
    {
        private readonly AppDbContextclear _db = new AppDbContextclear();
        private bool _isInitialized;
        private List<TopModelChartRow> _currentTop5 = new();

        public ViewAnalytics(User? currentUser = null)
        {
            InitializeComponent();

            UserInfoTextBlock.Text = string.IsNullOrWhiteSpace(currentUser?.Fcs)
                ? "Администратор"
                : currentUser.Fcs;
            UserRoleTextBlock.Text = $"Роль: {currentUser?.Role ?? "Администратор"}";

            var now = DateTime.Today;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            StartDatePicker.SelectedDate = firstDay;
            EndDatePicker.SelectedDate = lastDay;

            _isInitialized = true;
            Recalculate();
        }

        private void Period_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            Recalculate();
        }

        private void Recalculate()
        {
            if (StartDatePicker.SelectedDate is not DateTime start ||
                EndDatePicker.SelectedDate is not DateTime end)
            {
                ClearMetrics();
                return;
            }

            var from = start.Date;
            var toExclusive = end.Date.AddDays(1);

            if (toExclusive <= from)
            {
                ClearMetrics();
                return;
            }

            var orderIds = _db.Zakazs
                .AsNoTracking()
                .Where(z => z.DateOrder != null && z.DateOrder >= from && z.DateOrder < toExclusive)
                .Select(z => z.NomZakaza)
                .Distinct()
                .ToList();

            var rows = (from s in _db.SostavZakazas.AsNoTracking()
                        join t in _db.Tovars.AsNoTracking() on s.ArticleZakaa equals t.Article
                        where orderIds.Contains(s.IdSostavaZakaza)
                        select new
                        {
                            ModelName = t.NameTovar,
                            Manufacturer = t.Manufacture,
                            Qty = s.KolvoVzakaze,
                            Price = t.Price,
                            Discount = t.Discount
                        }).ToList();

            int totalOrdersCount = orderIds.Count;
            decimal totalAmount = rows.Sum(x => x.Qty * x.Price * (100m - x.Discount) / 100m);

            var top5Raw = rows
                .GroupBy(x => new { x.ModelName, x.Manufacturer })
                .Select(g => new
                {
                    ModelName = g.Key.ModelName,
                    Manufacturer = g.Key.Manufacturer,
                    OrderedPairs = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.OrderedPairs)
                .ThenBy(x => x.ModelName)
                .Take(5)
                .ToList();

            int maxPairs = top5Raw.Count > 0 ? top5Raw.Max(x => x.OrderedPairs) : 1;
            const double maxBarWidth = 260.0;

            _currentTop5 = top5Raw.Select((item, idx) => new TopModelChartRow
            {
                Rank = idx + 1,
                ModelName = item.ModelName,
                Manufacturer = item.Manufacturer,
                OrderedPairs = item.OrderedPairs,
                BarWidth = maxPairs > 0 ? Math.Max(18.0, (double)item.OrderedPairs / maxPairs * maxBarWidth) : 0
            }).ToList();

            int manufacturersCount = _currentTop5
                .Select(x => x.Manufacturer)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            TopManufacturersText.Text = manufacturersCount.ToString();
            TotalOrdersText.Text = totalOrdersCount.ToString();
            TotalAmountText.Text = totalAmount.ToString("C0", CultureInfo.GetCultureInfo("ru-RU"));

            TopModelsGrid.ItemsSource = _currentTop5;
            ChartItemsControl.ItemsSource = _currentTop5;
        }

        private void ClearMetrics()
        {
            _currentTop5 = new List<TopModelChartRow>();
            TopManufacturersText.Text = "0";
            TotalOrdersText.Text = "0";
            TotalAmountText.Text = "0 ₽";
            TopModelsGrid.ItemsSource = null;
            ChartItemsControl.ItemsSource = null;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Admin adminWindow)
            {
                adminWindow.ExitAnalyticsFullScreen();
                return;
            }

            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        private sealed class TopModelChartRow
        {
            public int Rank { get; set; }
            public string ModelName { get; set; } = string.Empty;
            public string Manufacturer { get; set; } = string.Empty;
            public int OrderedPairs { get; set; }
            public double BarWidth { get; set; }
        }
    }
}