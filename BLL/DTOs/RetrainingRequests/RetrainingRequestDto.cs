using DAL.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.RetrainingRequests
{
    public class RetrainingRequestDto
    {
        public int Id { get; set; }

        public int ExaminationId { get; set; }

        // Замість цілого об'єкта User, ми передаємо просто ПІБ лікаря для таблиці
        public string DoctorName { get; set; }

        public DateTime RequestedAt { get; set; }

        // Залишаємо тип Enum, щоб у WPF можна було легко перевіряти статус (if status == ...)
        public RetrainingRequestStatus Status { get; set; }

        public string? Comment { get; set; }

        public string ImagePath { get; set; }

        public RetrainingRequestType RequestType { get; set; }

        public string RequestTypeDisplayName { get; set; }
    }
}
