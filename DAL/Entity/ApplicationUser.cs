using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DAL.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } 
        public TypeGender Sex { get; set; }


        public DateTime CreatingDate { get; set; }
        public enum TypeGender
        {
            Ч,
            Ж
        }
    }
}
