using BLL.DTOs.AnalysisResults;
using DAL.Entity;

namespace BLL.Interface
{
    public interface IAnalysisResultService
    {
        Task<int> SaveAnalysisResultAsync(SaveAnalysisResultDto dto);

        // Старий метод поки залишаємо для сумісності
        Task<AnalysisResultDto?> GetLatestByExaminationIdAsync(int examinationId);

        // Новий правильний метод
        Task<AnalysisResultDto?> GetLatestByMedicalImageIdAsync(int medicalImageId);

        Task UpdateStatusAsync(int analysisResultId, AnalysisReviewStatus status, string? comment);
    } 
}