using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AppUsers
{
    public class EditStaffDto
    {
        public string Id { get; set; }       // Обов'язкове поле для пошуку!
        public string FullName { get; set; }
        public string Position { get; set; }
        public string Sex { get; set; }
        public string Login { get; set; }
        public string Password { get; set; } // Може бути порожнім
    }
}
