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
                MedicalImageId = r.MedicalImageId,

                DoctorName = r.RequestByUser?.FullName ?? "Невідомий лікар",

                RequestedAt = r.RequestedAt,
                Status = r.Status,
                Comment = r.Comment,

                ImagePath = r.MedicalImage?.FilePath ?? r.Examination?.Images?.FirstOrDefault()?.FilePath,

                RequestType = r.RequestType,
                RequestTypeDisplayName = GetRequestTypeDisplayName(r.RequestType),
            }).ToList();

            return dtos;
        }

        public async Task<bool> CreateRequestAsync(int examinationId, string userId, RetrainingRequestType requestType, string comment = null)
        {
            bool alreadyExists = await _repository.HasActiveRequestByExaminationIdAsync(examinationId);

            if (alreadyExists)
            {
                return false;
            }

            var newRequest = new RetrainingRequest
            {
                ExaminationId = examinationId,
                RequestByUserId = userId,
                RequestedAt = DateTime.Now,
                Status = RetrainingRequestStatus.Pending,
                RequestType = requestType,
                Comment = comment
            };

            await _repository.AddAsync(newRequest);

            return true;
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

        private static string GetRequestTypeDisplayName(RetrainingRequestType type)
        {
            return type switch
            {
                RetrainingRequestType.CorrectedPositive => "Неточна локалізація перелому",
                RetrainingRequestType.FalsePositive => "Хибне виявлення перелому",
                RetrainingRequestType.FalseNegative => "Пропущений перелом",
                _ => "Невизначений тип"
            };
        }


        public async Task<bool> CreateRequestForImageAsync(
    int examinationId,
    int medicalImageId,
    string userId,
    RetrainingRequestType requestType,
    string comment = null)
        {
            bool alreadyExists = await _repository.HasActiveRequestByMedicalImageIdAsync(medicalImageId);

            if (alreadyExists)
            {
                return false;
            }

            var newRequest = new RetrainingRequest
            {
                ExaminationId = examinationId,
                MedicalImageId = medicalImageId,
                RequestByUserId = userId,
                RequestedAt = DateTime.Now,
                Status = RetrainingRequestStatus.Pending,
                RequestType = requestType,
                Comment = comment
            };

            await _repository.AddAsync(newRequest);

            return true;
        }
    }
}
