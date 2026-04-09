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
    public class PatientRepository : IPatient
    {
        private readonly ApplicationDBContext _context;

        public PatientRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        // 1. Додавання (Add в пам'ять, SaveChangesAsync в базу)
        public async Task AddAsync(Patient entity)
        {
            _context.Patients.Add(entity);
            await _context.SaveChangesAsync();
        }

        // 2. Отримання всіх (Справжній асинхронний ToListAsync)
        public async Task<List<Patient>> GetAllAsync()
        {
            return await _context.Patients
                   .Include(p => p.MedicalCard)           // Підтягуємо медкартку
                   .ThenInclude(mc => mc.Examinations)    // Підтягуємо список обстежень
                   .ToListAsync();
        }

        // 3. Пошук за ID (Використовуємо FirstOrDefaultAsync)
        public async Task<Patient?> GetByIdAsync(string id)
        {
            return await _context.Patients
                   .Include(p => p.MedicalCard)           // Зазвичай ID шукають, щоб щось змінити, краще підтягнути картку одразу
                   .FirstOrDefaultAsync(p => p.Id == id);
        }

        // 4. Оновлення (Update в пам'ять, SaveChangesAsync в базу)
        public async Task UpdateAsync(Patient entity)
        {
            _context.Patients.Update(entity);
            await _context.SaveChangesAsync();
        }

        // 5. Видалення (Знаходимо і видаляємо асинхронно)
        public async Task DeleteAsync(string id)
        {
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
        }


    }
}
