using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace DAL.DBContext
{
    public class ApplicationDBContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
    {
        public ApplicationDBContext CreateDbContext(string[] args)
        {
            string basePath = Directory.GetCurrentDirectory();
            string uiProjectPath = Path.Combine(basePath, "..", "WPFhospitalXray");

            if (!Directory.Exists(uiProjectPath))
            {
                uiProjectPath = basePath;
            }

            string appSettingsPath = Path.Combine(uiProjectPath, "appsettings.json");
            string connectionString = ReadConnectionString(appSettingsPath);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDBContext(optionsBuilder.Options);
        }

        private static string ReadConnectionString(string appSettingsPath)
        {
            if (!File.Exists(appSettingsPath))
            {
                throw new FileNotFoundException("Файл appsettings.json не знайдено для створення DbContext.", appSettingsPath);
            }

            using var stream = File.OpenRead(appSettingsPath);
            using var document = JsonDocument.Parse(stream);

            if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) ||
                !connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            return defaultConnection.GetString()
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is empty.");
        }
    }
}
