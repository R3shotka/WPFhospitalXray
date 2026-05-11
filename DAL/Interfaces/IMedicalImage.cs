using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DAL.Entity;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IMedicalImage : IRepository<MedicalImage, int>
    {
        Task AddImageAsync(int examinationId, string imagePath, string contentType);
        Task<List<MedicalImage>> GetByExaminationIdAsync(int examinationId);
        
    }
}
