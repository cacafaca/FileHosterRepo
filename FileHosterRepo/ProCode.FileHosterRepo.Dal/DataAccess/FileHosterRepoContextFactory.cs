using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProCode.FileHosterRepo.Dal.DataAccess
{
    public class FileHosterRepoContextFactory : IDesignTimeDbContextFactory<FileHosterRepoContext>
    {
#if DEBUG
        const string connectionStringDevelopmentJsonFile = "DataAccess\\ConnectionStringDevelopment.json";
#else
        const string connectionStringProductionJsonFile = "DataAccess\\ConnectionStringProduction.json";
#endif

        private static string GetConnectionStringJsonFile()
        {
#if DEBUG
            return connectionStringDevelopmentJsonFile;
#else
            return connectionStringProductionJsonFile;
#endif
        }
        public FileHosterRepoContext CreateDbContext(string[] args)
        {
            return CreateMySqlDbContext();
        }

        private static FileHosterRepoContext CreateMySqlDbContext()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(GetConnectionStringJsonFile(), optional: true, reloadOnChange: true);
            var connectionString = builder.Build().GetSection("ConnectionString").Value;
            var optionsBuilder = new DbContextOptionsBuilder<FileHosterRepoContext>();
            optionsBuilder.UseMySQL(connectionString);
            return new FileHosterRepoContext(optionsBuilder.Options);
        }
        private static FileHosterRepoContext CreateMsSqlDbContext()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(GetConnectionStringJsonFile(), optional: true, reloadOnChange: true);
            var connectionString = builder.Build().GetSection("ConnectionString").Value;
            var optionsBuilder = new DbContextOptionsBuilder<FileHosterRepoContext>();
            //optionsBuilder.UseSqlServer(connectionString);
            return new FileHosterRepoContext(optionsBuilder.Options);
        }
    }
}
