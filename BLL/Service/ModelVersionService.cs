using BLL.DTOs.ModelVersions;
using BLL.Interface;
using DAL.Entity;
using DAL.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BLL.Service
{
    public class ModelVersionService : IModelVersionService
    {
        private readonly IModelVersion _repository;
        private readonly IApplicationPathService _pathService;
        private readonly IConfiguration _configuration;

        public ModelVersionService(
            IModelVersion repository,
            IApplicationPathService pathService,
            IConfiguration configuration)
        {
            _repository = repository;
            _pathService = pathService;
            _configuration = configuration;
        }

        public async Task<ModelVersionDto> GetActiveOrCreateDefaultAsync()
        {
            var activeModel = await _repository.GetActiveAsync();

            if (activeModel != null)
            {
                await EnsurePtPathAsync(activeModel);
                return MapToDto(activeModel);
            }

            string modelName = _configuration["AIModel:ModelName"] ?? "YOLOv8 fracture detector";
            string version = _configuration["AIModel:ModelVersion"] ?? "best1";
            string modelFileName = _configuration["AIModel:ModelFileName"] ?? "best1.onnx";
            string onnxPath = _pathService.GetModelPath(modelFileName);

            var existingModel = await _repository.GetByVersionAsync(version);

            if (existingModel != null)
            {
                await _repository.SetActiveAsync(existingModel.Id);
                existingModel = await _repository.GetByIdAsync(existingModel.Id);

                await EnsurePtPathAsync(existingModel!);
                return MapToDto(existingModel!);
            }

            var defaultModel = new ModelVersion
            {
                ModelName = modelName,
                Version = version,
                OnnxPath = onnxPath,
                PtPath = GetDefaultPtPath(modelFileName),
                Status = ModelVersionStatus.Active,
                IsActive = true,
                ActivatedAt = DateTime.Now,
                Comment = "Початкова активна модель, зареєстрована з конфігурації застосунку."
            };

            await _repository.AddAsync(defaultModel);

            return MapToDto(defaultModel);
        }

        public async Task<List<ModelVersionDto>> GetAllAsync()
        {
            var models = await _repository.GetAllAsync();

            return models.Select(MapToDto).ToList();
        }

        public async Task<List<ModelVersionDto>> GetCandidatesAsync()
        {
            var models = await _repository.GetCandidatesAsync();

            return models.Select(MapToDto).ToList();
        }

        public async Task<int> RegisterCandidateAsync(RegisterModelVersionDto dto)
        {
            var model = new ModelVersion
            {
                ModelName = dto.ModelName,
                Version = dto.Version,
                OnnxPath = dto.OnnxPath,
                PtPath = dto.PtPath,
                TrainingDatasetPath = dto.TrainingDatasetPath,
                OldDatasetPath = dto.OldDatasetPath,
                TrainingRunPath = dto.TrainingRunPath,
                ExperimentName = dto.ExperimentName,
                Precision = dto.Precision,
                Recall = dto.Recall,
                Map50 = dto.Map50,
                Map5095 = dto.Map5095,
                Status = ModelVersionStatus.Candidate,
                IsActive = false,
                Comment = dto.Comment
            };

            await _repository.AddAsync(model);

            return model.Id;
        }

        public async Task ActivateAsync(int modelVersionId)
        {
            await _repository.SetActiveAsync(modelVersionId);
        }

        private string? GetDefaultPtPath(string modelFileName)
        {
            string ptFileName = Path.ChangeExtension(modelFileName, ".pt");
            string ptPath = _pathService.GetModelPath(ptFileName);

            return File.Exists(ptPath) ? ptPath : null;
        }

        private async Task EnsurePtPathAsync(ModelVersion model)
        {
            if (!string.IsNullOrWhiteSpace(model.PtPath) && File.Exists(model.PtPath))
            {
                return;
            }

            string inferredPtPath = Path.ChangeExtension(model.OnnxPath, ".pt");

            if (!File.Exists(inferredPtPath))
            {
                return;
            }

            model.PtPath = inferredPtPath;

            await _repository.UpdateAsync(model);
        }

        private static ModelVersionDto MapToDto(ModelVersion model)
        {
            return new ModelVersionDto
            {
                Id = model.Id,
                ModelName = model.ModelName,
                Version = model.Version,
                OnnxPath = model.OnnxPath,
                PtPath = model.PtPath,
                TrainingDatasetPath = model.TrainingDatasetPath,
                OldDatasetPath = model.OldDatasetPath,
                TrainingRunPath = model.TrainingRunPath,
                ExperimentName = model.ExperimentName,
                Precision = model.Precision,
                Recall = model.Recall,
                Map50 = model.Map50,
                Map5095 = model.Map5095,
                CreatedAt = model.CreatedAt,
                ActivatedAt = model.ActivatedAt,
                Status = model.Status,
                IsActive = model.IsActive,
                Comment = model.Comment
            };
        }
    }
}
