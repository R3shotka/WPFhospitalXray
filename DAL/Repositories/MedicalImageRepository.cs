using DAL.DBContext;
using DAL.Entity;
using DAL.Interfaces;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class MedicalImageRepository : IMedicalImage
    {
        private readonly ApplicationDBContext _dbContext;

        public MedicalImageRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        // Тільки асинхронні методи
        public async Task AddAsync(MedicalImage entity)
        {
            await _dbContext.MedicalImages.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<MedicalImage?> GetByIdAsync(int id)
        {
            return await _dbContext.MedicalImages.FindAsync(id);
        }

        public async Task<List<MedicalImage>> GetAllAsync()
        {
            return await _dbContext.MedicalImages.ToListAsync();
        }

        public async Task UpdateAsync(MedicalImage entity)
        {
            _dbContext.MedicalImages.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var medImage = await _dbContext.MedicalImages.FindAsync(id);
            if (medImage != null)
            {
                _dbContext.MedicalImages.Remove(medImage);
                await _dbContext.SaveChangesAsync();
            }
        }

        

        public async Task AddImageAsync(int examinationId, string imagePath, string contentType)
        {
            var newImage = new MedicalImage
            {
                ExaminationId = examinationId,
                FilePath = imagePath,
                FileName = Path.GetFileName(imagePath),
                ContentType = contentType,
                UploadedAt = DateTime.Now
            };

            await _dbContext.MedicalImages.AddAsync(newImage);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<MedicalImage>> GetByExaminationIdAsync(int examinationId)
        {
            return await _dbContext.MedicalImages
                .Where(m => m.ExaminationId == examinationId)
                .OrderBy(m => m.UploadedAt)
                .ToListAsync();
        }
    }
}
