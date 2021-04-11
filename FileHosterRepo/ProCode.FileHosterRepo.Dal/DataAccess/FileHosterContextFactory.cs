using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Dal.DataAccess
{
    public class FileHosterContextFactory : IDesignTimeDbContextFactory<FileHosterContext>
    {
#if DEBUG
        const string connectionStringDevelopmentJsonFile = "DataAccess\\ConnectionStringDevelopment.json";
#else
        const string connectionStringProductionJsonFile = "DataAccess\\ConnectionStringDevelopment.json";
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
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(GetConnectionStringJsonFile(), optional: true, reloadOnChange: true);
            var connectionString = builder.Build().GetSection("ConnectionString").Value;                
            var optionsBuilder = new DbContextOptionsBuilder<FileHosterContext>();
            optionsBuilder.UseMySQL(connectionString);
            return new FileHosterContext(optionsBuilder.Options);
        }
    }
}
