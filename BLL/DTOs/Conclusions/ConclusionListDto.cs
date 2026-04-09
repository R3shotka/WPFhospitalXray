using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Conclusions
{
    public class ConclusionListDto
    {
        public int Id { get; set; }
        public string DoctorName { get; set; } // Тут буде вже готове ПІБ лікаря
        public string Type { get; set; }       // "Радіолог" або "Хірург" (перекладено для UI)
        public string ConclusionText { get; set; }
        public string CreatedAt { get; set; }  // Красиво відформатована дата (напр. "24.10.2023 14:30")
    }
}
