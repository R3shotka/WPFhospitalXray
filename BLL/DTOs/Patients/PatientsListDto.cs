using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Patients
{
    public class PatientsListDto
    {
        public string Id { get; set; } // Приховано в таблиці, але треба для кнопок

        // Відображаємо в UI:
        public string CreatingDate { get; set; }
        public string FullName { get; set; }
        public string Sex { get; set; }
        public string DateOfBirth { get; set; }

        // Змінили на кількість обстежень:
        public int ExaminationsCount { get; set; } // Тут буде просто число: 0, 1, 5...

        public string Status { get; set; }
    }
}
