using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AppUsers
{
    public class CreateStaffDto
    {
        public string FullName { get; set; }
        public string Position { get; set; } // Посада (Роль)
        public string Sex { get; set; }      // "Ч" або "Ж"
        public string Login { get; set; }    // UserName в Identity
        public string Password { get; set; } // Обов'язковий пароль
    }
}
