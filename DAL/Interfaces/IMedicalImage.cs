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
        void UpdateImagePath(int examinationImageId, string newImagePath);
    }
}
