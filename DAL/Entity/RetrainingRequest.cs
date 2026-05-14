using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entity
{
    public enum RetrainingRequestStatus
    {
        Pending,     // Очікує перевірки адміністратором
        Processing,  // Схвалено адміністратором і готово до експорту в датасет
        Exported,    // Експортовано у сформований датасет
        Completed,   // Використано у реальному донавчанні моделі
        Cancelled    // Відхилено адміністратором
    }
    public enum RetrainingRequestType
    {
        CorrectedPositive, // AI знайшов перелом, але лікар скоригував розмітку
        FalsePositive,     // AI знайшов перелом, але перелому немає
        FalseNegative      // AI не знайшов перелом, але лікар вручну його розмітив
    }

    public class RetrainingRequest
    {
        public int Id { get; set; }
        public int ExaminationId { get; set; }
        public Examination Examination { get; set; }



        // Новий правильний зв’язок:
        // запит на донавчання стосується конкретного знімка.
        public int MedicalImageId { get; set; }
        public MedicalImage MedicalImage { get; set; }



        public string RequestByUserId { get; set; }
        public ApplicationUser RequestByUser { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public string? Comment { get; set; }
        public RetrainingRequestStatus Status { get; set; }
        public RetrainingRequestType RequestType { get; set; }
    }
}
