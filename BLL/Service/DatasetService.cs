using BLL.Interface;
using System.IO;

namespace BLL.Service
{
    public class DatasetService : IDatasetService
    {
        private readonly IApplicationPathService _pathService;

        public DatasetService(IApplicationPathService pathService)
        {
            _pathService = pathService;
        }

        public async Task SaveSegmentationDataAsync(string originalImagePath, string yoloLabelString)
        {
            _pathService.EnsureStorageFolders();

            string labelPath = GetLabelPath(originalImagePath);

            await File.WriteAllTextAsync(labelPath, yoloLabelString);
        }

        public async Task DeleteTempLabelAsync(string originalImagePath)
        {
            string labelPath = GetLabelPath(originalImagePath);

            if (File.Exists(labelPath))
            {
                await Task.Run(() => File.Delete(labelPath));
            }
        }

        public async Task SaveEmptyLabelAsync(string originalImagePath)
        {
            _pathService.EnsureStorageFolders();

            string labelPath = GetLabelPath(originalImagePath);

            await File.WriteAllTextAsync(labelPath, string.Empty);
        }

        private string GetLabelPath(string originalImagePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(originalImagePath) + ".txt";

            return Path.Combine(_pathService.TempLabelsFolder, fileName);
        }
    }
}