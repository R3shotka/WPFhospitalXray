using BLL.DTOs.RetrainingRequests;
using DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IRetrainingRequestService
    {
        // Отримати всі запити (перетворені в DTO для UI)
        Task<List<RetrainingRequestDto>> GetAllAsync();

        // Створити новий запит (викликатиме лікар з вікна розмітки)
        Task CreateRequestAsync(int examinationId, string userId, string comment = null);

        // Оновити статус (викликатиме Адмін: Схвалити/Відхилити)
        Task UpdateStatusAsync(int requestId, RetrainingRequestStatus newStatus);
    }
}
