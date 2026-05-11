using DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL.Interfaces
{
    public interface IRetrainingRequest : IRepository<RetrainingRequest, int>
    {
        Task<List<RetrainingRequest>> GetByUserIdAsync(string userId);

        Task<List<RetrainingRequest>> GetByStatusAsync(RetrainingRequestStatus status);

        // Старий метод поки залишаємо для сумісності
        

        // Новий правильний метод
        Task<bool> HasActiveRequestByMedicalImageIdAsync(int medicalImageId);
    }
}
