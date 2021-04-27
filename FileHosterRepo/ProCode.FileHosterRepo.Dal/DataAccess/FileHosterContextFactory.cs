using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProCode.FileHosterRepo.Dal.DataAccess
{
    public class FileHosterContextFactory : IDesignTimeDbContextFactory<FileHosterContext>
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
        public FileHosterContext CreateDbContext(string[] args)
        {
            return CreateMySqlDbContext();
        }

        private static FileHosterContext CreateMySqlDbContext()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(GetConnectionStringJsonFile(), optional: true, reloadOnChange: true);
            var connectionString = builder.Build().GetSection("ConnectionString").Value;
            var optionsBuilder = new DbContextOptionsBuilder<FileHosterContext>();
            optionsBuilder.UseMySQL(connectionString);
            return new FileHosterContext(optionsBuilder.Options);
        }
        private static FileHosterContext CreateMsSqlDbContext()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(GetConnectionStringJsonFile(), optional: true, reloadOnChange: true);
            var connectionString = builder.Build().GetSection("ConnectionString").Value;
            var optionsBuilder = new DbContextOptionsBuilder<FileHosterContext>();
            //optionsBuilder.UseSqlServer(connectionString);
            return new FileHosterContext(optionsBuilder.Options);
        }
    }
}
