using BLL.DTOs.MedicalImages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IMedicalImageService
    {
        Task UpdateImagePathAsync(int examinationId, string imagePath);

        // Метод для отримання всіх знімків конкретного обстеження
        Task<List<MedicalImageDto>> GetImagesByExaminationIdAsync(int examinationId);

        // Метод для видалення знімка (на майбутнє)
        Task DeleteImageAsync(int imageId);
    }
}
