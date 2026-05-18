using BLL.DTOs.ModelVersions;

namespace BLL.Interface
{
    public interface IModelVersionService
    {
        Task<ModelVersionDto> GetActiveOrCreateDefaultAsync();

        Task<List<ModelVersionDto>> GetAllAsync();

        Task<List<ModelVersionDto>> GetCandidatesAsync();

        Task<int> RegisterCandidateAsync(RegisterModelVersionDto dto);

        Task ActivateAsync(int modelVersionId);
    }
}
