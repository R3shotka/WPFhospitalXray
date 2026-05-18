using BLL.DTOs.Patients;
using BLL.Interface; // Додаємо, щоб бачити IApplicationUserService
using BLL.Service;
using DAL.DBContext;
using DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFhospitalXray
{
    public partial class AdminPanel : Window
    {
        // Створюємо змінну для нашого сервісу
        private readonly IApplicationUserService _userService;
        private readonly IPatientService _patientService;
        private readonly string _currentUserRole;
        private readonly IMedicalCardService _medicalCardService;
        private readonly IExaminationService _examinationService;
        private readonly IMedicalImageService _imageService;
        private readonly IConclusionService _conclusionService;
        private readonly IAIAnalyzerService _aiAnalyzerService;
        private readonly IDatasetService _datasetService;
        private readonly IRetrainingRequestService _requestService;
        private readonly IAnalysisResultService _analysisResultService;
        private readonly IImageStorageService _imageStorageService;
        private readonly IApplicationPathService _pathService;
        private readonly IDatasetExportService _datasetExportService;
        private readonly IRolePermissionService _rolePermissionService;
        private readonly IModelTrainingService _modelTrainingService;
        private readonly IModelVersionService _modelVersionService;

        private readonly string _currentUserId;

        private IEnumerable<PatientsListDto> _allPatients;

        // DI автоматично передасть сюди готовий IApplicationUserService при відкритті AdminPanel
        public AdminPanel(
            IApplicationUserService userService,
            IPatientService patientService,
            string role,
            IMedicalCardService medicalCardService,
            IExaminationService examinationService,
            IMedicalImageService imageService,
            IConclusionService conclusionService,
            string currentUserId,
            IAIAnalyzerService aIAnalyzerService,
            IDatasetService datasetService,
            IRetrainingRequestService requestService,
            IAnalysisResultService analysisResultService,
            IImageStorageService imageStorageService,
            IApplicationPathService pathService,
            IDatasetExportService datasetExportService,
            IRolePermissionService rolePermissionService,
            IModelTrainingService modelTrainingService,
            IModelVersionService modelVersionService)
        {
            InitializeComponent();

            _userService = userService;
            _patientService = patientService;
            _currentUserRole = role;
            _medicalCardService = medicalCardService;
            _examinationService = examinationService;
            _imageService = imageService;
            _conclusionService = conclusionService;
            _currentUserId = currentUserId;
            _aiAnalyzerService = aIAnalyzerService;
            _datasetService = datasetService;
            _requestService = requestService;
            _analysisResultService = analysisResultService;
            _imageStorageService = imageStorageService;
            _pathService = pathService;
            _datasetExportService = datasetExportService;
            _rolePermissionService = rolePermissionService;
            _modelTrainingService = modelTrainingService;
            _modelVersionService = modelVersionService;

            ApplyPermissions();

            _ = LoadPatientsAsync();
        }
        private void ApplyPermissions()
        {
            if (!_rolePermissionService.CanManagePatients(_currentUserRole))
            {
                PatientPanel.Visibility = Visibility.Collapsed;
            }

            if (!_rolePermissionService.CanManageStaff(_currentUserRole) &&
                !_rolePermissionService.CanManageRetraining(_currentUserRole))
            {
                AdminStaffPanel.Visibility = Visibility.Collapsed;
            }

            if (_rolePermissionService.CanManagePatients(_currentUserRole))
            {
                this.Title = "Медсестра: Робота з пацієнтами";
            }
            else if (_rolePermissionService.CanManageStaff(_currentUserRole))
            {
                this.Title = "Адміністратор: Керування клінікою та ШІ";
            }
            else
            {
                this.Title = $"{_currentUserRole}: Список пацієнтів";
            }
        }

        // Обробник натискання для кнопки ШІ
        private void OpenRetrainManager_Click(object sender, RoutedEventArgs e)
        {
            if (!_rolePermissionService.CanManageRetraining(_currentUserRole))
            {
                MessageBox.Show("У вас немає прав для керування донавчанням ШІ.", "Доступ заборонено",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Відкриваємо вікно керування датасетом
            var retrainWindow = new RetrainManagerWindow(
                _requestService,
                _datasetService,
                _pathService,
                _datasetExportService,
                _modelTrainingService,
                _modelVersionService);

            retrainWindow.ShowDialog();
        }

        private async Task LoadPatientsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();

                _allPatients = patients;
                PerformSearch();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}");
            }
        }
        private void btn_Search_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void tb_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            PerformSearch();
        }

        private void btn_ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            tb_Search.Text = ""; // Очищаємо текст (це автоматично скине фільтр)
        }

        private void PerformSearch()
        {
            // Перевіряємо, чи список пацієнтів вже завантажився з бази
            if (_allPatients == null) return;

            string searchText = tb_Search.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                // Якщо поле порожнє - показуємо весь список
                DBGrid.ItemsSource = _allPatients;
            }
            else
            {
                // Миттєва фільтрація в оперативній пам'яті
                var filteredList = _allPatients.Where(p =>
                    (p.FullName != null && p.FullName.ToLower().Contains(searchText)) ||
                    (p.Id != null && p.Id.ToLower().Contains(searchText))
                ).ToList();

                DBGrid.ItemsSource = filteredList;
            }
        }

        private void CreateStuff_btn(object sender, RoutedEventArgs e)
        {
            // Вікно створення ми ще не переписували, тому залишаємо його поки старим способом
            var createWindow = new CreateMed(_userService);
            createWindow.ShowDialog();
        }

        private void EditStuff_btn(object sender, RoutedEventArgs e)
        {
            // ТЕПЕР ПЕРЕДАЄМО СЕРВІС ЯК ПАРАМЕТР!
            var editWindow = new EditMed(_userService);
            editWindow.ShowDialog();
        }


        // ==========================================
        // КНОПКИ ДЛЯ ПАЦІЄНТІВ
        // ==========================================
        private async void CreatePatient_btn(object sender, RoutedEventArgs e)
        {
            var createWin = new CreatePatient(_patientService, _medicalCardService);

            // 2. Відкриваємо його як модальне вікно (щоб користувач не міг клікати мишкою поза ним)
            createWin.ShowDialog();

            // 3. Коли вікно закриється (пацієнт буде збережений), ми одразу оновлюємо таблицю!

            tb_Search.Text = "";

            await LoadPatientsAsync();
        }

        private async void EditPatient_btn(object sender, RoutedEventArgs e)
        {
            var selectedPatient = DBGrid.SelectedItem as PatientsListDto;

            // 2. Перевіряємо, чи дійсно хтось вибраний (щоб не було помилки)
            if (selectedPatient != null)
            {
                // 3. ПЕРЕДАЧА ID! 
                // Ми беремо selectedPatient.Id (номер паспорта) і передаємо його прямо в конструктор нового вікна
                var editWin = new EditPatient(_patientService, selectedPatient.Id);

                editWin.ShowDialog();



                // Оновлюємо таблицю після того, як вікно редагування закриється
                await LoadPatientsAsync();
            }
        }

        private async void DeletePatient_btn(object sender, RoutedEventArgs e)
        {
            if (DBGrid.SelectedItem is PatientsListDto selectedPatient)
            {
                // Запитуємо підтвердження
                var result = MessageBox.Show($"Ви дійсно хочете видалити пацієнта:\n{selectedPatient.FullName}?",
                                             "Підтвердження видалення",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Відправляємо ID на видалення в базу
                        await _patientService.DeletePatientAsync(selectedPatient.Id);

                        MessageBox.Show("Пацієнта успішно видалено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Оновлюємо таблицю (переконайся, що метод називається саме так, як у тебе)
                        await LoadPatientsAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при видаленні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }   

        // Поява кнопки "Редагувати" при кліку на таблицю
        private void DBGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EditPatientBtn != null && DeletePatient_btn != null)
            {
                // Перевіряємо, чи вибрано рядок (true або false)
                bool hasSelection = DBGrid.SelectedItem != null;

                // Вмикаємо або вимикаємо (сірий колір) обидві кнопки
                EditPatientBtn.IsEnabled = hasSelection;
                DeletePatientBtn.IsEnabled = hasSelection;
            }
        }

        private async void DBGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. Перевірка ролі (щоб Адмін не міг відкрити)
            if (!_rolePermissionService.CanOpenMedicalCard(_currentUserRole))
            {
                return;
            }

            // 2. Отримуємо вибраного пацієнта
            if (DBGrid.SelectedItem is PatientsListDto selectedPatient)
            {
                // 3. СТВОРЮЄМО ВІКНО ПРАВИЛЬНО (через DI)
                // Передаємо всі необхідні сервіси, які вже є в AdminPanel
                var medicalCardWindow = new MedicalCardWindow(
                    _patientService,
                    _medicalCardService,
                    _examinationService,
                    _imageService,
                    _conclusionService,
                    _aiAnalyzerService,
                    _datasetService,
                    _requestService,
                    _analysisResultService,
                    _imageStorageService,
                    _rolePermissionService); // <— Якщо цього сервісу тут ще немає, додай його в конструктор AdminPanel!

                // 4. Передаємо дані (ID та Роль)
                medicalCardWindow.InitializeData(selectedPatient.Id, _currentUserRole, _currentUserId);

                medicalCardWindow.ShowDialog();

                tb_Search.Text = "";

                await LoadPatientsAsync();
            }
        }
    }
}
