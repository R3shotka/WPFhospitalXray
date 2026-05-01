using BLL.DTOs.MedicalCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IMedicalCardService
    {
        // Для кнопки "Зберегти" у вікні створення пацієнта
        Task CreateMedicalCardAsync(CreateMedicalCardDto dto);

        // Для подвійного кліку по таблиці
        Task<MedicalCardDto?> GetMedicalCardByPatientIdAsync(string patientId);
    }
}
