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
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IAuthService authService, IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _authService = authService;
            _serviceProvider = serviceProvider;
        }

        private async void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            var username = LoginTextBox.Text;
            var password = PasswordTextBox.Password;

            var authResult = await _authService.LoginAsync(username, password);

            if (!authResult.Success)
            {
                MessageBox.Show(
                    authResult.ErrorMessage ?? "Не вдалося увійти в систему.",
                    "Помилка входу",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(authResult.Role) || string.IsNullOrWhiteSpace(authResult.UserId))
            {
                MessageBox.Show(
                    "Не вдалося визначити роль або ID користувача.",
                    "Помилка входу",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (authResult.Role == "Admin" ||
                authResult.Role == "Nurse" ||
                authResult.Role == "Radiologist" ||
                authResult.Role == "Surgeon")
            {
                var window = ActivatorUtilities.CreateInstance<AdminPanel>(
                    _serviceProvider,
                    authResult.Role,
                    authResult.UserId);

                window.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Користувач має невідому роль.",
                    "Access denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}