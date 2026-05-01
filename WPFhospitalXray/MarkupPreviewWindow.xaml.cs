using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFhospitalXray
{
    public partial class MarkupPreviewWindow : Window
    {
        private readonly string _imagePath;

        // Конструктор приймає шлях до оригінальної картинки
        public MarkupPreviewWindow(string imagePath)
        {
            InitializeComponent();
            _imagePath = imagePath;

            DrawImageWithMarkup();
        }

        private void DrawImageWithMarkup()
        {
            try
            {
                // 1. Завантажуємо оригінальне фото
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                // 2. Готуємо полотно для малювання
                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext dc = visual.RenderOpen())
                {
                    // Малюємо саме фото
                    dc.DrawImage(bitmap, new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));

                    // 3. Формуємо шлях до текстового файлу з розміткою (у карантині)
                    string tempLabelsFolder = @"D:\HospitalServer\TempLabels";
                    string txtFileName = Path.GetFileNameWithoutExtension(_imagePath) + ".txt";
                    string txtFilePath = Path.Combine(tempLabelsFolder, txtFileName);

                    // 4. Якщо файл існує - малюємо зелений контур
                    if (File.Exists(txtFilePath))
                    {
                        string yoloText = File.ReadAllText(txtFilePath).Trim();
                        string[] parts = yoloText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        // Формат YOLO: "Клас X1 Y1 X2 Y2 ...". Потрібно мінімум 1 клас + 3 точки (разом 7 елементів)
                        if (parts.Length >= 7)
                        {
                            StreamGeometry geometry = new StreamGeometry();
                            using (StreamGeometryContext ctx = geometry.Open())
                            {
                                bool isFirstPoint = true;

                                // Починаємо з індексу 1, бо індекс 0 - це клас (0 для перелому)
                                for (int i = 1; i < parts.Length; i += 2)
                                {
                                    // Перетворюємо нормалізовані координати назад у пікселі
                                    double normX = double.Parse(parts[i], CultureInfo.InvariantCulture);
                                    double normY = double.Parse(parts[i + 1], CultureInfo.InvariantCulture);

                                    double pixelX = normX * bitmap.PixelWidth;
                                    double pixelY = normY * bitmap.PixelHeight;

                                    Point pt = new Point(pixelX, pixelY);

                                    if (isFirstPoint)
                                    {
                                        ctx.BeginFigure(pt, true, true); // true, true = закрита фігура із заливкою
                                        isFirstPoint = false;
                                    }
                                    else
                                    {
                                        ctx.LineTo(pt, true, true);
                                    }
                                }
                            }

                            // Налаштування стилю ліній: зелена рамка і напівпрозора зелена заливка
                            Pen contourPen = new Pen(Brushes.LimeGreen, 4);
                            Brush fillBrush = new SolidColorBrush(Color.FromArgb(60, 50, 205, 50)); // 60 - це прозорість

                            dc.DrawGeometry(fillBrush, contourPen, geometry);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Файл розмітки не знайдено. Можливо, його вже видалили.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                // 5. Виводимо результат на екран
                RenderTargetBitmap finalImage = new RenderTargetBitmap(bitmap.PixelWidth, bitmap.PixelHeight, 96, 96, PixelFormats.Pbgra32);
                finalImage.Render(visual);

                img_Viewer.Source = finalImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при формуванні зображення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}