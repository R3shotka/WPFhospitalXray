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
    public class ExaminationRepository : IExamination
    {
        private readonly ApplicationDBContext _dbContext;

        public ExaminationRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Examination entity)
        {
            await _dbContext.Examinations.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.Examinations.FindAsync(id);
            if (entity != null)
            {
                _dbContext.Examinations.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<Examination>> GetAllAsync()
        {
            return await _dbContext.Examinations
                .Include(e => e.Images)
                .Include(e => e.Conclusions)
                .ToListAsync();
        }

        public async Task<Examination?> GetByIdAsync(int id)
        {
            return await _dbContext.Examinations
                .Include(e => e.Images)
                .Include(e => e.Conclusions)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateAsync(Examination entity)
        {
            _dbContext.Examinations.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Examination>> GetByMedicalCardIdAsync(int medicalCardId)
        {
            return await _dbContext.Examinations
                .Include(e => e.Images)
                .Include(e => e.Conclusions)
                .Where(e => e.MedicalCardId == medicalCardId)
                .ToListAsync();
        }
    }
}

