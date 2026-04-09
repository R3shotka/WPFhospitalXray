using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entity
{
    public enum ConclusionType
    {
        Radiologist,
        Surgeon
    }
    public class Conclusion
    {
        public int Id { get; set; }

        // До якого обстеження належить цей висновок
        public int ExaminationId { get; set; }
        public Examination Examination { get; set; }

        // Який лікар його написав
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        // Роль лікаря в цьому висновку
        public ConclusionType Type { get; set; }

        public string ConclusionText { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
