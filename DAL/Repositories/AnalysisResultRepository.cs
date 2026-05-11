using DAL.DBContext;
using DAL.Entity;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class AnalysisResultRepository : IAnalysisResult
    {
        private readonly ApplicationDBContext _dbContext;

        public AnalysisResultRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(AnalysisResult entity)
        {
            await _dbContext.AnalysisResults.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.AnalysisResults.FindAsync(id);
            if (entity != null)
            {
                _dbContext.AnalysisResults.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<AnalysisResult>> GetAllAsync()
        {
            return await _dbContext.AnalysisResults
                .Include(a => a.DetectionBoxes)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<AnalysisResult?> GetByIdAsync(int id)
        {
            return await _dbContext.AnalysisResults
                .Include(a => a.DetectionBoxes)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<AnalysisResult>> GetByExaminationIdAsync(int examinationId)
        {
            return await _dbContext.AnalysisResults
                .Include(a => a.DetectionBoxes)
                .Include(a => a.User)
                .Where(a => a.ExaminationId == examinationId)
                .OrderByDescending(a => a.AnalyzedAt)
                .ToListAsync();
        }

        public async Task<AnalysisResult?> GetLatestByExaminationIdAsync(int examinationId)
        {
            return await _dbContext.AnalysisResults
                .Include(a => a.DetectionBoxes)
                .Include(a => a.User)
                .Where(a => a.ExaminationId == examinationId)
                .OrderByDescending(a => a.AnalyzedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(AnalysisResult entity)
        {
            _dbContext.AnalysisResults.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int analysisResultId, AnalysisReviewStatus status, string? comment)
        {
            var entity = await _dbContext.AnalysisResults.FindAsync(analysisResultId);

            if (entity == null)
            {
                throw new Exception("Результат AI-аналізу не знайдено.");
            }

            entity.Status = status;
            entity.DoctorComment = comment;

            await _dbContext.SaveChangesAsync();
        }


        public async Task<List<AnalysisResult>> GetByMedicalImageIdAsync(int medicalImageId)
        {
            return await _dbContext.AnalysisResults
                .Include(a => a.DetectionBoxes)
                .Include(a => a.User)
                .Where(a => a.MedicalImageId == medicalImageId)
                .OrderByDescending(a => a.AnalyzedAt)
                .ToListAsync();
        }

        public async Task<AnalysisResult?> GetLatestByMedicalImageIdAsync(int medicalImageId)
        {
            return await _dbContext.AnalysisResults
                .Include(a => a.DetectionBoxes)
                .Include(a => a.User)
                .Where(a => a.MedicalImageId == medicalImageId)
                .OrderByDescending(a => a.AnalyzedAt)
                .FirstOrDefaultAsync();
        }
    }
}
