using BLL.DTOs.FractureDetections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IAIAnalyzerService
    {
        // Метод для аналізу зображення за допомогою AI
        string ModelName { get; }

        string ModelVersion { get; }

        string ModelPath { get; }

        Task<List<FractureDetectionDto>> AnalyzeImageAsync(string imagePath);
    }
}
