using BLL.DTOs.AppUsers;
using BLL.Interface;
using DAL.Entity;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BLL.Service
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IAplicationUser _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationUserService(IAplicationUser userRepository, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task CreateAsync(CreateStaffDto dto)
        {
            var newUser = new ApplicationUser
            {
                UserName = dto.Login,
                FullName = dto.FullName,
                CreatingDate = DateTime.Now,

                // Перетворюємо текст ("Ч" або "Ж") у твій Enum TypeGender
                Sex = (ApplicationUser.TypeGender)Enum.Parse(typeof(ApplicationUser.TypeGender), dto.Sex)

            };

            var result = await _userRepository.CreateWithPasswordAsync(newUser, dto.Password);

            if (result.Succeeded)
            {
                result = await _userManager.AddToRoleAsync(newUser, dto.Position);
            }
            else
            {
                var errors = string.Join("\n", result.Errors.Select(e => e.Description));
                throw new Exception($"Помилка створення працівника:\n{errors}");
            }
        }

        public async Task DeleteAsync(string id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<List<StaffListDto>> GetAllStaffAsync()
        {
            var staffList = new List<StaffListDto>();

            // 1. ДОДАЛИ AWAIT і беремо всіх користувачів через DAL
            var users = await _userRepository.GetAllAsync();

            // 2. Проходимося по кожному користувачу
            foreach (var user in users)
            {
                // Отримуємо список його посад (ролей) з Identity
                var roles = await _userManager.GetRolesAsync(user);

                // 3. Пакуємо дані у нашу "коробку" DTO
                staffList.Add(new StaffListDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Sex = user.Sex.ToString(),
                    Login = user.UserName,
                    Position = string.Join(", ", roles)
                });
            }

            return staffList;
        }

        public async Task UpdateAsync(EditStaffDto dto)
        {
            // 1. Знаходимо користувача через НАШ репозиторій
            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null)
            {
                throw new Exception("Помилка: Працівника не знайдено в базі даних.");
            }

            // 2. Оновлюємо основні дані 
            user.FullName = dto.FullName;
            user.UserName = dto.Login;
            user.Sex = (ApplicationUser.TypeGender)Enum.Parse(typeof(ApplicationUser.TypeGender), dto.Sex);

            // 3. Зберігаємо текстові дані (тут залишаємо UserManager, бо він робить валідацію логіна)
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("\n", updateResult.Errors.Select(e => e.Description));
                throw new Exception($"Помилка оновлення даних:\n{errors}");
            }

            // 4. Оновлюємо пароль
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);

                if (!passResult.Succeeded)
                {
                    var errors = string.Join("\n", passResult.Errors.Select(e => e.Description));
                    throw new Exception($"Помилка зміни пароля:\n{errors}");
                }
            }

            // 5. Оновлюємо посаду (роль)
            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!currentRoles.Contains(dto.Position))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, dto.Position);
            }
        }

        public async Task<EditStaffDto> GetByIdAsync(string id)
        {
            // 1. Знаходимо користувача через НАШ репозиторій
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception("Працівника не знайдено.");
            }

            // 2. Ролі беремо через Identity
            var roles = await _userManager.GetRolesAsync(user);

            // 3. Пакуємо в DTO
            var dto = new EditStaffDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Login = user.UserName,
                Sex = user.Sex.ToString(),
                Position = Enumerable.FirstOrDefault(roles) ?? string.Empty
            };

            return dto;
        }
    }
}
