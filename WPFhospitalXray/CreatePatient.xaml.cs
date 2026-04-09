using BLL.DTOs.Patients;
using BLL.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    /// Interaction logic for CreatePatient.xaml
    /// </summary>
    public partial class CreatePatient : Window
    {
        private readonly IPatientService _patientService;
        public CreatePatient(IPatientService patientService)
        {
            InitializeComponent();
            _patientService = patientService;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
                var newPatient = new CreatePatientDto
                {
                    Id = Passport_textbox.Text,
                    FullName = FullName_textbox.Text,
                    Phone = phone_textbox.Text,
                    Address = Adress_textbox.Text,
                    DateOfBirth = BirthDate_picker.SelectedDate, // Передаємо як є
                    Sex = selectedSex // Передаємо null, якщо порожньо
                };

                // 2. МАГІЯ DATA ANNOTATIONS: Перевіряємо об'єкт
                var validationContext = new ValidationContext(newPatient);
                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

                // Метод поверне false, якщо хоч одне правило порушено
                bool isValid = Validator.TryValidateObject(newPatient, validationContext, validationResults, true);

                if (!isValid)
                {
                    // Беремо всі повідомлення про помилки і з'єднуємо їх через перенесення рядка (\n)
                    string errors = string.Join("\n", validationResults.Select(r => r.ErrorMessage));

                    // Показуємо користувачу всі помилки одразу!
                    MessageBox.Show(errors, "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Зупиняємо збереження
                }

                // 3. Якщо isValid == true, код йде далі до збереження
                await _patientService.CreatePatientAsync(newPatient);

                MessageBox.Show("Пацієнта успішно зареєстровано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
