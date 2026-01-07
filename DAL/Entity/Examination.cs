using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entity
{
    public class Examination
    {
        public int Id { get; set; }

        // зв'язок з медкартою
        public int MedicalCardId { get; set; }
        public MedicalCard MedicalCard { get; set; }


        public DateTime ExaminationDate { get; set; }
        public string DoctorConclusion { get; set; }

        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        public List<MedicalImage> Images { get; set; } = new();


    }
}
