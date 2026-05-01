using BLL.DTOs.MedicalCards;
using BLL.Interface;
using DAL.Entity;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class MedicalCardService : IMedicalCardService
    {
        private readonly IMedicalCard _medicalCardRepository;

        public MedicalCardService(IMedicalCard medicalCardRepository)
        {
            _medicalCardRepository = medicalCardRepository;
        }

        public async Task CreateMedicalCardAsync(CreateMedicalCardDto dto)
        {
            // 1. Захист: Перевіряємо, чи раптом у пацієнта вже немає медкартки
            var existingCard = await _medicalCardRepository.GetByPatientIdAsync(dto.PatientId);
            if (existingCard != null)
            {
                throw new Exception("У цього пацієнта вже є медична картка!");
            }

            // 2. Створюємо нову картку. Дати проставляються автоматично
            var newCard = new MedicalCard
            {
                PatientId = dto.PatientId,
                CreatedDate = DateTime.Now,
                LastUpdated = DateTime.Now
            };

            await _medicalCardRepository.AddAsync(newCard);
        }

        public async Task<MedicalCardDto?> GetMedicalCardByPatientIdAsync(string patientId)
        {
            // Дістаємо з бази
            var card = await _medicalCardRepository.GetByPatientIdAsync(patientId);

            if (card == null)
            {
                return null; // Картки ще немає
            }

            // Перекладаємо з Entity в DTO для UI
            return new MedicalCardDto
            {
                Id = card.Id,
                PatientId = card.PatientId,
                // Знаки питання захищають нас, якщо об'єкта Patient раптом не буде
                PatientFullName = card.Patient?.FullNamePatient ?? "Невідомо",
                CreatedDate = card.CreatedDate.ToString("dd.MM.yyyy"),
                LastUpdated = card.LastUpdated.ToString("dd.MM.yyyy HH:mm"),
                ExaminationsCount = card.Examinations?.Count ?? 0
            };
        }
    }
}
