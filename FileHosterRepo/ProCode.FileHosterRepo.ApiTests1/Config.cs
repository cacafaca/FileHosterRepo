using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ProCode.FileHosterRepo.Api;
using ProCode.FileHosterRepo.Dal.DataAccess;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.ApiTests
{
    static class Config
    {
        #region Fields
        private static IConfigurationRoot configurationRoot;
        private static readonly FileHosterContext fileHosterContext;
        private static readonly WebApplicationFactory<Startup> webAppFactory;
        private static readonly HttpClient client;
        #endregion

        #region Constructor
        static Config()
        {
            using IHost host = CreateHostBuilder(null).Build();

            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseMySQL(configurationRoot.GetConnectionString("FileHosterRepoConnectionString"));
            fileHosterContext = new FileHosterContext(optionsBuilder.Options);

            webAppFactory = new WebApplicationFactory<Startup>();
            client = webAppFactory.CreateClient();
        }
        #endregion

        #region Properties
        public static FileHosterContext DbContext { get { return fileHosterContext; } }

        public static HttpClient Client { get { return client; } }
        #endregion

        #region Methods
        public static async Task RecreateDatabaseAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
        }

        public static void SetToken(this HttpClient client, string token = null)
        {
            client.DefaultRequestHeaders.Remove("Authorization");
            if (token != null)
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    env.EnvironmentName = "Development"; // Check how this value can be set, outside of code.

                    configuration
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

                    configurationRoot = configuration.Build();
                });
        #endregion
    }
}
