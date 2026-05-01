using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.MedicalImages
{
    public class MedicalImageDto
    {
        public int Id { get; set; }
        public int ExaminationId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        // Форматована дата для красивого відображення в інтерфейсі (якщо знадобиться)
        public string UploadedAt { get; set; } = string.Empty;
    }
}
