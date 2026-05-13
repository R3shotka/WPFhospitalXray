using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IImageStorageService
    {
        Task<string> SaveImageAsync(int examinationId, string sourceFilePath);

        Task DeleteImageAsync(string imagePath);

        string GetContentType(string fileExtension);
    }
}
