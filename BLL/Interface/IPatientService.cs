using BLL.DTOs.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.Patients;

namespace BLL.Interface
{
    public interface IPatientService
    {
        // Повертає список наших готових DTO для таблиці
        Task<List<PatientsListDto>> GetAllPatientsAsync();

        // Метод для отримання одного пацієнта (щоб заповнити вікно редагування)
        Task<EditPatientDto> GetPatientByIdAsync(string id);

        Task CreatePatientAsync(CreatePatientDto dto);
        Task UpdatePatientAsync(EditPatientDto dto);
        Task DeletePatientAsync(string id);
    }
}
