using BLL.Interface;
using DAL.Entity;
using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // ⚠️ Це дуже важливий рядок, без нього не запрацює магія GetRequiredService!


namespace WPFhospitalXray
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        // 1. Додаємо змінну для нашої "служби доставки"
        private readonly IServiceProvider _serviceProvider;

        // 2. У конструкторі просимо IServiceProvider замість конкретних сервісів
        public MainWindow(IAuthService authService, UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _authService = authService;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
        }

        private async void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            var username = LoginTextBox.Text;
            var password = PasswordTextBox.Password;

            var loginTask = await _authService.LoginAsync(username, password);

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                MessageBox.Show("User not found after login (unexpected).");
                return;
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // 3. ПРО-ПІДХІД: Ми не пишемо new AdminPanel(...). 
                // Ми кажемо: "Дай мені готове вікно AdminPanel з усіма його нутрощами!"
                var adminWindow = ActivatorUtilities.CreateInstance<AdminPanel>(_serviceProvider, "Admin");
                adminWindow.Show();
            }
            else if (await _userManager.IsInRoleAsync(user, "Nurse"))
            {
                // На майбутнє: var nurseWindow = _serviceProvider.GetRequiredService<NurseWindow>();
                // nurseWindow.Show();
                var nurseWindow = ActivatorUtilities.CreateInstance<AdminPanel>(_serviceProvider, "Nurse");
                nurseWindow.Show();
            }
            else if (await _userManager.IsInRoleAsync(user, "Radiologist"))
            {
                // ...
            }
            else if (await _userManager.IsInRoleAsync(user, "Surgeon"))
            {
                // ...
            }
            else
            {
                MessageBox.Show("User has no role assigned.", "Access denied",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Закриваємо вікно логіну
            this.Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}