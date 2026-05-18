using DAL.DBContext;
using DAL.Entity;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ModelVersionRepository : IModelVersion
    {
        private readonly ApplicationDBContext _dbContext;

        public ModelVersionRepository(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(ModelVersion entity)
        {
            await _dbContext.ModelVersions.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.ModelVersions.FindAsync(id);

            if (entity != null)
            {
                _dbContext.ModelVersions.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<ModelVersion>> GetAllAsync()
        {
            return await _dbContext.ModelVersions
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<ModelVersion?> GetByIdAsync(int id)
        {
            return await _dbContext.ModelVersions
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<ModelVersion?> GetActiveAsync()
        {
            return await _dbContext.ModelVersions
                .Where(m => m.IsActive && m.Status == ModelVersionStatus.Active)
                .OrderByDescending(m => m.ActivatedAt ?? m.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ModelVersion?> GetByVersionAsync(string version)
        {
            return await _dbContext.ModelVersions
                .FirstOrDefaultAsync(m => m.Version == version);
        }

        public async Task<List<ModelVersion>> GetCandidatesAsync()
        {
            return await _dbContext.ModelVersions
                .Where(m => m.Status == ModelVersionStatus.Candidate)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(ModelVersion entity)
        {
            _dbContext.ModelVersions.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetActiveAsync(int modelVersionId)
        {
            var models = await _dbContext.ModelVersions.ToListAsync();
            var selectedModel = models.FirstOrDefault(m => m.Id == modelVersionId);

            if (selectedModel == null)
            {
                throw new Exception("Версію моделі не знайдено.");
            }

            foreach (var model in models.Where(m => m.IsActive))
            {
                model.IsActive = false;
                model.Status = ModelVersionStatus.Archived;
            }

            selectedModel.IsActive = true;
            selectedModel.Status = ModelVersionStatus.Active;
            selectedModel.ActivatedAt = DateTime.Now;

            await _dbContext.SaveChangesAsync();
        }
    }
}
