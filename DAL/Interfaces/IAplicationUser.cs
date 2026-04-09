using DAL.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
 

using System.Threading.Tasks;

namespace DAL.Interfaces
{

    public interface IAplicationUser : IRepository<ApplicationUser, string>
    {
        Task<ApplicationUser> FindByLoginAsync(string username);
        Task<IdentityResult> CreateWithPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName);
    }
}
