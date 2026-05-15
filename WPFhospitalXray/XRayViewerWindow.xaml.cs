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
using BLL.DTOs.MedicalImages;

namespace WPFhospitalXray
{
    public partial class XRayViewerWindow : Window
    {
        private readonly IAIAnalyzerService _aiService;
        private readonly IDatasetService _datasetService;
        private readonly IAnalysisResultService _analysisResultService;
        private int? _currentAnalysisResultId;
        private readonly string _currentRole;

        // НОВЕ: Додаємо сервіс запитів та дані для бази
        private readonly IRetrainingRequestService _requestService;
        private readonly int _examinationId;
        private readonly string _currentUserId;

        private readonly IMedicalImageService _imageService;
        private readonly IRolePermissionService _rolePermissionService;

        private List<MedicalImageDto> _images = new List<MedicalImageDto>();
        private int _currentImageIndex = 0;

        private MedicalImageDto CurrentImage => _images[_currentImageIndex];
        private string CurrentImagePath => CurrentImage.FilePath;
        private int CurrentMedicalImageId => CurrentImage.Id;

        private BitmapImage _originalImage;
        private RenderTargetBitmap _aiImage;
        private List<Point> _polygonPoints = new List<Point>();

        private AnalysisReviewStatus? _currentAnalysisStatus;
        private List<FractureDetectionDto> _currentDetections = new List<FractureDetectionDto>();

        // НОВЕ: Оновлений конструктор. Тепер він приймає ID обстеження, ID лікаря та сервіс
        public XRayViewerWindow(
    int examinationId,
    string currentUserId,
    string currentRole,
    IMedicalImageService imageService,
    IAIAnalyzerService aiService,
    IDatasetService datasetService,
    IRetrainingRequestService requestService,
    IAnalysisResultService analysisResultService,
    IRolePermissionService rolePermissionService)
        {
            InitializeComponent();

            _examinationId = examinationId;
            _currentUserId = currentUserId;
            _currentRole = currentRole;

            _imageService = imageService;
            _aiService = aiService;
            _datasetService = datasetService;
            _requestService = requestService;
            _analysisResultService = analysisResultService;
            _rolePermissionService = rolePermissionService;

            ConfigureUiForRole();

            _ = LoadImagesAsync();
        }

        private bool CanWorkWithAi()
        {
            return _rolePermissionService.CanWorkWithAi(_currentRole);
        }

        private void ConfigureUiForRole()
        {
            btn_RunAI.Visibility = CanWorkWithAi() ? Visibility.Visible : Visibility.Collapsed;

            btn_ConfirmAI.Visibility = Visibility.Collapsed;
            btn_RejectAI.Visibility = Visibility.Collapsed;
            btn_StartManual.Visibility = Visibility.Collapsed;

            btn_ClearPoints.Visibility = Visibility.Collapsed;
            btn_SaveForRetrain.Visibility = Visibility.Collapsed;
            tb_LiveLabel.Visibility = Visibility.Collapsed;
            DrawingCanvas.Visibility = Visibility.Collapsed;

            cb_ShowMarkup.Visibility = Visibility.Collapsed;
        }

        private void LoadOriginalImage()
        {
            try
            {
                _originalImage = new BitmapImage();
                _originalImage.BeginInit();
                _originalImage.UriSource = new Uri(CurrentImagePath, UriKind.Absolute);
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
            if (!CanWorkWithAi())
            {
                MessageBox.Show("У вас немає прав для запуску AI-аналізу.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentAnalysisResultId != null)
            {
                MessageBox.Show(
                    "Для цього обстеження вже існує AI-аналіз. Повторний запуск недоступний після створення результату.",
                    "AI-аналіз уже існує",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            btn_RunAI.IsEnabled = false;
            btn_RunAI.Content = "⏳ Аналіз...";
            tb_AIResult.Text = "";
            tb_AIResult.Foreground = new SolidColorBrush(Color.FromRgb(82, 102, 129));

            try
            {
                List<FractureDetectionDto> detections = await _aiService.AnalyzeImageAsync(CurrentImagePath);

                var saveDto = new SaveAnalysisResultDto
                {
                    ExaminationId = _examinationId,
                    MedicalImageId = CurrentMedicalImageId,
                    UserId = _currentUserId,
                    ModelName = _aiService.ModelName,
                    ModelVersion = _aiService.ModelVersion,
                    ModelPath = _aiService.ModelPath,
                    Detections = detections ?? new List<FractureDetectionDto>()
                };

                _currentAnalysisResultId = await _analysisResultService.SaveAnalysisResultAsync(saveDto);
                _currentAnalysisStatus = AnalysisReviewStatus.Pending;
                _currentDetections = detections ?? new List<FractureDetectionDto>();
                UpdateReviewButtonsVisibility();

                

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
            if (!CanWorkWithAi())
            {
                MessageBox.Show("У вас немає прав для ручної розмітки знімка.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!IsAnalysisPending())
            {
                MessageBox.Show(
                    "Ручна розмітка доступна тільки для AI-результату, який очікує перевірки.",
                    "Дія недоступна",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
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
            if (!CanWorkWithAi())
            {
                MessageBox.Show("У вас немає прав для збереження розмітки для навчання.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsAnalysisPending())
            {
                MessageBox.Show("Рішення щодо цього AI-результату вже прийнято.");
                return;
            }

            if (_polygonPoints.Count < 3)
            {
                MessageBox.Show("Для полігону потрібно мінімум 3 точки!", "Увага");
                return;
            }

            try
            {
                btn_SaveForRetrain.IsEnabled = false;
                btn_SaveForRetrain.Content = "Збереження...";

                string finalLabel = tb_LiveLabel.Text;
                await _datasetService.SaveSegmentationDataAsync(CurrentImagePath, finalLabel);

                RetrainingRequestType requestType =
                    _currentDetections == null || _currentDetections.Count == 0
                        ? RetrainingRequestType.FalseNegative
                        : RetrainingRequestType.CorrectedPositive;

                string comment = requestType == RetrainingRequestType.FalseNegative
                    ? $"False Negative: ШІ не виявив перелом, але лікар вручну розмітив його. Точок полігону: {_polygonPoints.Count}"
                    : $"Corrected Positive: лікар скоригував локалізацію перелому. Точок полігону: {_polygonPoints.Count}";

                bool requestCreated = await _requestService.CreateRequestForImageAsync(
                        _examinationId,
                        CurrentMedicalImageId,
                        _currentUserId,
                        requestType,
                        comment);

                if (_currentAnalysisResultId != null)
                {
                    await _analysisResultService.UpdateStatusAsync(
                        _currentAnalysisResultId.Value,
                        AnalysisReviewStatus.Corrected,
                        "AI-результат скориговано лікарем через ручну розмітку.");

                    MarkAnalysisAsFinal(AnalysisReviewStatus.Corrected);
                }

                btn_ClearPoints_Click(null, null);

                DrawingCanvas.Visibility = Visibility.Collapsed;
                tb_LiveLabel.Visibility = Visibility.Collapsed;
                btn_ClearPoints.Visibility = Visibility.Collapsed;
                btn_SaveForRetrain.Visibility = Visibility.Collapsed;

                tb_AIResult.Text += "\nСтатус: скориговано лікарем.";

                if (requestCreated)
                {
                    MessageBox.Show(
                        "Дані успішно збережені та відправлені на перевірку Адміністратору!",
                        "Успіх",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Розмітку збережено, але активний запит на донавчання для цього обстеження вже існує.",
                        "Інформація",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
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
                var savedResult = await _analysisResultService.GetLatestByMedicalImageIdAsync(CurrentMedicalImageId);

                if (savedResult == null)
                {
                    _currentAnalysisResultId = null;
                    _currentAnalysisStatus = null;
                    _currentDetections = new List<FractureDetectionDto>();

                    UpdateReviewButtonsVisibility();
                    return;
                }

                _currentAnalysisResultId = savedResult.Id;
                _currentAnalysisStatus = savedResult.Status;
                _currentDetections = savedResult.Detections ?? new List<FractureDetectionDto>();

                if (savedResult.Detections == null || savedResult.Detections.Count == 0)
                {
                    tb_AIResult.Foreground = new SolidColorBrush(Colors.ForestGreen);
                    tb_AIResult.Text = $"Збережений AI-аналіз від {savedResult.AnalyzedAt:dd.MM.yyyy HH:mm}: патологій не виявлено. " +
                   $"Статус: {GetAnalysisStatusDisplayName(savedResult.Status)}.";
                    UpdateReviewButtonsVisibility();
                    return;
                }

                CreateAiImageWithMarkup(savedResult.Detections);

                cb_ShowMarkup.Visibility = Visibility.Visible;
                cb_ShowMarkup.IsChecked = true;

                UpdateReviewButtonsVisibility();

                var bestConfidence = savedResult.Detections.Max(d => d.Confidence);
                tb_AIResult.Foreground = new SolidColorBrush(Colors.DarkOrange);
                tb_AIResult.Text = $"Завантажено збережений AI-аналіз від {savedResult.AnalyzedAt:dd.MM.yyyy HH:mm}. " +
                   $"Виявлено: {savedResult.Detections.Count}. Найвища впевненість: {Math.Round(bestConfidence * 100, 1)}%. " +
                   $"Статус: {GetAnalysisStatusDisplayName(savedResult.Status)}.";
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
            if (!CanWorkWithAi())
            {
                MessageBox.Show("У вас немає прав для підтвердження AI-результату.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_currentAnalysisResultId == null)
            {
                MessageBox.Show("Немає збереженого AI-аналізу для підтвердження.");
                return;
            }

            if (!IsAnalysisPending())
            {
                MessageBox.Show("Рішення щодо цього AI-результату вже прийнято.");
                return;
            }

            await _analysisResultService.UpdateStatusAsync(
                _currentAnalysisResultId.Value,
                AnalysisReviewStatus.Confirmed,
                "Результат підтверджено лікарем.");

            MarkAnalysisAsFinal(AnalysisReviewStatus.Confirmed);

            tb_AIResult.Text += "\nСтатус: підтверджено лікарем.";

            MessageBox.Show("AI-результат підтверджено.");
        }

        private async void btn_RejectAI_Click(object sender, RoutedEventArgs e)
        {
            if (!CanWorkWithAi())
            {
                MessageBox.Show("У вас немає прав для підтвердження AI-результату.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_currentAnalysisResultId == null)
            {
                MessageBox.Show("Немає збереженого AI-аналізу для відхилення.");
                return;
            }

            if (!IsAnalysisPending())
            {
                MessageBox.Show("Рішення щодо цього AI-результату вже прийнято.");
                return;
            }

            if (_currentDetections == null || _currentDetections.Count == 0)
            {
                MessageBox.Show(
                    "Відхилення використовується для випадку, коли ШІ знайшов перелом, але його немає. " +
                    "Якщо ШІ не знайшов перелом, але він є, використайте ручну розмітку.",
                    "Немає AI-локалізацій",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            await _analysisResultService.UpdateStatusAsync(
                _currentAnalysisResultId.Value,
                AnalysisReviewStatus.Rejected,
                "AI-результат відхилено лікарем: модель помилково визначила перелом.");

            await _datasetService.SaveEmptyLabelAsync(CurrentImagePath);

            bool requestCreated = await _requestService.CreateRequestForImageAsync(
                    _examinationId,
                    CurrentMedicalImageId,
                    _currentUserId,
                    RetrainingRequestType.FalsePositive,
                    "False Positive: ШІ виявив перелом, але лікар підтвердив, що перелому немає. Створено порожній label-файл.");

            MarkAnalysisAsFinal(AnalysisReviewStatus.Rejected);

            tb_AIResult.Text += "\nСтатус: відхилено лікарем.";

            if (requestCreated)
            {
                MessageBox.Show(
                    "AI-результат відхилено. Випадок передано на донавчання як хибне виявлення перелому.",
                    "Успіх",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "AI-результат відхилено. Активний запит на донавчання для цього обстеження вже існує.",
                    "Інформація",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }










        private async Task LoadImagesAsync()
        {
            try
            {
                _images = await _imageService.GetImagesByExaminationIdAsync(_examinationId);

                if (_images == null || _images.Count == 0)
                {
                    MessageBox.Show(
                        "Для цього обстеження немає завантажених знімків.",
                        "Знімки відсутні",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    Close();
                    return;
                }

                _currentImageIndex = 0;

                await LoadCurrentImageAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка завантаження знімків обстеження:\n{ex.Message}",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LoadCurrentImageAsync()
        {
            ResetViewerStateForImage();

            LoadOriginalImage();

            await LoadSavedAnalysisAsync();

            UpdateImageNavigationUi();
        }

        private void ResetViewerStateForImage()
        {
            _currentAnalysisResultId = null;
            _currentAnalysisStatus = null;
            _currentDetections = new List<FractureDetectionDto>();
            _aiImage = null;

            _polygonPoints.Clear();
            DrawPolygon.Points.Clear();
            tb_LiveLabel.Text = "";

            var ellipses = DrawingCanvas.Children.OfType<System.Windows.Shapes.Ellipse>().ToList();
            foreach (var dot in ellipses)
            {
                DrawingCanvas.Children.Remove(dot);
            }

            DrawingCanvas.Visibility = Visibility.Collapsed;
            tb_LiveLabel.Visibility = Visibility.Collapsed;
            btn_ClearPoints.Visibility = Visibility.Collapsed;
            btn_SaveForRetrain.Visibility = Visibility.Collapsed;

            cb_ShowMarkup.IsChecked = false;
            cb_ShowMarkup.Visibility = Visibility.Collapsed;

            tb_AIResult.Text = "";
            tb_AIResult.Foreground = new SolidColorBrush(Color.FromRgb(82, 102, 129));

            UpdateReviewButtonsVisibility();
        }
       

        private void UpdateImageNavigationUi()
        {
            tb_ImageCounter.Text = $"Знімок {_currentImageIndex + 1} з {_images.Count}";

            btn_PreviousImage.IsEnabled = _currentImageIndex > 0;
            btn_NextImage.IsEnabled = _currentImageIndex < _images.Count - 1;

            if (_images.Count <= 1)
            {
                btn_PreviousImage.Visibility = Visibility.Collapsed;
                btn_NextImage.Visibility = Visibility.Collapsed;
                tb_ImageCounter.Visibility = Visibility.Collapsed;
            }
            else
            {
                btn_PreviousImage.Visibility = Visibility.Visible;
                btn_NextImage.Visibility = Visibility.Visible;
                tb_ImageCounter.Visibility = Visibility.Visible;
            }
        }

        private async void btn_PreviousImage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImageIndex <= 0)
            {
                return;
            }

            _currentImageIndex--;
            await LoadCurrentImageAsync();
        }

        private async void btn_NextImage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImageIndex >= _images.Count - 1)
            {
                return;
            }

            _currentImageIndex++;
            await LoadCurrentImageAsync();
        }





        private bool IsAnalysisPending()
        {
            return _currentAnalysisResultId != null &&
                   _currentAnalysisStatus == AnalysisReviewStatus.Pending;
        }

        private void UpdateReviewButtonsVisibility()
        {
            bool hasAnalysis = _currentAnalysisResultId != null;
            bool canReview = IsAnalysisPending() && CanWorkWithAi();

            btn_RunAI.Visibility = CanWorkWithAi() && !hasAnalysis
                ? Visibility.Visible
                : Visibility.Collapsed;

            btn_ConfirmAI.Visibility = canReview ? Visibility.Visible : Visibility.Collapsed;
            btn_RejectAI.Visibility = canReview ? Visibility.Visible : Visibility.Collapsed;
            btn_StartManual.Visibility = canReview ? Visibility.Visible : Visibility.Collapsed;

            if (!canReview)
            {
                btn_ClearPoints.Visibility = Visibility.Collapsed;
                btn_SaveForRetrain.Visibility = Visibility.Collapsed;
                tb_LiveLabel.Visibility = Visibility.Collapsed;
                DrawingCanvas.Visibility = Visibility.Collapsed;
            }
        }

        private string GetAnalysisStatusDisplayName(AnalysisReviewStatus status)
        {
            return status switch
            {
                AnalysisReviewStatus.Pending => "очікує перевірки лікарем",
                AnalysisReviewStatus.Confirmed => "підтверджено лікарем",
                AnalysisReviewStatus.Rejected => "відхилено лікарем",
                AnalysisReviewStatus.Corrected => "скориговано лікарем",
                _ => "невідомий статус"
            };
        }

        private void MarkAnalysisAsFinal(AnalysisReviewStatus status)
        {
            _currentAnalysisStatus = status;
            UpdateReviewButtonsVisibility();
        }
    }
}