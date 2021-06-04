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

            System.Diagnostics.Trace.WriteLine("cacafaca: " + builder.HostEnvironment.BaseAddress);

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            LoadHttpClients(builder);

            await builder.Build().RunAsync();
        }

        private static void LoadHttpClients(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddHttpClient<IIndexViewModel, IndexViewModel>
                //("FileHosterRepoApiClient", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
                ("FileHosterRepoApiClient", client => client.BaseAddress = new Uri("https://localhost:44300"));
        }
    }
}
