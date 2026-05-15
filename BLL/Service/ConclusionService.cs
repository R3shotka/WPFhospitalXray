using BLL.DTOs.Conclusions;
using BLL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entity;
using DAL.Interfaces;

namespace BLL.Service
{
    public class ConclusionService : IConclusionService
    {
        private readonly IConclusion _conclusionRepository;
        private readonly IRolePermissionService _rolePermissionService;

        public ConclusionService(
            IConclusion conclusionRepository,
            IRolePermissionService rolePermissionService)
        {
            _conclusionRepository = conclusionRepository;
            _rolePermissionService = rolePermissionService;
        }
        public async Task CreateConclusionAsync(CreateConclusionDto dto)
        {
            var conclusion = new Conclusion
            {
                ExaminationId = dto.ExaminationId,
                DoctorId = dto.DoctorId,
                ConclusionText = dto.ConclusionText,
                CreatedAt = DateTime.Now,
                Type = (ConclusionType)Enum.Parse(typeof(ConclusionType), dto.Type)
            };

            await _conclusionRepository.AddAsync(conclusion);
        }

        public async Task DeleteConclusionAsync(int id)
        {
            await _conclusionRepository.DeleteAsync(id);

        }

        public async Task<List<ConclusionListDto>> GetConclusionsByExaminationIdAsync(int examinationId)
        {
            // Дістаємо висновки з бази (Repository вже підтягне дані про Doctor завдяки Include)
            var conclusions = await _conclusionRepository.GetByExaminationIdAsync(examinationId);

            // Перетворюємо їх у красиві DTO для екрану
            return conclusions.Select(c => new ConclusionListDto
            {
                Id = c.Id,

                // Якщо лікар є, беремо його ім'я, якщо ні - пишемо заглушку
                DoctorName = c.Doctor?.FullName ?? "Невідомий лікар",

                // Перекладаємо для українського інтерфейсу
                Type = c.Type == ConclusionType.Radiologist ? "Радіолог" : "Хірург",

                ConclusionText = c.ConclusionText,
                CreatedAt = c.CreatedAt.ToString("dd.MM.yyyy HH:mm")
            }).ToList();
        }

        public async Task SaveOrUpdateConclusionAsync(int examinationId, string currentRole, string text, string doctorId)
        {
            // 1. Визначаємо правильний тип (перетворюємо українську/англійську роль на Enum)
            ConclusionType targetType;

            if (_rolePermissionService.CanWriteRadiologistConclusion(currentRole))
            {
                targetType = ConclusionType.Radiologist;
            }
            else if (_rolePermissionService.CanWriteSurgeonConclusion(currentRole))
            {
                targetType = ConclusionType.Surgeon;
            }
            else
            {
                throw new UnauthorizedAccessException("Поточна роль не має права зберігати лікарський висновок.");
            }

            // 2. Шукаємо, чи є вже такий висновок у цього обстеження
            var allConclusions = await _conclusionRepository.GetByExaminationIdAsync(examinationId);
            var existingConclusion = allConclusions.FirstOrDefault(c => c.Type == targetType);

            if (existingConclusion != null)
            {
                // 3. Якщо вже є - просто оновлюємо текст!
                existingConclusion.ConclusionText = text;

                // Припускаю, що у твоєму репозиторії є метод UpdateAsync. Якщо ні - напиши, ми додамо.
                await _conclusionRepository.UpdateAsync(existingConclusion);
            }
            else
            {
                // 4. Якщо немає - створюємо новий через твій DTO
                var dto = new CreateConclusionDto
                {
                    ExaminationId = examinationId,
                    DoctorId = doctorId,
                    Type = targetType.ToString(), // "Radiologist" або "Surgeon"
                    ConclusionText = text
                };
                await CreateConclusionAsync(dto);
            }
        }
    }
}
