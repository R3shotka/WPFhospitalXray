using System;
using System.Collections.Generic;

namespace DAL.Entity
{
    public enum AnalysisReviewStatus
    {
        Pending,
        Confirmed,
        Rejected,
        Corrected
    }

    public class AnalysisResult
    {
        public int Id { get; set; }

        public int ExaminationId { get; set; }
        public Examination Examination { get; set; }


        // Новий правильний зв’язок:
        // AI-результат належить конкретному рентгенівському знімку.
        public int? MedicalImageId { get; set; }
        public MedicalImage? MedicalImage { get; set; }



        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string ModelName { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;

        public DateTime AnalyzedAt { get; set; } = DateTime.Now;

        public AnalysisReviewStatus Status { get; set; } = AnalysisReviewStatus.Pending;

        public string? DoctorComment { get; set; }

        public List<DetectionBox> DetectionBoxes { get; set; } = new();
    }
}