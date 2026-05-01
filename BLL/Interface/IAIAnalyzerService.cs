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
        Task<List<FractureDetectionDto>> AnalyzeImageAsync(string imagePath);
    }
}
