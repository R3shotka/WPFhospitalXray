
using BLL.DTOs.AppUsers;

using BLL.Interface;
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
    /// <summary>
    /// Interaction logic for EditingStuff.xaml
    /// </summary>
    public partial class EditingStuff : Window
    {
        private readonly IApplicationUserService _userService;

        private readonly string _currentStaffId;
        //private readonly Action _refreshCallback;
        public EditingStuff(IApplicationUserService userService, string id)
        {
            InitializeComponent();
            _userService = userService;
            _currentStaffId = id;

            // Підписуємося на подію "вікно завантажилось"
            this.Loaded += EditingStuff_Loaded;
        }

        // Цей метод автоматично спрацює, щойно вікно з'явиться на екрані
        private async void EditingStuff_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMedStuffDataAsync();
        }

        private async Task LoadMedStuffDataAsync()
        {
            try
            {
                var medStuffToEdit = await _userService.GetByIdAsync(_currentStaffId);

                if (medStuffToEdit != null)
                {
                    FullName_textbox.Text = medStuffToEdit.FullName;
                    Login_textbox.Text = medStuffToEdit.Login;

                    // ВАЖЛИВО: Пароль ніколи не виводимо на екран! 
                    // Залишаємо поле порожнім.
                    Pass_textbox.Text = "";

                    // --- Вибираємо правильну Посаду ---
                    foreach (ComboBoxItem item in Job_combobox.Items)
                    {
                        if (item.Content.ToString() == GetPositionDisplayName(medStuffToEdit.Position))
                        {
                            Job_combobox.SelectedItem = item;
                            break;
                        }
                    }

                    // --- Вибираємо правильну Стать ---
                    foreach (ComboBoxItem item in Sex_combobox.Items)
                    {
                        // medStuffToEdit.Sex повертає TypeGender (Ч або Ж)
                        if (item.Content.ToString() == GetSexDisplayName(medStuffToEdit.Sex))
                        {
                            Sex_combobox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==========================================
        // ПЕРЕКЛАДАЧІ: З БАЗИ В UI (Для завантаження)
        // ==========================================

        private string GetPositionDisplayName(string dbPosition)
        {
            return dbPosition switch
            {
                "Admin" => "Адміністратор",
                "Nurse" => "Медсестра",
                "Orthopedist" => "Ортопед",
                _ => "Медсестра" // Якщо в базі щось незрозуміле, ставимо дефолт
            };
        }

        private string GetSexDisplayName(string dbSex)
        {
            return dbSex switch
            {
                "Ч" => "Чоловіча",
                "Ж" => "Жіноча",
                _ => "Чоловіча" // Дефолтне значення на випадок збою
            };
        }

        // ==========================================
        // ПЕРЕКЛАДАЧІ: З UI В БАЗУ (Для кнопки Зберегти)
        // ==========================================

        private string GetPositionFromDisplayName(string displayName)
        {
            return displayName switch
            {
                "Адміністратор" => "Admin",
                "Медсестра" => "Nurse",
                "Рентгенолог" => "Radiologist",
                _ => "Nurse" // Дефолтне значення
            };
        }

        private string GetSexFromDisplayName(string displayName)
        {
            return displayName switch
            {
                "Чоловіча" => "Ч",
                "Жіноча" => "Ж",
                _ => "Ч" // Дефолтне значення
            };
        }

       

        // Кнопка "Зберегти"
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Перевіряємо, чи вибрані значення у випадаючих списках
                if (Job_combobox.SelectedItem == null || Sex_combobox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, оберіть посаду та стать!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Безпечно дістаємо текст з ComboBox
                string selectedJob = (Job_combobox.SelectedItem as ComboBoxItem).Content.ToString();
                string selectedSex = (Sex_combobox.SelectedItem as ComboBoxItem).Content.ToString();

                // 2. Пакуємо всі дані з екрана у наш DTO
                var editDto = new EditStaffDto // Перевір, щоб назва класу збігалася з твоєю DTO
                {
                    Id = _currentStaffId, // ID працівника, якого ми запам'ятали при відкритті вікна
                    FullName = FullName_textbox.Text,
                    Login = Login_textbox.Text,

                    // Якщо поле пароля порожнє, ми так і передаємо "". 
                    // Твій BLL (ApplicationUserService) має бути налаштований так, 
                    // щоб ігнорувати зміну пароля, якщо прийшов порожній рядок.
                    Password = Pass_textbox.Text,

                    Position = GetPositionFromDisplayName(selectedJob), // Перекладаємо "Адміністратор" -> "Admin"
                    Sex = GetSexFromDisplayName(selectedSex)            // Перекладаємо "Чоловіча" -> "Ч"
                };

                // 3. Відправляємо в базу асинхронно
                await _userService.UpdateAsync(editDto);

                MessageBox.Show("Дані успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                // Закриваємо вікно. Після цього батьківське вікно саме оновить таблицю!
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Кнопка "Вийти"
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}

