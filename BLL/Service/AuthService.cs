using BLL.DTOs.Auth;
using BLL.Interface;
using DAL.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AuthResultDto> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Введіть логін і пароль."
                };
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Користувача з таким логіном не знайдено."
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Невірний логін або пароль."
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault(r =>
                r == "Admin" ||
                r == "Nurse" ||
                r == "Radiologist" ||
                r == "Surgeon");

            if (string.IsNullOrWhiteSpace(role))
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "Користувач не має призначеної ролі."
                };
            }

            return new AuthResultDto
            {
                Success = true,
                UserId = user.Id,
                Role = role,
                FullName = user.FullName
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
