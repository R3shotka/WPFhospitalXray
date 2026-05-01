using BLL.DTOs.RetrainingRequests;
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
    public class RetrainingRequestService : IRetrainingRequestService
    {
        private readonly IRetrainingRequest _repository;

        public RetrainingRequestService(IRetrainingRequest repository)
        {
            _repository = repository;
        }

        public async Task<List<RetrainingRequestDto>> GetAllAsync()
        {
            // Беремо всі сутності з бази
            var requests = await _repository.GetAllAsync();

            // Перетворюємо (Мапимо) їх у DTO
            var dtos = requests.Select(r => new RetrainingRequestDto
            {
                Id = r.Id,
                ExaminationId = r.ExaminationId,

                // Витягуємо ПІБ лікаря безпечно (якщо раптом користувач видалений)
                DoctorName = r.RequestByUser?.FullName ?? "Невідомий лікар",

                RequestedAt = r.RequestedAt,
                Status = r.Status,
                Comment = r.Comment,
                ImagePath = r.Examination?.Images?.FirstOrDefault()?.FilePath
            }).ToList();

            return dtos;
        }

        public async Task CreateRequestAsync(int examinationId, string userId, string comment = null)
        {
            var newRequest = new RetrainingRequest
            {
                ExaminationId = examinationId,
                RequestByUserId = userId,
                RequestedAt = DateTime.Now,
                Status = RetrainingRequestStatus.Pending, // Автоматично ставимо "Очікує"
                Comment = comment
            };

            await _repository.AddAsync(newRequest);
        }

        public async Task UpdateStatusAsync(int requestId, RetrainingRequestStatus newStatus)
        {
            // Знаходимо запит у базі
            var request = await _repository.GetByIdAsync(requestId);

            if (request != null)
            {
                // Оновлюємо тільки статус
                request.Status = newStatus;
                await _repository.UpdateAsync(request);
            }
            else
            {
                throw new Exception("Запит не знайдено в базі даних.");
            }
        }
    }
}
