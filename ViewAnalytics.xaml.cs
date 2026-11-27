using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfColors = QuestPDF.Helpers.Colors;
using System;
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

        private void ButtonPrintToPdf_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate is not DateTime start ||
        EndDatePicker.SelectedDate is not DateTime end)
            {
                MessageBox.Show(
                    "Выберите период для экспорта.",
                    "Экспорт в PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var from = start.Date;
            var toExclusive = end.Date.AddDays(1);

            if (toExclusive <= from)
            {
                MessageBox.Show(
                    "Дата окончания периода должна быть больше даты начала.",
                    "Экспорт в PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "PDF документы (*.pdf)|*.pdf",
                FileName = $"Аналитика_заказы_{DateTime.Now:dd.MM.yyyy}.pdf",
                DefaultExt = ".pdf",
                AddExtension = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                // Состав заказов за период + сам заказ + адрес ПВЗ (через навигации)
                var items = _db.SostavZakazas
                    .AsNoTracking()
                    .Include(s => s.IdSostavaZakazaNavigation)
                        .ThenInclude(z => z.PvzNavigation)
                    .Where(s => s.IdSostavaZakazaNavigation.DateOrder != null
                             && s.IdSostavaZakazaNavigation.DateOrder >= from
                             && s.IdSostavaZakazaNavigation.DateOrder < toExclusive)
                    .OrderBy(s => s.IdSostavaZakazaNavigation.DateOrder)
                    .ThenBy(s => s.IdSostavaZakaza)
                    .ToList();

                string formedBy = UserInfoTextBlock.Text;
                string formedRole = UserRoleTextBlock.Text;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);           // портрет
                        page.Margin(14);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(col =>
                        {
                            col.Item().Text($"Аналитика заказов (всего: {items.Count})")
                                .FontSize(15).Bold();
                            col.Item().Text(
                                $"Период: {from:dd.MM.yyyy} — {end:dd.MM.yyyy} | " +
                                $"Сформировал: {formedBy} ({formedRole}) | " +
                                $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(9).FontColor(PdfColors.Grey.Darken1);
                            col.Item().PaddingTop(4)
                                .LineHorizontal(0.8f)
                                .LineColor(PdfColors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(6).Column(col =>
                        {
                            col.Spacing(6);

                            if (items.Count == 0)
                            {
                                col.Item().Text("Заказов за выбранный период не найдено.")
                                    .Italic().FontColor(PdfColors.Grey.Darken1);
                                return;
                            }

                            foreach (var item in items)
                            {
                                var z = item.IdSostavaZakazaNavigation;
                                var p = z.PvzNavigation;

                                col.Item().Row(row =>
                                {
                                    // Левая карточка
                                    row.RelativeItem()
                                        .Border(1f).BorderColor(PdfColors.Black)
                                        .Padding(8)
                                        .Column(card =>
                                        {
                                            card.Item().Text(txt =>
                                            {
                                                txt.Span("Артикул: ").Bold().FontSize(16);
                                                txt.Span(item.ArticleZakaa).Bold().FontSize(16);
                                            });
                                            card.Item().Text($"Статус: {z.Statys}");
                                            card.Item().Text(
                                                $"ПВЗ: {p.Gorod}, {p.Street}, {p.Dom}");
                                            card.Item().Text(
                                                $"Дата заказа: {z.DateOrder:dd.MM.yyyy}");
                                        });

                                    // Правая карточка «Дата доставки»
                                    row.ConstantItem(150)
                                        .Border(3f).BorderColor(PdfColors.Black)
                                        .Padding(8)
                                        .AlignMiddle()
                                        .Column(card =>
                                        {
                                            card.Item().AlignCenter().Text("Дата доставки");
                                            card.Item().AlignCenter()
                                                .Text($"{z.DarteDilivery:dd.MM.yyyy}")
                                                .FontSize(16);
                                        });
                                });
                            }
                        });

                        page.Footer().AlignRight().Text(txt =>
                        {
                            txt.Span("Стр. ");
                            txt.CurrentPageNumber();
                            txt.Span(" из ");
                            txt.TotalPages();
                        });
                    });
                }).GeneratePdf(dialog.FileName);

                MessageBox.Show(
                    $"PDF-файл сохранён:\n{dialog.FileName}",
                    "Экспорт в PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не удалось сформировать PDF: {ex.Message}\n\n" +
                    "Порядок действий:\n" +
                    "1. Закройте файл, если он уже открыт в другой программе.\n" +
                    "2. Выберите другую папку (например, «Документы») и повторите сохранение.",
                    "Ошибка экспорта в PDF",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}