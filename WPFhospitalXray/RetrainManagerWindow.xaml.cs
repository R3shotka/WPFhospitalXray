using BLL.DTOs.RetrainingRequests;   // Додали using для наших DTO!
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

        public RetrainManagerWindow(IRetrainingRequestService requestService, IDatasetService datasetService)
        {
            InitializeComponent();
            _requestService = requestService;
            _datasetService = datasetService;

            // Запускаємо завантаження даних
            _ = LoadRequestsAsync();
            
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
                MarkupPreviewWindow previewWindow = new MarkupPreviewWindow(selectedReq.ImagePath);
                previewWindow.ShowDialog();
            }
        }

        private void btn_StartRetrain_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ця кнопка оживе на наступному етапі! 🚀");
        }
    }
}