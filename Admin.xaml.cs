using ShopObyv.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfColors = QuestPDF.Helpers.Colors;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Collections;



namespace ShopObyv.View
{
    public partial class Admin : Window
    {
        private readonly AppDbContextclear db = new AppDbContextclear();
        private readonly User? _currentUser;
        private ViewTovar viewTovar;

        public Admin()
        {
            InitializeComponent();
            BtnAddTovar.Visibility = Visibility.Visible;
            BtnAddZakaz.Visibility = Visibility.Visible;

            viewTovar = new ViewTovar();
            FrameAdmin.Content = viewTovar;

            SortComboBox.Items.Add("Без сортировки");
            SortComboBox.Items.Add("по возрастанию");
            SortComboBox.Items.Add("по убыванию");
            SortComboBox.SelectedIndex = 0;

            FilterComboBox.Items.Add("Все поставщики");
            foreach (var s in db.Suppliers.ToList())
                FilterComboBox.Items.Add(s.IdSupplier);
            FilterComboBox.SelectedIndex = 0;
        }

        public Admin(User? currentUser = null) : this()
        {
            _currentUser = currentUser;
            if (!string.IsNullOrWhiteSpace(currentUser?.Fcs))
                CurrentUserLabel.Content = currentUser.Fcs;
        }

        private void SetAnalyticsFullScreen(bool isFullScreen)
        {
            var state = isFullScreen ? Visibility.Collapsed : Visibility.Visible;

            TitleLabel.Visibility = state;
            SubTitleLabel.Visibility = state;
            DescLabel.Visibility = state;
            FilterPanel.Visibility = state;
            TopBarPanel.Visibility = state;
            ActionButtonsPanel.Visibility = state;

            if (isFullScreen)
            {
                Grid.SetRow(FrameAdmin, 0);
                Grid.SetRowSpan(FrameAdmin, 5);
                Grid.SetColumn(FrameAdmin, 0);
                Grid.SetColumnSpan(FrameAdmin, 4);
                FrameAdmin.Margin = new Thickness(0);
            }
            else
            {
                Grid.SetRow(FrameAdmin, 4);
                Grid.SetRowSpan(FrameAdmin, 1);
                Grid.SetColumn(FrameAdmin, 0);
                Grid.SetColumnSpan(FrameAdmin, 4);
                FrameAdmin.Margin = new Thickness(5);
            }
        }

        public void ExitAnalyticsFullScreen()
        {
            SetAnalyticsFullScreen(false);
            BtnAddTovar.Visibility = Visibility.Visible;
            BtnAddZakaz.Visibility = Visibility.Collapsed;
            FrameAdmin.Content = viewTovar;
        }

        private void ButtonClickAnalytics(object sender, RoutedEventArgs e)
        {
            SetAnalyticsFullScreen(true);
            FrameAdmin.Navigate(new ViewAnalytics(_currentUser));
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

        private void ButtonClickAddTovar(object sender, RoutedEventArgs e)
        {
            AddTovar addTovar = new AddTovar();
            addTovar.ShowDialog();
        }

        private void ButtonClickZakaz(object sender, RoutedEventArgs e)
        {
            SetAnalyticsFullScreen(false);
            FrameAdmin.Navigate(new ViewOrders());
            BtnAddZakaz.Visibility = Visibility.Visible;
            BtnAddTovar.Visibility = Visibility.Collapsed;
        }

        private void ButtonClickTovar(object sender, RoutedEventArgs e)
        {
            SetAnalyticsFullScreen(false);
            viewTovar = new ViewTovar();
            FrameAdmin.Navigate(viewTovar);
            BtnAddTovar.Visibility = Visibility.Visible;
            BtnAddZakaz.Visibility = Visibility.Collapsed;
        }

        private void ButtonClickAddZakaz(object sender, RoutedEventArgs e)
        {
            AddOrder addOrder = new AddOrder();
            addOrder.ShowDialog();
        }

        private void ButtonClickPrintTovarPdf(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF документы (*.pdf)|*.pdf",
                FileName = $"Список_товаров_{DateTime.Now:yyyy-MM-dd}.pdf",
                DefaultExt = ".pdf",
                AddExtension = true
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                List<Tovar> allProducts = GetCurrentProductsList();

                // Для небольшого списка (до 80 товаров) рендерим визуальные карточки как скриншоты;
                // для больших списков (сотни и тысячи товаров) строим быстрый многостраничный PDF без зависаний
                List<byte[]>? cardScreenshots = allProducts.Count <= 80
                    ? TryCaptureProductCards(FrameAdmin, allProducts.Count)
                    : null;

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(14);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(col =>
                        {
                            col.Item().Text($"Список товаров (всего: {allProducts.Count})").FontSize(15).Bold();
                            col.Item().Text($"Сформировал: {CurrentUserLabel.Content} ({_currentUser?.Role ?? "Администратор"}) | Дата: {DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(9).FontColor(PdfColors.Grey.Darken1);
                            col.Item().PaddingTop(4).LineHorizontal(0.8f).LineColor(PdfColors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(6).AlignTop().Column(col =>
                        {
                            col.Spacing(6);

                            if (cardScreenshots != null && cardScreenshots.Count > 0)
                            {
                                foreach (var img in cardScreenshots)
                                {
                                    col.Item().AlignTop().Image(img).FitWidth();
                                }
                            }
                            else
                            {
                                foreach (var item in allProducts)
                                {
                                    string priceInfo = item.Discount > 0 && item.NewPrice.HasValue
                                        ? $"Цена: {item.Price:0.##} -> {item.NewPrice.Value:0.##} (скидка {item.Discount}%)"
                                        : $"Цена: {item.Price:0.##} (скидка {item.Discount}%)";

                                    col.Item().Border(0.8f).BorderColor(PdfColors.Grey.Lighten1).Padding(6).Column(card =>
                                    {
                                        card.Item().Text($"{item.NameTovar} | Артикул: {item.Article}").Bold();
                                        if (!string.IsNullOrWhiteSpace(item.Description))
                                            card.Item().Text($"Описание товара: {item.Description}").FontSize(9);
                                        card.Item().Text($"Производитель: {item.Manufacture ?? "-"} | Поставщик: {item.Supplier ?? "-"}").FontSize(9);
                                        card.Item().Text($"{priceInfo} | Единица измерения: {item.Unit ?? "-"} | На складе: {item.UnitStorage}").FontSize(9);
                                    });
                                    byte[]? photoBytes = TryGetProductPhotoBytes(item);

                                    col.Item().Border(0.8f).BorderColor(PdfColors.Grey.Lighten1).Padding(6).Row(row =>
                                    {
                                        if (photoBytes != null && photoBytes.Length > 0)
                                        {
                                            row.ConstantItem(75).Height(65).AlignCenter().AlignMiddle()
                                                .Image(photoBytes).FitArea();
                                            row.ConstantItem(10);
                                        }

                                        row.RelativeItem().Column(card =>
                                        {
                                            card.Item().Text($"{item.NameTovar} | Артикул: {item.Article}").Bold();
                                            if (!string.IsNullOrWhiteSpace(item.Description))
                                                card.Item().Text($"Описание товара: {item.Description}").FontSize(9);
                                            card.Item().Text($"Производитель: {item.Manufacture ?? "-"} | Поставщик: {item.Supplier ?? "-"}").FontSize(9);
                                            card.Item().Text($"{priceInfo} | Единица измерения: {item.Unit ?? "-"} | На складе: {item.UnitStorage}").FontSize(9);
                                        });
                                    });
                                }
                            }
                        });
                    });
                }).GeneratePdf(dialog.FileName);

                MessageBox.Show(
                    $"PDF-файл сохранён:\n{dialog.FileName}",
                    "Печать PDF",
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


        private static byte[]? TryGetProductPhotoBytes(Tovar item)
        {
            if (item == null)
                return null;

            string rawImage = item.Image?.Trim() ?? string.Empty;
            string fileName = string.IsNullOrWhiteSpace(rawImage)
                ? "picture.png"
                : Path.GetFileName(rawImage.Replace('/', '\\'));

            string[] roots =
            {
        AppDomain.CurrentDomain.BaseDirectory,
        Directory.GetCurrentDirectory(),
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..")),
        Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."))
    };

            foreach (var root in roots.Distinct())
            {
                string[] candidates =
                {
            Path.Combine(root, "image", fileName),
            Path.Combine(root, "Images", fileName),
            Path.Combine(root, "image", "picture.png"),
            Path.Combine(root, fileName)
        };

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                        return File.ReadAllBytes(path);
                }
            }

            string[] resourceUris =
            {
        $"pack://application:,,,/image/{fileName}",
        $"pack://application:,,,/Images/{fileName}",
        $"pack://application:,,,/image/picture.png",
        $"/image/{fileName}",
        $"/image/picture.png"
    };

            foreach (var uriStr in resourceUris)
            {
                try
                {
                    var info = Application.GetResourceStream(new Uri(uriStr, UriKind.RelativeOrAbsolute));
                    if (info?.Stream != null)
                    {
                        using var ms = new MemoryStream();
                        info.Stream.CopyTo(ms);
                        if (ms.Length > 0)
                            return ms.ToArray();
                    }
                }
                catch
                {
                    // Игнорируем ненайденный URI ресурса
                }
            }

            return null;
        }
        private List<Tovar> GetCurrentProductsList()
         {
           return db.Tovars.AsNoTracking().ToList()
    .GroupBy(t => t.Article)
    .Select(g => g.First())
    .ToList();
        }


        private static List<byte[]>? TryCaptureProductCards(DependencyObject root, int expectedCount)
        {
            var itemsControl = FindVisualChild<ItemsControl>(root);
            if (itemsControl == null)
                return null;

            var result = new List<byte[]>();

            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement container)
                {
                    container.UpdateLayout();
                    var png = CaptureElementToPng(container);
                    if (png.Length > 0)
                        result.Add(png);
                }
            }

            return result.Count == expectedCount ? result : null;
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed)
                    return typed;

                var found = FindVisualChild<T>(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static byte[] CaptureElementToPng(FrameworkElement element)
        {
            int width = Math.Max(1, (int)Math.Ceiling(element.ActualWidth));
            int height = Math.Max(1, (int)Math.Ceiling(element.ActualHeight));

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }

    }
}