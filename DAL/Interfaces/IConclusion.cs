using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entity;

namespace DAL.Interfaces
{
    public interface IConclusion : IRepository<Conclusion, int>
    {
        Task<List<Conclusion>> GetByExaminationIdAsync(int examinationId);
    }
}
