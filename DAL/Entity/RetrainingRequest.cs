using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entity
{
    public enum RetrainingRequestStatus
    {
        Pending,    // Очікує перевірки адміном (Новий запит від лікаря)
        Processing, // Схвалено адміном (Чекає на запуск ШІ)
        Completed,  // УСПІШНО ВИКОРИСТАНО В НАВЧАННІ (ШІ вже на ньому навчився)
        Cancelled   // Відхилено адміном (Брак, погана розмітка)
    }

    public class RetrainingRequest
    {
        public int Id { get; set; }
        public int ExaminationId { get; set; }
        public Examination Examination { get; set; }

        public string RequestByUserId { get; set; }
        public ApplicationUser RequestByUser { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public string? Comment { get; set; }
        public RetrainingRequestStatus Status { get; set; }
    }
}
