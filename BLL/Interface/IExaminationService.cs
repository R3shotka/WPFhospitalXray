using BLL.DTOs.Examinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IExaminationService
    {
        Task<List<ExaminationListDto>> GetExaminationsByCardIdAsync(int medicalCardId);
        // Згодом додамо сюди методи для створення обстеження
        Task CreateEmptyExaminationAsync(int medicalCardId);

        Task DeleteExaminationAsync(int id);
    }
}
