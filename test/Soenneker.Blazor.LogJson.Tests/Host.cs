using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Soenneker.Blazor.LogJson.Registrars;
using Soenneker.Blazor.MockJsRuntime.Registrars;
using Soenneker.TestHosts.Unit;
using Soenneker.Utils.Test;

namespace Soenneker.Blazor.LogJson.Tests;

public class Host : UnitTestHost
{
    public override Task InitializeAsync()
    {
        SetupIoC(Services);

        Services.AddMockJsRuntimeAsScoped();

        return base.InitializeAsync();
    }

    private static void SetupIoC(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: false);
        });

        var config = TestUtil.BuildConfig();
        services.AddSingleton(config);

        services.AddLogJsonInteropAsScoped();
    }
}
