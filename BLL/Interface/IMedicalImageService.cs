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
        Task AddImageAsync(int examinationId, string imagePath, string contentType);

        Task UpdateImagePathAsync(int examinationId, string imagePath);

        Task<List<MedicalImageDto>> GetImagesByExaminationIdAsync(int examinationId);

        Task DeleteImageAsync(int imageId);
    }
}
