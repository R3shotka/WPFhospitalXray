using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entity
{
    public class MedicalImage
    {
        public int Id { get; set; }

        // Зв’язок із конкретним обстеженням
        public int ExaminationId { get; set; }
        public Examination Examination { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // шлях у файловій системі або URL
        public string ContentType { get; set; } = string.Empty; // наприклад, "image/png"
        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
