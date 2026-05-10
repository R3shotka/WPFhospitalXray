using BLL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class DatasetService : IDatasetService
    {
        public async Task SaveSegmentationDataAsync(string originalImagePath, string yoloLabelString)
        {
            // 1. Папка "Карантин" для розміток, які ще не перевірив Адмін
            string tempLabelsFolder = @"D:\HospitalServer\TempLabels";

            // Якщо папки немає - створюємо
            Directory.CreateDirectory(tempLabelsFolder);

            // 2. Робимо так, щоб ім'я текстового файлу ідеально збігалося з іменем картинки
            // Наприклад: якщо картинка "Exam_123_Guid.jpg", то файл буде "Exam_123_Guid.txt"
            string fileName = Path.GetFileNameWithoutExtension(originalImagePath) + ".txt";
            string destLabelPath = Path.Combine(tempLabelsFolder, fileName);

            // 3. Зберігаємо ТІЛЬКИ текст. 
            // Картинку ми поки що не дублюємо, щоб не засмічувати диск до перевірки Адміном!
            await File.WriteAllTextAsync(destLabelPath, yoloLabelString);
        }

        public async Task DeleteTempLabelAsync(string originalImagePath)
        {
            string tempLabelsFolder = @"D:\HospitalServer\TempLabels";
            string fileName = Path.GetFileNameWithoutExtension(originalImagePath) + ".txt";
            string destLabelPath = Path.Combine(tempLabelsFolder, fileName);

            if (File.Exists(destLabelPath))
            {
                await Task.Run(() => File.Delete(destLabelPath));
            }
        }

        public async Task SaveEmptyLabelAsync(string originalImagePath)
        {
            string tempLabelsFolder = @"D:\HospitalServer\TempLabels";

            Directory.CreateDirectory(tempLabelsFolder);

            string fileName = Path.GetFileNameWithoutExtension(originalImagePath) + ".txt";
            string destLabelPath = Path.Combine(tempLabelsFolder, fileName);

            await File.WriteAllTextAsync(destLabelPath, string.Empty);
        }
    }
}
