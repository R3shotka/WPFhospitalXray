using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.MedicalCards
{
    public class CreateMedicalCardDto
    {
        [Required(ErrorMessage = "Не вказано пацієнта для створення медкартки.")]
        public string PatientId { get; set; } // Нам потрібен тільки паспорт, решта генерується сама
    }
}
