using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Conclusions
{
    public class CreateConclusionDto
    {
        public int ExaminationId { get; set; } // До якого обстеження
        public string DoctorId { get; set; }   // Хто написав (ID лікаря)
        public string Type { get; set; }       // "Radiologist" або "Surgeon"
        public string ConclusionText { get; set; } // Сам текст
    }
}
