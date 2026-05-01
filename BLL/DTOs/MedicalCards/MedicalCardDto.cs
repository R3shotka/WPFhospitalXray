using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.MedicalCards
{
    public class MedicalCardDto
    {
        public int Id { get; set; }
        public string PatientId { get; set; }
        public string PatientFullName { get; set; }
        public string CreatedDate { get; set; }
        public string LastUpdated { get; set; }
        public int ExaminationsCount { get; set; } // Скільки знімків вже є в картці
    }
}
