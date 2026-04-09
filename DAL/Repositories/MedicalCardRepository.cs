using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Interfaces;
using DAL.Entity;
using DAL.DBContext;

namespace DAL.Repositories
{
    public class MedicalCardRepository : IMedicalCard

    {
        private readonly ApplicationDBContext _dbContext;

        public MedicalCardRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public void Add(MedicalCard entity)
        {
            _dbContext.MedicalCards.Add(entity);
            _dbContext.SaveChanges();
        }

        public Task AddAsync(MedicalCard entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var medCard = _dbContext.MedicalCards.Find(id);
            if (medCard != null)
            {
                _dbContext.MedicalCards.Remove(medCard);
                _dbContext.SaveChanges();
            }
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public List<MedicalCard> GetAll()
        {
            return _dbContext.MedicalCards.ToList();
        }

        public Task<List<MedicalCard>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public MedicalCard? GetById(int id)
        {
            var medCard = _dbContext.MedicalCards.Find(id);
            return medCard;
        }

        public Task<MedicalCard?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(MedicalCard entity)
        {
            _dbContext.MedicalCards.Update(entity);
            _dbContext.SaveChanges();
        }

        public Task UpdateAsync(MedicalCard entity)
        {
            throw new NotImplementedException();
        }
    }
}
