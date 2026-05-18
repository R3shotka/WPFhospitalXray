using BLL.DTOs.ModelTraining;

namespace BLL.Interface
{
    public interface IModelTrainingService
    {
        Task<ModelTrainingResultDto> StartTrainingAsync();
    }
}
