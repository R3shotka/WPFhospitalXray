using DAL.Entity;

namespace DAL.Interfaces
{
    public interface IAnalysisResult : IRepository<AnalysisResult, int>
    {
        Task<List<AnalysisResult>> GetByExaminationIdAsync(int examinationId);
        Task<AnalysisResult?> GetLatestByExaminationIdAsync(int examinationId);
        Task UpdateStatusAsync(int analysisResultId, AnalysisReviewStatus status, string? comment);
    }
}
