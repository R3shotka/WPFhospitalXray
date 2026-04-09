using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL.Interfaces;
using System.Threading.Tasks;
using DAL.Entity;
using DAL.DBContext;

namespace DAL.Repositories
{
    public class MedicalImageRepository : IMedicalImage
    {
        private readonly ApplicationDBContext _dbContext;
        public MedicalImageRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }
        public void Add(MedicalImage entity)
        {
            _dbContext.MedicalImages.Add(entity);
            _dbContext.SaveChanges();
        }

        public Task AddAsync(MedicalImage entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var medImage = _dbContext.MedicalImages.Find(id);
            if (medImage != null)
            {
                _dbContext.MedicalImages.Remove(medImage);
                _dbContext.SaveChanges();
            }

        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public List<MedicalImage> GetAll()
        {
            return _dbContext.MedicalImages.ToList();
        }

        public Task<List<MedicalImage>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public MedicalImage? GetById(int id)
        {
            var medImage = _dbContext.MedicalImages.Find(id);
            return medImage;
        }

        public Task<MedicalImage?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(MedicalImage entity)
        {
            _dbContext.MedicalImages.Update(entity);
            _dbContext.SaveChanges();
        }

        public Task UpdateAsync(MedicalImage entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateImagePath(int examinationImageId, string newImagePath)
        {
            var medImage = _dbContext.MedicalImages.FirstOrDefault(m => m.ExaminationId == examinationImageId);
            if (medImage != null)
            {
                medImage.FilePath = newImagePath;
                _dbContext.SaveChanges();
            }
        }
    }
}
