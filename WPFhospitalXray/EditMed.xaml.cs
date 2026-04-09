using BLL.DTOs; // Або BLL.DTOs.AppUsers, перевір, де лежить StaffListDto
using BLL.DTOs.AppUsers;
using BLL.Interface;
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
    public partial class EditMed : Window
    {
        // Наш новий крутий сервіс
        private readonly IApplicationUserService _userService;

        // Список для зберігання даних таблиці
        public List<StaffListDto> MedStuffs = new List<StaffListDto>();

        // DI автоматично передасть сюди готовий IApplicationUserService
        public EditMed(IApplicationUserService userService)
        {
            InitializeComponent();
            _userService = userService;

            // Щоб викликати асинхронний метод при завантаженні вікна,
            // краще використовувати подію Loaded
            this.Loaded += EditMed_Loaded;
        }

        private async void EditMed_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMedStuffsAsync();
        }

        // Асинхронний метод для завантаження списку
        private async Task LoadMedStuffsAsync()
        {
            try
            {
                // Беремо дані з бази через наш сервіс
                MedStuffs = await _userService.GetAllStaffAsync();
                DBStuff.ItemsSource = MedStuffs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження працівників:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditStuff_btn(object sender, RoutedEventArgs e)
        {
            // Перевіряємо, чи виділено рядок і приводимо до нашого StaffListDto
            if (DBStuff.SelectedItem is StaffListDto selected)
            {
                // Передаємо Id у вікно редагування (зверни увагу, що у вікні EditingStuff теж треба буде поміняти конструктор)
                var editWindow = new EditingStuff(_userService, selected.Id);
                editWindow.Owner = this;
                editWindow.ShowDialog();

                // Оновлюємо таблицю після того, як вікно редагування закриється
                await LoadMedStuffsAsync();
            }
            else
            {
                MessageBox.Show("Оберіть працівника для редагування", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteStuff_btn(object sender, RoutedEventArgs e)
        {
            if (DBStuff.SelectedItem is StaffListDto selected)
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете видалити працівника {selected.FullName}?",
                                             "Підтвердження видалення",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Видаляємо через наш новий сервіс (який сам працює з Identity)
                        await _userService.DeleteAsync(selected.Id);

                        // Оновлюємо таблицю на екрані
                        await LoadMedStuffsAsync();

                        MessageBox.Show("Працівника успішно видалено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка при видаленні:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Оберіть працівника для видалення", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


           

        }


    }
}