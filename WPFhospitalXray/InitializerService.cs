using DAL.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFhospitalXray
{
    public class InitializerService : IInitializerService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public InitializerService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task InitializeAsync()
        {
            string[] roles = { "Admin", "Nurse", "Radiologist", "Surgeon" };

            foreach (var role in roles)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

            var admin = await _userManager.FindByNameAsync("admin");
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    FullName = "Адміністратор",
                    CreatingDate = DateTime.UtcNow,
                    Sex = ApplicationUser.TypeGender.Ч
                };

                var res = await _userManager.CreateAsync(admin, "Admin@123");
                if (!res.Succeeded)
                    throw new Exception(string.Join("; ", res.Errors.Select(er => er.Description)));
            }

            if (!await _userManager.IsInRoleAsync(admin, "Admin"))
                await _userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
