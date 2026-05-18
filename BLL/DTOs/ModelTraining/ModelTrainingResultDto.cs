namespace BLL.DTOs.ModelTraining
{
    public class ModelTrainingResultDto
    {
        public bool IsSuccess { get; set; }

        public int? ModelVersionId { get; set; }

        public string TrainingRunPath { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Output { get; set; } = string.Empty;

        public string ErrorOutput { get; set; } = string.Empty;
    }
}
