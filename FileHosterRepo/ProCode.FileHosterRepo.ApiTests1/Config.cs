using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ProCode.FileHosterRepo.Api;
using ProCode.FileHosterRepo.Dal.DataAccess;

namespace ProCode.FileHosterRepo.ApiTests
{
    static class Config
    {
        #region Fields
        static IConfigurationRoot configurationRoot;
        #endregion

        #region Constructor
        static Config()
        {
            using IHost host = CreateHostBuilder(null).Build();
        }
        #endregion

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    env.EnvironmentName = "Developemnt"; // Check how this value can be set, outside of code.

                    configuration
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);

                    configurationRoot = configuration.Build();

                    //TransientFaultHandlingOptions options = new();
                    //configurationRoot.GetSection(nameof(TransientFaultHandlingOptions))
                    //                 .Bind(options);

                    //Console.WriteLine($"TransientFaultHandlingOptions.Enabled={options.Enabled}");
                    //Console.WriteLine($"TransientFaultHandlingOptions.AutoRetryDelay={options.AutoRetryDelay}");
                });
        public static string GetSecretKey()
        {
            return configurationRoot.GetSection("Jwt:Key").Value;
        }

        public static string GetConnectionString()
        {
            return configurationRoot.GetConnectionString("FileHosterRepoConnectionString");
        }

        public static FileHosterContext GetDbContext()
        {
            DbContextOptionsBuilder optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseMySQL(ApiTests.Config.GetConnectionString());
            var context = new FileHosterContext(optionsBuilder.Options);
            return context;
        }

        public static JwtAuthenticationManager GetJwtAuthenticationManager()
        {
            return new JwtAuthenticationManager(GetSecretKey());
        }
    }
}
