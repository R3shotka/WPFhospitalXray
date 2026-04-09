using DAL.DBContext;
using DAL.Entity;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ConclusionRepository : IConclusion
    {
        private readonly ApplicationDBContext _dbContext;

        public ConclusionRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task AddAsync(Conclusion entity)
        {
            await _dbContext.Conclusions.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.Conclusions.FindAsync(id);
            if (entity != null)
            {
                _dbContext.Conclusions.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Conclusion>> GetAllAsync()
        {
            var conclusions = await _dbContext.Conclusions
                .Include(c => c.Doctor)
                .ToListAsync();
            return conclusions;
        }

        public async Task<List<Conclusion>> GetByExaminationIdAsync(int examinationId)
        {
            return await _dbContext.Conclusions
                .Include(c => c.Doctor) // Підтягуємо лікаря, щоб у вікні показати "Радіолог: Іван Іванов"
                .Where(c => c.ExaminationId == examinationId)
                .ToListAsync();
        }

        public async Task<Conclusion?> GetByIdAsync(int id)
        {
            return await _dbContext.Conclusions
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Conclusion entity)
        {
            _dbContext.Conclusions.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
