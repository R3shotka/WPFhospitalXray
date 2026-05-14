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

            services.AddSingleton<IApplicationPathService, ApplicationPathService>();
            services.AddScoped<IImageStorageService, ImageStorageService>();

            // --- ДОДАЄМО НАШІ НОВІ СЕРВІСИ ---

            // 1. Реєструємо репозиторій з бази даних (DAL)
            services.AddScoped<IAplicationUser, ApplicationUserRepository>();

            // 2. Реєструємо нашу бізнес-логіку персоналу (BLL)
            services.AddScoped<IApplicationUserService, ApplicationUserService>();

            services.AddScoped<IPatient, PatientRepository>();
            services.AddScoped<IPatientService, PatientService>();

            // --- МЕДИЧНІ КАРТКИ ---
            services.AddScoped<IMedicalCard, MedicalCardRepository>();
            services.AddScoped<IMedicalCardService, MedicalCardService>();

            // --- ВИСНОВКИ (Conclusions) ---
            // (Перевір, чи правильно я написав назву твого інтерфейсу DAL для висновків)
            services.AddScoped<IConclusion, ConclusionRepository>();
            services.AddScoped<IConclusionService, ConclusionService>();

            services.AddScoped<IExamination, ExaminationRepository>();
            services.AddScoped<IExaminationService, ExaminationService>();

            // У шарі DAL:
            services.AddScoped<IMedicalImage, MedicalImageRepository>();

            // У шарі BLL:
            services.AddScoped<IMedicalImageService, MedicalImageService>();

            services.AddScoped<IAIAnalyzerService, AIAnalyzerService>();

            services.AddScoped<IDatasetService, DatasetService>();
            services.AddScoped<IDatasetExportService, DatasetExportService>();

            services.AddScoped<IRetrainingRequest, RetrainingRequestRepository>();
            services.AddScoped<IRetrainingRequestService, RetrainingRequestService>();

            services.AddScoped<IAnalysisResult, AnalysisResultRepository>();
            services.AddScoped<IAnalysisResultService, AnalysisResultService>();

            // ----------------------------------

            // Реєстрація вікон
            services.AddTransient<MainWindow>();
            services.AddTransient<AdminPanel>(); // <--- ДОДАЙ ЦЕЙ РЯДОК
            services.AddTransient<EditMed>();
            services.AddTransient<MedicalCardWindow>();

            // 💡 ПІДКАЗКА: Якщо в тебе вікна створення і редагування (рис. 2 і рис. 3) 
            // це окремі вікна, їх теж треба тут зареєструвати. Наприклад:
            // services.AddTransient<CreateStaff>();
            // services.AddTransient<EditStaff>();

            return services;
        }
    }
}