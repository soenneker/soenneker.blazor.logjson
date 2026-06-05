using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Registrars;

namespace Soenneker.Blazor.LogJson.Registrars;

/// <summary>
/// A Blazor interop library that logs JSON (like HTTP requests/responses) within the browser
/// </summary>
public static class LogJsonRegistrar
{
    /// <summary>
    /// Adds log json interop as scoped.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The result of the operation.</returns>
    public static IServiceCollection AddLogJsonInteropAsScoped(this IServiceCollection services)
    {
        services.AddModuleImportUtilAsScoped().TryAddScoped<ILogJsonInterop, LogJsonInterop>();

        return services;
    }
}
