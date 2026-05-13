using BLL.Interface;
using System;
using System.IO;

namespace BLL.Service
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly IApplicationPathService _pathService;

        public ImageStorageService(IApplicationPathService pathService)
        {
            _pathService = pathService;
        }

        public async Task<string> SaveImageAsync(int examinationId, string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                throw new ArgumentException("Шлях до вихідного файлу не може бути порожнім.", nameof(sourceFilePath));
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Вихідний файл знімка не знайдено.", sourceFilePath);
            }

            _pathService.EnsureStorageFolders();

            string fileExtension = Path.GetExtension(sourceFilePath);
            string uniqueFileName = $"Exam_{examinationId}_{Guid.NewGuid()}{fileExtension}";
            string destinationFilePath = Path.Combine(_pathService.ImagesFolder, uniqueFileName);

            await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, overwrite: true));

            return destinationFilePath;
        }

        public async Task DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            if (!File.Exists(imagePath))
            {
                return;
            }

            await Task.Run(() => File.Delete(imagePath));
        }

        public string GetContentType(string fileExtension)
        {
            return fileExtension.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };
        }
    }
}