using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AppUsers
{
    public class ApplicationUserDto
    {
        public string? FullName { get; set; }

        // Відповідає випадаючому списку "Посада" (наприклад, Лікар, Реєстратор)
        public string? Position { get; set; }

        // Відповідає випадаючому списку "Стать" ("Ч" або "Ж")
        public string? Sex { get; set; }

        // Відповідає полю "Логін" (UserName в Identity)
        public string? Login { get; set; }

        // Відповідає полю "Пароль"
        public string? Password { get; set; }
    }
}
