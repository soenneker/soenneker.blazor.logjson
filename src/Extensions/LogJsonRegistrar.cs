using Microsoft.Extensions.DependencyInjection;
using Soenneker.Blazor.LogJson.Abstract;

namespace Soenneker.Blazor.LogJson.Extensions;

public static class LogJsonRegistrar
{
    /// <summary>
    /// Shorthand for <code>services.AddScoped</code>
    /// </summary>
    public static void AddLogJson(this IServiceCollection services)
    {
        services.AddScoped<ILogJsonInterop, LogJsonInterop>();
    }
}