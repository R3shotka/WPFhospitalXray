using BLL.DTOs.Patients;
using BLL.Interface; // Додаємо, щоб бачити IApplicationUserService
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

        // DI автоматично передасть сюди готовий IApplicationUserService при відкритті AdminPanel
        public AdminPanel(IApplicationUserService userService, IPatientService patientService, string role)
        {
            InitializeComponent();
            _userService = userService;
            _patientService = patientService;
            _currentUserRole = role;

            ApplyPermissions();
            LoadPatientsAsync(); // Завантажуємо таблицю одразу при відкритті
        }
        private void ApplyPermissions()
        {
            if (_currentUserRole == "Nurse")
            {
                AdminStaffPanel.Visibility = Visibility.Collapsed;
                this.Title = "Медсестра: Робота з пацієнтами";
            }
        }

        private async void LoadPatientsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                DBGrid.ItemsSource = patients;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}");
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
        private void CreatePatient_btn(object sender, RoutedEventArgs e)
        {
            var createWin = new CreatePatient(_patientService);

            // 2. Відкриваємо його як модальне вікно (щоб користувач не міг клікати мишкою поза ним)
            createWin.ShowDialog();

            // 3. Коли вікно закриється (пацієнт буде збережений), ми одразу оновлюємо таблицю!
            LoadPatientsAsync();
        }

        private void EditPatient_btn(object sender, RoutedEventArgs e)
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
                LoadPatientsAsync();
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
                        LoadPatientsAsync();
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
    }
}