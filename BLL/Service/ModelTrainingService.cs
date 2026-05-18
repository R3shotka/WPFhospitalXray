using BLL.DTOs.ModelTraining;
using BLL.DTOs.ModelVersions;
using BLL.Interface;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.Json;

namespace BLL.Service
{
    public class ModelTrainingService : IModelTrainingService
    {
        private readonly IApplicationPathService _pathService;
        private readonly IModelVersionService _modelVersionService;
        private readonly IConfiguration _configuration;

        public ModelTrainingService(
            IApplicationPathService pathService,
            IModelVersionService modelVersionService,
            IConfiguration configuration)
        {
            _pathService = pathService;
            _modelVersionService = modelVersionService;
            _configuration = configuration;
        }

        public async Task<ModelTrainingResultDto> StartTrainingAsync()
        {
            var activeModel = await _modelVersionService.GetActiveOrCreateDefaultAsync();

            if (string.IsNullOrWhiteSpace(activeModel.PtPath) || !File.Exists(activeModel.PtPath))
            {
                return new ModelTrainingResultDto
                {
                    IsSuccess = false,
                    Message = "Для донавчання потрібен .pt-файл активної моделі. Для ONNX inference він не потрібен, але Ultralytics навчається саме з .pt."
                };
            }

            string pythonExePath = GetRequiredConfiguration("Training:PythonExePath");
            string trainingScriptPath = GetRequiredConfiguration("Training:TrainingScriptPath");
            string oldDatasetPath = GetRequiredConfiguration("Training:OldDatasetPath");
            string outputRoot = _configuration["Training:OutputRoot"] ?? Path.Combine(_pathService.BaseDataFolder, "TrainingRuns");
            string experimentName = _configuration["Training:ExperimentName"] ?? "E5_mix_67old_33new_full";
            string device = _configuration["Training:Device"] ?? "0";
            string epochs = _configuration["Training:Epochs"] ?? "80";
            string dryRun = _configuration["Training:DryRun"] ?? "false";

            string newDatasetPath = FindLatestDatasetYaml();
            string runPath = Path.Combine(outputRoot, $"run_{DateTime.Now:yyyyMMdd_HHmmss}");

            if (!File.Exists(pythonExePath))
            {
                return Failed($"Python exe не знайдено: {pythonExePath}", runPath);
            }

            if (!File.Exists(trainingScriptPath))
            {
                return Failed($"Скрипт донавчання не знайдено: {trainingScriptPath}", runPath);
            }

            if (!File.Exists(oldDatasetPath))
            {
                return Failed($"Старий data.yaml не знайдено: {oldDatasetPath}", runPath);
            }

            Directory.CreateDirectory(runPath);

            string arguments =
                $"\"{trainingScriptPath}\" " +
                $"--old-data \"{oldDatasetPath}\" " +
                $"--old-model \"{activeModel.PtPath}\" " +
                $"--new-data \"{newDatasetPath}\" " +
                $"--output-root \"{runPath}\" " +
                $"--experiment \"{experimentName}\" " +
                $"--device \"{device}\" " +
                $"--epochs \"{epochs}\" " +
                $"--dry-run \"{dryRun}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExePath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string errorOutput = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return new ModelTrainingResultDto
                {
                    IsSuccess = false,
                    TrainingRunPath = runPath,
                    Output = output,
                    ErrorOutput = errorOutput,
                    Message = $"Донавчання завершилось з помилкою. ExitCode: {process.ExitCode}"
                };
            }

            string resultPath = Path.Combine(runPath, "training_result.json");

            if (!File.Exists(resultPath))
            {
                return new ModelTrainingResultDto
                {
                    IsSuccess = true,
                    TrainingRunPath = runPath,
                    Output = output,
                    ErrorOutput = errorOutput,
                    Message = "Скрипт завершився успішно, але training_result.json не знайдено. Модель-кандидат не зареєстровано автоматично."
                };
            }

            var trainingResult = await ReadTrainingResultAsync(resultPath);

            int modelVersionId = await _modelVersionService.RegisterCandidateAsync(new RegisterModelVersionDto
            {
                ModelName = trainingResult.ModelName,
                Version = trainingResult.Version,
                OnnxPath = trainingResult.OnnxPath,
                PtPath = trainingResult.PtPath,
                TrainingDatasetPath = newDatasetPath,
                OldDatasetPath = oldDatasetPath,
                TrainingRunPath = runPath,
                ExperimentName = experimentName,
                Precision = trainingResult.Precision,
                Recall = trainingResult.Recall,
                Map50 = trainingResult.Map50,
                Map5095 = trainingResult.Map5095,
                Comment = "Модель-кандидат створена після локального донавчання."
            });

            return new ModelTrainingResultDto
            {
                IsSuccess = true,
                ModelVersionId = modelVersionId,
                TrainingRunPath = runPath,
                Output = output,
                ErrorOutput = errorOutput,
                Message = "Донавчання завершено. Нову модель зареєстровано як кандидат."
            };
        }

        private string FindLatestDatasetYaml()
        {
            if (!Directory.Exists(_pathService.RetrainDataFolder))
            {
                throw new DirectoryNotFoundException($"Папку датасетів не знайдено: {_pathService.RetrainDataFolder}");
            }

            var latestDataset = Directory.GetDirectories(_pathService.RetrainDataFolder, "dataset_*")
                .OrderByDescending(Directory.GetCreationTime)
                .FirstOrDefault();

            if (latestDataset == null)
            {
                throw new FileNotFoundException("Не знайдено сформований датасет для донавчання.");
            }

            string dataYamlPath = Path.Combine(latestDataset, "data.yaml");

            if (!File.Exists(dataYamlPath))
            {
                throw new FileNotFoundException("У останньому датасеті не знайдено data.yaml.", dataYamlPath);
            }

            return dataYamlPath;
        }

        private string GetRequiredConfiguration(string key)
        {
            return _configuration[key]
                ?? throw new InvalidOperationException($"Не задано конфігурацію '{key}'.");
        }

        private static ModelTrainingResultDto Failed(string message, string runPath)
        {
            return new ModelTrainingResultDto
            {
                IsSuccess = false,
                TrainingRunPath = runPath,
                Message = message
            };
        }

        private static async Task<TrainingResultFile> ReadTrainingResultAsync(string resultPath)
        {
            await using var stream = File.OpenRead(resultPath);

            var result = await JsonSerializer.DeserializeAsync<TrainingResultFile>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result ?? throw new InvalidOperationException("training_result.json має некоректний формат.");
        }

        private sealed class TrainingResultFile
        {
            public string ModelName { get; set; } = "YOLOv8 fracture detector";

            public string Version { get; set; } = string.Empty;

            public string OnnxPath { get; set; } = string.Empty;

            public string PtPath { get; set; } = string.Empty;

            public double? Precision { get; set; }

            public double? Recall { get; set; }

            public double? Map50 { get; set; }

            public double? Map5095 { get; set; }
        }
    }
}
