using BlazorApp2.Client.FeatureFlag;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorApp2.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            builder.Services.AddScoped(_ => httpClient);

            var featureFlagClientService = new FeatureFlagClientService();
            builder.Services.AddScoped(_ => featureFlagClientService);

            var app = builder.Build();

            featureFlagClientService.NavigationManager = (NavigationManager)app.Services.GetService(typeof(NavigationManager));
            featureFlagClientService.HttpClient = httpClient;
            await featureFlagClientService.StartListener();

            await app.RunAsync();
        }
    }
}