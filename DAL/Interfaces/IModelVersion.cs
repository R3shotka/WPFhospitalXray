using DAL.Entity;

namespace DAL.Interfaces
{
    public interface IModelVersion : IRepository<ModelVersion, int>
    {
        Task<ModelVersion?> GetActiveAsync();

        Task<ModelVersion?> GetByVersionAsync(string version);

        Task<List<ModelVersion>> GetCandidatesAsync();

        Task SetActiveAsync(int modelVersionId);
    }
}
