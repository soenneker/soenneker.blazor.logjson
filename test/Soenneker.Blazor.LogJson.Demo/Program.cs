using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Soenneker.Blazor.LogJson.Registrars;

namespace Soenneker.Blazor.LogJson.Demo;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            ConfigureLogging(builder.Services);

            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });

            builder.Services
                .AddBlazorise(options =>
                {
                    options.Immediate = true;
                })
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons();

            builder.Services.AddLogJson();

            WebAssemblyHost host = builder.Build();

            var jsRuntime = (IJSRuntime)host.Services.GetService(typeof(IJSRuntime))!;

            SetGlobalLogger(jsRuntime);

            await host.RunAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Stopped program because of exception");
            throw;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static void ConfigureLogging(IServiceCollection services)
    {
        SelfLog.Enable(m => Console.Error.WriteLine(m));

        services.AddLogging(builder =>
        {
            builder.ClearProviders();

            builder.AddSerilog(dispose: true);
        });
    }

    private static void SetGlobalLogger(IJSRuntime jsRuntime)
    {
        var logEventLevel = LogEventLevel.Verbose;

        var loggerConfig = new LoggerConfiguration();
        loggerConfig.MinimumLevel.Is(logEventLevel);

        loggerConfig.Enrich.FromLogContext();

        loggerConfig.WriteTo.BrowserConsole(jsRuntime: jsRuntime, restrictedToMinimumLevel: logEventLevel);

        Log.Logger = loggerConfig.CreateLogger();
    }
}