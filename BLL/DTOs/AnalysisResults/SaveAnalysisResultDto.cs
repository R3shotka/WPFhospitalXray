using BLL.DTOs.FractureDetections;

namespace BLL.DTOs.AnalysisResults
{
    public class SaveAnalysisResultDto
    {
        public int ExaminationId { get; set; }
        public string UserId { get; set; }

        public string ModelName { get; set; }
        public string ModelVersion { get; set; }
        public string ModelPath { get; set; }

        public List<FractureDetectionDto> Detections { get; set; } = new();
    }
}