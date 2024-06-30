using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Blazor.Utils.ResourceLoader.Registrars;

namespace Soenneker.Blazor.LogJson.Registrars;

/// <summary>
/// A Blazor interop library that logs JSON (like HTTP requests/responses) within the browser
/// </summary>
public static class LogJsonRegistrar
{
    /// <summary>
    /// Shorthand for <code>services.TryAddScoped</code>
    /// </summary>
    public static void AddLogJson(this IServiceCollection services)
    {
        services.AddResourceLoader();
        services.TryAddSingleton<ILogJsonInterop, LogJsonInterop>();
    }
}