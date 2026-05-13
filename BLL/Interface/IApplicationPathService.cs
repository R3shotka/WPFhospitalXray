using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IApplicationPathService
    {
        string BaseDataFolder { get; }

        string ImagesFolder { get; }

        string TempLabelsFolder { get; }

        string RetrainDataFolder { get; }

        string ModelsFolder { get; }

        string GetModelPath(string modelFileName);

        void EnsureStorageFolders();
    }
}
