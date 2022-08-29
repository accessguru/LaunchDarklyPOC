using System.Runtime.CompilerServices;
using BlazorApp2.Client.FeatureFlag;
using BlazorApp2.Server;
using BlazorApp2.Server.FeatureFlag;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();
            builder.Services.AddMemoryCache();

            var featureFlagService = new FeatureFlagServerService();
            builder.Services.AddScoped(_ => featureFlagService);

            var app = builder.Build();

            featureFlagService.FeatureFlagHubContext = app.Services.GetRequiredService<IHubContext<FeatureFlagHub, IFeatureFlagClient>>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.MapRazorPages();
            app.MapControllers();
            app.MapHub<FeatureFlagHub>("/featureFlagHub");

            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}