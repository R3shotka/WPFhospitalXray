using BLL.DTOs.FractureDetections;
using DAL.Entity;

namespace BLL.DTOs.AnalysisResults
{
    public class AnalysisResultDto
    {
        public int Id { get; set; }

        public int ExaminationId { get; set; }

        // Новий зв’язок із конкретним знімком
        public int MedicalImageId { get; set; }

        public string ModelName { get; set; }
        public string ModelVersion { get; set; }
        public string ModelPath { get; set; }

        public DateTime AnalyzedAt { get; set; }

        public AnalysisReviewStatus Status { get; set; }

        public string? DoctorComment { get; set; }

        public List<FractureDetectionDto> Detections { get; set; } = new();
    }
}