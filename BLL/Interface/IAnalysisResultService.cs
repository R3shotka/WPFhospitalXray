using BLL.DTOs.AnalysisResults;
using DAL.Entity;

namespace BLL.Interface
{
    public interface IAnalysisResultService
    {
        Task<int> SaveAnalysisResultAsync(SaveAnalysisResultDto dto);
        Task<AnalysisResultDto?> GetLatestByExaminationIdAsync(int examinationId);
        Task UpdateStatusAsync(int analysisResultId, AnalysisReviewStatus status, string? comment);
    }
}