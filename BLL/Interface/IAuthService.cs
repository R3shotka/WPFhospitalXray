using BLL.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IAuthService
    {
        Task LogoutAsync();
        Task<AuthResultDto> LoginAsync(string username, string password);
    }
}
