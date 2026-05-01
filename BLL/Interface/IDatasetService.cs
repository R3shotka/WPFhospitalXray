using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public  interface IDatasetService
    {
        Task SaveSegmentationDataAsync(string originalImagePath, string yoloLabelString);
        Task DeleteTempLabelAsync(string originalImagePath);
    }
}
