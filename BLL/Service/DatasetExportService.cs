using BLL.DTOs.Datasets;
using BLL.DTOs.RetrainingRequests;
using BLL.Interface;
using DAL.Entity;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace BLL.Service
{
    public class DatasetExportService : IDatasetExportService
    {
        private readonly IRetrainingRequestService _requestService;
        private readonly IApplicationPathService _pathService;

        private readonly int _minimumItems;
        private readonly int _maximumItems;
        private readonly double _maximumBackgroundRatio;
        private readonly double _validationRatio;

        public DatasetExportService(
            IRetrainingRequestService requestService,
            IApplicationPathService pathService,
            IConfiguration configuration)
        {
            _requestService = requestService;
            _pathService = pathService;

            _minimumItems = ReadInt(configuration, "DatasetExport:MinimumItemsForTraining", 300);
            _maximumItems = ReadInt(configuration, "DatasetExport:MaximumItemsForTraining", 500);
            _maximumBackgroundRatio = ReadDouble(configuration, "DatasetExport:MaximumBackgroundRatio", 0.10);
            _validationRatio = ReadDouble(configuration, "DatasetExport:ValidationRatio", 0.20);
        }

        public async Task<DatasetExportResultDto> ExportApprovedRequestsAsync()
        {
            _pathService.EnsureStorageFolders();

            var allRequests = await _requestService.GetAllAsync();

            var approvedRequests = allRequests
                .Where(r => r.Status == RetrainingRequestStatus.Processing)
                .OrderBy(r => r.RequestedAt)
                .ToList();

            var result = new DatasetExportResultDto
            {
                TotalRequests = approvedRequests.Count
            };

            if (approvedRequests.Count == 0)
            {
                result.Warnings.Add("Немає схвалених запитів для формування датасету.");
                return result;
            }

            var candidates = BuildValidCandidates(approvedRequests, result);

            result.ValidItems = candidates.Count;

            var selectedCandidates = SelectTrainingReadyCandidates(candidates, result);

            if (selectedCandidates.Count < _minimumItems)
            {
                result.Warnings.Add(
                    $"Недостатньо придатних даних для донавчання. " +
                    $"Потрібно мінімум {_minimumItems}, доступно {selectedCandidates.Count}.");

                return result;
            }

            string datasetName = $"dataset_{DateTime.Now:yyyyMMdd_HHmmss}";
            string datasetPath = Path.Combine(_pathService.RetrainDataFolder, datasetName);

            string imagesTrainPath = Path.Combine(datasetPath, "images", "train");
            string imagesValPath = Path.Combine(datasetPath, "images", "val");
            string labelsTrainPath = Path.Combine(datasetPath, "labels", "train");
            string labelsValPath = Path.Combine(datasetPath, "labels", "val");

            Directory.CreateDirectory(imagesTrainPath);
            Directory.CreateDirectory(imagesValPath);
            Directory.CreateDirectory(labelsTrainPath);
            Directory.CreateDirectory(labelsValPath);

            result.DatasetPath = datasetPath;

            int validationCount = CalculateValidationCount(selectedCandidates.Count);

            var validationRequestIds = selectedCandidates
                .Where((candidate, index) => index % 5 == 0)
                .Take(validationCount)
                .Select(candidate => candidate.Request.Id)
                .ToHashSet();

            var exportedRequestIds = new List<int>();

            foreach (var candidate in selectedCandidates)
            {
                bool isValidationItem = validationRequestIds.Contains(candidate.Request.Id);

                string targetImagesFolder = isValidationItem ? imagesValPath : imagesTrainPath;
                string targetLabelsFolder = isValidationItem ? labelsValPath : labelsTrainPath;

                await ExportCandidateAsync(candidate, targetImagesFolder, targetLabelsFolder);

                if (isValidationItem)
                {
                    result.ValidationItems++;
                }
                else
                {
                    result.TrainItems++;
                }

                result.ExportedItems++;
                exportedRequestIds.Add(candidate.Request.Id);

                await _requestService.UpdateStatusAsync(
                    candidate.Request.Id,
                    RetrainingRequestStatus.Exported);
            }

            await CreateDataYamlAsync(datasetPath);

            result.SummaryPath = await CreateSummaryAsync(
                datasetPath,
                selectedCandidates,
                result,
                exportedRequestIds);

            result.IsSuccess = true;
            result.IsTrainingReady = true;

            return result;
        }

        private List<DatasetExportCandidate> BuildValidCandidates(
            List<RetrainingRequestDto> requests,
            DatasetExportResultDto result)
        {
            var candidates = new List<DatasetExportCandidate>();

            foreach (var request in requests)
            {
                if (string.IsNullOrWhiteSpace(request.ImagePath) || !File.Exists(request.ImagePath))
                {
                    result.SkippedItems++;
                    result.Warnings.Add($"Запит #{request.Id}: файл знімка не знайдено.");
                    continue;
                }

                string labelFileName = Path.GetFileNameWithoutExtension(request.ImagePath) + ".txt";
                string sourceLabelPath = Path.Combine(_pathService.TempLabelsFolder, labelFileName);

                bool isBackground = request.RequestType == RetrainingRequestType.FalsePositive;

                if (!isBackground && !File.Exists(sourceLabelPath))
                {
                    result.SkippedItems++;
                    result.Warnings.Add($"Запит #{request.Id}: label-файл не знайдено.");
                    continue;
                }

                candidates.Add(new DatasetExportCandidate
                {
                    Request = request,
                    SourceImagePath = request.ImagePath,
                    SourceLabelPath = isBackground ? null : sourceLabelPath,
                    IsBackground = isBackground
                });
            }

            return candidates;
        }

        private List<DatasetExportCandidate> SelectTrainingReadyCandidates(
            List<DatasetExportCandidate> candidates,
            DatasetExportResultDto result)
        {
            var positiveCandidates = candidates
                .Where(c => !c.IsBackground)
                .OrderBy(c => c.Request.RequestedAt)
                .ThenBy(c => c.Request.Id)
                .Take(_maximumItems)
                .ToList();

            int maxBackgroundCountByRatio = _maximumBackgroundRatio <= 0
                ? 0
                : (int)Math.Floor((positiveCandidates.Count * _maximumBackgroundRatio) / (1 - _maximumBackgroundRatio));

            int remainingSlots = _maximumItems - positiveCandidates.Count;

            int backgroundToTake = Math.Min(maxBackgroundCountByRatio, remainingSlots);

            var backgroundCandidates = candidates
                .Where(c => c.IsBackground)
                .OrderBy(c => c.Request.RequestedAt)
                .ThenBy(c => c.Request.Id)
                .Take(backgroundToTake)
                .ToList();

            var selectedCandidates = positiveCandidates
                .Concat(backgroundCandidates)
                .OrderBy(c => c.Request.RequestedAt)
                .ThenBy(c => c.Request.Id)
                .ToList();

            result.FalsePositiveItems = selectedCandidates.Count(c => c.Request.RequestType == RetrainingRequestType.FalsePositive);
            result.FalseNegativeItems = selectedCandidates.Count(c => c.Request.RequestType == RetrainingRequestType.FalseNegative);
            result.CorrectedPositiveItems = selectedCandidates.Count(c => c.Request.RequestType == RetrainingRequestType.CorrectedPositive);

            result.BackgroundRatio = selectedCandidates.Count == 0
                ? 0
                : (double)result.FalsePositiveItems / selectedCandidates.Count;

            int availableBackgroundCount = candidates.Count(c => c.IsBackground);

            if (availableBackgroundCount > backgroundCandidates.Count)
            {
                result.Warnings.Add(
                    $"Частину FalsePositive-запитів не включено в датасет, " +
                    $"щоб частка background-знімків не перевищувала {_maximumBackgroundRatio:P0}. " +
                    $"Доступно: {availableBackgroundCount}, включено: {backgroundCandidates.Count}.");
            }

            if (candidates.Count > selectedCandidates.Count)
            {
                result.Warnings.Add(
                    $"До датасету включено {selectedCandidates.Count} із {candidates.Count} придатних запитів.");
            }

            return selectedCandidates;
        }

        private int CalculateValidationCount(int totalCount)
        {
            int validationCount = (int)Math.Round(totalCount * _validationRatio);

            if (validationCount < 1)
            {
                validationCount = 1;
            }

            if (validationCount >= totalCount)
            {
                validationCount = totalCount - 1;
            }

            return validationCount;
        }

        private static async Task ExportCandidateAsync(
            DatasetExportCandidate candidate,
            string targetImagesFolder,
            string targetLabelsFolder)
        {
            string imageFileName = Path.GetFileName(candidate.SourceImagePath);
            string labelFileName = Path.GetFileNameWithoutExtension(candidate.SourceImagePath) + ".txt";

            string targetImagePath = Path.Combine(targetImagesFolder, imageFileName);
            string targetLabelPath = Path.Combine(targetLabelsFolder, labelFileName);

            await Task.Run(() => File.Copy(candidate.SourceImagePath, targetImagePath, overwrite: true));

            if (candidate.IsBackground)
            {
                await File.WriteAllTextAsync(targetLabelPath, string.Empty);
            }
            else
            {
                await Task.Run(() => File.Copy(candidate.SourceLabelPath!, targetLabelPath, overwrite: true));
            }
        }

        private static async Task CreateDataYamlAsync(string datasetPath)
        {
            string yamlPath = Path.Combine(datasetPath, "data.yaml");

            string yaml = string.Join(Environment.NewLine, new[]
            {
                "path: .",
                "train: images/train",
                "val: images/val",
                "names:",
                "  0: fracture"
            });

            await File.WriteAllTextAsync(yamlPath, yaml);
        }

        private static async Task<string> CreateSummaryAsync(
            string datasetPath,
            List<DatasetExportCandidate> candidates,
            DatasetExportResultDto result,
            List<int> exportedRequestIds)
        {
            string summaryPath = Path.Combine(datasetPath, "dataset_summary.json");

            var summary = new
            {
                GeneratedAt = DateTime.Now,
                DatasetPath = datasetPath,
                TotalItems = result.ExportedItems,
                TrainItems = result.TrainItems,
                ValidationItems = result.ValidationItems,
                FalsePositiveItems = result.FalsePositiveItems,
                FalseNegativeItems = result.FalseNegativeItems,
                CorrectedPositiveItems = result.CorrectedPositiveItems,
                BackgroundRatio = result.BackgroundRatio,
                RequestIds = exportedRequestIds,
                MedicalImageIds = candidates.Select(c => c.Request.MedicalImageId).ToList()
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(summary, options);

            await File.WriteAllTextAsync(summaryPath, json);

            return summaryPath;
        }

        private static int ReadInt(IConfiguration configuration, string key, int defaultValue)
        {
            string? value = configuration[key];

            return int.TryParse(value, out int parsed)
                ? parsed
                : defaultValue;
        }

        private static double ReadDouble(IConfiguration configuration, string key, double defaultValue)
        {
            string? value = configuration[key];

            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)
                ? parsed
                : defaultValue;
        }

        private sealed class DatasetExportCandidate
        {
            public RetrainingRequestDto Request { get; set; } = null!;

            public string SourceImagePath { get; set; } = string.Empty;

            public string? SourceLabelPath { get; set; }

            public bool IsBackground { get; set; }
        }
    }
}
