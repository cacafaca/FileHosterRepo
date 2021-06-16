using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace ProCode.FileHosterRepo.Dal.DataAccess
{
    public class FileHosterRepoContextFactory : IDesignTimeDbContextFactory<FileHosterRepoContext>
    {
        #region Fields
        private static IConfigurationRoot configurationRoot;
        #endregion

        #region Constructors
        public FileHosterRepoContextFactory()
        {
            using IHost host = CreateHostBuilder(null).Build();
        }
        #endregion

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();
                    IHostEnvironment env = hostingContext.HostingEnvironment;
                    //throw new System.Exception(env.EnvironmentName);
                    //configuration.AddJsonFile($"ConnectionString.{env.EnvironmentName}.json", true, true);
                    configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    configurationRoot = configuration.Build();
                });

        public FileHosterRepoContext CreateDbContext(string[] args)
        {
            return CreateMySqlDbContext();
        }

        private static FileHosterRepoContext CreateMySqlDbContext()
        {
            var connectionString = configurationRoot.GetConnectionString("FileHosterRepoConnectionString");
            var optionsBuilder = new DbContextOptionsBuilder<FileHosterRepoContext>();
            optionsBuilder.UseMySQL(connectionString);
            return new FileHosterRepoContext(optionsBuilder.Options);
        }
    }
}
