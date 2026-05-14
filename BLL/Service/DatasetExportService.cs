using BLL.DTOs.Datasets;
using BLL.DTOs.RetrainingRequests;
using BLL.Interface;
using DAL.Entity;
using System.Globalization;
using System.IO;

namespace BLL.Service
{
    public class DatasetExportService : IDatasetExportService
    {
        private readonly IRetrainingRequestService _requestService;
        private readonly IApplicationPathService _pathService;

        public DatasetExportService(
            IRetrainingRequestService requestService,
            IApplicationPathService pathService)
        {
            _requestService = requestService;
            _pathService = pathService;
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

            for (int i = 0; i < approvedRequests.Count; i++)
            {
                var request = approvedRequests[i];

                bool isValidationItem = ShouldPutToValidationSet(i, approvedRequests.Count);

                string targetImagesFolder = isValidationItem ? imagesValPath : imagesTrainPath;
                string targetLabelsFolder = isValidationItem ? labelsValPath : labelsTrainPath;

                bool exported = await TryExportRequestAsync(
                    request,
                    targetImagesFolder,
                    targetLabelsFolder,
                    result);

                if (exported)
                {
                    result.ExportedItems++;

                    await _requestService.UpdateStatusAsync(
                        request.Id,
                        RetrainingRequestStatus.Completed);
                }
                else
                {
                    result.SkippedItems++;
                }
            }

            await CreateDataYamlAsync(datasetPath);

            return result;
        }

        private async Task<bool> TryExportRequestAsync(
            RetrainingRequestDto request,
            string targetImagesFolder,
            string targetLabelsFolder,
            DatasetExportResultDto result)
        {
            if (string.IsNullOrWhiteSpace(request.ImagePath) || !File.Exists(request.ImagePath))
            {
                result.Warnings.Add($"Запит #{request.Id}: файл знімка не знайдено.");
                return false;
            }

            string imageFileName = Path.GetFileName(request.ImagePath);
            string labelFileName = Path.GetFileNameWithoutExtension(request.ImagePath) + ".txt";

            string sourceLabelPath = Path.Combine(_pathService.TempLabelsFolder, labelFileName);

            string targetImagePath = Path.Combine(targetImagesFolder, imageFileName);
            string targetLabelPath = Path.Combine(targetLabelsFolder, labelFileName);

            await Task.Run(() => File.Copy(request.ImagePath, targetImagePath, overwrite: true));

            if (File.Exists(sourceLabelPath))
            {
                await Task.Run(() => File.Copy(sourceLabelPath, targetLabelPath, overwrite: true));
                return true;
            }

            if (request.RequestType == RetrainingRequestType.FalsePositive)
            {
                await File.WriteAllTextAsync(targetLabelPath, string.Empty);
                return true;
            }

            result.Warnings.Add($"Запит #{request.Id}: label-файл не знайдено.");
            return false;
        }

        private static bool ShouldPutToValidationSet(int index, int totalCount)
        {
            if (totalCount < 5)
            {
                return false;
            }

            return index % 5 == 0;
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
    }
}