using BLL.DTOs;
using BLL.DTOs.AnalysisResults;
using BLL.DTOs.FractureDetections;
using BLL.Interface;
using DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFhospitalXray
{
    public partial class XRayViewerWindow : Window
    {
        private readonly IAIAnalyzerService _aiService;
        private readonly IDatasetService _datasetService;
        private readonly IAnalysisResultService _analysisResultService;
        private int? _currentAnalysisResultId;

        // НОВЕ: Додаємо сервіс запитів та дані для бази
        private readonly IRetrainingRequestService _requestService;
        private readonly int _examinationId;
        private readonly string _currentUserId;

        private readonly string _imagePath;

        private BitmapImage _originalImage;
        private RenderTargetBitmap _aiImage;
        private List<Point> _polygonPoints = new List<Point>();

        // НОВЕ: Оновлений конструктор. Тепер він приймає ID обстеження, ID лікаря та сервіс
        public XRayViewerWindow(string imagePath, int examinationId, string currentUserId,
                                IAIAnalyzerService aiService, IDatasetService datasetService,
                                IRetrainingRequestService requestService, IAnalysisResultService analysisResultService)
        {
            InitializeComponent();

            _imagePath = imagePath;
            _examinationId = examinationId;
            _currentUserId = currentUserId;

            _aiService = aiService;
            _datasetService = datasetService;
            _requestService = requestService;



            LoadOriginalImage();

            _analysisResultService = analysisResultService;
            _ = LoadSavedAnalysisAsync();
        }

        private void LoadOriginalImage()
        {
            try
            {
                _originalImage = new BitmapImage();
                _originalImage.BeginInit();
                _originalImage.UriSource = new Uri(_imagePath, UriKind.Absolute);
                _originalImage.CacheOption = BitmapCacheOption.OnLoad;
                _originalImage.EndInit();

                img_Viewer.Source = _originalImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження знімка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btn_RunAI_Click(object sender, RoutedEventArgs e)
        {
            btn_RunAI.IsEnabled = false;
            btn_RunAI.Content = "⏳ Аналіз...";
            tb_AIResult.Text = "";
            tb_AIResult.Foreground = new SolidColorBrush(Color.FromRgb(82, 102, 129));

            try
            {
                List<FractureDetectionDto> detections = await _aiService.AnalyzeImageAsync(_imagePath);

                var saveDto = new SaveAnalysisResultDto
                {
                    ExaminationId = _examinationId,
                    UserId = _currentUserId,
                    ModelName = "YOLOv8 fracture detector",
                    ModelVersion = "best1",
                    ModelPath = @"Models\best1.onnx",
                    Detections = detections ?? new List<FractureDetectionDto>()
                };

                _currentAnalysisResultId = await _analysisResultService.SaveAnalysisResultAsync(saveDto);

                btn_ConfirmAI.Visibility = Visibility.Visible;
                btn_RejectAI.Visibility = Visibility.Visible;

                if (detections == null || detections.Count == 0)
                {
                    tb_AIResult.Foreground = new SolidColorBrush(Colors.ForestGreen);
                    tb_AIResult.Text = "🤖 Патологій не виявлено. Знімок чистий.";
                    cb_ShowMarkup.Visibility = Visibility.Collapsed;
                    return;
                }

                tb_AIResult.Foreground = new SolidColorBrush(Colors.DarkOrange);
                string confidenceStr = Math.Round(detections.First().Confidence * 100, 1).ToString();

                if (detections.Count == 1)
                    tb_AIResult.Text = $"⚠️ Виявлено перелом! (Впевненість ШІ: {confidenceStr}%)";
                else
                    tb_AIResult.Text = $"⚠️ Виявлено {detections.Count} ділянки(ок) перелому. (Найвища впевненість: {confidenceStr}%)";

                CreateAiImageWithMarkup(detections);

                cb_ShowMarkup.Visibility = Visibility.Visible;
                cb_ShowMarkup.IsChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка нейромережі: {ex.Message}", "Помилка ШІ", MessageBoxButton.OK, MessageBoxImage.Error);
                tb_AIResult.Text = "Помилка аналізу";
            }
            finally
            {
                btn_RunAI.IsEnabled = true;
                btn_RunAI.Content = "🤖 Аналіз ШІ";
            }
        }

        private void CreateAiImageWithMarkup(List<FractureDetectionDto> detections)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawImage(_originalImage, new Rect(0, 0, _originalImage.PixelWidth, _originalImage.PixelHeight));

                Pen boxPen = new Pen(Brushes.DeepSkyBlue, 4);
                Typeface typeface = new Typeface("Arial");

                foreach (var det in detections)
                {
                    Rect rect = new Rect(det.X, det.Y, det.Width, det.Height);
                    dc.DrawRectangle(null, boxPen, rect);

                    string label = $"{det.ClassName} {Math.Round(det.Confidence * 100)}%";
                    FormattedText fText = new FormattedText(
                        label,
                        System.Globalization.CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        typeface, 24, Brushes.Red,
                        VisualTreeHelper.GetDpi(visual).PixelsPerDip);

                    dc.DrawText(fText, new Point(det.X, Math.Max(0, det.Y - 30)));
                }
            }

            _aiImage = new RenderTargetBitmap(_originalImage.PixelWidth, _originalImage.PixelHeight, 96, 96, PixelFormats.Pbgra32);
            _aiImage.Render(visual);
        }

        private void cb_ShowMarkup_Checked(object sender, RoutedEventArgs e)
        {
            if (_aiImage != null) img_Viewer.Source = _aiImage;
        }

        private void cb_ShowMarkup_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_originalImage != null) img_Viewer.Source = _originalImage;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_polygonPoints != null && _polygonPoints.Count > 0)
            {
                btn_ClearPoints_Click(null, null);
                tb_AIResult.Text = "⚠️ Розмір вікна змінено. Координати збилися. Будь ласка, обведіть перелом заново.";
                tb_AIResult.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void btn_StartManual_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Visibility = Visibility.Visible;
            tb_LiveLabel.Visibility = Visibility.Visible;
            btn_ClearPoints.Visibility = Visibility.Visible;
            btn_SaveForRetrain.Visibility = Visibility.Visible;
            btn_StartManual.Visibility = Visibility.Collapsed;

            tb_AIResult.Text = "Клікайте по контуру перелому (мінімум 3 точки)";
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(DrawingCanvas);
            _polygonPoints.Add(clickPoint);
            DrawPolygon.Points.Add(clickPoint);

            System.Windows.Shapes.Ellipse dot = new System.Windows.Shapes.Ellipse
            {
                Width = 6,
                Height = 6,
                Fill = Brushes.Yellow,
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };
            Canvas.SetLeft(dot, clickPoint.X - 3);
            Canvas.SetTop(dot, clickPoint.Y - 3);
            DrawingCanvas.Children.Add(dot);

            UpdateLiveLabel();
        }

        private void UpdateLiveLabel()
        {
            if (_polygonPoints.Count == 0) return;

            double canvasW = DrawingCanvas.ActualWidth;
            double canvasH = DrawingCanvas.ActualHeight;
            double imgW = _originalImage.PixelWidth;
            double imgH = _originalImage.PixelHeight;

            double ratio = Math.Min(canvasW / imgW, canvasH / imgH);
            double offsetX = (canvasW - imgW * ratio) / 2;
            double offsetY = (canvasH - imgH * ratio) / 2;

            string yoloString = "0";

            foreach (var p in _polygonPoints)
            {
                double actualX = (p.X - offsetX) / ratio;
                double actualY = (p.Y - offsetY) / ratio;

                double normX = Math.Clamp(actualX / imgW, 0.0, 1.0);
                double normY = Math.Clamp(actualY / imgH, 0.0, 1.0);

                yoloString += $" {normX.ToString(System.Globalization.CultureInfo.InvariantCulture)} {normY.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            }

            tb_LiveLabel.Text = yoloString;
        }

        private void btn_ClearPoints_Click(object sender, RoutedEventArgs e)
        {
            _polygonPoints.Clear();
            DrawPolygon.Points.Clear();
            tb_LiveLabel.Text = "";

            var ellipses = DrawingCanvas.Children.OfType<System.Windows.Shapes.Ellipse>().ToList();
            foreach (var dot in ellipses)
            {
                DrawingCanvas.Children.Remove(dot);
            }
        }

        // НОВЕ: Оновлена логіка збереження
        private async void btn_SaveForRetrain_Click(object sender, RoutedEventArgs e)
        {
            if (_polygonPoints.Count < 3)
            {
                MessageBox.Show("Для полігону потрібно мінімум 3 точки!", "Увага");
                return;
            }

            try
            {
                btn_SaveForRetrain.IsEnabled = false;
                btn_SaveForRetrain.Content = "Збереження...";

                // 1. Зберігаємо текстовий файл (.txt)
                string finalLabel = tb_LiveLabel.Text;
                await _datasetService.SaveSegmentationDataAsync(_imagePath, finalLabel);

                // 2. Створюємо запит у базі даних (для Адміна)
                string comment = $"Ручна розмітка. Клас: 0 (Перелом). Точок полігону: {_polygonPoints.Count}";
                await _requestService.CreateRequestAsync(_examinationId, _currentUserId, comment);

                MessageBox.Show("Дані успішно збережені та відправлені на перевірку Адміністратору!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                btn_ClearPoints_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження: {ex.Message}");
            }
            finally
            {
                btn_SaveForRetrain.IsEnabled = true;
                btn_SaveForRetrain.Content = "💾 Зберегти для навчання";
            }
        }

        private async Task LoadSavedAnalysisAsync()
        {
            try
            {
                var savedResult = await _analysisResultService.GetLatestByExaminationIdAsync(_examinationId);

                if (savedResult == null)
                {
                    return;
                }

                _currentAnalysisResultId = savedResult.Id;

                if (savedResult.Detections == null || savedResult.Detections.Count == 0)
                {
                    tb_AIResult.Foreground = new SolidColorBrush(Colors.ForestGreen);
                    tb_AIResult.Text = $"Збережений AI-аналіз від {savedResult.AnalyzedAt:dd.MM.yyyy HH:mm}: патологій не виявлено.";
                    return;
                }

                CreateAiImageWithMarkup(savedResult.Detections);

                cb_ShowMarkup.Visibility = Visibility.Visible;
                cb_ShowMarkup.IsChecked = true;

                btn_ConfirmAI.Visibility = Visibility.Visible;
                btn_RejectAI.Visibility = Visibility.Visible;

                var bestConfidence = savedResult.Detections.Max(d => d.Confidence);
                tb_AIResult.Foreground = new SolidColorBrush(Colors.DarkOrange);
                tb_AIResult.Text = $"Завантажено збережений AI-аналіз від {savedResult.AnalyzedAt:dd.MM.yyyy HH:mm}. " +
                                   $"Виявлено: {savedResult.Detections.Count}. Найвища впевненість: {Math.Round(bestConfidence * 100, 1)}%. " +
                                   $"Статус: {savedResult.Status}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження збереженого AI-аналізу:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// НОВЕ: Додаємо можливість лікарю підтвердити або відхилити результат AI-аналізу. Це оновить статус у базі даних, і Адмін зможе бачити ці зміни при перегляді запитів на перенавчання.
        private async void btn_ConfirmAI_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAnalysisResultId == null)
            {
                MessageBox.Show("Немає збереженого AI-аналізу для підтвердження.");
                return;
            }

            await _analysisResultService.UpdateStatusAsync(
                _currentAnalysisResultId.Value,
                AnalysisReviewStatus.Confirmed,
                "Результат підтверджено лікарем.");

            MessageBox.Show("AI-результат підтверджено.");
        }

        private async void btn_RejectAI_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAnalysisResultId == null)
            {
                MessageBox.Show("Немає збереженого AI-аналізу для відхилення.");
                return;
            }

            await _analysisResultService.UpdateStatusAsync(
                _currentAnalysisResultId.Value,
                AnalysisReviewStatus.Rejected,
                "Результат відхилено лікарем.");

            MessageBox.Show("AI-результат відхилено.");
        }
    }
}