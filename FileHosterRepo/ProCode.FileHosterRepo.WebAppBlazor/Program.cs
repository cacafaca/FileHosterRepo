using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProCode.FileHosterRepo.WebAppBlazor.ViewModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //throw new Exception("Mamu ti...");

            Common.Util.Trace("Initialize web application!");

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            // This adds client to web application. Not WebAPI.
            Common.Util.Trace(builder.HostEnvironment.BaseAddress);
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            Thread.Sleep(5000);

            Uri clientUri;
            if (builder.HostEnvironment.IsDevelopment())
            {
                clientUri = new Uri("https://api.filehosterrepo.development");
            }
            else if (builder.HostEnvironment.IsStaging())
            {
                throw new Exception("wrong environment: " + builder.HostEnvironment.Environment);
                clientUri = new Uri("https://api.filehosterrepo.development");
            }
            else if (builder.HostEnvironment.IsProduction())
            {
                throw new Exception("wrong environment: " + builder.HostEnvironment.Environment);
                clientUri = new Uri("https://api.filehosterrepo.development");
            }
            else
            {
                throw new UnknownEnvironmentException(builder.HostEnvironment.Environment);
            }
            builder.Services.AddHttpClient(BaseViewModel.HttpClientName, c => { c.BaseAddress = clientUri; });

            RegisterViewModels(builder);

            builder.Services.AddScoped<IMenuService, MenuService>();

            Common.Util.Trace("Run web application!");

            await builder.Build().RunAsync();            
        }

        private static void RegisterViewModels(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<IIndexViewModel>(sp => new IndexViewModel(sp.GetService<IHttpClientFactory>()));
            builder.Services.AddSingleton<ViewModel.Admin.IAdminViewModel>(sp => new ViewModel.Admin.AdminViewModel(sp.GetService<IHttpClientFactory>()));
            builder.Services.AddSingleton<ViewModel.User.IUserViewModel>(sp => new ViewModel.User.UserViewModel(sp.GetService<IHttpClientFactory>(), sp.GetService<ViewModel.Admin.IAdminViewModel>()));
            builder.Services.AddSingleton<ViewModel.Media.IMediaViewModel>(sp => new ViewModel.Media.MediaViewModel(sp.GetService<IHttpClientFactory>(), sp.GetService<ViewModel.User.IUserViewModel>()));
        }
    }
}
