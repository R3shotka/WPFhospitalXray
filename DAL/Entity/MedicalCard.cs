using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entity
{
    public class MedicalCard
    {
        public int Id { get; set; }





        // Зв'язок з пацієнтом (One-to-One)
        public string PatientId { get; set; }
        public Patient Patient { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public List<Examination> Examinations { get; set; } = new();
    }
}
