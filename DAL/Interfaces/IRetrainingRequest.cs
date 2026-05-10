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
        // Додатковий метод: отримати всі запити конкретного лікаря
        Task<List<RetrainingRequest>> GetByUserIdAsync(string userId);

        // Додатковий метод: отримати всі запити за певним статусом (напр. тільки "На доопрацювання")
        Task<List<RetrainingRequest>> GetByStatusAsync(RetrainingRequestStatus status);

        

        Task<bool> HasActiveRequestByExaminationIdAsync(int examinationId);

    }
}
