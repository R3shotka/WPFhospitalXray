using BLL.DTOs.RetrainingRequests;   // Додали using для наших DTO!
using BLL.DTOs.ModelVersions;
using BLL.Interface;
using DAL.Entity; // Для доступу до статусу (RetrainingRequestStatus)
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WPFhospitalXray
{
    public partial class RetrainManagerWindow : Window
    {
        private readonly IRetrainingRequestService _requestService;
        private readonly IDatasetService _datasetService;
        private readonly IApplicationPathService _pathService;
        private readonly IDatasetExportService _datasetExportService;
        private readonly IModelTrainingService _modelTrainingService;
        private readonly IModelVersionService _modelVersionService;

        public RetrainManagerWindow(
            IRetrainingRequestService requestService,
            IDatasetService datasetService,
            IApplicationPathService pathService,
            IDatasetExportService datasetExportService,
            IModelTrainingService modelTrainingService,
            IModelVersionService modelVersionService)
        {
            InitializeComponent();

            _requestService = requestService;
            _datasetService = datasetService;
            _pathService = pathService;
            _datasetExportService = datasetExportService;
            _modelTrainingService = modelTrainingService;
            _modelVersionService = modelVersionService;

            Loaded += RetrainManagerWindow_Loaded;
        }

        private async void RetrainManagerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRequestsAsync();
            await LoadModelVersionsAsync();
        }

        private async Task LoadRequestsAsync()
        {
            try
            {
                // Тепер це повертає List<RetrainingRequestDto>
                var allRequests = await _requestService.GetAllAsync();

                // Фільтруємо
                var activeRequests = allRequests
                    .Where(r => r.Status == RetrainingRequestStatus.Pending || r.Status == RetrainingRequestStatus.Processing)
                    .OrderByDescending(r => r.RequestedAt)
                    .ToList();

                dg_Requests.ItemsSource = activeRequests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження запитів:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadModelVersionsAsync()
        {
            try
            {
                var models = await _modelVersionService.GetAllAsync();

                dg_ModelVersions.ItemsSource = models;
                UpdateActivateButtonState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження версій моделей:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btn_Approve_Click(object sender, RoutedEventArgs e)
        {
            // ЗВЕРНИ УВАГУ: тепер ми перевіряємо на DTO!
            if (dg_Requests.SelectedItem is RetrainingRequestDto selectedReq)
            {
                if (selectedReq.Status == RetrainingRequestStatus.Processing)
                {
                    MessageBox.Show("Цей запит вже був схвалений раніше!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // ВИПРАВЛЕНО: Викликаємо правильний метод сервісу і передаємо ID та новий статус
                await _requestService.UpdateStatusAsync(selectedReq.Id, RetrainingRequestStatus.Processing);

                await LoadRequestsAsync();
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть запит із таблиці.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btn_Reject_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Requests.SelectedItem is RetrainingRequestDto selectedReq)
            {
                var result = MessageBox.Show("Ви впевнені, що хочете відхилити цей знімок?\nТимчасовий файл розмітки буде назавжди видалено.",
                                             "Відхилення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 1. Оновлюємо статус у базі даних
                        await _requestService.UpdateStatusAsync(selectedReq.Id, RetrainingRequestStatus.Cancelled);

                        // 2. Видаляємо фізичний .txt файл з папки TempLabels
                        // Переконайся, що у твоєму RetrainingRequestDto є поле ImagePath!
                        await _datasetService.DeleteTempLabelAsync(selectedReq.ImagePath);

                        // 3. Оновлюємо таблицю на екрані
                        await LoadRequestsAsync();

                        MessageBox.Show("Запит відхилено, а файл успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при відхиленні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть запит із таблиці.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btn_PreviewMarkup_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button?.DataContext is RetrainingRequestDto selectedReq)
            {
                // Перевіряємо, чи є шлях до знімка
                if (string.IsNullOrEmpty(selectedReq.ImagePath))
                {
                    MessageBox.Show("Шлях до знімка відсутній у базі даних.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Відкриваємо наше нове вікно перегляду
                string labelFileName = System.IO.Path.GetFileNameWithoutExtension(selectedReq.ImagePath) + ".txt";
                string labelPath = System.IO.Path.Combine(_pathService.TempLabelsFolder, labelFileName);

                MarkupPreviewWindow previewWindow = new MarkupPreviewWindow(selectedReq.ImagePath, labelPath);
                previewWindow.ShowDialog();
            }
        }

        private async void btn_StartRetrain_Click(object sender, RoutedEventArgs e)
        {
            var confirmation = MessageBox.Show(
                "Запустити локальне донавчання моделі на основі останнього сформованого датасету?\n\n" +
                "Цей процес може тривати довго і має виконуватися на адміністративному ПК або технічній машині.",
                "Локальне донавчання ШІ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                btn_StartRetrain.IsEnabled = false;
                btn_StartRetrain.Content = "Навчання...";

                var result = await _modelTrainingService.StartTrainingAsync();

                string message =
                    $"{result.Message}\n\n" +
                    $"Папка запуску: {result.TrainingRunPath}";

                if (result.ModelVersionId != null)
                {
                    message += $"\nID моделі-кандидата: {result.ModelVersionId}";
                }

                if (result.IsSuccess)
                {
                    await LoadModelVersionsAsync();
                }

                MessageBox.Show(
                    message,
                    result.IsSuccess ? "Донавчання завершено" : "Донавчання не запущено",
                    MessageBoxButton.OK,
                    result.IsSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка запуску донавчання:\n{ex.Message}",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btn_StartRetrain.IsEnabled = true;
                btn_StartRetrain.Content = "🚀 Запустити донавчання ШІ";
            }
        }

        private async void btn_RefreshModels_Click(object sender, RoutedEventArgs e)
        {
            await LoadModelVersionsAsync();
        }

        private async void btn_ActivateModel_Click(object sender, RoutedEventArgs e)
        {
            if (dg_ModelVersions.SelectedItem is not ModelVersionDto selectedModel)
            {
                MessageBox.Show("Будь ласка, виберіть модель із таблиці.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!CanActivateModel(selectedModel))
            {
                MessageBox.Show("Активувати можна тільки неактивну модель зі статусом Candidate або Archived.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmation = MessageBox.Show(
                $"Активувати вибрану модель?\n\nID: {selectedModel.Id}\nВерсія: {selectedModel.Version}\n\nПоточна активна модель стане архівною.",
                "Активація моделі",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmation != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                btn_ActivateModel.IsEnabled = false;

                await _modelVersionService.ActivateAsync(selectedModel.Id);
                await LoadModelVersionsAsync();

                MessageBox.Show(
                    "Модель активовано. Нові аналізи будуть використовувати цю версію.",
                    "Активація моделі",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка активації моделі:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateActivateButtonState();
            }
        }

        private void dg_ModelVersions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateActivateButtonState();
        }

        private void UpdateActivateButtonState()
        {
            if (btn_ActivateModel == null || dg_ModelVersions == null)
            {
                return;
            }

            btn_ActivateModel.IsEnabled =
                dg_ModelVersions.SelectedItem is ModelVersionDto selectedModel &&
                CanActivateModel(selectedModel);
        }

        private static bool CanActivateModel(ModelVersionDto model)
        {
            return !model.IsActive &&
                   (model.Status == ModelVersionStatus.Candidate ||
                    model.Status == ModelVersionStatus.Archived);
        }

        private async void btn_ExportDataset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btn_ExportDataset.IsEnabled = false;
                btn_ExportDataset.Content = "Формування...";

                var result = await _datasetExportService.ExportApprovedRequestsAsync();

                await LoadRequestsAsync();

                if (!result.IsSuccess)
                {
                    string warningMessage =
                        "Датасет не сформовано.\n\n" +
                        $"Схвалених запитів: {result.TotalRequests}\n" +
                        $"Придатних запитів: {result.ValidItems}\n" +
                        $"Пропущено: {result.SkippedItems}";

                    if (result.Warnings.Any())
                    {
                        warningMessage += "\n\nПричини:\n" + string.Join("\n", result.Warnings);
                    }

                    MessageBox.Show(
                        warningMessage,
                        "Формування датасету",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                string message =
                    $"Датасет сформовано.\n\n" +
                    $"Шлях: {result.DatasetPath}\n" +
                    $"Summary: {result.SummaryPath}\n\n" +
                    $"Експортовано: {result.ExportedItems}\n" +
                    $"Train: {result.TrainItems}\n" +
                    $"Validation: {result.ValidationItems}\n\n" +
                    $"FalsePositive: {result.FalsePositiveItems}\n" +
                    $"FalseNegative: {result.FalseNegativeItems}\n" +
                    $"CorrectedPositive: {result.CorrectedPositiveItems}\n" +
                    $"Background ratio: {result.BackgroundRatio:P2}";

                if (result.Warnings.Any())
                {
                    message += "\n\nПопередження:\n" + string.Join("\n", result.Warnings);
                }

                MessageBox.Show(
                    message,
                    "Формування датасету",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка формування датасету:\n{ex.Message}",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                btn_ExportDataset.IsEnabled = true;
                btn_ExportDataset.Content = "📦 Сформувати датасет";
            }
        }
    }
}
