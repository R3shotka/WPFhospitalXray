using BLL.DTOs.Patients;
using BLL.Interface;
using BLL.Service;
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
using System.ComponentModel.DataAnnotations;

namespace WPFhospitalXray
{
    /// <summary>
    /// Interaction logic for EditPatient.xaml
    /// </summary>
    public partial class EditPatient : Window
    {
        private readonly IPatientService _patientService;
        private readonly string _patientId;
        public EditPatient(IPatientService patientService, string patientId)
        {
            InitializeComponent();
            _patientService = patientService;
            _patientId = patientId;

            // Забороняємо змінювати номер паспорта
            Passport_textbox.IsEnabled = false;

            // Підписуємося на подію: щойно вікно відкриється, запуститься метод завантаження
            this.Loaded += EditPatient_Loaded;
        }
        private async void EditPatient_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Просимо сервіс знайти пацієнта за паспортом
                var patient = await _patientService.GetPatientByIdAsync(_patientId);

                if (patient != null)
                {
                    // 2. Розкидаємо дані по текстових полях
                    Passport_textbox.Text = patient.Id;
                    FullName_textbox.Text = patient.FullName;
                    phone_textbox.Text = patient.Phone;
                    Adress_textbox.Text = patient.Address;
                    BirthDate_picker.SelectedDate = patient.DateOfBirth;

                    // 3. Відновлюємо вибрану стать у випадаючому списку
                    if (patient.Sex == "Ч")
                        Sex_combobox.SelectedIndex = 0; // Чоловіча
                    else
                        Sex_combobox.SelectedIndex = 1; // Жіноча
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Save_btn(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Акуратно дістаємо стать. Якщо не вибрано - залишаємо null.
                string? selectedSex = null;
                if (Sex_combobox.SelectedItem is ComboBoxItem selectedItem)
                {
                    selectedSex = selectedItem.Content.ToString() == "Чоловіча" ? "Ч" : "Ж";
                }

                // 2. Збираємо дані. Ніяких "?? DateTime.Now" більше немає!
                var updateDto = new EditPatientDto
                {
                    Id = Passport_textbox.Text,
                    FullName = FullName_textbox.Text,
                    Phone = phone_textbox.Text,
                    Address = Adress_textbox.Text,
                    DateOfBirth = BirthDate_picker.SelectedDate, // Передаємо як є
                    Sex = selectedSex // Передаємо null, якщо порожньо
                };

                // 3. МАГІЯ ВАЛІДАЦІЇ (Data Annotations)
                var validationContext = new ValidationContext(updateDto);
                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

                bool isValid = Validator.TryValidateObject(updateDto, validationContext, validationResults, true);

                if (!isValid)
                {
                    string errors = string.Join("\n", validationResults.Select(r => r.ErrorMessage));
                    MessageBox.Show(errors, "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Зупиняємо збереження, якщо є помилки
                }

                // 4. Якщо все чудово - відправляємо в базу на оновлення
                await _patientService.UpdatePatientAsync(updateDto);

                MessageBox.Show("Дані пацієнта успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Закриваємо вікно
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_btn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
