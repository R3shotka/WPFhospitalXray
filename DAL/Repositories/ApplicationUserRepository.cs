using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Interfaces;
using DAL.DBContext;
using Microsoft.AspNetCore.Identity;
using DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories

{
    
    public class ApplicationUserRepository : IAplicationUser
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ApplicationUserRepository(ApplicationDBContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task AddAsync(ApplicationUser entity)
        {
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }


        public async Task UpdateAsync(ApplicationUser entity)
        {
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
        }



        public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName)
        {
            bool roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!roleResult.Succeeded)
                {
                    return roleResult;
                }
            }
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result;
        }

        public async Task<IdentityResult> CreateWithPasswordAsync(ApplicationUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return result;
        }

        

        public async Task<ApplicationUser> FindByLoginAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
             return user;
        }

       
    }
}
