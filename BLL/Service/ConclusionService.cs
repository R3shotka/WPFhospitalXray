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
    }
}
