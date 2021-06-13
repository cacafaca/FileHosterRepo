using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProCode.FileHosterRepo.WebAppBlazor.ViewModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.WebAppBlazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();

            // This adds client to web application. Not WebAPI.
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddHttpClient(BaseViewModel.HttpClientName, c => { c.BaseAddress = new Uri("https://localhost:44300"); });

            RegisterViewModels(builder);

            builder.Services.AddScoped<IMenuService, MenuService>();

            await builder.Build().RunAsync();
            Util.Trace("Run!");
        }

        private static void RegisterViewModels(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddScoped<IIndexViewModel>(sp => new IndexViewModel(sp.GetService<IHttpClientFactory>()));
            builder.Services.AddSingleton<ViewModel.Admin.IAdminViewModel>(sp => new ViewModel.Admin.AdminViewModel(sp.GetService<IHttpClientFactory>()));
            builder.Services.AddSingleton<ViewModel.User.IUserViewModel>(sp => new ViewModel.User.UserViewModel(sp.GetService<IHttpClientFactory>(), sp.GetService<ViewModel.Admin.IAdminViewModel>()));
        }
    }
}
