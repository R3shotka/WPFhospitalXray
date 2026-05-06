using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.AppUsers
{
    public class EditStaffDto
    {
        public string Id { get; set; }       // Обов'язкове поле для пошуку!

        [Required(ErrorMessage = "ПІБ працівника є обов'язковим.")]
        [MinLength(5, ErrorMessage = "ПІБ працівника занадто коротке.")]
        [StringLength(100, ErrorMessage = "ПІБ працівника не може перевищувати 100 символів.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Оберіть посаду працівника.")]
        public string Position { get; set; }
        [Required(ErrorMessage = "Оберіть стать працівника.")]
        public string Sex { get; set; }
        [Required(ErrorMessage = "Логін є обов'язковим.")]
        [MinLength(4, ErrorMessage = "Логін має містити щонайменше 4 символи.")]
        [StringLength(50, ErrorMessage = "Логін не може перевищувати 50 символів.")]
        public string Login { get; set; }
        public string Password { get; set; } // Може бути порожнім

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
            {
                yield return new ValidationResult(
                    "Новий пароль має містити щонайменше 6 символів.",
                    new[] { nameof(Password) });
            }
        }
    }
}
