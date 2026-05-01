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
    public class MedicalCardRepository : IMedicalCard

    {
        private readonly ApplicationDBContext _dbContext;

        public MedicalCardRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task AddAsync(MedicalCard entity)
        {
            await _dbContext.MedicalCards.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }



        public async Task UpdateAsync(MedicalCard entity)
        {
            _dbContext.MedicalCards.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var medCard = await _dbContext.MedicalCards.FindAsync(id);
            if (medCard != null)
            {
                _dbContext.MedicalCards.Remove(medCard);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<MedicalCard>> GetAllAsync()
        {
            var medCards = await _dbContext.MedicalCards
                .Include(m => m.Patient)      // Підтягуємо дані пацієнта
                .Include(m => m.Examinations) // Підтягуємо всі його обстеження
                .ToListAsync();

            return medCards;
        }

        public async Task<MedicalCard?> GetByIdAsync(int id)
        {
            return await _dbContext.MedicalCards
                .Include(m => m.Patient)
                .Include(m => m.Examinations)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task<MedicalCard?> GetByPatientIdAsync(string patientId)
        {
            return await _dbContext.MedicalCards
                .Include(m => m.Patient)
                .Include(m => m.Examinations)
                .FirstOrDefaultAsync(m => m.PatientId == patientId);
        }
    }
}
