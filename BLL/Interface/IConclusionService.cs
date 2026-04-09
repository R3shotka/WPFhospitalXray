using BLL.DTOs.Conclusions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IConclusionService
    {
        Task CreateConclusionAsync(CreateConclusionDto dto);

        // Отримати всі висновки для конкретного обстеження (щоб показати їх на екрані)
        Task<List<ConclusionListDto>> GetConclusionsByExaminationIdAsync(int examinationId);

        // Видалити висновок (раптом лікар помилився)
        Task DeleteConclusionAsync(int id);
    }
}
