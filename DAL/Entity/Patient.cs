using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.Entity.ApplicationUser;

namespace DAL.Entity
{
    public class Patient
    {

        public int Id { get; set; }
        public string FullNamePatient { get; set; } = string.Empty;
        public TypeGender Sex { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatingDate { get; set; } = DateTime.Now;


        // Зв'язок з медкартою
        public int MedicalCardId { get; set; }
        public MedicalCard MedicalCard { get; set; }
   
        public string Status { get; set; } = string.Empty;


      

    }
}
