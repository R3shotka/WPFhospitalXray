using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Examinations
{
    public class ExaminationListDto
    {
        public int Id { get; set; }
        public string ExaminationDate { get; set; }

        // Окремі поля для таблиці
        public string RadiologistConclusion { get; set; }
        public string SurgeonConclusion { get; set; }
        public string ImagePath { get; set; }
    }
}
