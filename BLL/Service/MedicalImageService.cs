using BLL.DTOs.MedicalImages;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Interface;

namespace BLL.Service
{
    public class MedicalImageService : IMedicalImageService
    {
        private readonly IMedicalImage _medicalImageRepository;

        // Впроваджуємо залежність DAL-репозиторію
        public MedicalImageService(IMedicalImage medicalImageRepository)
        {
            _medicalImageRepository = medicalImageRepository;
        }

        public async Task UpdateImagePathAsync(int examinationId, string imagePath)
        {
            // Викликаємо наш розумний метод з репозиторію, який сам вирішить: 
            // оновити існуючий чи створити новий знімок
            await _medicalImageRepository.UpdateImagePathAsync(examinationId, imagePath);
        }

        public async Task<List<MedicalImageDto>> GetImagesByExaminationIdAsync(int examinationId)
        {
            var images = await _medicalImageRepository.GetByExaminationIdAsync(examinationId);

            return images
                .Select(img => new MedicalImageDto
                {
                    Id = img.Id,
                    ExaminationId = img.ExaminationId,
                    FileName = img.FileName,
                    FilePath = img.FilePath,
                    UploadedAt = img.UploadedAt.ToString("dd.MM.yyyy HH:mm")
                })
                .ToList();
        }

        public async Task DeleteImageAsync(int imageId)
        {
            await _medicalImageRepository.DeleteAsync(imageId);
        }

        public async Task AddImageAsync(int examinationId, string imagePath, string contentType)
        {
            await _medicalImageRepository.AddImageAsync(examinationId, imagePath, contentType);
        }
    }
}
