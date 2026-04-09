using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BLL.DTOs.Patients
{
    public class EditPatientDto
    {
        [Required(ErrorMessage = "Номер паспорта є обов'язковим!")]
        [StringLength(14, MinimumLength = 6, ErrorMessage = "Довжина паспорта має бути від 6 до 14 символів.")]
        public string Id { get; set; }

        [Required(ErrorMessage = "ПІБ пацієнта є обов'язковим!")]
        [MinLength(5, ErrorMessage = "ПІБ занадто коротке.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Оберіть стать пацієнта!")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Дата народження є обов'язковою!")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Номер телефону є обов'язковим!")]
        [Phone(ErrorMessage = "Невірний формат номеру телефону.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Адреса є обов'язковою!")]
        public string Address { get; set; }
    }
}
