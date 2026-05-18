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
            string runPath = string.Empty;

            try
            {
                var activeModel = await _modelVersionService.GetActiveOrCreateDefaultAsync();

                if (string.IsNullOrWhiteSpace(activeModel.PtPath) || !File.Exists(activeModel.PtPath))
                {
                    return new ModelTrainingResultDto
                    {
                        IsSuccess = false,
                        Message = "Для донавчання потрібен .pt-файл активної моделі. ONNX використовується для inference, але Ultralytics навчається саме з .pt."
                    };
                }

                string pythonExePath = GetRequiredConfiguration("Training:PythonExePath");
                string trainingScriptPath = ResolveConfiguredPath(GetRequiredConfiguration("Training:TrainingScriptPath"));
                string oldDatasetPath = ResolveConfiguredPath(GetRequiredConfiguration("Training:OldDatasetPath"));
                string outputRoot = ResolveTrainingOutputRoot();
                string experimentName = _configuration["Training:ExperimentName"] ?? "E5_mix_67old_33new_full";
                string device = _configuration["Training:Device"] ?? "0";
                string epochs = _configuration["Training:Epochs"] ?? "80";
                string dryRun = _configuration["Training:DryRun"] ?? "false";

                string newDatasetPath = FindLatestDatasetYaml();
                runPath = Path.Combine(outputRoot, $"run_{DateTime.Now:yyyyMMdd_HHmmss}");

                if (!IsExecutableAvailable(pythonExePath))
                {
                    return Failed($"Python не знайдено: {pythonExePath}", runPath);
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

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                startInfo.ArgumentList.Add(trainingScriptPath);
                startInfo.ArgumentList.Add("--old-data");
                startInfo.ArgumentList.Add(oldDatasetPath);
                startInfo.ArgumentList.Add("--old-model");
                startInfo.ArgumentList.Add(activeModel.PtPath);
                startInfo.ArgumentList.Add("--new-data");
                startInfo.ArgumentList.Add(newDatasetPath);
                startInfo.ArgumentList.Add("--output-root");
                startInfo.ArgumentList.Add(runPath);
                startInfo.ArgumentList.Add("--experiment");
                startInfo.ArgumentList.Add(experimentName);
                startInfo.ArgumentList.Add("--device");
                startInfo.ArgumentList.Add(device);
                startInfo.ArgumentList.Add("--epochs");
                startInfo.ArgumentList.Add(epochs);
                startInfo.ArgumentList.Add("--dry-run");
                startInfo.ArgumentList.Add(dryRun);

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
                        Message = "Скрипт завершився успішно, але training_result.json не знайдено. Якщо це був dry-run, модель-кандидат коректно не реєструється."
                    };
                }

                var trainingResult = await ReadTrainingResultAsync(resultPath);

                if (string.IsNullOrWhiteSpace(trainingResult.OnnxPath) || !File.Exists(trainingResult.OnnxPath))
                {
                    return Failed("Скрипт завершився, але ONNX-файл нової моделі не знайдено.", runPath, output, errorOutput);
                }

                if (string.IsNullOrWhiteSpace(trainingResult.PtPath) || !File.Exists(trainingResult.PtPath))
                {
                    return Failed("Скрипт завершився, але PT-файл нової моделі не знайдено.", runPath, output, errorOutput);
                }

                if (string.IsNullOrWhiteSpace(trainingResult.Version))
                {
                    return Failed("Скрипт завершився, але не повернув версію нової моделі.", runPath, output, errorOutput);
                }

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
                    Comment = "Модель-кандидат створена після локального донавчання. Основні метрики взято з combined_test."
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
            catch (Exception ex)
            {
                return new ModelTrainingResultDto
                {
                    IsSuccess = false,
                    TrainingRunPath = runPath,
                    Message = $"Помилка запуску донавчання: {ex.Message}"
                };
            }
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
                throw new FileNotFoundException("В останньому датасеті не знайдено data.yaml.", dataYamlPath);
            }

            return dataYamlPath;
        }

        private string ResolveTrainingOutputRoot()
        {
            string configuredPath = _configuration["Training:OutputRoot"] ?? Path.Combine(_pathService.BaseDataFolder, "TrainingRuns");

            if (Path.IsPathRooted(configuredPath))
            {
                return configuredPath;
            }

            return Path.Combine(_pathService.BaseDataFolder, configuredPath);
        }

        private static string ResolveConfiguredPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.Combine(AppContext.BaseDirectory, path);
        }

        private string GetRequiredConfiguration(string key)
        {
            return _configuration[key]
                ?? throw new InvalidOperationException($"Не задано конфігурацію '{key}'.");
        }

        private static bool IsExecutableAvailable(string executablePath)
        {
            if (Path.IsPathRooted(executablePath) || executablePath.Contains(Path.DirectorySeparatorChar) || executablePath.Contains(Path.AltDirectorySeparatorChar))
            {
                return File.Exists(executablePath);
            }

            return !string.IsNullOrWhiteSpace(executablePath);
        }

        private static ModelTrainingResultDto Failed(string message, string runPath, string output = "", string errorOutput = "")
        {
            return new ModelTrainingResultDto
            {
                IsSuccess = false,
                TrainingRunPath = runPath,
                Output = output,
                ErrorOutput = errorOutput,
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
