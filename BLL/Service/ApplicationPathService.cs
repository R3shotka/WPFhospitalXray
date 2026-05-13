using BLL.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BLL.Service
{
    public class ApplicationPathService : IApplicationPathService
    {
        private const string DefaultApplicationFolderName = "HospitalXray";

        private readonly IConfiguration _configuration;

        public ApplicationPathService(IConfiguration configuration)
        {
            _configuration = configuration;

            BaseDataFolder = ResolveBaseDataFolder();

            EnsureStorageFolders();
        }

        public string BaseDataFolder { get; }

        public string ImagesFolder => Path.Combine(BaseDataFolder, "Images");

        public string TempLabelsFolder => Path.Combine(BaseDataFolder, "TempLabels");

        public string RetrainDataFolder => Path.Combine(BaseDataFolder, "RetrainData");

        public string ModelsFolder
        {
            get
            {
                string? configuredModelsFolder = _configuration["Storage:ModelsFolder"];

                if (!string.IsNullOrWhiteSpace(configuredModelsFolder))
                {
                    return Path.IsPathRooted(configuredModelsFolder)
                        ? configuredModelsFolder
                        : Path.Combine(AppContext.BaseDirectory, configuredModelsFolder);
                }

                return Path.Combine(AppContext.BaseDirectory, "Models");
            }
        }

        public string GetModelPath(string modelFileName)
        {
            return Path.Combine(ModelsFolder, modelFileName);
        }

        public void EnsureStorageFolders()
        {
            Directory.CreateDirectory(BaseDataFolder);
            Directory.CreateDirectory(ImagesFolder);
            Directory.CreateDirectory(TempLabelsFolder);
            Directory.CreateDirectory(RetrainDataFolder);
        }

        private string ResolveBaseDataFolder()
        {
            string? configuredBaseDataFolder = _configuration["Storage:BaseDataFolder"];

            if (!string.IsNullOrWhiteSpace(configuredBaseDataFolder))
            {
                return configuredBaseDataFolder;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                DefaultApplicationFolderName);
        }
    }
}
