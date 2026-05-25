using BLL.Interface;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32; // Для вікна вибору файлу (OpenFileDialog)
using System.IO;       // Для роботи з файлами (File.Exists)
using System.Windows.Media.Imaging;

namespace WPFhospitalXray
{
    public partial class MedicalCardWindow : Window
    {
        private readonly IPatientService _patientService;
        private readonly IMedicalCardService _medicalCardService;
        private readonly IExaminationService _examinationService;
        private readonly IMedicalImageService _imageService;
        private readonly IConclusionService _conclusionService;
        private readonly IDatasetService _datasetService;
        private readonly IRetrainingRequestService _requestService;
        private readonly IAnalysisResultService _analysisResultService;
        private readonly IImageStorageService _imageStorageService;
        private readonly IRolePermissionService _rolePermissionService;

        private string _currentRole;
        private string _patientId;
        private int _medicalCardId;

        private string _currentUserId;

        private readonly IAIAnalyzerService _aiService;
        public MedicalCardWindow(
            IPatientService patientService,
            IMedicalCardService medicalCardService,
            IExaminationService examinationService,
            IMedicalImageService imageService,
            IConclusionService conclusionService,
            IAIAnalyzerService aiService,
            IDatasetService datasetService,
            IRetrainingRequestService requestService,
            IAnalysisResultService analysisResultService,
            IImageStorageService imageStorageService,
            IRolePermissionService rolePermissionService)
        {
            InitializeComponent();
            _patientService = patientService;
            _medicalCardService = medicalCardService;
            _examinationService = examinationService;
            _imageService = imageService;
            _conclusionService = conclusionService;
            _aiService = aiService;
            _datasetService = datasetService;
            _requestService = requestService;
            _analysisResultService = analysisResultService;
            _imageStorageService = imageStorageService;
            _rolePermissionService = rolePermissionService;
        }
        // ЗРОБИЛИ МЕТОД ASYNC, щоб він міг чекати на дані з бази
        public async void InitializeData(string patientId, string userRole, string userId)
        {
            _patientId = patientId;
            _currentRole = userRole;

            _currentUserId = userId;

            // 1. Налаштовуємо кнопки
            ConfigureUIForRole();

            // 2. Підтягуємо дані пацієнта
            await LoadPatientDataAsync();

            // Пізніше розблокуємо це для таблиці:
             await LoadExaminationsAsync();
        }

        private async Task LoadPatientDataAsync()
        {
            try
            {
                // Використовуємо твій існуючий метод, який повертає EditPatientDto
                var patientDto = await _patientService.GetPatientByIdAsync(_patientId);

                if (patientDto != null)
                {
                    tb_PatientId.Text = patientDto.Id;
                    tb_PatientName.Text = patientDto.FullName;
                    tb_Gender.Text = patientDto.Sex;

                    // Якщо дата народження є, красиво її форматуємо
                    if (patientDto.DateOfBirth != null)
                    {
                        tb_DateOfBirth.Text = patientDto.DateOfBirth.Value.ToString("dd.MM.yyyy");
                    }

                    // Заповнюємо нові поля
                    tb_Phone.Text = patientDto.Phone;
                    tb_Address.Text = patientDto.Address;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні даних пацієнта:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LoadExaminationsAsync()
        {
            try
            {
                // Використовуємо твій метод пошуку картки за PatientId!
                var currentCard = await _medicalCardService.GetMedicalCardByPatientIdAsync(_patientId);

                if (currentCard != null)
                {
                    _medicalCardId = currentCard.Id; // Зберігаємо ID картки для подальшої роботи

                    // Дістаємо обстеження через наш сервіс
                    var exams = await _examinationService.GetExaminationsByCardIdAsync(_medicalCardId);

                    // Закидаємо їх у таблицю
                    dg_Examinations.ItemsSource = exams.OrderByDescending(e => e.Id).ToList(); ;
                }
                else
                {
                    MessageBox.Show("У цього пацієнта ще немає медичної картки!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні обстежень:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureUIForRole()
        {
            btn_StartExamination.Visibility = Visibility.Collapsed;
            btn_AddImage.Visibility = Visibility.Collapsed;
            btn_DeleteExam.Visibility = Visibility.Collapsed;
            btn_DeletePatient.Visibility = Visibility.Collapsed;
            sp_ConclusionBlock.Visibility = Visibility.Collapsed;

            tb_RadiologistConclusion.IsReadOnly = true;
            tb_RadiologistConclusion.Focusable = false;
            tb_RadiologistConclusion.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(232, 238, 245));

            tb_SurgeonConclusion.IsReadOnly = true;
            tb_SurgeonConclusion.Focusable = false;
            tb_SurgeonConclusion.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(232, 238, 245));

            if (_rolePermissionService.CanCreateExamination(_currentRole))
            {
                btn_StartExamination.Visibility = Visibility.Visible;
            }

            if (_rolePermissionService.CanUploadImages(_currentRole))
            {
                btn_AddImage.Visibility = Visibility.Visible;
            }

            if (_rolePermissionService.CanDeleteExamination(_currentRole))
            {
                btn_DeleteExam.Visibility = Visibility.Visible;
            }

            if (_rolePermissionService.CanDeletePatient(_currentRole))
            {
                btn_DeletePatient.Visibility = Visibility.Visible;
            }

            if (_rolePermissionService.CanWriteRadiologistConclusion(_currentRole))
            {
                sp_ConclusionBlock.Visibility = Visibility.Visible;
                tb_RadiologistConclusion.IsReadOnly = false;
                tb_RadiologistConclusion.Focusable = true;
                tb_RadiologistConclusion.Background = System.Windows.Media.Brushes.White;
            }

            if (_rolePermissionService.CanWriteSurgeonConclusion(_currentRole))
            {
                sp_ConclusionBlock.Visibility = Visibility.Visible;
                tb_SurgeonConclusion.IsReadOnly = false;
                tb_SurgeonConclusion.Focusable = true;
                tb_SurgeonConclusion.Background = System.Windows.Media.Brushes.White;
            }
        }


        private async void btn_DeletePatient_Click(object sender, RoutedEventArgs e)
        {
            if (!_rolePermissionService.CanDeletePatient(_currentRole))
            {
                MessageBox.Show("У вас немає прав для видалення пацієнта.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 1. Запитуємо підтвердження у користувача (Захист від випадкового кліку)
            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити цього пацієнта та всю його медичну історію?\nЦю дію неможливо скасувати!",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            // 2. Якщо користувач натиснув "Так"
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Викликаємо твій метод видалення з PatientService
                    await _patientService.DeletePatientAsync(_patientId);

                    MessageBox.Show("Пацієнта та його медичну картку успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Закриваємо вікно медкартки, оскільки пацієнта більше немає
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при видаленні пацієнта:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void btn_StartExamination_Click(object sender, RoutedEventArgs e)
        {
            if (!_rolePermissionService.CanCreateExamination(_currentRole))
            {
                MessageBox.Show("У вас немає прав для створення обстеження.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                if (_medicalCardId == 0)
                {
                    MessageBox.Show("Неможливо створити обстеження: медкартку не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Створюємо порожнє обстеження
                await _examinationService.CreateEmptyExaminationAsync(_medicalCardId);
                await _patientService.UpdatePatientStatusAsync(_patientId, "Очікує висновку");

                // Оновлюємо таблицю, щоб лікар одразу побачив новий рядок!
                await LoadExaminationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при створенні обстеження:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void btn_AddImage_Click(object sender, RoutedEventArgs e)
        {
            if (!_rolePermissionService.CanUploadImages(_currentRole))
            {
                MessageBox.Show("У вас немає прав для завантаження знімків.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 1. Перевіряємо, чи вибрав лікар обстеження в таблиці
            if (dg_Examinations.SelectedItem is not BLL.DTOs.Examinations.ExaminationListDto selectedExam)
            {
                MessageBox.Show("Будь ласка, спочатку виберіть обстеження з таблиці!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Відкриваємо вікно вибору файлу
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Виберіть рентгенівський знімок",
                Filter = "Зображення (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Всі файли (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFilePath = openFileDialog.FileName;
                    string fileExtension = Path.GetExtension(sourceFilePath);

                    string destinationFilePath = await _imageStorageService.SaveImageAsync(
                        selectedExam.Id,
                        sourceFilePath);

                    await _imageService.AddImageAsync(
                        selectedExam.Id,
                        destinationFilePath,
                        _imageStorageService.GetContentType(fileExtension));

                    MessageBox.Show(
                        "Знімок успішно завантажено та збережено у файловому сховищі!",
                        "Успіх",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await LoadExaminationsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Помилка при збереженні знімка:\n{ex.Message}",
                        "Помилка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        private async void btn_DeleteExam_Click(object sender, RoutedEventArgs e)
        {
            if (!_rolePermissionService.CanDeleteExamination(_currentRole))
            {
                MessageBox.Show("У вас немає прав для видалення обстеження.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // 1. Перевіряємо, чи вибрано обстеження
            if (dg_Examinations.SelectedItem is not BLL.DTOs.Examinations.ExaminationListDto selectedExam)
            {
                MessageBox.Show("Будь ласка, спочатку виберіть обстеження з таблиці!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Запитуємо підтвердження
            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити це обстеження?\nВідповідний рентгенівський знімок також буде назавжди видалено з сервера!",
                "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // 3. ЗБЕРІГАЄМО ШЛЯХ до файлу перед тим, як видалити запис з БД
                    var imagesToDelete = await _imageService.GetImagesByExaminationIdAsync(selectedExam.Id);

                    await _examinationService.DeleteExaminationAsync(selectedExam.Id);

                    foreach (var image in imagesToDelete)
                    {
                        await _imageStorageService.DeleteImageAsync(image.FilePath);
                    }

                    MessageBox.Show("Обстеження та знімок успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    // 6. Оновлюємо таблицю
                    await LoadExaminationsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при видаленні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void dg_Examinations_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dg_Examinations.SelectedItem is BLL.DTOs.Examinations.ExaminationListDto selectedExam)
            {
                tb_RadiologistConclusion.Text = selectedExam.RadiologistConclusion;
                tb_SurgeonConclusion.Text = selectedExam.SurgeonConclusion;

                await ShowFirstImagePreviewAsync(selectedExam.Id);
            }
            else
            {
                tb_RadiologistConclusion.Clear();
                tb_SurgeonConclusion.Clear();
                img_Preview.Source = null;
            }
        }
        private async Task ShowFirstImagePreviewAsync(int examinationId)
        {
            try
            {
                var images = await _imageService.GetImagesByExaminationIdAsync(examinationId);
                var firstImage = images.FirstOrDefault();

                if (firstImage == null || string.IsNullOrEmpty(firstImage.FilePath) || !File.Exists(firstImage.FilePath))
                {
                    img_Preview.Source = null;
                    return;
                }

                ShowImage(firstImage.FilePath);
            }
            catch
            {
                img_Preview.Source = null;
            }
        }

        private void ShowImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                img_Preview.Source = null; // Якщо картинки немає, очищаємо квадрат
                return;
            }

            try
            {
                // Використовуємо BitmapCacheOption.OnLoad, щоб програма не "блокувала" файл на диску
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                img_Preview.Source = bitmap;
            }
            catch (Exception ex)
            {
                // Якщо формат картинки битий, програма не впаде
                img_Preview.Source = null;
            }
        }
        private void btn_ViewImage_Click(object sender, RoutedEventArgs e)
        {
            if (!_rolePermissionService.CanViewImages(_currentRole))
            {
                MessageBox.Show("У вас немає прав для перегляду знімків.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var button = sender as System.Windows.Controls.Button;

            if (button?.DataContext is BLL.DTOs.Examinations.ExaminationListDto selectedExam)
            {
                if (selectedExam.ImagesCount == 0)
                {
                    MessageBox.Show("Для цього обстеження ще не завантажено жодного знімка.",
                                    "Знімки відсутні",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    return;
                }

                try
                {
                    XRayViewerWindow viewerWindow = new XRayViewerWindow(
                         selectedExam.Id,
                         _currentUserId,
                         _currentRole,
                         _imageService,
                         _aiService,
                         _datasetService,
                         _requestService,
                         _analysisResultService,
                         _rolePermissionService);

                    viewerWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося відкрити знімки обстеження:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void img_Preview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { }
        private async void btn_SaveConclusion_Click(object sender, RoutedEventArgs e)
        {
            if (dg_Examinations.SelectedItem is not BLL.DTOs.Examinations.ExaminationListDto selectedExam)
            {
                MessageBox.Show("Будь ласка, спочатку виберіть обстеження з таблиці!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string textToSave = "";

                // Визначаємо, з якого поля брати текст, спираючись на роль користувача
                if (_rolePermissionService.CanWriteSurgeonConclusion(_currentRole))
                {
                    textToSave = tb_SurgeonConclusion.Text;
                }
                else if (_rolePermissionService.CanWriteRadiologistConclusion(_currentRole))
                {
                    textToSave = tb_RadiologistConclusion.Text;
                }
                else
                {
                    MessageBox.Show("У вас немає прав для збереження медичних висновків.", "Доступ заборонено",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ПЕРЕДАЄМО ДАНІ У СЕРВІС!
                await _conclusionService.SaveOrUpdateConclusionAsync(selectedExam.Id, _currentRole, textToSave, _currentUserId);

                if (_rolePermissionService.CanWriteSurgeonConclusion(_currentRole))
                {
                    await _patientService.UpdatePatientStatusAsync(_patientId, "Висновок отримано");
                }

                MessageBox.Show("Висновок успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                // Оновлюємо таблицю, щоб підтягнути свіжі дані з бази
                await LoadExaminationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні висновку:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        
    }
}
