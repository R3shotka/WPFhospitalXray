
using DAL.DBContext;
using BLL.DTOs.AppUsers;
using BLL.Interface;
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
    /// Interaction logic for CreateMed.xaml
    /// </summary>
    public partial class CreateMed : Window
    {
        private readonly IApplicationUserService _userService;

        public CreateMed(IApplicationUserService userService)
        {
            InitializeComponent();

            _userService = userService;
            //var context = new ApplicationDBContext();
            //_medStuffService = new MedStuffService(context);

        }
        private async void Save_btn(object sender, RoutedEventArgs e)
        {
            try
            {
                // Перевірка, чи всі поля заповнені (особливо випадаючі списки)
                if (Job_combobox.SelectedItem == null || Sex_combobox.SelectedItem == null)
                {
                    MessageBox.Show("Будь ласка, оберіть посаду та стать!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Безпечно дістаємо текст з ComboBox
                string selectedJob = (Job_combobox.SelectedItem as ComboBoxItem).Content.ToString();
                string selectedSex = (Sex_combobox.SelectedItem as ComboBoxItem).Content.ToString();

                var createStaffDto = new CreateStaffDto
                {
                    FullName = FullName_textbox.Text,
                    Position = GetPositionFromDisplayName(selectedJob), // Перекладаємо для БД
                    Sex = GetSexFromDisplayName(selectedSex),           // Перекладаємо для БД
                    Login = Login_textbox.Text,
                    Password = Pass_textbox.Text
                };

                // 3. Додали await! Тепер чекаємо, поки БД реально збереже людину
                await _userService.CreateAsync(createStaffDto);

                MessageBox.Show("Працівника успішно створено!", "Успіх",
                       MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при створенні:\n{ex.Message}", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        public void Exit_btn(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- МЕТОДИ-ПЕРЕКЛАДАЧІ ---

        private string GetPositionFromDisplayName(string displayName)
        {
            // Identity Roles зазвичай зберігаються англійською. 
            // ЗАМІНИ ці англійські слова на ті назви ролей, які в тебе реально створені в базі!
            return displayName switch
            {
                "Адміністратор" => "Admin",
                "Медсестра" => "Nurse",
                "Ортопед" => "Orthopedist", // або "Doctor" чи "Surgeon"
                _ => "Nurse" // Значення за замовчуванням
            };
        }

        private string GetSexFromDisplayName(string displayName)
        {
            // ЗАМІНИ "Male"/"Female" на ті значення, які є у твоєму ApplicationUser.TypeGender Enum!
            // Наприклад, якщо там TypeGender.Man і TypeGender.Woman, пиши "Man" і "Woman"
            return displayName switch
            {
                "Чоловіча" => "Ч",
                "Жіноча" => "Ж",
                _ => "Ч" // За замовчуванням
            };
        }

    }
}
