using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.DBContext;
using DAL.Entity;
using DAL.Interfaces;

namespace DAL.Repositories
{
    public class ExaminationRepository : IExamination
    {
        private readonly ApplicationDBContext _dbContext;

        public ExaminationRepository(ApplicationDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public void Add(Examination entity)
        {
            _dbContext.Examinations.Add(entity);
            _dbContext.SaveChanges();
        }

        public Task AddAsync(Examination entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var examination = _dbContext.Examinations.Find(id);
            if (examination != null)
            {
                _dbContext.Examinations.Remove(examination);
                _dbContext.SaveChanges();
            }
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public List<Examination> GetAll()
        {
            var exams = _dbContext.Examinations;
            return exams.ToList();
        }

        public Task<List<Examination>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Examination? GetById(int id)
        {
            var examination = _dbContext.Examinations.Find(id);
            return examination;
        }

        public Task<Examination?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new NotImplementedException();
        }

        public void Update(Examination entity)
        {
            _dbContext.Examinations.Update(entity);
            _dbContext.SaveChanges();
        }

        public Task UpdateAsync(Examination entity)
        {
            throw new NotImplementedException();
        }
    }
}
