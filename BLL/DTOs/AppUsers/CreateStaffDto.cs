using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AppUsers
{
    public class CreateStaffDto
    {
        [Required(ErrorMessage = "ПІБ працівника є обов'язковим.")]
        [MinLength(5, ErrorMessage = "ПІБ працівника занадто коротке.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Оберіть посаду працівника.")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Оберіть стать працівника.")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Логін є обов'язковим.")]
        [MinLength(4, ErrorMessage = "Логін має містити щонайменше 4 символи.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль є обов'язковим.")]
        [MinLength(6, ErrorMessage = "Пароль має містити щонайменше 6 символів.")]
        public string Password { get; set; }
    }
}
