using BLL.Interface;
using BLL.Service;
using DAL.DBContext;
using DAL.Entity;
using DAL.Interfaces; // Додаємо namespace твого DAL (перевір, чи він такий)
using DAL.Repositories; // Додаємо namespace твого DAL
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFhospitalXray
{
    public static class ConfigureUIService
    {
        public static IServiceCollection AddUiServices(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDBContext>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDBContext>()
                .AddDefaultTokenProviders();

            // Твої існуючі сервіси
            services.AddTransient<IInitializerService, InitializerService>();
            services.AddTransient<IAuthService, BLL.Service.AuthService>();

            // --- ДОДАЄМО НАШІ НОВІ СЕРВІСИ ---

            // 1. Реєструємо репозиторій з бази даних (DAL)
            services.AddScoped<IAplicationUser, ApplicationUserRepository>();

            // 2. Реєструємо нашу бізнес-логіку персоналу (BLL)
            services.AddScoped<IApplicationUserService, ApplicationUserService>();

            services.AddTransient<IPatient, PatientRepository>();
            services.AddTransient<IPatientService, PatientService>();

            // ----------------------------------

            // Реєстрація вікон
            services.AddTransient<MainWindow>();
            services.AddTransient<AdminPanel>(); // <--- ДОДАЙ ЦЕЙ РЯДОК
            services.AddTransient<EditMed>();

            // 💡 ПІДКАЗКА: Якщо в тебе вікна створення і редагування (рис. 2 і рис. 3) 
            // це окремі вікна, їх теж треба тут зареєструвати. Наприклад:
            // services.AddTransient<CreateStaff>();
            // services.AddTransient<EditStaff>();

            return services;
        }
    }
}