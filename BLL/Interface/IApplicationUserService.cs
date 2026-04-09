using BLL.DTOs.AppUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.DTOs.AppUsers;

namespace BLL.Interface
{
    public interface IApplicationUserService
    {
        Task<List<StaffListDto>> GetAllStaffAsync();
        // Створення нового працівника
        Task CreateAsync(CreateStaffDto dto);

        // НОВИЙ МЕТОД: Редагування працівника
        Task UpdateAsync(EditStaffDto dto);

        // НОВИЙ МЕТОД: Видалення працівника
        Task DeleteAsync(string id);

        Task<EditStaffDto> GetByIdAsync(string id);
    }
}
