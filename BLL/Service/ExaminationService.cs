using BLL.DTOs.Examinations;
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
    public class ExaminationService : IExaminationService
    {
        private readonly IExamination _examinationRepository;

        public ExaminationService(IExamination examinationRepository)
        {
            _examinationRepository = examinationRepository;
        }

        public async Task<List<ExaminationListDto>> GetExaminationsByCardIdAsync(int medicalCardId)
        {
            var exams = await _examinationRepository.GetByMedicalCardIdAsync(medicalCardId);

            return exams.Select(e => new ExaminationListDto
            {
                Id = e.Id,
                ExaminationDate = e.ExaminationDate.ToString("dd.MM.yyyy"),

                ImagePath = e.Images.FirstOrDefault()?.FilePath,

                RadiologistConclusion = e.Conclusions
                    .FirstOrDefault(c => c.Type == ConclusionType.Radiologist)?.ConclusionText ?? "Очікує висновку",

                SurgeonConclusion = e.Conclusions
                    .FirstOrDefault(c => c.Type == ConclusionType.Surgeon)?.ConclusionText ?? "Очікує висновку"

            }).ToList();
        }
        public async Task DeleteExaminationAsync(int id)
        {
            // Просто передаємо команду на видалення в репозиторій бази даних
            await _examinationRepository.DeleteAsync(id);
        }
        public async Task CreateEmptyExaminationAsync(int medicalCardId)
        {
            var newExam = new DAL.Entity.Examination
            {
                MedicalCardId = medicalCardId,
                ExaminationDate = DateTime.Now
            };

            // Використовуємо твій репозиторій для збереження
            await _examinationRepository.AddAsync(newExam);
        }

        

    }


}
