using DAL.DBContext;
using DAL.Entity;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class RetrainingRequestRepository : IRetrainingRequest
    {
        private readonly ApplicationDBContext _dbContext;

        public RetrainingRequestRepository(ApplicationDBContext context)
        {
            _dbContext = context;
        }
        public async Task AddAsync(RetrainingRequest entity)
        {
            await _dbContext.RetrainingRequests.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.RetrainingRequests.FindAsync(id);
            if (entity != null)
            {
                _dbContext.RetrainingRequests.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<List<RetrainingRequest>> GetAllAsync()
        {
            var requests = await _dbContext.RetrainingRequests
                .Include(r => r.RequestByUser)
                .Include(r => r.Examination)
                    .ThenInclude(e => e.Images)
                .ToListAsync();

            return requests;
        }

        public async Task<RetrainingRequest?> GetByIdAsync(int id)
        {
            var request = await _dbContext.RetrainingRequests
                .Include(r => r.RequestByUser)
                .Include(r => r.Examination)
                .FirstOrDefaultAsync(r => r.Id == id);

            return request;
        }

        public async Task<List<RetrainingRequest>> GetByStatusAsync(RetrainingRequestStatus status)
        {
            var requests = await _dbContext.RetrainingRequests
                .Include(r => r.RequestByUser)
                .Include(r => r.Examination)
                .Where(r => r.Status == status)
                .ToListAsync();

            return requests;
        }

        public async Task<List<RetrainingRequest>> GetByUserIdAsync(string userId)
        {
            var requests = await _dbContext.RetrainingRequests
                
                .Include(r => r.Examination)
                .Where(r => r.RequestByUserId == userId)
                .ToListAsync();

            return requests;
        }

        public async Task UpdateAsync(RetrainingRequest entity)
        {
            _dbContext.RetrainingRequests.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
