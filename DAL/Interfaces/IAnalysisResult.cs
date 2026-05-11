using DAL.Entity;

namespace DAL.Interfaces
{
    public interface IAnalysisResult : IRepository<AnalysisResult, int>
    {
        
        Task UpdateStatusAsync(int analysisResultId, AnalysisReviewStatus status, string? comment);


        // Нові правильні методи
        Task<List<AnalysisResult>> GetByMedicalImageIdAsync(int medicalImageId);
        Task<AnalysisResult?> GetLatestByMedicalImageIdAsync(int medicalImageId);
    }
}
