using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AppUsers
{
    public class StaffListDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; } // Тут будемо зберігати роль (Лікар, Адмін)
        public string Sex { get; set; }
        public string Login { get; set; }
    }
}
