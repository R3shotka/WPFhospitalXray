using BLL.DTOs.AnalysisResults;
using BLL.DTOs.FractureDetections;
using BLL.Interface;
using DAL.Entity;
using DAL.Interfaces;

namespace BLL.Service
{
    public class AnalysisResultService : IAnalysisResultService
    {
        private readonly IAnalysisResult _repository;

        public AnalysisResultService(IAnalysisResult repository)
        {
            _repository = repository;
        }

        public async Task<int> SaveAnalysisResultAsync(SaveAnalysisResultDto dto)
        {
            var entity = new AnalysisResult
            {
                ExaminationId = dto.ExaminationId,
                UserId = dto.UserId,
                ModelName = dto.ModelName,
                ModelVersion = dto.ModelVersion,
                ModelPath = dto.ModelPath,
                AnalyzedAt = DateTime.Now,
                Status = AnalysisReviewStatus.Pending,
                DetectionBoxes = dto.Detections.Select(d => new DetectionBox
                {
                    X = d.X,
                    Y = d.Y,
                    Width = d.Width,
                    Height = d.Height,
                    Confidence = d.Confidence,
                    ClassName = d.ClassName,
                    IsManuallyCorrected = false,
                    Source = "AI"
                }).ToList()
            };

            await _repository.AddAsync(entity);

            return entity.Id;
        }

        public async Task<AnalysisResultDto?> GetLatestByExaminationIdAsync(int examinationId)
        {
            var entity = await _repository.GetLatestByExaminationIdAsync(examinationId);

            if (entity == null)
            {
                return null;
            }

            return new AnalysisResultDto
            {
                Id = entity.Id,
                ExaminationId = entity.ExaminationId,
                ModelName = entity.ModelName,
                ModelVersion = entity.ModelVersion,
                ModelPath = entity.ModelPath,
                AnalyzedAt = entity.AnalyzedAt,
                Status = entity.Status,
                DoctorComment = entity.DoctorComment,
                Detections = entity.DetectionBoxes.Select(b => new FractureDetectionDto
                {
                    ClassName = b.ClassName,
                    Confidence = b.Confidence,
                    X = b.X,
                    Y = b.Y,
                    Width = b.Width,
                    Height = b.Height
                }).ToList()
            };
        }

        public async Task UpdateStatusAsync(int analysisResultId, AnalysisReviewStatus status, string? comment)
        {
            await _repository.UpdateStatusAsync(analysisResultId, status, comment);
        }
    }
}